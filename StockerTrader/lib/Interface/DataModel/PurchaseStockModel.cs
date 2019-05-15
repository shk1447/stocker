using Common.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface.Model
{
    public class PurchaseScheduleModel : BaseViewModel
    {
        public override string _id { get; set; }

        private string conditionName;
        /// <summary>
        /// 조건식 이름
        /// </summary>
        public string ConditionName
        {
            get
            {
                return conditionName;
            }
            set
            {
                conditionName = value;
                OnPropertyChanged("ConditionName");
            }
        }

        private int conditionIndex;
        /// <summary>
        /// 조건식 인덱스
        /// </summary>
        public int ConditionIndex
        {
            get
            {
                return conditionIndex;
            }
            set
            {
                conditionIndex = value;
                OnPropertyChanged("ConditionIndex");
            }
        }

        private DateTime createDateTime;
        public DateTime CreateDateTime
        {
            get
            {
                return createDateTime;
            }
            set
            {
                createDateTime = value;
                OnPropertyChanged("CreateDateTime");
            }
        }

        private int purchaseRuleId;
        public int PurchaseRuleId
        {
            get
            {
                return purchaseRuleId;
            }
            set
            {
                purchaseRuleId = value;
                OnPropertyChanged("PurchaseRuleId");
            }
        }

        private int sellingRuleId;
        public int SellingRuleId
        {
            get
            {
                return sellingRuleId;
            }
            set
            {
                sellingRuleId = value;
                OnPropertyChanged("SellingRuleId");
            }
        }

        private ObservableCollection<ScheduleTime> scheduleTimeList;
        public ObservableCollection<ScheduleTime> ScheduleTimeList
        {
            get { return scheduleTimeList; }
            set
            {
                scheduleTimeList = value;
                OnPropertyChanged("ScheduleTimeList");
            }
        }
    }

    public class ScheduleTime
    {
        public string DayOfWeek { get; set; }
        public TimeSpan BeginTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Interval { get; set; }

        public int RunLastDay { get; set; }
        public int RunLastSecound { get; set; }
    }
}
