using Common.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Interface.DataFormat
{
    public class StockInfo : Base
    {
        private string stCode;
        public string StCode
        {
            get
            {
                return this.stCode;
            }
            set
            {
                this.stCode = value;
                this._id = value;
            }
        }

        public string StName { get; set; }
        public bool IsKospi { get; set; }
    }
        
    public class RegistStockInfo : BaseViewModel
    {
        public enum RegistType
        {
            Direct, Kiwoom, Rest
        }

        public DateTime Time { get; set; }
        public string Type { get; set; }

        public int CurrentPrice
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

        public double ChangeRate
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

        public long CumulativeVolume
        {
            get
            {
                return cumulativeVolume;
            }

            set
            {
                cumulativeVolume = value;
                OnPropertyChanged("cumulativeVolume");
            }
        }

        public int ChangePrice
        {
            get
            {
                return changePrice;
            }

            set
            {
                changePrice = value;
                OnPropertyChanged("ChangePrice");
            }
        }

        private int currentPrice;
        private int changePrice;
        private double changeRate;
        private long cumulativeVolume;

        private string stCode;
        public string StCode
        {
            get
            {
                return this.stCode;
            }
            set
            {
                this.stCode = value;
                this._id = value;
            }
        }

        public string StName { get; set; }
        public bool IsKospi { get; set; }

        public RegistStockInfo()
        {
        }

        public RegistStockInfo(StockInfo info, RegistType type)
        {
            this.StCode = info.StCode;
            this.StName = info.StName;
            this.IsKospi = info.IsKospi;
            this.Time = DateTime.Now;
            this.Type = type.ToString();
        }
    }
}
