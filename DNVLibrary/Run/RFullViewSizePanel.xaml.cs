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
using System.Windows.Controls.Primitives;
using DevExpress.Xpf.Grid;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DNVLibrary.Run
{
    /// <summary>
    /// RFullViewSizePanel.xaml 的交互逻辑
    /// </summary>
    /// 


    public partial class RFullViewSizePanel : UserControl
    {
        public RFullViewSizePanel()
        {
            InitializeComponent();
            LineStyle ls = new LineStyle();
            ls.DashStyle = DashStyles.Dot; 
            ls.Thickness = 2;
            BDZ110_FOR_VOL.LineStyle = ls;
            BDZ220_FOR_VOL.LineStyle = ls;
            BDZ500_FOR_VOL.LineStyle = ls;
            BYQ2_FOR_VOL.LineStyle = ls;
            BYQ3_FOR_VOL.LineStyle = ls;
            LineNew_FOR_VOL.LineStyle = ls;
            LineOld_FOR_VOL.LineStyle = ls;
            HWG_FOR_VOL.LineStyle = ls;
            InitData();
            //LoadTreeListData();
            LoadTreeViewData();
        }

        private void LoadTreeViewData()
        {
            List<NodeEntry> m_NodeEntrys;
            List<NodeEntry> m_outputList;
            m_NodeEntrys = new List<NodeEntry>()   
             {   
                new NodeEntry { ID = 1, Name = "变电站" },   
                new NodeEntry{ID=2,Name="开关"},
                new NodeEntry{ID=3,Name="变压器"},
                new NodeEntry{ID=4,Name="线路"},

                new NodeEntry{ID=11,Name="110KV变电站",ParentID=1},
                new NodeEntry{ID=12,Name="220KV变电站",ParentID=1},
                new NodeEntry{ID=13,Name="500KV变电站",ParentID=1},

                new NodeEntry{ID=111,Name="变电站座数：",Value=lstBDZ110_NUM[9].Value.ToString(),Unit="座",ParentID=11},
                new NodeEntry{ID=112,Name="变电站容量：",Value=lstBDZ110_VOL[9].Value.ToString(),Unit="KW.H",ParentID=11},

                new NodeEntry{ID=121,Name="变电站座数：",Value=lstBDZ220_NUM[9].Value.ToString(),Unit="座",ParentID=12},
                new NodeEntry{ID=122,Name="变电站容量：",Value=lstBDZ220_VOL[9].Value.ToString(),Unit="KW.H",ParentID=12},

                new NodeEntry{ID=131,Name="变电站座数：",Value=lstBDZ500_NUM[9].Value.ToString(),Unit="座",ParentID=13},
                new NodeEntry{ID=132,Name="变电站容量：",Value=lstBDZ500_VOL[9].Value.ToString(),Unit="KW.H",ParentID=13},

                new NodeEntry{ID=21,Name="分段开关个数：",Value=lstKGD_NUM[9].Value.ToString(),Unit="个",ParentID=2},
                new NodeEntry{ID=22,Name="联络开关个数：",Value=lstKGL_NUM[9].Value.ToString(),Unit="个",ParentID=2},
                new NodeEntry{ID=23,Name="环网柜个数：",Value=lstHWG_VOL[9].Value.ToString(),Unit="个",ParentID=2},

                new NodeEntry{ID=31,Name="两卷变：",ParentID=3},
                new NodeEntry{ID=32,Name="三卷变：",ParentID=3},

                new NodeEntry{ID=311,Name="两卷变座数：",Value=lstBYQ2_NUM[9].Value.ToString(),Unit="座",ParentID=31},
                new NodeEntry{ID=312,Name="两卷变容量：",Value=lstBYQ2_VOL[9].Value.ToString(),Unit="KW.H",ParentID=31},

                new NodeEntry{ID=321,Name="三卷变座数：",Value=lstBYQ3_NUM[9].Value.ToString(),Unit="座",ParentID=32},
                new NodeEntry{ID=322,Name="三卷变容量：",Value=lstBYQ3_VOL[9].Value.ToString(),Unit="KW.H",ParentID=32},

                new NodeEntry{ID=41,Name="已有线路条数：",Value=lstLineOld_NUM[9].Value.ToString(),Unit="条",ParentID=4},
                new NodeEntry{ID=42,Name="已有线路长度：",Value=lstLineOld_VOL[9].Value.ToString(),Unit="千米",ParentID=4},
             };
            m_outputList = TreeViewDataBind.Bind(m_NodeEntrys);
            this.treeView1.ItemsSource = m_outputList;
        }

        private void LoadTreeListData()
        {
            
            List<ListData> items = new List<ListData>();
            items.Add(new ListData(100, 0, "变电站规模"));
            items.Add(new ListData(100, 1, "开关规模"));
            items.Add(new ListData(100, 2, "变压器规模"));
            items.Add(new ListData(100, 3, "线路规模"));

            items.Add(new ListData(0, 4, "110KV变电站"));
            items.Add(new ListData(0, 5, "220KV变电站"));
            items.Add(new ListData(0, 6, "500KV变电站"));
            items.Add(new ListData(4, 7, "变电站座数:"+lstBDZ110_NUM[9].Value));
            items.Add(new ListData(4, 8, "变电站容量:"+lstBDZ110_VOL[9].Value));
            items.Add(new ListData(5, 9, "变电站座数:"+lstBDZ220_NUM[9].Value));
            items.Add(new ListData(5, 10, "变电站容量:"+lstBDZ220_VOL[9].Value));
            items.Add(new ListData(6, 11, "变电站座数:"+lstBDZ500_NUM[9].Value));
            items.Add(new ListData(6, 12, "变电站容量:"+lstBDZ500_VOL[9].Value));
            items.Add(new ListData(1, 13, "分段开关个数:"+lstKGD_NUM[9].Value));
            items.Add(new ListData(1, 14, "联络开关个数:"+lstKGL_NUM[9].Value));
            items.Add(new ListData(1, 15, "环网柜个数:"+lstHWG_VOL[9].Value));
            items.Add(new ListData(2, 16, "两卷变规模"));
            items.Add(new ListData(2, 17, "三卷变规模"));
            items.Add(new ListData(16, 18, "两卷变座数:"+lstBYQ2_NUM[9].Value));
            items.Add(new ListData(16, 19, "两卷变容量:"+lstBYQ2_VOL[9].Value));
            items.Add(new ListData(17, 20, "三卷变座数:"+lstBYQ3_NUM[9].Value));
            items.Add(new ListData(17, 21, "三卷变容量:"+lstBYQ3_VOL[9].Value));
            items.Add(new ListData(3, 22, "已有线路条数:"+lstLineOld_NUM[9].Value));
            items.Add(new ListData(3, 23, "已有线路长度:"+lstLineOld_VOL[9].Value));
            //treeListCtrl.ItemsSource = items;
        }
        List<SeriesPoint> lstBDZ110_VOL;
        List<SeriesPoint> lstBDZ220_VOL;
        List<SeriesPoint> lstBDZ500_VOL;
        List<SeriesPoint> lstBDZ110_NUM;
        List<SeriesPoint> lstBDZ220_NUM;
        List<SeriesPoint> lstBDZ500_NUM;
        List<SeriesPoint> lstKGD_NUM;
        List<SeriesPoint> lstKGL_NUM;
        List<SeriesPoint> lstHWG_VOL;
        List<SeriesPoint> lstBYQ2_VOL;
        List<SeriesPoint> lstBYQ3_VOL;
        List<SeriesPoint> lstBYQ2_NUM;
        List<SeriesPoint> lstBYQ3_NUM;
        List<SeriesPoint> lstLineOld_VOL;
        List<SeriesPoint> lstLineOld_NUM;
        private void InitData()
        {
            //模拟数据
            //110KV变电站容量（历史数据）
            lstBDZ110_VOL = RFullViewSizeDataGenerator.SimuData(1000, 1000, 2006, 10);
            //220KV变电站容量（历史数据）
            lstBDZ220_VOL = RFullViewSizeDataGenerator.SimuData(1200, 1000, 2006, 10);
            //500KV变电站容量（历史数据）
            lstBDZ500_VOL = RFullViewSizeDataGenerator.SimuData(1500, 1000, 2006, 10);
            //110KV变电站座数（历史数据）
            lstBDZ110_NUM = RFullViewSizeDataGenerator.SimuData(500, 500, 2006, 10);
            //220KV变电站座数（历史数据）
            lstBDZ220_NUM = RFullViewSizeDataGenerator.SimuData(400, 400, 2006, 10);
            //500KV变电站座数（历史数据）
            lstBDZ500_NUM = RFullViewSizeDataGenerator.SimuData(300, 300, 2006, 10);

            //110KV变电站容量（预测数据）
            List<SeriesPoint> lstBDZ110_FOR_VOL = RFullViewSizeDataGenerator.SimuData(1000, 1000, 2016, 5); lstBDZ110_FOR_VOL.Add(lstBDZ110_VOL[9]);
            //220KV变电站容量（预测数据）
            List<SeriesPoint> lstBDZ220_FOR_VOL = RFullViewSizeDataGenerator.SimuData(1200, 1000, 2016, 5); lstBDZ220_FOR_VOL.Add(lstBDZ220_VOL[9]);
            //500KV变电站容量（预测数据）
            List<SeriesPoint> lstBDZ500_FOR_VOL = RFullViewSizeDataGenerator.SimuData(1500, 1000, 2016, 5); lstBDZ500_FOR_VOL.Add(lstBDZ500_VOL[9]);

            //110KV变电站座数（预测数据）
            List<SeriesPoint> lstBDZ110_FOR_NUM = RFullViewSizeDataGenerator.SimuData(500, 500, 2016, 5);
            //220KV变电站座数（预测数据）
            List<SeriesPoint> lstBDZ220_FOR_NUM = RFullViewSizeDataGenerator.SimuData(400, 400, 2016, 5);
            //500KV变电站座数（预测数据）
            List<SeriesPoint> lstBDZ500_FOR_NUM = RFullViewSizeDataGenerator.SimuData(300, 300, 2016, 5);

            //分段开关个数（历史数据）
            lstKGD_NUM = RFullViewSizeDataGenerator.SimuData(2000, 1000, 2006, 10);
            //联络开关个数（历史数据）
            lstKGL_NUM = RFullViewSizeDataGenerator.SimuData(2000, 1000, 2006, 10);
            //环网柜个数（历史数据）
            lstHWG_VOL = RFullViewSizeDataGenerator.SimuData(3000, 2000, 2006, 10);

            //分段开关个数（预测数据）
            List<SeriesPoint> lstKGD_FOR_NUM = RFullViewSizeDataGenerator.SimuData(2000, 1000, 2016, 5);
            //联络开关个数（预测数据）
            List<SeriesPoint> lstKGL_FOR_NUM = RFullViewSizeDataGenerator.SimuData(2000, 1000, 2016, 5);
            //环网柜个数（预测）
            List<SeriesPoint> lstHWG_FOR_VOL = RFullViewSizeDataGenerator.SimuData(3000, 2000, 2016, 5); lstHWG_FOR_VOL.Add(lstHWG_VOL[9]);

            //两卷变容量（历史数据）
            lstBYQ2_VOL = RFullViewSizeDataGenerator.SimuData(2000, 1000, 2006, 10);
            //三卷变容量（历史数据）
            lstBYQ3_VOL = RFullViewSizeDataGenerator.SimuData(3000, 2000, 2006, 10);

            //两卷变座数（历史数据）
            lstBYQ2_NUM = RFullViewSizeDataGenerator.SimuData(1000, 1000, 2006, 10);
            //三卷变座数（历史数据）
            lstBYQ3_NUM = RFullViewSizeDataGenerator.SimuData(1500, 1000, 2006, 10);

            //两卷变容量（预测数据）
            List<SeriesPoint> lstBYQ2_FOR_VOL = RFullViewSizeDataGenerator.SimuData(2000, 1000, 2016, 5); lstBYQ2_FOR_VOL.Add(lstBYQ2_VOL[9]);
            //三卷变容量（预测数据）
            List<SeriesPoint> lstBYQ3_FOR_VOL = RFullViewSizeDataGenerator.SimuData(3000, 2000, 2016, 5); lstBYQ3_FOR_VOL.Add(lstBYQ3_VOL[9]);

            //两卷变座数（预测数据）
            List<SeriesPoint> lstBYQ2_FOR_NUM = RFullViewSizeDataGenerator.SimuData(1000, 1000, 2016, 5);
            //三卷变座数（预测数据）
            List<SeriesPoint> lstBYQ3_FOR_NUM = RFullViewSizeDataGenerator.SimuData(1500, 1000, 2016, 5);

            //

            //新建线路长度（历史数据）
            List<SeriesPoint> lstLineNew_VOL = RFullViewSizeDataGenerator.SimuData(2000, 2000, 2006, 10);
            //已有线路长度（历史数据）
            lstLineOld_VOL = RFullViewSizeDataGenerator.SimuData(4000, 3000, 2006, 10);

            //新建线路条数（历史数据）
            List<SeriesPoint> lstLineNew_NUM = RFullViewSizeDataGenerator.SimuData(1000, 1000, 2006, 10);
            //已有线路条数（历史数据）
            lstLineOld_NUM = RFullViewSizeDataGenerator.SimuData(3000, 3000, 2006, 10);

            //新建线路长度（预测数据）
            List<SeriesPoint> lstLineNew_FOR_VOL = RFullViewSizeDataGenerator.SimuData(2000, 2000, 2016, 5); lstLineNew_FOR_VOL.Add(lstLineNew_VOL[9]);
            //已有线路长度（预测数据）
            List<SeriesPoint> lstLineOld_FOR_VOL = RFullViewSizeDataGenerator.SimuData(4000, 3000, 2016, 5); lstLineOld_FOR_VOL.Add(lstLineOld_VOL[9]);

            //新建线路条数（预测数据）
            List<SeriesPoint> lstLineNew_FOR_NUM = RFullViewSizeDataGenerator.SimuData(1000, 1000, 2016, 5);
            //已有线路条数（预测数据）
            List<SeriesPoint> lstLineOld_FOR_NUM = RFullViewSizeDataGenerator.SimuData(3000, 3000, 2016, 5);


            //加载变电站数据

            foreach (SeriesPoint sp in lstBDZ110_VOL)
            {
                BDZ110_VOL.Points.Add(sp);
            }

            foreach (SeriesPoint sp in lstBDZ220_VOL)
            {
                BDZ220_VOL.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstBDZ500_VOL)
            {
                BDZ500_VOL.Points.Add(sp);
            }

            foreach (SeriesPoint sp in lstBDZ110_NUM)
            {
                BDZ110_NUM.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstBDZ220_NUM)
            {
                BDZ220_NUM.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstBDZ500_NUM)
            {
                BDZ500_NUM.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstBDZ110_FOR_VOL)
            {
                BDZ110_FOR_VOL.Points.Add(sp);
            }

            foreach (SeriesPoint sp in lstBDZ220_FOR_VOL)
            {
                BDZ220_FOR_VOL.Points.Add(sp);
            }

            foreach (SeriesPoint sp in lstBDZ500_FOR_VOL)
            {
                BDZ500_FOR_VOL.Points.Add(sp);
            }

            foreach (SeriesPoint sp in lstBDZ110_FOR_NUM)
            {
                BDZ110_FOR_NUM.Points.Add(sp);

            }
            foreach (SeriesPoint sp in lstBDZ220_FOR_NUM)
            {
                BDZ220_FOR_NUM.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstBDZ500_FOR_NUM)
            {
                BDZ500_FOR_NUM.Points.Add(sp);
            }

            //加载开关数据
            foreach (SeriesPoint sp in lstKGD_NUM)
            {
                KGD_NUM.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstKGL_NUM)
            {
                KGL_NUM.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstKGD_FOR_NUM)
            {
                KGD_FOR_NUM.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstKGL_FOR_NUM)
            {
                KGL_FOR_NUM.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstHWG_VOL)
            {
                HWG_VOL.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstHWG_FOR_VOL)
            {
                HWG_FOR_VOL.Points.Add(sp);
            }
            //加载变压器数据

            foreach (SeriesPoint sp in lstBYQ2_VOL)
            {
                BYQ2_VOL.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstBYQ3_VOL)
            {
                BYQ3_VOL.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstBYQ2_NUM)
            {
                BYQ2_NUM.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstBYQ3_NUM)
            {
                BYQ3_NUM.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstBYQ2_FOR_VOL)
            {
                BYQ2_FOR_VOL.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstBYQ3_FOR_VOL)
            {
                BYQ3_FOR_VOL.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstBYQ2_FOR_NUM)
            {
                BYQ2_FOR_NUM.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstBYQ3_FOR_NUM)
            {
                BYQ3_FOR_NUM.Points.Add(sp);
            }
            //加载线路数据

            foreach (SeriesPoint sp in lstLineNew_VOL)
            {
                LineNew_VOL.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstLineOld_VOL)
            {
                LineOld_VOL.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstLineNew_NUM)
            {
                LineNew_NUM.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstLineOld_NUM)
            {
                LineOld_NUM.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstLineNew_FOR_VOL)
            {
                LineNew_FOR_VOL.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstLineOld_FOR_VOL)
            {
                LineOld_FOR_VOL.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstLineNew_FOR_NUM)
            {
                LineNew_FOR_NUM.Points.Add(sp);
            }
            foreach (SeriesPoint sp in lstLineOld_FOR_NUM)
            {
                LineOld_FOR_NUM.Points.Add(sp);
            }
        }
        //private Popup pop = new Popup();
        private void Chart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (pop != null && pop.Child != null)
            {
                pop.Child = null;
                pop.IsOpen = false;
                //tmp = null;
            }
            ChartControl chart = sender as ChartControl;
            
            if (chart.Name != "KGChart")
            {
                System.Windows.Point mouseposition = e.GetPosition(mainFrame);
                ChartHitInfo hitInfo = chart.CalcHitInfo(e.GetPosition(chart));
                if (!hitInfo.InSeries || hitInfo.InLegend) return;
                //Popup pop = new Popup();
                pop.IsOpen = true;
                //pop.AllowsTransparency = true;

                pop.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
                pop.PlacementTarget = mainFrame;
                pop.HorizontalOffset = mouseposition.X +  1;
                pop.VerticalOffset = mouseposition.Y ;

                
                pop.Height = 350;
                pop.Width = 700;
                pop.Child = new RFullViewSizeSubPanel(hitInfo);
                pop.Effect = new System.Windows.Media.Effects.DropShadowEffect();
                //tmp = pop;


                //mainFrame.Children.Add(new RFullViewSizeSubPanel(hitInfo));
            }
        }

        private void scale_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            pop.IsOpen = (bool)e.NewValue;
            if (!pop.IsOpen)
                pop.Child = null;
        }

    }

    public class ListData
    {
        private int id;
        private int pid;
        private string txt;
        public ListData(int ppid, int iid, string txtt) { id = iid; pid = ppid; txt = txtt; }

        public string Txt
        {
            get { return txt; }
        }
        public int Pid
        {
            get { return pid; }
        }
        public int Id
        {
            get { return id; }
        }
    }

    class TreeViewLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TreeViewItem item = (TreeViewItem)value;
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);
            return ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
}
