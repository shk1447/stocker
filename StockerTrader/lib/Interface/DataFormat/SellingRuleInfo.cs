using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface.DataFormat
{
    public class SellingRuleInfo : RuleModel
    {
        //손절
        public string CutRate { get; set; }

        /// <summary>
        /// 매매 비중
        /// </summary>
        public double TradePercent { get; set; }
    }
}
