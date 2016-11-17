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

namespace DNVLibrary.Interact
{
    /// <summary>
    /// ITimesPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ITimesPanel : UserControl
    {
        public ITimesPanel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        UCDNV863 root;

        private void UserControl_Initialized(object sender, EventArgs e)
        {
        }

        #region ========== 模式控制 ==========
        bool isTimelineMode = true;
        System.Windows.Media.Animation.DoubleAnimation ani1 = new System.Windows.Media.Animation.DoubleAnimation() { Duration = TimeSpan.FromSeconds(0.5), From = 1, To = 0 };
        System.Windows.Media.Animation.DoubleAnimation ani2 = new System.Windows.Media.Animation.DoubleAnimation() { Duration = TimeSpan.FromSeconds(0.5), From = 0, To = 1 };
        private void btnFlip_Click(object sender, RoutedEventArgs e)
        {
            ani1.Completed += new EventHandler(ani1_Completed);
            panel1.IsHitTestVisible = panel2.IsHitTestVisible = false;
            if (isTimelineMode)
            {
                panel1.BeginAnimation(Border.OpacityProperty, ani1);
                panel2.BeginAnimation(Border.OpacityProperty, ani2);
            }
            else
            {
                panel1.BeginAnimation(Border.OpacityProperty, ani2);
                panel2.BeginAnimation(Border.OpacityProperty, ani1);
            }



            isTimelineMode = !isTimelineMode;
            if (isTimelineMode)
            {
                cht.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                cht.Visibility = System.Windows.Visibility.Collapsed;
                tmr.Stop();
                btnPause.Content = " 继续 ";
            }
        }

        void ani1_Completed(object sender, EventArgs e)
        {
            if (isTimelineMode)
                panel1.IsHitTestVisible = true;
            else
                panel2.IsHitTestVisible = true;
        }
        #endregion

        #region ===== 读取统计数据，各对象数据单独读取=====
        SceneDataItem alltimeindex = new SceneDataItem(null);  //全年指标
        SceneDatas scenedata = new SceneDatas();  //场景数据集
        TimeDatas timedata = new TimeDatas(); //时间轴数据集
        ///<summary>读取所有数据</summary>
        internal void readalldata()
        {
            readAllIndex();
            readSceneData();
            readTimeData();

            initTimeline();

            grdMain.DataContext = alltimeindex;  //设置仪表为全年指标

        }

        ///<summary>读取全年统计指标</summary>
        void readAllIndex()
        {
            //全年统计指标
            string sql = "";
            string sim = "select top 1 iv1e1 as id, fmin0max1 as idx1, fmin0.9max1.1 as idx2, fmin3000max4000 as idx3, fmin0max0.4 as idx4, fmin0max1 as idx5, fmin0max1 as idx6, imin0max400 as hours";
            DataTable dt = DataLayer.DataProvider.getDataTable(sql, sim, DataLayer.EReadMode.模拟).Value;

            alltimeindex.idx1.value = dt.Rows[0].getDouble("idx1");
            alltimeindex.idx2.value = dt.Rows[0].getDouble("idx2");
            alltimeindex.idx3.value = dt.Rows[0].getDouble("idx3");
            alltimeindex.idx4.value = dt.Rows[0].getDouble("idx4");
            alltimeindex.idx5.value = dt.Rows[0].getDouble("idx5");
            alltimeindex.idx6.value = dt.Rows[0].getDouble("idx6");

        }

        ///<summary>读取场景统计数据</summary>
        void readSceneData()
        {
            //40场景
           string sql = "";
           string sim = "select top 40 iv1e1 as id, fmin0max1 as idx1, fmin0.9max1.1 as idx2, fmin0max4000 as idx3, fmin0max0.4 as idx4, fmin0max1 as idx5, fmin0max1 as idx6, imin0max400 as hours";
           DataTable dt = DataLayer.DataProvider.getDataTable(sql, sim, DataLayer.EReadMode.模拟).Value;

            foreach (DataRow dr in dt.Rows)
            {
                SceneDataItem tmp = new SceneDataItem(scenedata);
                tmp.num = dr.getInt("id");
                tmp.hours = dr.getInt("hours");
                tmp.idx1.value = dr.getDouble("idx1");
                tmp.idx2.value = dr.getDouble("idx2");
                tmp.idx3.value = dr.getDouble("idx3");
                tmp.idx4.value = dr.getDouble("idx4");
                tmp.idx5.value = dr.getDouble("idx5");
                tmp.idx6.value = dr.getDouble("idx6");

                scenedata.items.Add(tmp);

            }
            scenedata.init();
            foreach (var item in scenedata.items)
            {
                scenepanel.Children.Add(item.button);
                item.button.MouseDown += new MouseButtonEventHandler(button_MouseDown);
            }


        }

        ///<summary>读取时间统计数据</summary>
        void readTimeData()
        {
            //全年8760小时
            string sql = "";
            string sim = "select top 8760 dhv2001.01.01e1 as time, fmin0max1 as idx1, fmin0.9max1.1 as idx2, fmin0max4000 as idx3, fmin0max0.4 as idx4, fmin0max1 as idx5, fmin0max1 as idx6, imin3000max4000 as load, imin0max300 as green, imin0max400 as auto";
            DataTable dt = DataLayer.DataProvider.getDataTable(sql, sim, DataLayer.EReadMode.模拟).Value;

            foreach (DataRow dr in dt.Rows)
            {
                TimeDataItem tmp = new TimeDataItem(timedata);
                tmp.time = dr.getDatetime("time");
                tmp.idx1.value = dr.getDouble("idx1");
                tmp.idx2.value = dr.getDouble("idx2");
                tmp.idx3.value = dr.getDouble("idx3");
                tmp.idx4.value = dr.getDouble("idx4");
                tmp.idx5.value = dr.getDouble("idx5");
                tmp.idx6.value = dr.getDouble("idx6");
                tmp.load = dr.getDouble("load");
                tmp.greenpower = dr.getDouble("green");
                tmp.autoload = dr.getDouble("auto");
                timedata.items.Add(tmp);
            }
        }
        #endregion
        
        
        #region ========== 场景部分 ==========

        int curSceneNum = 0;
        ///<summary>选择场景</summary>
        void button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SceneButton btn = sender as SceneButton;
            if (curSceneNum > 0)
            {
                scenedata.items[curSceneNum - 1].button.Background = Brushes.Black;
                scenedata.items[curSceneNum - 1].button.txtNum.Foreground = new SolidColorBrush(Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF));
            }

            btn.Background = Brushes.AliceBlue;
            btn.txtNum.Foreground = new SolidColorBrush(Color.FromArgb(0x7F, 0x99, 0x32, 0xCC));

            curSceneNum = btn.data.num;
            refreshData();

            e.Handled = true;
        }
        ///<summary>场景排序</summary>
        private void btnord1_Click(object sender, RoutedEventArgs e)
        {
            scenepanel.Children.Clear();
            foreach (var item in scenedata.items.OrderByDescending(p=>p.hours))
                scenepanel.Children.Add(item.button);
        }

        private void btnord2_Click(object sender, RoutedEventArgs e)
        {
            scenepanel.Children.Clear();
            foreach (var item in scenedata.items.OrderByDescending(p => p.idx3.value))
                scenepanel.Children.Add(item.button);

        }

        private void btnord3_Click(object sender, RoutedEventArgs e)
        {
            scenepanel.Children.Clear();
            foreach (var item in scenedata.items.OrderByDescending(p => p.idx4.value))
                scenepanel.Children.Add(item.button);

        }

        #endregion

        #region ========= 时间轴部分 ==========
        ///<summary>基础时间片分钟数</summary>
        public int baseMinuteSpan = 60 * 24;

        DateTime curTime; //当前时间
        TimeSpan timespan; //每5秒间隔时间
        double progress; //进度值
        TimeSpan alltimes;  //全部时间
        System.Windows.Threading.DispatcherTimer tmr;
        List<MyClassLibrary.DevShare.ChartDataPoint> alldata = new List<MyClassLibrary.DevShare.ChartDataPoint>();

        private DateTime _startDate;
        public DateTime startDate
        {
            get { return _startDate; }
            set { _startDate = value; validTime(); }
        }

        private DateTime _endDate;
        public DateTime endDate
        {
            get { return _endDate; }
            set { _endDate = value; validTime(); }
        }

        ///<summary>初始化时间线相关</summary>
        void initTimeline()
        {
            tmr = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromSeconds(5) };
            tmr.Tick += new EventHandler(tmr_Tick);
            trc.EditValueChanged += new DevExpress.Xpf.Editors.EditValueChangedEventHandler(trc_EditValueChanged);

            timespan = TimeSpan.FromMinutes(baseMinuteSpan * 4);

            startDate = timedata.starttime; // new DateTime(2016, 1, 1);
            endDate = timedata.endtime;// new DateTime(2017, 1, 1);
            initchart();
        }


        //校验时间
        void validTime()
        {
            curTime = startDate;
            progress = 0;

            controlPanel.IsEnabled = endDate > startDate;
            alltimes = endDate - startDate;

            alldata.Clear();
            cht.DataSource = null;

            trc.Value = 0;
            tmr.Stop();

            refreshtime();
        }
        ///<summary>刷新时间控制板</summary>
        void refreshtime()
        {
            txtDate.Text = curTime.ToString("yyyy.MM.dd HH:mm");
            txtRad.Text = string.Format("{0:f0}m", timespan.TotalMinutes);
        }

        void initchart()
        {
            alldata.Clear();
            //DateTime tmpdate = startDate;
            //while (tmpdate < endDate)
            //{
            //    alldata.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate = tmpdate, value = rd.Next(200), sort = "供电负荷" });
            //    alldata.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate = tmpdate, value = rd.Next(30), sort = "清洁能源出力" });
            //    alldata.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate = tmpdate, value = rd.Next(20), sort = "充电桩负荷" });
            //    ////alldata.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate = tmpdate, value = rd.Next(60), sort = "大客户负荷" });
            //    ////alldata.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate = tmpdate, value = rd.Next(20), sort = "停电损失负荷" });
            //    tmpdate = tmpdate.AddMinutes(baseMinuteSpan);
            //}

            var tmp = timedata.items.Where(p => p.time.Hour == 12); //仅取每日12点绘制
            foreach (var item in tmp)
            {
                alldata.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate = item.time, value = item.load, sort = "供电负荷" });
                alldata.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate = item.time, value = item.greenpower, sort = "清洁能源出力" });
                alldata.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate =item.time, value = item.autoload, sort = "充电桩负荷" });
            }


            cht.DataSource = null;
            cht.DataSource = alldata;
        }
        ///<summary>清除数据并刷新界面</summary>
        internal void clearData()
        {

            //zh注
            root.distnet.clearFlow();
            root.distnet.clearLoadCol();
            root.distnet.clearVLContour();

        }



        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            //zh注
            root.distnet.showFlow();
            root.distnet.showLoadCol();
            root.distnet.showVLContour();

            //initchart();

            curTime = startDate;
            progress = 0;
            refreshData();
            tmr.Start();
            btnPause.Content = " 暂停 ";
        }


        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (tmr.IsEnabled)
            {
                tmr.Stop();
                btnPause.Content = " 继续 ";
            }
            else
            {
                tmr.Start();
                btnPause.Content = " 暂停 ";
            }
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            timespan += TimeSpan.FromMinutes(baseMinuteSpan);
            btnReduce.IsEnabled = timespan > TimeSpan.FromMinutes(baseMinuteSpan);
            btnPlus.IsEnabled = timespan < TimeSpan.FromMinutes(baseMinuteSpan * 30);
            refreshtime();
        }

        private void btnReduce_Click(object sender, RoutedEventArgs e)
        {
            timespan -= TimeSpan.FromMinutes(baseMinuteSpan);
            btnReduce.IsEnabled = timespan > TimeSpan.FromMinutes(baseMinuteSpan);
            btnPlus.IsEnabled = timespan < TimeSpan.FromMinutes(baseMinuteSpan * 30);
            refreshtime();
        }

        private void trc_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            progress = trc.Value;
            int count = (int)(progress * alltimes.TotalMinutes / baseMinuteSpan);
            curTime = startDate + TimeSpan.FromMinutes(count * baseMinuteSpan);
            chtlin.Value = curTime;
            refreshtime();
            refreshData();

        }

        void tmr_Tick(object sender, EventArgs e)
        {
            curTime = startDate + TimeSpan.FromMinutes(progress * alltimes.TotalMinutes) + timespan;
            chtlin.Value = curTime;
            trc.EditValueChanged -= new DevExpress.Xpf.Editors.EditValueChangedEventHandler(trc_EditValueChanged);
            progress = (curTime - startDate).TotalMinutes / alltimes.TotalMinutes;
            trc.Value = progress;
            trc.EditValueChanged += new DevExpress.Xpf.Editors.EditValueChangedEventHandler(trc_EditValueChanged);
            refreshtime();
            refreshData();
        }

        private void diagram_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(cht);
            DevExpress.Xpf.Charts.DiagramCoordinates diagramCoordinates = diagram.PointToDiagram(position);
            if (!diagramCoordinates.IsEmpty)
            {
                curTime = diagramCoordinates.DateTimeArgument;
                chtlin.Value = curTime;
                trc.EditValueChanged -= new DevExpress.Xpf.Editors.EditValueChangedEventHandler(trc_EditValueChanged);
                progress = (curTime - startDate).TotalMinutes / alltimes.TotalMinutes;
                trc.Value = progress;
                trc.EditValueChanged += new DevExpress.Xpf.Editors.EditValueChangedEventHandler(trc_EditValueChanged);
                refreshtime();
                refreshData();
            }
        }

        #endregion

        ///<summary>读取数据并刷新界面</summary>
        void refreshData()
        {
            //读取指定场景或指定时间数据
            if (isTimelineMode)
            {
                grdMain.DataContext = timedata.items.First(p=>p.time==curTime);


            }
            else
            {
                grdMain.DataContext = scenedata.items[curSceneNum - 1];

            }


            //zh注
            root.distnet.showFlow();
            root.distnet.showLoadCol();
            root.distnet.showVLContour();

        }


    }
}
