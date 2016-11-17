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

namespace DataLayer.WCF
{
    /// <summary>
    /// UCLog.xaml 的交互逻辑
    /// </summary>
    public partial class UCLog : UserControl
    {
        public UCLog()
        {
            InitializeComponent();
        }
        WcfDataService.LogInfoType loginfo;
        List<string> types = new List<string>();
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            loginfo = DataProvider.wcf.ReadLogInfomation();
            txtcount.Text = loginfo.count.ToString();
            txtstart.Text = loginfo.startDate;
            txtend.Text = loginfo.endDate;

            lstCinfig.ItemsSource = loginfo.configs;

            dateStart.DateTime =DateTime.Parse(loginfo.startDate);
            dateEnd.DateTime = DateTime.Parse(loginfo.endDate);
            types.Add("全部类型");
            foreach (var item in loginfo.types)
                types.Add(item);
            cmbTypes.ItemsSource = types;
            cmbTypes.SelectedIndex = 0;
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataProvider.wcf.ReadLogs(dateStart.DateTime, dateEnd.DateTime, cmbTypes.SelectedIndex == 0 ? "" : cmbTypes.SelectedItem.ToString());
            lstlog.ItemsSource = dt.AsDataView();
        }


    }
}
