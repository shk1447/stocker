using AxKHOpenAPILib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScalKing.Controls
{
    /// <summary>
    /// TestControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class KHControl : UserControl
    {
        public KHControl()
        {
            InitializeComponent();
            
            Loaded += KHControl_Loaded;
        }

        void KHControl_Loaded(object sender, RoutedEventArgs e)
        {
            //WindowsFormsHost host = new WindowsFormsHost();
            //host.Visibility = Visibility.Collapsed;

            //AxKHOpenAPI khAPI = new AxKHOpenAPI();
            //host.Child = khAPI;

            KHControlViewModel viewModel = this.DataContext as KHControlViewModel;
            viewModel?.InitializeAPI(khAPI);
        }
    }
}
