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

namespace ScalKing.Windows
{
    /// <summary>
    /// SimpleInputWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SimpleInputWindow : Window
    {
        public SimpleInputWindow()
        {
            InitializeComponent();

            Loaded += SimpleInputWindow_Loaded;
        }

        void SimpleInputWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.searchTextBox.KeyDown += searchTextBox_KeyDown;
            this.searchTextBox.Focus();
        }

        void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                addButton_Click(sender, null);
            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string GetText()
        {
            return this.searchTextBox.Text;
        }
    }
}
