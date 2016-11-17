using System;
using System.Data;
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
    /// PEvaluteKPIMain.xaml 的交互逻辑
    /// </summary>
    public partial class PEvaluteKPIMain : UserControl
    {
        public PEvaluteKPIMain()
        {
            InitializeComponent();
        }


    

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            initdata();
        }
    void initdata()
        {
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select case t1.INDEXNAME when '综合线损率' then '电能利用率' else t1.INDEXNAME end argu,case t1.INDEXNAME when '综合线损率' then 1-AVG(t2.value)/100 else AVG(t2.value)/100 end value from D_INDEX t1 join D_INDEX t2 on t1.ID=t2.KPI where t1.SORT0='863kpi' group by t1.INDEXNAME");
            cht.DataSource = dt;
        }

    }
}
