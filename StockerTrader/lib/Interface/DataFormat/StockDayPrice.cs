using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    public class StockTimePriceTick : StockTimePrice
    {
        public string _id
        {
            get
            {
                return this.StCode;
            }
        }
    }

    public class StockTimePriceDay : StockTimePrice
    {
        public string _id
        {
            get
            {
                return this.StCode;
            }
        }
    }

    /// <summary>
    /// 현재가 정보
    /// </summary>
    public class StockTimePrice
    {
        public enum TimeType
        {
            Tick, Minute, Day, Week, Month, DayTrend, Multy, BarTick, BarMinute, BarDay
        }

        public TimeType Type { get; set; }
        /// <summary>
        /// 종목명
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 종목코드
        /// </summary>
        public string StCode { get; set; }
        /// <summary>
        /// 날짜 데이터
        /// </summary>
        public List<TimePrice> PriceList { get; set; }
    }

    public class TimePrice
    {
        /// <summary>
        /// 일자
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// 일자
        /// </summary>
        public long UnixDateTime { get; set; }
        /// <summary>
        /// 현재가(종가)
        /// </summary>
        public int Price { get; set; }
        /// <summary>
        /// 거래량
        /// </summary>
        public long Volume { get; set; }
        /// <summary>
        /// 시가
        /// </summary>
        public int StartPrice { get; set; }
        /// <summary>
        /// 저가
        /// </summary>
        public int LowPrice { get; set; }
        /// <summary>
        /// 고가
        /// </summary>
        public int HighPrice { get; set; }
    }

    public class StockTimePriceArgs : EventArgs
    {
        public StockTimePrice StockTimePrice { get; set; }

        public StockTimePriceArgs(StockTimePrice stockTimePrice)
        {
            this.StockTimePrice = stockTimePrice;
        }
    }
}
