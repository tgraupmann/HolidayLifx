﻿using LIFX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Net;
using System.Threading;
using System.Runtime.Serialization.Json;

namespace HolidayLifx
{
    class Program
    {
        static List<Light> _sLights = new List<Light>();

        static void GetAllLights()
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
                Console.WriteLine("Found: {0} Connected={1}", light.id, light.connected);
                if (light.connected)
                {
                    _sLights.Add(light);
                }
            }
        }

        static void Main(string[] args)
        {
            if (true)
            {
                GetAllLights();

                DataContractJsonSerializer serSetState = new DataContractJsonSerializer(typeof(SetState));

                foreach (Light light in _sLights)
                {
                    SetState setState = new SetState();

                    setState.defaults = new Defaults();
                    setState.defaults.duration = 2.0;
                    setState.defaults.power = "on";

                    setState.states = new List<State>();

                    State state;

                    state = new State();
                    state.brightness = 1.0;
                    state.power = "on";
                    setState.states.Add(state);

                    state = new State();
                    state.brightness = 0.5;
                    state.power = "on";
                    setState.states.Add(state);

                    state = new State();
                    state.brightness = 0.1;
                    state.power = "on";
                    setState.states.Add(state);

                    string body = "";
                    using (MemoryStream ms = new MemoryStream())
                    {
                        serSetState.WriteObject(ms, setState);
                        ms.Flush();
                        ms.Position = 0;
                        using (StreamReader sr = new StreamReader(ms))
                        {
                            body = sr.ReadToEnd();
                        }
                        Console.WriteLine(body);
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
                            Console.WriteLine("id: {0} Response={1}", light.id, responseData);
                        }
                        catch (WebException ex)
                        {
                            if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                            {
                                var resp = (HttpWebResponse)ex.Response;
                                Console.Error.WriteLine("Failed to cycle id={0} status={1}", light.id, resp.StatusCode);
                            }
                            else
                            {
                                Console.Error.WriteLine("Failed to cycle id={0}", light.id);
                            }
                        }
                    }

                    Thread.Sleep(1000);
                }
            }

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
