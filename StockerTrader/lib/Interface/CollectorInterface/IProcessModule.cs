using CollectorInterface.Class;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorInterface.Interface
{
    public interface IProcessModule
    {
        JObject Run(JobInfo jobInfo, string collectData);
    }
}
