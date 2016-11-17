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
using DevExpress.Xpf.Charts;
namespace DNVLibrary.Run
{
    /// <summary>
    /// RFullViewProduceCount.xaml 的交互逻辑
    /// </summary>
    public partial class RFullViewProduceCount : UserControl
    {
        public RFullViewProduceCount()
        {
            InitializeComponent();

            InitData();

            InitTreeViewData();
        }

        private void InitTreeViewData()
        {
            List<NodeEntry> m_NodeEntrys;
            List<NodeEntry> m_outputList;
            m_NodeEntrys = new List<NodeEntry>()   
             {   
                new NodeEntry{ID=0,Name="截止昨日本年度生产数据统计"},

                new NodeEntry{ID=1,Name="供电数据",ParentID=0},
                new NodeEntry{ID=2,Name="停电数据",ParentID=0},
                new NodeEntry{ID=3,Name="客户用电数据",ParentID=0},
                new NodeEntry{ID = 4, Name = "清洁能源",ParentID=0},   
                new NodeEntry{ID=5,Name="其它",ParentID=0},

                new NodeEntry{ID=41,Name="供电量：",Value="22892",Unit="KW.H",ParentID=1},
                new NodeEntry{ID=42,Name="平均负荷：",Value="1682",Unit="KW",ParentID=1},
                new NodeEntry{ID=43,Name="最大负荷：",Value="2282",Unit="KW",ParentID=1},
                new NodeEntry{ID=44,Name="最小负荷：",Value="892",Unit="KW",ParentID=1},
                new NodeEntry{ID=45,Name="平均负荷率：",Value="68%",Unit="",ParentID=1},

                new NodeEntry{ID=21,Name="停电时间：",Value="228",Unit="H",ParentID=2},
                new NodeEntry{ID=22,Name="停电电量：",Value="22802",Unit="KW.H",ParentID=2},

                new NodeEntry{ID=31,Name="大客户用电：",Value="22823",Unit="KW.H",ParentID=3},
                new NodeEntry{ID=32,Name="充电桩用电：",Value="12823",Unit="KW.H",ParentID=3},
                new NodeEntry{ID=33,Name="储能装置充放电：",Value="2823",Unit="KW.H",ParentID=3},
               
                new NodeEntry{ID=12,Name="风力发电：",Value="23099",Unit="KW.H",ParentID=4},
                new NodeEntry{ID=13,Name="光伏发电：",Value="32899",Unit="KW.H",ParentID=4},

                new NodeEntry{ID=51,Name="平均馈线电压标幺值：",Value="1.02",Unit="",ParentID=5},
             };
            m_outputList = TreeViewDataBind.Bind(m_NodeEntrys);
            this.treeView.ItemsSource = m_outputList;
        }

        public void InitData()
        {
            //模拟风力发电
            foreach (SeriesPoint sp in RFullViewProduceDataGenerator.SimulateData(1000,800,2015))
            {
                bar_Wind.Points.Add(sp);
                line_Wind.Points.Add(sp);
            }
            //模拟太阳能发电
            foreach (SeriesPoint sp in RFullViewProduceDataGenerator.SimulateData(1000, 800, 2015))
            {
                bar_Sun.Points.Add(sp);
                line_Sun.Points.Add(sp);
            }
            //模拟水力发电
            foreach (SeriesPoint sp in RFullViewProduceDataGenerator.SimulateData(1000, 800, 2015))
            {
                bar_Water.Points.Add(sp);
                line_Water.Points.Add(sp);
            }
            //模拟供电
            foreach (SeriesPoint sp in RFullViewProduceDataGenerator.SimulateData(1000,1000,2015))
            {
                elec_vol.Points.Add(sp);
            }
            //模拟高峰负荷
            foreach (SeriesPoint sp in RFullViewProduceDataGenerator.SimulateData(1500,500,2015))
            {
                fh_Low.Points.Add(sp);
            }
            //模拟低谷负荷
            foreach(SeriesPoint sp in RFullViewProduceDataGenerator.SimulateData(1500,500,2015))
            {
                fh_High.Points.Add(sp);
            }
            //模拟停电时间
            foreach (SeriesPoint sp in  RFullViewProduceDataGenerator.SimulateData(300,100,2015))
            {
                td_time.Points.Add(sp);
            }
            //模拟停电量
            foreach (SeriesPoint sp in RFullViewProduceDataGenerator.SimulateData(2000,1000,2015))
            {
                td_Vol.Points.Add(sp);
            }
            //模拟大客户用电
            foreach(SeriesPoint sp in RFullViewProduceDataGenerator.SimulateData(2000,1000,2015))
            {
                customer_usg.Points.Add(sp);
            }
            //模拟充电桩用电
            foreach (SeriesPoint sp in RFullViewProduceDataGenerator.SimulateData(2000, 1000, 2015))
            {
                cdz_usg.Points.Add(sp);
            }
            //模拟储能冲发电
            foreach (SeriesPoint sp in RFullViewProduceDataGenerator.SimulateData(2000, 1000, 2015))
            {
                cn_usg.Points.Add(sp);
            }
        }
    }
}
