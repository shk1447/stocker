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
using Interface;

namespace ScalKing.Controls
{
    class MainControlViewModel : BaseViewModel
    {
        #region | Properties |
        public TradeManagerControlViewModel TradeManagerControlViewModel { get; set; }
        #endregion

        #region | Private |
        private KHControlViewModel kHControlViewModel;
        #endregion

        #region | Command |        
        #endregion

        #region | Ctor |
        public MainControlViewModel(KHControlViewModel kHControlViewModel)
        {
            this.kHControlViewModel = kHControlViewModel;
            this.kHControlViewModel.ReceiveRealCurrentData += kHControlViewModel_ReceiveRealCurrentData;
            this.kHControlViewModel.ReceiveConditionRealStCode += kHControlViewModel_ReceiveConditionRealStCode;

            Initialize();
        }
        #endregion

        #region | Events |

        void kHControlViewModel_ReceiveRealCurrentData(object sender, RealCurrentDataArgs e)
        {
            //var findItem = this.RegisteredItems.FirstOrDefault(x => x.StCode.Equals(e.RealCurrentData.StCode));
            //if (findItem != null)
            //{
            //    findItem.CurrentPrice = e.RealCurrentData.CurrentPrice;
            //    findItem.CumulativeVolume = e.RealCurrentData.거래량;
            //    findItem.ChangeRate = e.RealCurrentData.등락율;
            //    findItem.ChangePrice = e.RealCurrentData.전일대비;
            //}
        }

        void kHControlViewModel_ReceiveConditionRealStCode(object sender, ConditionRealStCodeArgs e)
        {
            if (!e.ConditionRealData.IsAdded)
            {
                return;
            }
        }
        #endregion

        #region | Method |
        private void Initialize()
        {
            this.TradeManagerControlViewModel = new TradeManagerControlViewModel(this.kHControlViewModel, this);

            InitKiwoomRegist();
        }

        private void InitKiwoomRegist()
        {
            this.kHControlViewModel.GetCondition();
        }
        
        public override void RemoveItem<T>(T info)
        {
            var item = info as RegistStockInfo;
            //this.RegisteredItems.Remove(item);
        }

        public override void RemoveItem<T>(IEnumerable<T> info)
        {
            foreach (var item in info)
            {
                RemoveItem(item);
            }
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
                this.TradeManagerControlViewModel.Dispose();

                disposed = true;
            }
        }
        #endregion
    }
}
