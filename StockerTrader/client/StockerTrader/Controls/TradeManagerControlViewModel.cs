using Common.Base;
using Interface.DataFormat;
using Interface.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScalKing.Windows;

namespace ScalKing.Controls
{
    //이벤트만 받아서 처리하면 될듯
    class TradeManagerControlViewModel : BaseViewModel
    {
        #region | Properties |
        public ObservableCollection<TradeControlViewModel> TradeControlItems { get; set; }
        public AccountModel TotalAccountModel { get; set; }

        #endregion

        #region | Private |
        private KHControlViewModel kHControlViewModel;
        private MainControlViewModel mainControlViewModel;
        private Dictionary<string, AccountModel> allAcountModel;
        #endregion

        #region | Command |
        private MyCommand addTradeControl;
        public MyCommand AddTradeControl
        {
            get
            {
                return addTradeControl ?? (addTradeControl = new MyCommand(para =>
                {
                    SimpleInputWindow window = new SimpleInputWindow();
                    if (window.ShowDialog() == true)
                    {
                        var stCode = window.GetText();

                        var existsItem = this.TradeControlItems.FirstOrDefault(x => x.AccountModel.StCode.Equals(stCode));
                        if (existsItem != null)
                        {
                            return;
                        }

                        var tradeControl = new TradeControlViewModel();
                        tradeControl.InitMatherControls(this.kHControlViewModel, this,new AccountModel() { StCode = stCode });

                        this.TradeControlItems.Add(tradeControl);
                    }
                }));
            }
        }
        #endregion

        #region | Ctor |
        public TradeManagerControlViewModel(KHControlViewModel kHControlViewModel, MainControlViewModel mainControlViewModel)
        {
            this.kHControlViewModel = kHControlViewModel;
            this.mainControlViewModel = mainControlViewModel;

            Initialize();
        }
        #endregion

        #region | Events |
        void TradeControlItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        var item = e.NewItems[0] as TradeControlViewModel;
                        if (item == null)
                        {
                            return;
                        }

                        this.allAcountModel[item.AccountModel.StCode] = new AccountModel();
                        //개별 추가 시 계좌 정보 조회
                        this.kHControlViewModel.GetAccountInfo();
                        //Communicator.RedisClient.Instance.Upsert<TradeControlViewModel>(item);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        var item = e.OldItems[0] as TradeControlViewModel;
                        if (item == null)
                        {
                            return;
                        }

                        if (this.allAcountModel.ContainsKey(item.AccountModel.StCode))
                        {
                            var accountModel = this.allAcountModel[item.AccountModel.StCode];
                            this.TotalAccountModel.EvalPrice -= accountModel.EvalPrice;
                            this.TotalAccountModel.PurchaseAmount -= accountModel.PurchaseAmount;
                            this.TotalAccountModel.GainLoss -= accountModel.GainLoss;
                            //수익율 재 계산
                            this.TotalAccountModel.GainLossRate = Math.Round((this.TotalAccountModel.GainLoss / this.TotalAccountModel.PurchaseAmount) * 100, 2);
                            this.allAcountModel.Remove(item.AccountModel.StCode);
                        }

                        //Communicator.RedisClient.Instance.Remove<TradeControlViewModel>(item._id);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new Exception("와우~");
                default:
                    break;
            }
        }

        private void TradeManagerControlViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsRunning":
                    //SGO2에 Running 상태 정보 전송
                    //Schedule Running 과 Stop 으로 볼수있겠음
                    break;
            }
        }

        /// <summary>
        /// 자식들의 Property Change 될시 총합 내용의 변경 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TradeControlAccountItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AccountModel accountModel = sender as AccountModel;

            if (this.allAcountModel.ContainsKey(accountModel.StCode))
            {
                var searchAccountModel = this.allAcountModel[accountModel.StCode];

                switch (e.PropertyName)
                {
                    case "GainLoss":
                        this.TotalAccountModel.GainLoss += accountModel.GainLoss - searchAccountModel.GainLoss;
                        searchAccountModel.GainLoss = accountModel.GainLoss;

                        //수익율 재 계산
                        this.TotalAccountModel.GainLossRate = Math.Round((this.TotalAccountModel.GainLoss / this.TotalAccountModel.PurchaseAmount) * 100, 2);
                        break;
                    case "PurchaseAmount":
                        this.TotalAccountModel.PurchaseAmount += accountModel.PurchaseAmount - searchAccountModel.PurchaseAmount;
                        searchAccountModel.PurchaseAmount = accountModel.PurchaseAmount;
                        break;
                    case "EvalPrice":
                        this.TotalAccountModel.EvalPrice += accountModel.EvalPrice - searchAccountModel.EvalPrice;
                        searchAccountModel.EvalPrice = accountModel.EvalPrice;
                        break;
                    default:
                        break;
                }
            }
        }

        private void KHControlViewModel_ReceiveAccountInfo(object sender, AccountModelArgs e)
        {
            var existsItem = this.TradeControlItems.FirstOrDefault(x => x.AccountModel.StCode.Equals(e.AccountModel.StCode));
            if (existsItem != null)
            {
                return;
            }

            //var findItem = Communicator.RedisClient.Instance.Get<StockInfo>(e.AccountModel.StCode);
            //if (findItem == null)
            //{
            //    return;
            //}

            var tradeControl = new TradeControlViewModel();
            tradeControl.InitMatherControls(this.kHControlViewModel, this, e.AccountModel);

            this.TradeControlItems.Add(tradeControl);
        }
        #endregion

        #region | Method |
        public void Initialize()
        {
            this.allAcountModel = new Dictionary<string, AccountModel>();
            this.TotalAccountModel = new AccountModel();
            this.TradeControlItems = new ObservableCollection<TradeControlViewModel>();            

            this.TradeControlItems.CollectionChanged += TradeControlItems_CollectionChanged;
            this.kHControlViewModel.ReceiveAccountInfo += KHControlViewModel_ReceiveAccountInfo;
            this.PropertyChanged += TradeManagerControlViewModel_PropertyChanged;
        }

        public KHControlViewModel GetKHControlViewModel()
        {
            return this.kHControlViewModel;
        }

        private void LoadHaveStockItem()
        {

        }

        public override void RemoveItem<T>(T info)
        {
            var item = info as TradeControlViewModel;
            this.TradeControlItems.Remove(item);
        }
        #endregion

        #region | Dispose |
        public new void Dispose()
        {
            this.Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                foreach (var item in this.TradeControlItems.ToList())
                {
                    item.Dispose();
                }

                disposed = true;
            }
        }
        #endregion
    }
}
