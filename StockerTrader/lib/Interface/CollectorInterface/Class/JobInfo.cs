using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorInterface.Class
{
    public class JobInfo
    {
        
        public string Collector { get; set; }
        public string Preprocessor { get; set; }

        public JObject Request { get; set; }

        public JObject Before { get; set; }
        public JArray Filter { get; set; }

        public JObject Map { get; set; }
        public JObject Reduce { get; set; }
    }
}
