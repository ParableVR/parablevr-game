using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace parable.objects
{
    public class CloudEventSessionResponse
    {
        public string message { get; set; }
        public CloudEventSession event_session { get; set; }
    }
}
