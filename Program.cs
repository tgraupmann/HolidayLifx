using LIFX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Runtime.Serialization.Json;

namespace HolidayLifx
{
    class Program
    {
        static bool _sKeepWorking = true;
        static List<Light> _sLights = new List<Light>();
        static Dictionary<Light, int> _sOldColors = new Dictionary<Light, int>();

        static string[] _sColors =
        {
            "white saturation:0.0",
            "green saturation:1.0",
            "white saturation:0.0",
            "red saturation:1.0",
        };

        static void GetConnectedLights()
        {
            try
            {
                string url = "https://api.lifx.com/v1/lights/all";
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Headers["Authorization"] = string.Format("Bearer {0}", Authentication.TOKEN);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Light[]));
                Light[] lights = (Light[])ser.ReadObject(stream);
                foreach (Light light in lights)
                {
                    Console.WriteLine("id={0} Connected={1}", light.id, light.connected);
                    if (light.connected)
                    {
                        _sLights.Add(light);
                        _sOldColors[light] = 0;
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    var resp = (HttpWebResponse)ex.Response;
                    Console.Error.WriteLine("[ERROR] Failed to get connected lights status={0}", resp.StatusCode);
                }
                else
                {
                    Console.Error.WriteLine("[ERROR] Failed to get connected lights");
                }
            }
        }

        static void DoCycle(Light light)
        {
            DataContractJsonSerializer dataContractJsonSerializer =
                    new DataContractJsonSerializer(typeof(CycleInput));

            int oldColor = _sOldColors[light];
            int newColor = (oldColor + 1) % _sColors.Length;
            _sOldColors[light] = newColor;

            #region Cycle

            CycleInput cycleInput = new CycleInput();

            cycleInput.defaults = new Defaults();
            cycleInput.defaults.duration = 2;
            cycleInput.defaults.power = "on";

            cycleInput.states = new List<State>();

            State state;

            state = new State();
            state.brightness = 1.0;
            state.color = string.Format("{0}", _sColors[oldColor]);
            state.power = "on";
            cycleInput.states.Add(state);

            state = new State();
            state.brightness = 1.0;
            state.color = string.Format("{0}", _sColors[newColor]);
            state.power = "on";
            cycleInput.states.Add(state);

            string body = "";
            using (MemoryStream ms = new MemoryStream())
            {
                dataContractJsonSerializer.WriteObject(ms, cycleInput);
                ms.Flush();
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                {
                    body = sr.ReadToEnd();
                }
                //Console.WriteLine("id={0} cycle", light.id);
                //Console.WriteLine("id={0} body={1}", light.id, body);
            }

            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Authorization", string.Format("Bearer {0}", Authentication.TOKEN));
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                string url = string.Format("https://api.lifx.com/v1/lights/id:{0}/cycle", light.id);
                try
                {
                    string responseData = client.UploadString(url, "POST", body);

                    // Decode and display the response.
                    //Console.WriteLine("id={0} Response={1}", light.id, responseData);
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                    {
                        var resp = (HttpWebResponse)ex.Response;
                        Console.Error.WriteLine("[ERROR] id={0} Failed to cycle status={1}", light.id, resp.StatusCode);
                    }
                    else
                    {
                        Console.Error.WriteLine("[ERROR] id={0} Failed to cycle", light.id);
                    }
                }
            }

            #endregion
        }

        static void DoSet(Light light)
        {
            DataContractJsonSerializer dataContractJsonSerializer =
                    new DataContractJsonSerializer(typeof(SetInput));

            int oldColor = _sOldColors[light];
            int newColor = (oldColor + 1) % _sColors.Length;
            _sOldColors[light] = newColor;

            #region Set

            SetInput setInput = new SetInput();

            setInput.power = "on";
            setInput.color = _sColors[newColor];
            setInput.brightness = 1.0;
            setInput.duration = 0;

            string body = "";
            using (MemoryStream ms = new MemoryStream())
            {
                dataContractJsonSerializer.WriteObject(ms, setInput);
                ms.Flush();
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                {
                    body = sr.ReadToEnd();
                }
                //Console.WriteLine("id={0} cycle", light.id);
                //Console.WriteLine("id={0} body={1}", light.id, body);
            }

            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Authorization", string.Format("Bearer {0}", Authentication.TOKEN));
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                string url = string.Format("https://api.lifx.com/v1/lights/id:{0}/state", light.id);
                try
                {
                    string responseData = client.UploadString(url, "PUT", body);

                    // Decode and display the response.
                    //Console.WriteLine("id={0} Response={1}", light.id, responseData);
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                    {
                        var resp = (HttpWebResponse)ex.Response;
                        Console.Error.WriteLine("[ERROR] id={0} Failed to cycle status={1}", light.id, resp.StatusCode);
                    }
                    else
                    {
                        Console.Error.WriteLine("[ERROR] id={0} Failed to cycle", light.id);
                    }
                }
            }

            #endregion
        }

        static void CycleWorker(Object threadObject)
        {
            Random random = new Random();
            Light light = threadObject as Light;
            while (_sKeepWorking)
            {
                //DoCycle(light);
                //Thread.Sleep(1000);

                DoSet(light);
                Thread.Sleep(1000+random.Next()%100);
            }

            _sOldColors[light] = _sOldColors.Count - 1;
            DoSet(light);
        }

        static void Main(string[] args)
        {
            new Thread(new ThreadStart(() =>
            {
                GetConnectedLights();

                foreach (Light light in _sLights)
                {
                    ParameterizedThreadStart ts = new ParameterizedThreadStart(CycleWorker);
                    Thread thread = new Thread(ts);
                    thread.Start(light);
                }
            })).Start();

            Console.WriteLine("Press a key to exit.");
            Console.ReadKey();

            Console.WriteLine("Key found exiting...");
            _sKeepWorking = false;
        }

        private static void PrintStream(Stream stream)
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                Console.WriteLine(sr.ReadToEnd());
            }
        }
    }
}
