using Interface;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Interface.Model;

namespace ScalKing.Windows
{
    /// <summary>
    /// SendOrderWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SendOrderWindow : MetroWindow
    {
        public OrderModel Model { get; set; }
        public string[] OrderTypes { get; set; }
        public string[] HogaCodes { get; set; }

        public string OrderType { get; set; }
        public string HogaCode { get; set; }

        private SendOrderWindow()
        {
            InitializeComponent();
        }

        public SendOrderWindow(OrderModel orderModel)
            : this()
        {
            this.Model = orderModel;

            this.OrderTypes = Enum.GetNames(typeof(OrderType));
            this.HogaCodes = Enum.GetNames(typeof(HogaCode));

            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
