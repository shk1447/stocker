using Common.Base;
using Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface.Model
{
    public class ChartPriceModel : BaseViewModel
    {
        private string[] timeTypes;
        public string[] TimeTypes
        {
            get
            {
                return timeTypes ?? (timeTypes = Enum.GetNames(typeof(StockTimePrice.TimeType)));
            }
        }

        private string timeType;
        public string TimeType
        {
            get
            {
                return timeType;
            }
            set
            {
                timeType = value;
                OnPropertyChanged("TimeType");
            }
        }

        private string range;
        public string Range
        {
            get
            {
                return range;
            }
            set
            {
                range = value;
                OnPropertyChanged("Range");
            }
        }

        private string startDate;
        public string StartDate
        {
            get
            {
                return startDate;
            }
            set
            {
                startDate = value;
                OnPropertyChanged("StartDate");
            }
        }

        private string endDate;
        public string EndDate
        {
            get
            {
                return endDate;
            }
            set
            {
                endDate = value;
                OnPropertyChanged("EndDate");
            }
        }
    }
}
