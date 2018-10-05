using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace parable.objects
{
    public class CloudEventSession
    {
        public string id { get; set; }
        public string session { get; set; }
        public DateTime when_started { get; set; }
        public DateTime? when_deleted { get; set; }
        public string timeline_file { get; set; }
        public List<CloudEventObject> events { get; set; }
    }

    public class CloudEventObject
    {
        public string id { get; set; }
        public string type { get; set; }
        public string user { get; set; }
        public DateTime when_occured { get; set; }
        public double contact_duration { get; set; }
        public bool result { get; set; }
        public string object_coordinator { get; set; }
        public List<string> objects_involved { get; set; }
    }
}
