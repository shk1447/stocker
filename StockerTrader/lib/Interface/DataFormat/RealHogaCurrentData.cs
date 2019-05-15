﻿using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    public class RealHogaDataOnMongo : RealHogaData
    {
        public ObjectId _id;
    }

    public class RealHogaData
    {
        public string StCode { get; set; }
        public DateTime DateTime { get; set; }
        public List<int> 매도호가 { get; set; }
        public List<int> 매도호가수량 { get; set; }
        public List<int> 매수호가 { get; set; }
        public List<int> 매수호가수량 { get; set; }
        public int 매도호가총잔량 { get; set; }
        public int 매수호가총잔량 { get; set; }
        //[21] = 호가시간
        //[41] = 매도호가1
        //[61] = 매도호가수량1
        //[81] = 매도호가직전대비1
        //[51] = 매수호가1
        //[71] = 매수호가수량1
        //[91] = 매수호가직전대비1
        //[42] = 매도호가2
        //[62] = 매도호가수량2
        //[82] = 매도호가직전대비2
        //[52] = 매수호가2
        //[72] = 매수호가수량2
        //[92] = 매수호가직전대비2
        //[43] = 매도호가3
        //[63] = 매도호가수량3
        //[83] = 매도호가직전대비3
        //[53] = 매수호가3
        //[73] = 매수호가수량3
        //[93] = 매수호가직전대비3
        //[44] = 매도호가4
        //[64] = 매도호가수량4
        //[84] = 매도호가직전대비4
        //[54] = 매수호가4
        //[74] = 매수호가수량4
        //[94] = 매수호가직전대비4
        //[45] = 매도호가5
        //[65] = 매도호가수량5
        //[85] = 매도호가직전대비5
        //[55] = 매수호가5
        //[75] = 매수호가수량5
        //[95] = 매수호가직전대비5
        //[46] = 매도호가6
        //[66] = 매도호가수량6
        //[86] = 매도호가직전대비6
        //[56] = 매수호가6
        //[76] = 매수호가수량6
        //[96] = 매수호가직전대비6
        //[47] = 매도호가7
        //[67] = 매도호가수량7
        //[87] = 매도호가직전대비7
        //[57] = 매수호가7
        //[77] = 매수호가수량7
        //[97] = 매수호가직전대비7
        //[48] = 매도호가8
        //[68] = 매도호가수량8
        //[88] = 매도호가직전대비8
        //[58] = 매수호가8
        //[78] = 매수호가수량8
        //[98] = 매수호가직전대비8
        //[49] = 매도호가9
        //[69] = 매도호가수량9
        //[89] = 매도호가직전대비9
        //[59] = 매수호가9
        //[79] = 매수호가수량9
        //[99] = 매수호가직전대비9
        //[50] = 매도호가10
        //[70] = 매도호가수량10
        //[90] = 매도호가직전대비10
        //[60] = 매수호가10
        //[80] = 매수호가수량10
        //[100] = 매수호가직전대비10
        //[121] = 매도호가총잔량
        //[122] = 매도호가총잔량직전대비
        //[125] = 매수호가총잔량
        //[126] = 매수호가총잔량직전대비
        //[23] = 예상체결가
        //[24] = 예상체결수량
        //[128] = 순매수잔량
        //[129] = 매수비율
        //[138] = 순매도잔량
        //[139] = 매도비율
        //[200] = 예상체결가전일종가대비
        //[201] = 예상체결가전일종가대비등락율
        //[238] = 예상체결가전일종가대비기호
        //[291] = 예상체결가
        //[292] = 예상체결량
        //[293] = 예상체결가전일대비기호
        //[294] = 예상체결가전일대비
        //[295] = 예상체결가전일대비등락율
        //[621] = LP매도호가수량1
        //[631] = LP매수호가수량1
        //[622] = LP매도호가수량2
        //[632] = LP매수호가수량2
        //[623] = LP매도호가수량3
        //[633] = LP매수호가수량3
        //[624] = LP매도호가수량4
        //[634] = LP매수호가수량4
        //[625] = LP매도호가수량5
        //[635] = LP매수호가수량5
        //[626] = LP매도호가수량6
        //[636] = LP매수호가수량6
        //[627] = LP매도호가수량7
        //[637] = LP매수호가수량7
        //[628] = LP매도호가수량8
        //[638] = LP매수호가수량8
        //[629] = LP매도호가수량9
        //[639] = LP매수호가수량9
        //[630] = LP매도호가수량10
        //[640] = LP매수호가수량10
        //[13] = 누적거래량
        //[299] = 전일거래량대비예상체결률
        //[215] = 장운영구분
        //[216] = 투자자별ticker
    }

    public class RealHogaDataArgs : EventArgs
    {
        public RealHogaData RealHogaData { get; set; }

        public RealHogaDataArgs(RealHogaData realHogaData)
        {
            this.RealHogaData = realHogaData;
        }
    }
}
