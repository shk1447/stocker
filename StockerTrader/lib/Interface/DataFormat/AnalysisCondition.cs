using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    public class AnalysisCondition
    {
        public static string[] Operatories = { "+", "-", "*", "/", "(", ")" };
        public static string[] Comparatories = { "<", "<=", ">", ">=", "=" };

        public enum CountType
        {
            틱, 분, 일
        }

        public static string[] CountTypes = Enum.GetNames(typeof(AnalysisCondition.CountType));

        public enum ConditionType
        {
            주가_현재, 주가_평, 주가_합, 주가_변동율, 주가_봉율, 주가_MIN, 주가_MAX, 시가, 종가, 고가, 저가, 주가_지수평, PIVOT,
            거래량_현재, 거래량_평, 거래량_합, 거래량_OSC, 거래량_MIN, 거래량_MAX, 저항, 지지, 저항수, 지지수, 종목추출매수, 종목추출매도,
            단타매수테스트, 단타매도테스트,
            볼린저밴드, RSI, 스토캐스틱, MACD, MACD_OSC, 이격도, TRIX
        }

        public static string[] ConditionTypeNames = Enum.GetNames(typeof(AnalysisCondition.ConditionType));

        public enum SellConditionType
        {
            분,
            초,
            수익률,
            /// <summary>
            /// 매수한 시기로 부터 매도할때까지의 기간 중 최 고점에서의 퍼센트율 ,  50%라고 설정시 100원이 -> 200까지 갔으면 150보다 작아질 시 판매함
            /// </summary>
            손익절
        }

        public static string[] SellConditionTypeNames = Enum.GetNames(typeof(AnalysisCondition.SellConditionType));

        //public enum ConditionValueType
        //{
        //    Condition, Operator
        //}
    }

    //public class AnalysisConditionValue
    //{
    //    public AnalysisCondition.ConditionValueType ValueType { get; set; }
    //    public object Value { get; set; }

    //    public AnalysisConditionValue(AnalysisCondition.ConditionValueType  valueType, object value)
    //    {
    //        this.ValueType = valueType;
    //        this.Value = value;
    //    }
    //}
}
