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
    /// PAllGauge.xaml 的交互逻辑
    /// </summary>
    public partial class PAllGauge : UserControl
    {
        public PAllGauge()
        {
            InitializeComponent();

        }

        private ProjectData _prjdata;
        ///<summary>规划方案</summary>
        public ProjectData prjdata
        {
            get { return _prjdata; }
            set { _prjdata = value; grdMain.DataContext = value; updateChartDataSource(); updateObjListDatasource(); }
        }

                
        private ProjectData _nowPrj;
        ///<summary>现状方案</summary>
        public ProjectData nowPrj
        {
            get { return _nowPrj; }
            set { _nowPrj = value; initChartDataSource(); }
        }
        

        void initChartDataSource()
        {
            foreach (Index item in nowPrj.idxes.indexes.Values)
            {
                List<MyClassLibrary.DevShare.ChartDataPoint> pd = new List<MyClassLibrary.DevShare.ChartDataPoint>();
                pd.Add(new MyClassLibrary.DevShare.ChartDataPoint("配网现状", item.name, item.value));
                //pd.Add(new MyClassLibrary.DevShare.ChartDataPoint("规划", "", 0));
                chartDataSource.Add(item.name, pd);

                radarDataSource.Add(new Interact.RadarDataItem() { sort="现状", argu=item.shortname, minvalue=item.min, maxvalue=item.max, format=item.format.Replace('V','0'), value=item.value });
            }

            rad.pens.Add(new Pen() { Brush = Brushes.Orange, Thickness = 1 });
            rad.pens.Add(new Pen() { Brush = Brushes.Lime, Thickness = 1, DashStyle = DashStyles.Dash });

        }


        List<Interact.RadarDataItem> radarDataSource = new List<Interact.RadarDataItem>();


        void updateChartDataSource()
        {
            radarDataSource.RemoveAll(p => p.sort == "规划"); 
            if (prjdata == nowPrj)
            {
                foreach (var item in chartDataSource.Values)
                {
                    item.RemoveAll(p => p.argu == "规划方案");
                }
            }
            else
            {
                foreach (Index item in prjdata.idxes.indexes.Values)
                {
                    List<MyClassLibrary.DevShare.ChartDataPoint> pd = chartDataSource[item.name];
                    MyClassLibrary.DevShare.ChartDataPoint gh = pd.FirstOrDefault(p => p.argu == "规划方案");
                    if (gh == null)
                    {
                        gh = new MyClassLibrary.DevShare.ChartDataPoint("规划方案", item.name, item.value);
                        pd.Add(gh);
                    }
                    gh.value = item.value;

                    radarDataSource.Add(new Interact.RadarDataItem() { sort = "规划", argu = item.shortname, minvalue = item.min, maxvalue = item.max, format = item.format.Replace('V', '0'), value = item.value });
                }
            }
            serial1.DataSource = null;
            serial1.DataSource = chartDataSource.Values.ElementAt(0);

            rad.dataSource = radarDataSource;

        }

        Dictionary<string, List<MyClassLibrary.DevShare.ChartDataPoint>> chartDataSource = new Dictionary<string, List<MyClassLibrary.DevShare.ChartDataPoint>>();

        void updateObjListDatasource()
        {
            lstObjects.ItemsSource = null;
            lstObjects.ItemsSource = prjdata.addobjs;
        }

        System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer();
        int curIdx = 0;
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            tmr.Interval = TimeSpan.FromSeconds(10);
            tmr.Tick += new EventHandler(tmr_Tick);
            
            tmr.Start();
        }

        void tmr_Tick(object sender, EventArgs e)
        {
            if (chartDataSource.Count == 0) return;
            curIdx = curIdx < chartDataSource.Count - 1 ? curIdx + 1 : 0;
            title.Content = chartDataSource.Values.ElementAt(curIdx)[0].sort;
            //yRange.MaxValue = nowPrj.idxes.indexes[chartDataSource.Values.ElementAt(curIdx)[0].sort].max;
            //yRange.MinValue = nowPrj.idxes.indexes[chartDataSource.Values.ElementAt(curIdx)[0].sort].min;
            slabel.TextPattern = nowPrj.idxes.indexes[chartDataSource.Values.ElementAt(curIdx)[0].sort].format;

            serial1.DataSource = null;
            serial1.DataSource = chartDataSource.Values.ElementAt(curIdx);
        }



        
      
    }
}
