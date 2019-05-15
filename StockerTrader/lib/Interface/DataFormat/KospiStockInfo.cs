using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    public class KospiStockInfoOnMongo
    {
        public string _id { get; set; }
        public List<string> KospiItems { get; set; }
    }
}
