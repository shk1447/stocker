using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    public class ConditionMultiStockArgs : EventArgs
    {
        public List<StockCurrentPrice> ConditionMultiStockList { get; set; }

        public ConditionMultiStockArgs(List<StockCurrentPrice> conditionMultiStockList)
        {
            this.ConditionMultiStockList = conditionMultiStockList;
        }
    }
}
