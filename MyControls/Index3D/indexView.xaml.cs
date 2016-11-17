using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Data.OleDb;
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


namespace MyControlLibrary.Controls3D.Index3D
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class indexView : ContentControl
    {
        public indexView(string indexname, string viewtype, string viewinfo, List<TRow> detaildata, string strvalue)
        {
            InitializeComponent();

            _detaildata = detaildata;
            _indexID = indexname; 
            _visualView = viewtype;
            _viewInfo = viewinfo;
            _strvalue = strvalue;
            showContent();
        }

        public indexView(string indexname,string viewtype,string viewinfo,string sql, DateTime datebegin, DateTime dateend)
        {
            InitializeComponent();
            _indexID = indexname;
            _visualView = viewtype;
            _viewInfo = viewinfo;
            _sql = sql;
            dateb = datebegin;
            datee = dateend;
            showContent();
            /*
            _indexID = sID;
            txtName.Text = name;
            txtInfo.Text = info;
            if (charttype == "3D比例图")
            {
                ChartControl zChart = new ChartControl();
                zChart.Background = null;
                zChart.Diagram = new SimpleDiagram3D();
                (zChart.Diagram as SimpleDiagram3D).ZoomPercent = 150;
                PieSeries3D s1 = new PieSeries3D();
                s1.Label = new SeriesLabel();
                (s1.Label as SeriesLabel).ElementTemplate = (DataTemplate)this.FindResource("tplPie");
                s1.PointOptions = new PointOptions();
                s1.PointOptions.PointView = PointView.ArgumentAndValues;
                NumericOptions opt= new NumericOptions();
                opt.Format = NumericFormat.Percent;
                opt.Precision = 1;
                s1.PointOptions.ValueNumericOptions = opt;
                s1.DataSource = dv;
                s1.ArgumentDataMember = "s1";
                s1.ValueDataMember = "v1";
                zChart.Diagram.Series.Add(s1);
                grdChart.Children.Add(zChart);
            }
            else if (charttype == "柱图")
            {
                ChartControl zChart = new ChartControl();
                zChart.Diagram = new XYDiagram2D();
                BarSideBySideSeries2D s1 = new BarSideBySideSeries2D();
                s1.DataSource = dv;
                s1.ArgumentDataMember = "s1";
                s1.ValueDataMember = "v1";
                zChart.Diagram.Series.Add(s1);
                grdChart.Children.Add(zChart);
            }
            else if (charttype == "曲线图")
            {
                ChartControl zChart = new ChartControl();
                zChart.Diagram = new XYDiagram2D();
                (zChart.Diagram as XYDiagram2D).AxisX = new AxisX2D();
                (zChart.Diagram as XYDiagram2D).AxisX.Label = new AxisLabel();
                (zChart.Diagram as XYDiagram2D).AxisX.Label.Visible = false;
                LineSeries2D s1 = new LineSeries2D();
                s1.MarkerVisible = false;
                s1.Label = new SeriesLabel();
                s1.Label.Visible = false;
                s1.DataSource = dv;
                s1.ArgumentDataMember = "ztime";
                s1.ValueDataMember = "v1";
                zChart.Diagram.Series.Add(s1);
                grdChart.Children.Add(zChart);
            }
            else if (charttype == "面积图")
            {
                ChartControl zChart = new ChartControl();
                zChart.Diagram = new XYDiagram2D();
                AreaSeries2D s1 = new AreaSeries2D();
                (zChart.Diagram as XYDiagram2D).AxisX = new AxisX2D();
                (zChart.Diagram as XYDiagram2D).AxisX.Label = new AxisLabel();
                (zChart.Diagram as XYDiagram2D).AxisX.Label.Visible = false;
                s1.MarkerVisible = false;
                s1.Label = new SeriesLabel();
                s1.Label.Visible = false;
                s1.DataSource = dv;
                s1.ArgumentDataMember = "ztime";
                s1.ValueDataMember = "v1";
                zChart.Diagram.Series.Add(s1);
                grdChart.Children.Add(zChart);
            }
            */
        }

        private string _indexID;
        public string indexID
        { get { return _indexID; } }
        private string _visualView;
        private string _viewInfo;
        private string _sql;
        private string _strvalue;
        private DateTime dateb, datee;

        private List<TRow> _detaildata;

        public Point orgPoint { get; set; }

        public event EventHandler CloseEvent;
        protected virtual void OnCloseEvent(object sender, EventArgs e)
        {
            EventHandler handler = CloseEvent;

            if (handler != null)
            {
                handler(this, e);
            }
        }


        private void showContent()
        {
            //DataTable dt;
            UserControl view;
            //switch (_visualView)
            //{
            //    case "项目时间比例视图":
            //        // 备忘：传入细节数据SQL，暂在内容临时用固定模拟数据
            //        //dt = (new simulationData()).getDataTable("项目时间比率类",0);
            //        //dt = createDataTable(_sql);

            //        view = new viewXMSJBL();
            //        //(view as viewXMSJBL).loadChart(createDataTable(_sql), _viewInfo);
            //        (view as viewXMSJBL).loadChart(_detaildata,_viewInfo,_strvalue);
            //        grdChart.Children.Add(view);
            //        break;
            //    case "范围偏差视图":
            //        //dt = (new simulationData()).getDataTable("数据偏差类", 0);
            //        view = new viewPC();
            //        //(view as viewPC).loadChart(createDataTable(_sql), _viewInfo);
            //        (view as viewPC).loadChart(_detaildata, _viewInfo,_strvalue);
            //        grdChart.Children.Add(view);
            //        break;
            //    case "数值统计视图":
            //        //dt = (new simulationData()).getDataTable("数据偏差类", 0);
            //        view = new viewSZ();
            //        (view as viewSZ).loadChart(_detaildata, _viewInfo,_strvalue);
            //        grdChart.Children.Add(view);
            //        break;
            //    case "利用小时视图":
            //        //dt = (new simulationData()).getDataTable("数据偏差类", 0);
            //        view = new view_LYXS();
            //        //(view as view_LYXS).loadChart(createDataTable(_sql), _viewInfo);
            //        (view as view_LYXS).loadChart(_detaildata, _viewInfo,_strvalue);
            //        grdChart.Children.Add(view);
            //        break;

            //    case "常规视图":
            //        //dt = (new simulationData()).getDataTable("数据偏差类", 0);
            //        view = new viewNormal();
            //        (view as viewNormal).loadChart(_detaildata, _viewInfo,_strvalue);
            //        grdChart.Children.Add(view);
            //        break;
            //    case "饼视图":
            //        //dt = (new simulationData()).getDataTable("数据偏差类", 0);
            //        view = new viewPie();
            //        (view as viewPie).loadChart(_detaildata, _viewInfo,_strvalue);
            //        grdChart.Children.Add(view);
            //        break;
            //}
        }


        public void setActive(Boolean isActive)
        {
            if (isActive)
            {
                grdShadow.Visibility = Visibility.Hidden;
                grdSHControl.Visibility = Visibility.Visible;
            }
            else
            {
                grdShadow.Visibility = Visibility.Visible;
                grdSHControl.Visibility = Visibility.Hidden;
            }
        }

        private void btnclose_Click(object sender, RoutedEventArgs e)
        {
            OnCloseEvent(this, e);
        }



        private List<TRow> createDataTable(string sql)
        {
            if (sql == null || sql == "")
                return null;

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("zDate", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("zName", typeof(string)));
            dt.Columns.Add(new DataColumn("zValue1", typeof(double)));
            dt.Columns.Add(new DataColumn("zValue2", typeof(double)));
            
            

            OleDbConnection conn = new OleDbConnection("Provider=MSDAORA;Data Source=vids;Persist Security Info=True;Password=vids;User ID=vids");


            string sdb = dateb.ToString("yyyy/MM/dd");
            string sde = datee.AddDays(1).ToString("yyyy/MM/dd");

            DataContext db = new DataContext(conn);
            sql = sql.Replace("{0}", sdb);
            sql = sql.Replace("{1}", sde);
            
            //db.ObjectTrackingEnabled = false;
            List<TRow> result = db.ExecuteQuery<TRow>(sql).ToList<TRow>();
            /*
            foreach (TRow e in result)
            {
                DataRow dr = dt.NewRow();
                dr[0] = e.zdate;
                dr[1] = e.zname;
                dr[2] = (double)e.zvalue1;
                dr[3] = (double)e.zvalue2;
                dt.Rows.Add(dr);
            }
            
            return dt;
           */
            return result;
        }

    }
}
