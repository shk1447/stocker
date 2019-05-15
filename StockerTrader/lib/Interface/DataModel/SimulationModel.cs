using Common.Base;
using Interface.DataFormat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface.Model
{
    public class SimulationModel : BaseViewModel
    {
        public override string _id { get; set; }

        private DateTime startDate;
        public DateTime StartDate
        {
            get { return startDate; }
            set
            {
                startDate = value;
                OnPropertyChanged("StartDate");
            }
        }

        private string serverIp;
        public string ServerIp
        {
            get { return serverIp; }
            set
            {
                serverIp = value;
                OnPropertyChanged("ServerIp");
            }
        }

        private string stockItems;
        public string StockItems
        {
            get { return stockItems; }
            set
            {
                stockItems = value;
                OnPropertyChanged("StockItems");
            }
        }

        private string logMessage;
        public string LogMessage
        {
            get { return logMessage; }
            set
            {
                logMessage = value;
                OnPropertyChanged("LogMessage");
            }
        }

        private string resultMessage;
        public string ResultMessage
        {
            get { return resultMessage; }
            set
            {
                resultMessage = value;
                OnPropertyChanged("ResultMessage");
            }
        }

        private string conditionIds;
        public string ConditionIds
        {
            get { return conditionIds; }
            set
            {
                conditionIds = value;
                OnPropertyChanged("ConditionIds");
            }
        }

        private string ruleIds;
           public string RuleIds
        {
            get { return ruleIds; }
            set
            {
                ruleIds = value;
                OnPropertyChanged("RuleIds");
            }
        }

        public ObservableCollection<StockInfo> RegisteredItems { get; set; }
    }
}
