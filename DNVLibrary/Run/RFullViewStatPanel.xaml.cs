using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DNVLibrary.Run
{
    /// <summary>
    /// RFullViewStatPanel.xaml 的交互逻辑
    /// </summary>
    public partial class RFullViewStatPanel : UserControl
    {
        public RFullViewStatPanel()
        {
            InitializeComponent();
        }

        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            lst.Margin = new Thickness(20, RFullViewStatData.topSpan, 0, 0);
            lst.ItemsSource = RFullViewStatData.datas.Values.OrderBy(p=>p.ord);
        }
    }
}
