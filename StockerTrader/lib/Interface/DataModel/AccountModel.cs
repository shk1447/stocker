using Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface.Model
{
    public class AccountModel : BaseViewModel
    {
        public override string _id
        {
            get
            {
                return this.StCode;
            }
        }

        public string StCode { get; set; }
        public long LastUnixTime { get; set; }

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

        private double gainLoss;
        /// <summary>
        /// 평가손익
        /// </summary>
        public double GainLoss
        {
            get
            {
                return gainLoss;
            }
            set
            {
                gainLoss = value;
                OnPropertyChanged("GainLoss");
            }
        }

        private int evalPrice;
        /// <summary>
        /// 평가금액
        /// </summary>
        public int EvalPrice
        {
            get
            {
                return evalPrice;
            }
            set
            {
                evalPrice = value;
                OnPropertyChanged("EvalPrice");
            }
        }

        private double gainLossRate;
        /// <summary>
        /// 수익률(%)
        /// </summary>
        public double GainLossRate
        {
            get
            {
                return gainLossRate;
            }
            set
            {
                gainLossRate = value;
                OnPropertyChanged("GainLossRate");
            }
        }

        private int eveClosingPrice;
        /// <summary>
        /// 전일종가
        /// </summary>
        public int EveClosingPrice
        {
            get
            {
                return eveClosingPrice;
            }
            set
            {
                eveClosingPrice = value;
                OnPropertyChanged("EveClosingPrice");
            }
        }

        private int haveStockCount;
        /// <summary>
        /// 보유수량
        /// </summary>
        public int HaveStockCount
        {
            get
            {
                return haveStockCount;
            }
            set
            {
                haveStockCount = value;
                OnPropertyChanged("HaveStockCount");
            }
        }

        private int currentPrice;
        /// <summary>
        /// 현재가
        /// </summary>
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

        private double purchasePrice;
        /// <summary>
        /// 매입가
        /// </summary>
        public double PurchasePrice
        {
            get
            {
                return purchasePrice;
            }
            set
            {
                purchasePrice = value;
                OnPropertyChanged("PurchasePrice");
            }
        }

        private int sumFees;
        /// <summary>
        /// 수수료합
        /// </summary>
        public int SumFees
        {
            get
            {
                return sumFees;
            }
            set
            {
                sumFees = value;
                OnPropertyChanged("SumFees");
            }
        }

        private int purchaseAmount;
        /// <summary>
        /// 매입금액
        /// </summary>
        public int PurchaseAmount
        {
            get
            {
                return purchaseAmount;
            }
            set
            {
                purchaseAmount = value;
                OnPropertyChanged("PurchaseAmount");
            }
        }

        private int purchaseFees;
        /// <summary>
        /// 매입수수료
        /// </summary>
        public int PurchaseFees
        {
            get
            {
                return purchaseFees;
            }
            set
            {
                purchaseFees = value;
                OnPropertyChanged("PurchaseFees");
            }
        }
    }

    public class AccountModelArgs : EventArgs
    {
        public AccountModel AccountModel { get; set; }

        public AccountModelArgs(AccountModel accountModel)
        {
            this.AccountModel = accountModel;
        }
    }
}
