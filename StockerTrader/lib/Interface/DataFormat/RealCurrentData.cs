using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    public class RealCurrentData
    {
        public string StCode { get; set; }
        public DateTime DateTime { get; set; }
        public long UnixTime { get; set; }
        public int CurrentPrice { get; set; }
        public int 전일대비 { get; set; }
        public double 등락율 { get; set; }
        //public string 매도호가 { get; set; }
        //public string 매수호가 { get; set; }
        public long 거래량 { get; set; }
        //public string 누적거래량 { get; set; }
        //public string 누적거래대금 { get; set; }
        public int 시가 { get; set; }
        public int 고가 { get; set; }
        public int 저가 { get; set; }
        //public string 전일대비기호 { get; set; }
        //public string 전일거래량대비_계약 { get; set; }
        //public string 거래대금증감 { get; set; }
        //public string 전일거래량대비_비율 { get; set; }
        //public string 거래회전율 { get; set; }
        //public string 거래비용 { get; set; }
        //public string 체결강도 { get; set; }
        //public string 시가총액 { get; set; }
        //public string 장구분 { get; set; }
        //public string KO접근도 { get; set; }
        //public string 상한가발생시간 { get; set; }
        //public string 하한가발생시간 { get; set; }
    }

    public class RealCurrentDataArgs : EventArgs
    {
        public RealCurrentData RealCurrentData { get; set; }

        public RealCurrentDataArgs(RealCurrentData realCurrentData)
        {
            this.RealCurrentData = realCurrentData;
        }
    }
}
