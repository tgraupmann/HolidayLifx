using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LIFX
{
    public class Color
    {
        public double hue { get; set; }
        public double saturation { get; set; }
        public int kelvin { get; set; }
    }

    public class Group
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Location
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Capabilities
    {
        public bool has_color { get; set; }
        public bool has_variable_color_temp { get; set; }
        public bool has_ir { get; set; }
        public bool has_multizone { get; set; }
    }

    public class Product
    {
        public string name { get; set; }
        public string identifier { get; set; }
        public string company { get; set; }
        public Capabilities capabilities { get; set; }
    }

    [DataContract]
    public class Light
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string uuid { get; set; }
        [DataMember]
        public string label { get; set; }
        [DataMember]
        public bool connected { get; set; }
        [DataMember]
        public string power { get; set; }
        [DataMember]
        public Color color { get; set; }
        [DataMember]
        public double brightness { get; set; }
        [DataMember]
        public Group group { get; set; }
        [DataMember]
        public Location location { get; set; }
        [DataMember]
        public Product product { get; set; }
        [DataMember]
        public object infrared { get; set; }
        [DataMember]
        public string last_seen { get; set; }
        [DataMember]
        public double seconds_since_seen { get; set; }
    }

    public class State
    {
        public double brightness { get; set; }
        public string color { get; set; }
        public string power { get; set; }
    }

    public class Defaults
    {
        public string power { get; set; }
        public double duration { get; set; }
    }

    [DataContract]
    public class CycleInput
    {
        [DataMember]
        public List<State> states { get; set; }
        [DataMember]
        public Defaults defaults { get; set; }
    }

    [DataContract]
    public class SetInput
    {
        [DataMember]
        public string power { get; set; }
        [DataMember]
        public string color { get; set; }
        [DataMember]
        public double brightness { get; set; }
        [DataMember]
        public double duration { get; set; }
    }
}
