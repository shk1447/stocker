using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    /// <summary>
    /// 현재가 정보
    /// </summary>
    public class StockCurrentPrice
    {
        /// <summary>
        /// 종목코드
        /// </summary>
        public string StCode { get; set; }
        /// <summary>
        /// 종목명
        /// </summary>
        public string StName { get; set; }
        /// <summary>
        /// 현재가
        /// </summary>
        public int CurrentPrice { get; set; }
        /// <summary>
        /// 전일대비
        /// </summary>
        public int ChangePrice { get; set; }
        /// <summary>
        /// 등락율
        /// </summary>
        public double ChangeRate { get; set; }
        /// <summary>
        /// 누적거래량
        /// </summary>
        public long CumulativeVolume { get; set; }

        public string StType { get; set; }
    }

    public class StockCurrentPriceArgs : EventArgs
    {
        public StockCurrentPrice StockCurrentPrice { get; set; }

        public StockCurrentPriceArgs(StockCurrentPrice stockCurrentPrice)
        {
            this.StockCurrentPrice = stockCurrentPrice;
        }
    }
}
