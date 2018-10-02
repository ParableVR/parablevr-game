using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace parable.objects
{
    public class CloudObject
    {
        public string id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float yaw { get; set; }
        public float pitch { get; set; }
        public float roll { get; set; }
        public float scale_x { get; set; }
        public float scale_y { get; set; }
        public float scale_z { get; set; }
        public bool significant { get; set; }
    }
}
