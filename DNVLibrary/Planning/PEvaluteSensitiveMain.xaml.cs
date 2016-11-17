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
    /// PEvaluteSensitiveMain.xaml 的交互逻辑
    /// </summary>
    public partial class PEvaluteSensitiveMain : UserControl
    {
        public PEvaluteSensitiveMain()
        {
            InitializeComponent();
        }

        ViewModel viewmodel = new ViewModel();

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            string sql = "select t1 as asort, t2 as csort, v1, v2, v3  from map_data2 where sort='收益率敏感分析' order by ord";
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(sql);

            IEnumerable<string> g = dt.AsEnumerable().GroupBy(p => p.Field<string>("asort")).Select(p => p.Key);

            foreach (string sortname in g)
            {
                ASort asort = new ASort();
                viewmodel.sorts.Add(asort);
                readdata(asort, sortname, dt);
            }

            viewmodel.cal();

            grdMain.DataContext = viewmodel;
        }


        void readdata(ASort asort, string asortname, DataTable dt)
        {
            asort.name = asortname+"-收益率敏感性" ;
            IEnumerable<string> g = dt.AsEnumerable().Where(p => p.Field<string>("asort") == asortname && p.Field<string>("csort")!="基本方案").GroupBy(p => p.Field<string>("csort")).Select(p => p.Key);
            foreach (string key in g)
            {
                Ana ana = new Ana();
                asort.anas.Add(ana);
                IEnumerable<DataRow> drs = dt.AsEnumerable().Where(p => p.Field<string>("asort") == asortname && (p.Field<string>("csort") == key || p.Field<string>("csort") == "基本方案")).OrderBy(p=>p.Field<double>("v1"));
                ana.kp=new Point[drs.Count()];
                ana.name = key;
                int idx = 0;
                foreach (DataRow dr in drs)
                {
                    GridDataRow gd = new GridDataRow() { name = key }; 
                    double tmp=double.Parse(dr["v1"].ToString());
                    gd.changeorg = (tmp > 0 ? "+" : "") + tmp.ToString("f0") + "%"; 
                    double tmp1=double.Parse(dr["v2"].ToString());
                    gd.changedest=tmp1.ToString("f2")+"%";
                    double tmp2=double.Parse(dr["v3"].ToString());
                    gd.changerate = tmp == 0 ? "" : (tmp2.ToString("f2") + "%");
                    gd.senesitive =tmp==0?0: tmp2 / tmp;// tmp==0?"": (tmp2 / tmp).ToString("f2");
                    ana.griddata.Add(gd);
                    ana.kp[idx] = new Point(tmp, tmp1);
                    idx++;
                }
            }
        }

    }



    internal class ViewModel
    {
        public ViewModel()
        {
            sorts = new List<ASort>();
        }


        public List<ASort> sorts { get; set; }


        public void cal()
        {
            foreach (ASort asort in sorts)
            {
                asort.cal();
            }
        }
    }

    internal class ASort
    {
        public string name { get; set; }

        public List<MyClassLibrary.DevShare.ChartDataPoint> pnts { get; set; }
        public List<GridDataRow> griddata { get; set; }

        public List<Ana> anas = new List<Ana>();
        
        public void cal()
        {
            pnts=new List<MyClassLibrary.DevShare.ChartDataPoint>();
            griddata = new List<GridDataRow>();

            foreach (Ana item in anas)
            {
                item.cal();

                pnts = pnts.Union(item.pnts).ToList();
                griddata = griddata.Union(item.griddata).ToList();
            }


        }
    }

    internal class Ana
    {
        public string name {get;set;}

        public List<GridDataRow> griddata = new List<GridDataRow>();

        public Point[] kp;
        public List<MyClassLibrary.DevShare.ChartDataPoint> pnts=new List<MyClassLibrary.DevShare.ChartDataPoint>();

        public void cal()
        { 
            Point[] cp1, cp2;
            MyClassLibrary.MyFunction.GetCurveControlPoints(kp, out cp1, out cp2);

            Point pnt;

            for (int i = 0; i < kp.Count()-1; i++)
            {
                Point[] cp = new Point[4];
                cp[0] = kp[i]; cp[1] = cp1[i]; cp[2] = cp2[i]; cp[3] = kp[i + 1];

                for (int j = 1; j < 10; j++)
                {
                    pnt = MyClassLibrary.MyFunction.PointOnBezier(cp, 1.0*j / 10);
                    pnts.Add(new MyClassLibrary.DevShare.ChartDataPoint() { sort = name, argudouble = pnt.X, value = pnt.Y });
                }
            }
            foreach (Point pnt2 in kp)
            {
                pnts.Add(new MyClassLibrary.DevShare.ChartDataPoint() { sort = name, argudouble = pnt2.X, value = pnt2.Y });
            }

        }

    }

    internal class GridDataRow
    {
        public string name {get;set;}
        public string changeorg { get; set; }
        public string changedest { get; set; }
        public string changerate { get; set; }
        public double senesitive { get; set; }
    }

}
