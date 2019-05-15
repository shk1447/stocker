using CollectorInterface.Class;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorInterface.Interface
{
    public interface IDBModule
    {
        void Initialize(JObject config);
        void Push(JObject config, JObject data);
        JObject Get(JObject query);
    }
}
