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
using System.Windows.Shapes;

namespace IDXKPI
{
    /// <summary>
    /// WinHelp.xaml 的交互逻辑
    /// </summary>
    public partial class WinHelp : Window
    {
        public WinHelp()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            string tmp= AppDomain.CurrentDomain.BaseDirectory+"help.htm";

           //Source="F:\当前项目文件夹\程序\配网可视化863\DNV863\DNV863\bin\Debug\Help.htm"
            web.Source =new Uri(tmp);//("F:\\当前项目文件夹\\程序\\配网可视化863\\DNV863\\DNV863\\bin\\Debug\\Help.htm");

           // web.Navigate(uri);
        }
    }
}
