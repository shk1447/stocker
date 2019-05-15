using CollectorInterface.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorInterface.Interface
{
    public interface ICollectModule
    {
        IEnumerator<string> Run(JobInfo jobInfo);
    }
}
