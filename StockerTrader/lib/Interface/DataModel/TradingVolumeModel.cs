using Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface.Model
{
    public class TradingVolumeModel : BaseViewModel
    {
        public override string _id { get; set; }
        public string StCode { get; set; }

        private string stName;
        public string StName
        {
            get
            {
                return stName;
            }
            set
            {
                stName = value;
                OnPropertyChanged("StName");
            }
        }

        private string changeRate;
        /// <summary>
        /// 등락률
        /// </summary>
        public string ChangeRate
        {
            get
            {
                return changeRate;
            }
            set
            {
                changeRate = value;
                OnPropertyChanged("ChangeRate");
            }
        }

        private string currentPrice;
        /// <summary>
        /// 현재가
        /// </summary>
        public string CurrentPrice
        {
            get
            {
                return currentPrice;
            }
            set
            {
                currentPrice = value;
                OnPropertyChanged("CurrentPrice");
            }
        }

        private string beforeDayValue;
        /// <summary>
        /// 전일대비
        /// </summary>
        public string BeforeDayValue
        {
            get
            {
                return beforeDayValue;
            }
            set
            {
                beforeDayValue = value;
                OnPropertyChanged("BeforeDayValue");
            }
        }

        private string beforeTradeVolumn;
        /// <summary>
        /// 이전거래량
        /// </summary>
        public string BeforeTradeVolumn
        {
            get
            {
                return beforeTradeVolumn;
            }
            set
            {
                beforeTradeVolumn = value;
                OnPropertyChanged("BeforeTradeVolumn");
            }
        }

        private string currentTradeVolumn;
        /// <summary>
        /// 현재거래량
        /// </summary>
        public string CurrentTradeVolumn
        {
            get
            {
                return currentTradeVolumn;
            }
            set
            {
                currentTradeVolumn = value;
                OnPropertyChanged("CurrentTradeVolumn");
            }
        }

        private string surgeAmount;
        /// <summary>
        /// 급증량
        /// </summary>
        public string SurgeAmount
        {
            get
            {
                return surgeAmount;
            }
            set
            {
                surgeAmount = value;
                OnPropertyChanged("SurgeAmount");
            }
        }

        private string surgeRate;
        /// <summary>
        /// 급증률
        /// </summary>
        public string SurgeRate
        {
            get
            {
                return surgeRate;
            }
            set
            {
                surgeRate = value;
                OnPropertyChanged("SurgeRate");
            }
        }
    }
}
