using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface.DataFormat
{
    public class Base
    {
        public string _id { get; set; }
    }

    public static class BaseExtensions
    {
        public static string NextBaseId(this IEnumerable<Base> baseCollection)
        {
            var idNum = 1;
            if (baseCollection.Any())
            {
                var maxValue = baseCollection.Max(x => Convert.ToInt32(x._id));
                idNum = maxValue + 1;
            }

            return idNum.ToString();
        }

        public static void ResetBaseId(this IEnumerable<Base> baseCollection)
        {
            int sequence = 0;
            foreach (var item in baseCollection)
            {
                item._id = (++sequence).ToString();
            }
        }
    }
}
