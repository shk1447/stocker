using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    public class OrderConcludInfoOnMongo : OrderConcludInfo
    {
        public ObjectId _id;
    }

    public class OrderConcludInfo
    {
        public int ScheduleId { get; set; }
        /// <summary>
        /// "9201" : "계좌번호" 
        /// </summary>
        public string Account { get; set; }
        //"9203" : "주문번호" 
        public string OrderNumber { get; set; }
        //"9001" : "종목코드" 
        public string StCode { get; set; }
        //"913" : "주문상태" 
        public string OrderStatus { get; set; }
        //"302" : "종목명" 
        public string StName { get; set; }
        //"900" : "주문수량" 
        public int OrderCount { get; set; }
        //"901" : "주문가격" 
        public int OrderPrice { get; set; }
        //"902" : "미체결수량" 
        public int UnfinishedCount { get; set; }
        //"903" : "체결누계금액" 
        public int TotalPrice { get; set; }
        //"904" : "원주문번호" 
        //"905" : "주문구분" 
        public string OrderDivision { get; set; }
        //"906" : "매매구분" 
        public string TradingDivision { get; set; }
        //"907" : "매도수구분" 
        //"908" : "주문/체결시간" 
        public DateTime OrderConcludTime { get; set; }
        //"909" : "체결번호" 
        //"910" : "체결가" 
        public int OrderConcludPrice { get; set; }
        //"911" : "체결량"
        public int OrderConcludCount { get; set; }

        /// <summary>
        /// 수수료 이후 가격
        /// </summary>
        public int PriceAfterFees { get; set; }

        public int GainLossPrice { get; set; }
        public double GainLossRate { get; set; }

        public string Day { get; set; }

        //"10" : "현재가" 
        //public int CurrentPrice { get; set; }
        //"27" : "(최우선)매도호가" 
        //"28" : "(최우선)매수호가" 
        //"914" : "단위체결가" 
        //"915" : "단위체결량" 
        //"919" : "거부사유" 
        //"920" : "화면번호" 
        //"917" : "신용구분" 
        //"916" : "대출일" 
        //"930" : "보유수량" 
        //public int HaveStockCount { get; set; }
        //"931" : "매입단가" 
        //"932" : "총매입가"
        //public int TotalPurchasePrice { get; set; }
        //"933" : "주문가능수량" 
        //"945" : "당일순매수수량" 
        //"946" : "매도/매수구분" 
        //"950" : "당일총매도손일" 
        //"951" : "예수금" 
        //"307" : "기준가" 
        //"8019" : "손익율" 
        //public double GainLossRate { get; set; }
        //"957" : "신용금액" 
        //"958" : "신용이자" 
        //"918" : "만기일" 
        //"990" : "당일실현손익(유가)" 
        //"991" : "당일실현손익률(유가)" 
        //"992" : "당일실현손익(신용)" 
        //"993" : "당일실현손익률(신용)" 
        //"397" : "파생상품거래단위" 
        //"305" : "상한가" 
        //"306" : "하한가"
    }

    public class OrderConcludInfoArgs : EventArgs
    {
        public OrderConcludInfo OrderConcludInfo { get; set; }

        public OrderConcludInfoArgs(OrderConcludInfo orderConcludInfo)
        {
            this.OrderConcludInfo = orderConcludInfo;
        }
    }
}
