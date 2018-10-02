using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace parable.objects
{
    public class CloudScenario
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string goal { get; set; }
        public DateTime when_created { get; set; }
        public DateTime? when_deleted { get; set; }
        public string linked_preb_scenario { get; set; }
        public string linked_next_scenario { get; set; }
        public List<CloudObject> objects { get; set; }
    }
}
