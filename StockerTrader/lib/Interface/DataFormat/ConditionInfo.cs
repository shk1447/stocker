using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    public class ConditionInfo
    {
        public List<Condition> ConditionList { get; set; }
    }

    public class KiwoomCondition : Condition
    {
        public string _id { get; set; }
        public bool IsAutoRegist { get; set; }
    }

    public class Condition
    {
        public int ConditionIndex { get; set; }
        public string ConditionName { get; set; }
        public ConditionOriginType Type { get; set; }
        public string StCodes { get; set; }
    }

    public enum ConditionOriginType
    {
        Kiwum, Custom
    }

    public class ConditionInfoArgs : EventArgs
    {
        public List<Condition> ConditionList { get; set; }

        public ConditionInfoArgs(List<Condition> conditionList)
        {
            this.ConditionList = conditionList;
        }
    }

    public class ConditionStCodeArgs : EventArgs
    {
        public string StCodes { get; set; }
        public int ConditionIndex { get; set; }

        public ConditionStCodeArgs(string stCodes, int conditionIndex)
        {
            this.StCodes = stCodes;
            this.ConditionIndex = conditionIndex;
        }
    }

    public class ConditionRealStCode
    {
        public string StCode { get; set; }
        /// <summary>
        /// 종목 편입여부, False일 시 종목 이탈
        /// </summary>
        public bool IsAdded { get; set; }

        public int ConditionIndex { get; set; }
    }

    public class ConditionRealStCodeArgs : EventArgs
    {
        public ConditionRealStCode ConditionRealData { get; set; }

        public ConditionRealStCodeArgs(ConditionRealStCode conditionRealData)
        {
            this.ConditionRealData = conditionRealData;
        }
    }
}
