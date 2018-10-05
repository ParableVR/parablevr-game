using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace parable.objects
{
    public class CloudSession
    {
        public string id { get; set; }
        public string name { get; set; }
        public string scenario { get; set; }
        public DateTime when_created { get; set; }
        public DateTime? when_started { get; set; }
        public DateTime? when_ended { get; set; }
        public List<CloudSessionPeople> people { get; set; }
    }

    public class CloudSessionPeople
    {
        public string user { get; set; }
        public bool spectator { get; set; }
        public bool host { get; set; }
        public DateTime when_joined { get; set; }
        public DateTime? when_left { get; set; }
    }
}
