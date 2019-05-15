using Common.Base;
using Interface.DataFormat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    public class ConditionModel : BaseViewModel
    {
        private bool isUse;
        public bool IsUse
        {
            get
            {
                return this.isUse;
            }
            set
            {
                this.isUse = value;
                OnPropertyChanged("IsUse");
            }
        }

        public ObservableCollection<ConditionItemModel> PurchaseRuleConditionList { get; set; }
        public ObservableCollection<ConditionItemModel> SellingRuleConditionList { get; set; }

        private ConditionModel()
        {
            if (this.PurchaseRuleConditionList == null)
            {
                this.PurchaseRuleConditionList = new ObservableCollection<ConditionItemModel>();
            }
            if (this.SellingRuleConditionList == null)
            {
                this.SellingRuleConditionList = new ObservableCollection<ConditionItemModel>();
            }
        }

        public ConditionModel(string id)
            : this()
        {
            this._id = id;
        }
    }

    public class ConditionItemModel : BaseViewModel
    {
        public ObservableCollection<CustomCondition> ConditionList { get; set; }

        private string value;
        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }
        //분할 거래에 대한 정보 추가!!

        private ConditionItemModel()
        {
            if (this.ConditionList == null)
            {
                this.ConditionList = new ObservableCollection<CustomCondition>();
            }
        }

        public ConditionItemModel(string id)
            : this()
        {
            this._id = id;
        }
    }

    public class TradeRule
    {
        public bool IsUse { get; set; }
        /// <summary>
        /// 우선순위
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 룰 컨디션
        /// </summary>
        public ObservableCollection<CustomCondition> RuleConditionList { get; set; }
        /// <summary>
        /// 거래 금액 타입
        /// </summary>
        public TradeType TradeType { get; set; }
        /// <summary>
        /// 금액 Or 비율
        /// </summary>
        public int TradeValue { get; set; }
        /// <summary>
        /// 분할 거래
        /// </summary>
        public ObservableCollection<DivisionTrade> DivisionTradeList { get; set; }
        /// <summary>
        /// 주문 타입
        /// </summary>
        public OrderType OrderType { get; set; }
        /// <summary>
        /// 호가 코드
        /// </summary>
        public HogaCode HogaCode { get; set; }
    }

    public class CustomCondition : BaseViewModel
    {
        public ObservableCollection<string> ConditionList { get; set; }

        private string byString;
        public string ByString
        {
            get
            {
                return byString;
            }

            set
            {
                byString = value;
                OnPropertyChanged("ByString");
            }
        }

        public CustomCondition()
        {
            if (this.ConditionList == null)
            {
                this.ConditionList = new ObservableCollection<string>();
            }

            this.ConditionList.CollectionChanged += CustomCondition_CollectionChanged;
        }

        private void CustomCondition_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.ByString = string.Join(" ", this.ConditionList);
        }
    }

    public enum RuleConditionType
    {
        /// <summary>
        /// 손익률
        /// </summary>
        손익률,
        /// <summary>
        /// 골든크로스
        /// </summary>
        골든크로스,
        /// <summary>
        /// 데드 크로스
        /// </summary>
        데드크로스,
        테스트하드
    }

    public enum TradeType
    {
        /// <summary>
        /// 퍼센트 율
        /// </summary>
        Percent,
        /// <summary>
        /// 금액 Only
        /// </summary>
        Price
    }

    public class DivisionTrade
    {
        /// <summary>
        /// 금액비율 (max 100)
        /// </summary>
        public int PricePercent { get; set; }
        /// <summary>
        /// 거래비율
        /// </summary>
        public int TradePercent { get; set; }
    }
}
