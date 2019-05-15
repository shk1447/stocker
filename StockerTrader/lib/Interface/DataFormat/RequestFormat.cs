using Interface.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface.DataFormat
{
    public class RequestFormat
    {
        public List<string> StCodes { get; set; }
        public ConditionModel ConditionModel { get; set; }
        public string StartDate { get; set; }
    }
}
