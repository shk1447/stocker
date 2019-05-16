using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    public class RepeatAnalysisInfoOnMongo : RepeatAnalysisInfo
    {
    }

    public class RepeatAnalysisInfo
    {
        //승률
        public double WinRate { get; set; }
        public bool IsKospi { get; set; }
        public double TradeCount { get; set; }
        public double TotalGainLossRate { get; set; }
        public double MaxGainLossRate { get; set; }
        public double TradeDayAvg { get; set; }
        public double CountGainLossRateAvg { get; set; }
        public double DayAvgGainLossRate { get; set; }
        public double WinPossibleRate { get; set; }
        public double NextDayWinRate { get; set; }
        public double NextDaySumGainLossRate { get; set; }
        public double NextDaySumGainLossRateDayAvg { get; set; }

        public Dictionary<string, double> RepeatAnalysisValues { get; set; }
    }
}
