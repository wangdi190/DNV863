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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DNVLibrary.Run
{
    /// <summary>
    /// RHistoryReplayPanel.xaml 的交互逻辑
    /// </summary>
    public partial class RHistoryReplayPanel : UserControl, BaseIPanel
    {
        public RHistoryReplayPanel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
        }

        UCDNV863 root;
        PanelData paneldata;
        Random rd = new Random();

        List<MyClassLibrary.DevShare.ChartDataPoint> alldata = new List<MyClassLibrary.DevShare.ChartDataPoint>();

        ///<summary>基础时间片分钟数</summary>
        public int baseMinuteSpan = 5;

        DateTime curTime; //当前时间
        TimeSpan timespan; //每5秒间隔时间
        double progress; //进度值
        TimeSpan alltimes;  //全部时间

        System.Windows.Threading.DispatcherTimer tmr;


        private void UserControl_Initialized(object sender, EventArgs e)
        {
            tmr = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromSeconds(5) };
            tmr.Tick += new EventHandler(tmr_Tick);
            trc.EditValueChanged += new DevExpress.Xpf.Editors.EditValueChangedEventHandler(trc_EditValueChanged);

            timespan = TimeSpan.FromMinutes(baseMinuteSpan * 4);


            paneldata = new PanelData(root);
            grdMain.DataContext = paneldata;

        }

        public void load()
        {
            root.earth.config.tooltipMoveEnable = true;
        }

        public void unload()
        {
            tmr.Stop();
            DataGenerator.UpdateLineData = DataGenerator.UpdateStationData = DataGenerator.UpdateSwitchData = null;

            root.earth.config.tooltipMoveEnable = false;
            root.distnet.clearVLContour();
            root.distnet.clearFlow();
            root.distnet.clearLoadCol();

            if (root.earth.objManager.zLayers.Keys.Contains("停电图层"))
            {
                root.earth.objManager.zLayers.Remove("停电图层");
                //cutAreaLayer.pModels.Clear();
            }

        }

     



        ///<summary>读取数据并刷新界面</summary>
        void refreshData()
        {
            paneldata.readData();

            //zh注
            root.distnet.showFlow();
            root.distnet.showLoadCol();
            root.distnet.showVLContour();

        }

        ///<summary>刷新时间控制板</summary>
        void refreshtime()
        {
            txtDate.Text = curTime.ToString("yyyy.MM.dd HH:mm");
            txtRad.Text = string.Format("{0:f0}m", timespan.TotalMinutes);
        }

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

        void initchart()
        {
            alldata.Clear();
            DateTime tmpdate = startDate;
            while (tmpdate<endDate)
            {
                alldata.Add(new MyClassLibrary.DevShare.ChartDataPoint(){argudate=tmpdate, value=rd.Next(200), sort="供电负荷"});
                alldata.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate = tmpdate, value = rd.Next(30), sort = "清洁能源出力" });
                alldata.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate = tmpdate, value = rd.Next(20), sort = "充电桩负荷" });
                alldata.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate = tmpdate, value = rd.Next(60), sort = "大客户负荷" });
                alldata.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate = tmpdate, value = rd.Next(20), sort = "停电损失负荷" });
                tmpdate = tmpdate.AddMinutes(baseMinuteSpan);
            }

            cht.DataSource = null;
            cht.DataSource = alldata;
        }
        

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            //zh注
            root.distnet.showFlow();
            root.distnet.showLoadCol();
            root.distnet.showVLContour();

            initchart();
          
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
            curTime = startDate + TimeSpan.FromMinutes(count*baseMinuteSpan);
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
                curTime= diagramCoordinates.DateTimeArgument;
                chtlin.Value = curTime;
                trc.EditValueChanged -= new DevExpress.Xpf.Editors.EditValueChangedEventHandler(trc_EditValueChanged);
                progress = (curTime - startDate).TotalMinutes / alltimes.TotalMinutes;
                trc.Value = progress;
                trc.EditValueChanged += new DevExpress.Xpf.Editors.EditValueChangedEventHandler(trc_EditValueChanged);
                refreshtime();
                refreshData();
            }
        }
    }
}
