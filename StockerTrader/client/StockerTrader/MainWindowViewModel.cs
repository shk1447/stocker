using Common.Base;
using ScalKing.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScalKing
{
    class MainWindowViewModel : BaseViewModel
    {
        #region | Properties |
        public KHControlViewModel KHControlViewModel { get; set; }
        public MainControlViewModel MainControlViewModel { get; set; }
        #endregion

        #region | Private |
        #endregion

        #region | Command |
        #endregion

        #region | Events |
        #endregion

        #region | Ctor |
        public MainWindowViewModel()
        {
            Initialize();
        }
        #endregion

        #region | Methods |
        public void Initialize()
        {
            this.KHControlViewModel = new KHControlViewModel();
            this.MainControlViewModel = new MainControlViewModel(this.KHControlViewModel);
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
                this.KHControlViewModel.Dispose();
                this.MainControlViewModel.Dispose();

                disposed = true;
            }
        }
        #endregion
    }
}
