using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    public class UserInfo
    {
        public string UserID { get; set; }
        public string Name { get; set; }
        public string[] Accounts { get; set; }
        public bool IsMockInvestment { get; set; }
    }

    public class SimulationResult
    {
        public string StCode { get; set; }
        public double TotalPurchaseAmount { get; set; }
        public double TotalSellingAmount { get; set; }
    }

    public class AccountInfo
    {
        public int 총매입금액 { get; set; }
        public int 예수금 { get; set; }
        public int 유가잔고평가액 { get; set; }
        public int 예탁자산평가액 { get; set; }
        public int 누적투자원금 { get; set; }
        public int 누적투자손익 { get; set; }
        public int 누적손익율 { get; set; }
    }

    public class ScheduleStockItemOnMongo : ScheduleStockItem
    {
        public ObjectId _id;
    }

    public class ScheduleStockItem
    {
        public int ScheduleId { get; set; }
        public string StCode { get; set; }
        public List<TradeHistory> TradeHistory { get; set; }
        public int CumulativePrice { get; set; }
        public int CumulativeCount { get; set; }
        /// <summary>
        /// 매도여부
        /// </summary>
        public bool IsSelling { get; set; }
    }

    public class PurchaseStockItem
    {
        public int HaveStockCount { get; set; }
        public int OrderCount { get; set; }
        public int OrderPrice { get; set; }
        public int PurchaseIndex { get; set; }
    }

    public class TradeHistory
    {
        public int Price { get; set; }
        public int Count { get; set; }
    }

    public class ChartModel
    {
        public double Avg5 { get; set; }
        public double Avg20 { get; set; }
        public double Avg60 { get; set; }
        public double Avg120 { get; set; }
    }

    public class AllStockInfo
    {
        /// <summary>
        /// 코스피
        /// </summary>
        public Dictionary<string, string> KosP { get; set; }
        /// <summary>
        /// 코스닥
        /// </summary>
        public Dictionary<string, string> KosQ { get; set; }
    }

    public enum OrderType
    {
        //1:신규매수, 2:신규매도 3:매수취소, 4:매도취소, 5:매수정정, 6:매도정정
        신규매수 = 1, 신규매도 = 2, 매수취소 = 3, 매도취소 = 4, 매수정정 = 5, 매도정정 = 6
    }

    public enum HogaCode
    {
        지정가 = 0, 시장가 = 3, 조건부지정가 = 5, 최유리지정가 = 6, 최우선지정가 = 7, 지정가IOC = 10,
        시장가IOC = 13, 최유리IOC = 16, 지정가FOK = 20, 시장가FOK = 23, 최유리FOK = 26, 시간외단일가매매 = 61, 시간외종가 = 81
    }
}
