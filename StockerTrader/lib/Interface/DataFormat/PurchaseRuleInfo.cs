using Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface.DataFormat
{
    public class PurchaseRuleInfo : RuleModel
    {
        /// <summary>
        /// 구매액 ( 1% = 보유 금액의 1% )
        /// </summary>
        public string Price { get; set; }
    }

    public class RuleModel : BaseViewModel
    {
        public enum TimeType
        {
            RealTime, StartTime, EndTime
        }

        public enum RuleType
        {
            Range, Signal
        }

        private string[] timeTypes;
        public string[] TimeTypes
        {
            get
            {
                return timeTypes ?? (timeTypes = Enum.GetNames(typeof(TimeType)));
            }
        }

        private string[] ruleTypes;
        public string[] RuleTypes
        {
            get
            {
                return ruleTypes ?? (ruleTypes = Enum.GetNames(typeof(RuleType)));
            }
        }

        private int sequence { get; set; }
        /// <summary>
        /// 순서
        /// </summary>
        public int Sequence
        {
            get
            {
                return this.sequence;
            }
            set
            {
                this.sequence = value;
                OnPropertyChanged("Sequence");
            }
        }

        public TimeType timeTypeInfo;
        /// <summary>
        /// 시간 기준
        /// </summary>
        public TimeType TimeTypeInfo
        {
            get
            {
                return this.timeTypeInfo;
            }
            set
            {
                this.TimeTypeString = value.ToString();
                this.timeTypeInfo = value;
            }
        }

        public string TimeTypeString { get; set; }

        private RuleType ruleTypeInfo;
        /// <summary>
        /// Rule 기준
        /// </summary>
        public RuleType RuleTypeInfo
        {
            get
            {
                return this.ruleTypeInfo;
            }
            set
            {
                this.RuleTypeString = value.ToString();
                this.ruleTypeInfo = value;
            }
        }

        public string RuleTypeString { get; set; }
        /// <summary>
        /// Rule 값
        /// </summary>
        public string RuleValue { get; set; }

        public RuleModel()
        {
            this.RuleTypeInfo = RuleType.Range;
            this.TimeTypeInfo = TimeType.RealTime;
        }
    }
}
