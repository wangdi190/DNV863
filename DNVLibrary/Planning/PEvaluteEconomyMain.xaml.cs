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

namespace DNVLibrary.Planning
{
    /// <summary>
    /// PEvaluteEconomyMain.xaml 的交互逻辑
    /// </summary>
    public partial class PEvaluteEconomyMain : UserControl
    {
        public PEvaluteEconomyMain()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            grid.ItemsSource = DataLayer.DataProvider.getDataTableFromSQL("select t2,t3,t4,v1 from map_data2 where sort='经济评价指标' and t1='2014年二批次' order by cast(t2 as float)");
        }
    }
}
