using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    public class ConditionAnalysisLogOnMongo : ConditionAnalysisLog
    {
        public ObjectId _id;
    }

    public class ConditionAnalysisLog
    {
        public string StCode { get; set; }
        //False Kosdac
        public bool IsKospi { get; set; }
        public double GainLossRate { get; set; }
        public bool IsWin { get; set; }

        public int PurchasePrice { get; set; }
        public int SellingPrice { get; set; }

        public DateTime MaxPriceTime { get; set; }

        public DateTime PurcahseTime { get; set; }
        public DateTime SellingTime { get; set; }

        public int PurchaseDays { get; set; }
        public int MaxPrice { get; set; }
        public int MinPrice { get; set; }

        /// <summary>
        /// 지표명, 기준값, 실제값
        /// </summary>
        public ConditionAnalysisLogSubInfo PurchaseSubInfo { get; set; }
        public ConditionAnalysisLogSubInfo SellingSubInfo { get; set; }
        public ConditionAnalysisLogSubInfo MaxSubInfo { get; set; }
        public ConditionAnalysisLogSubInfo MinSubInfo { get; set; }

        public double NextDayHighPrice { get; set; }
    }

    public class ConditionAnalysisLogSubInfo
    {
        public Dictionary<string, double> 스토캐스틱 { get; set; }
        public Dictionary<string, double> RSI { get; set; }
        public Dictionary<string, double> PIVOT { get; set; }
        public Dictionary<string, double> 거래량 { get; set; }
        public Dictionary<string, double> 볼린저밴드 { get; set; }
        public Dictionary<string, double> MACD { get; set; }
        public Dictionary<string, double> MACD_OSC { get; set; }
        public Dictionary<string, double> 이격도 { get; set; }
        public Dictionary<string, double> TRIX { get; set; }
        public Dictionary<string, double> 주가 { get; set; }
    }
}
