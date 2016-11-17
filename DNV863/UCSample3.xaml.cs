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

namespace DNV863
{
    /// <summary>
    /// UCSample3.xaml 的交互逻辑
    /// </summary>
    public partial class UCSample3 : UserControl
    {
        public UCSample3()
        {
            InitializeComponent();

            grdMain.Children.Add(new UCSample1());
            grd2.Children.Add(new UCSample2());

        }
    }
}
