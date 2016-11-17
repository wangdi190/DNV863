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
using System.Windows.Shapes;
using WpfEarthLibrary;

namespace DNV863
{
    /// <summary>
    /// tempWinTransCood863.xaml 的交互逻辑
    /// </summary>
    public partial class tempWinTransCood863 : Window
    {
        public tempWinTransCood863()
        {
            InitializeComponent();
        }

        private void btntran_Click(object sender, RoutedEventArgs e)
        {
            string sql = @"
                    select t1.name,t2.value cname from 
                    (select * from sysobjects t1 where xtype='U') t1 left join
                    (select * from sys.extended_properties   where minor_id=0 and name='MS_Description') t2 on t1.id=t2.major_id order by name
                    ";
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string tname = dr.Field<string>("name").ToLower();

                // **************** 不处理以前的t_打头的表  ******************
                if (tname.Substring(0, 2) == "t_") continue;

                sql = @"
                    SELECT syscolumns.name,systypes.name ftype,t3.value cname 
                    FROM syscolumns join systypes on syscolumns.xusertype = systypes.xusertype 
	                    left join sys.extended_properties t3 on syscolumns.id=t3.major_id and syscolumns.colorder=t3.minor_id
                    WHERE syscolumns.id = object_id('{0}') order by name
";
                DataTable dt2 = DataLayer.DataProvider.getDataTableFromSQL(String.Format(sql, tname));
                List<string> fields = new List<string>();
                foreach (DataRow item in dt2.Rows)
                {
                    fields.Add(item.Field<string>("name").ToLower());
                }


                DataTable dt3 = DataLayer.DataProvider.getDataTableFromSQL("select * from " + dr.Field<string>("name"));
                if (fields.Contains("f_id") && dt3.Rows.Count > 0) //有主键字段且有数据
                {
                    sqls.Clear();
                    string ename, gname;
                    ename = "f_ex"; gname = "f_gx";
                    if (fields.Contains(ename) && fields.Contains(gname))
                        filltwo(dt3, ename, gname, tname);
                    ename = "f_ey"; gname = "f_gy";
                    if (fields.Contains(ename) && fields.Contains(gname))
                        filltwo(dt3, ename, gname, tname);
                    ename = "f_ezbch"; gname = "f_gzbch";
                    if (fields.Contains(ename) && fields.Contains(gname))
                        filltwo(dt3, ename, gname, tname);
                    DataLayer.DataProvider.bacthExecute(sqls);
                    //======================
                    sqls.Clear();
                    dt3 = DataLayer.DataProvider.getDataTableFromSQL("select * from " + dr.Field<string>("name"));
                    transCood(dt3, fields, tname);
                    DataLayer.DataProvider.bacthExecute(sqls);
                }
            }

            infos.Insert(0, "*************** 处理完毕 **************");
        }

        List<string> sqls = new List<string>();
        void filltwo(DataTable dt, string ename, string gname, string tablename)
        {
            if (dt.Rows.Count == 0) return;
           
            foreach (DataRow dr in dt.Rows)
            {
                string id = dr.Field<string>("f_id");
                string ev = dr[ename].ToString();
                string gv = dr[gname].ToString();
                string s;
                if (ev != gv)
                {
                    if (ev == "0" || ev == "无" || string.IsNullOrWhiteSpace(ev))
                        s = string.Format("update {0} set {1}={2} where f_id='{3}'", tablename, ename, gname, id);
                    else
                        s = string.Format("update {0} set {1}={2} where f_id='{3}'", tablename, gname, ename, id);
                    //DataLayer.DataProvider.ExecuteSQL(s);
                    sqls.Add(s);
                }
            }

        }

        void transCood(DataTable dt, List<string> fields, string tablename)
        {
            double xdiv = 530, ydiv = 100; //x y 偏移，地方坐标附加的偏移
            bool istransed = false;
            if (fields.Contains("f_gx") && fields.Contains("f_gy"))
            {
              
                foreach (DataRow dr in dt.Rows)
                {
                    string id = dr.Field<string>("f_id");
                    if (dr["f_gx"] is DBNull || dr["f_gy"] is DBNull) continue;
                    Point inp = geohelper.TransformToD(new Point(dr.Field<double>("f_gx")+xdiv, -1 * (dr.Field<double>("f_gy")+ydiv)));
                    if (inp.X < 1000) continue;  //已转换过的，不再进行转换, 适宜于只有G, 没有E的情形
                    inp = geohelper.Plane2Geo(inp);
                    string s = string.Format("update {0} set f_gx={1}, f_gy={2} where f_id='{3}'", tablename, inp.X, inp.Y, id);
                    //DataLayer.DataProvider.ExecuteSQL(s);
                    sqls.Add(s);
                }
                istransed = true;
            }
                        
            if (fields.Contains("f_gzbch"))
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string id = dr.Field<string>("f_id");
                    if (dr["f_gzbch"] is DBNull) continue;
                    string s = dr.Field<string>("f_gzbch");
                    int tmpi;
                    if (!int.TryParse(s.Substring(0,1),out tmpi))
                        continue;
                    string[] ss = s.Split(',');
                    double tmp;
                    if (ss.Count() > 0 && double.TryParse(ss[0], out tmp))//已转换过的，不再进行转换, 适宜于只有G, 没有E的情形
                        continue;

                    PointCollection geos = new PointCollection();
                    System.Windows.Point[] pnts = new System.Windows.Point[ss.Length];
                    System.Windows.Point pnt = new System.Windows.Point(0, 0);
                        for (int i = 0; i < ss.Length; i++)
                        {
                            pnt = System.Windows.Point.Parse(ss[i].Replace(' ', ',').Replace(";",""));
                            pnts[i] = pnt;
                            Point inp = geohelper.TransformToD(new Point(pnt.X+xdiv, -pnt.Y-ydiv));
                            inp = geohelper.Plane2Geo(inp);
                            geos.Add(new System.Windows.Point(inp.X, inp.Y));
                        }
                        string gs = geos.ToString();

                        s = string.Format("update {0} set f_gzbch='{1}' where f_id='{2}'", tablename, gs, id);
                        //DataLayer.DataProvider.ExecuteSQL(s);
                        sqls.Add(s);
                  
                  

                }
               
                istransed = true;
            }

            if (istransed)
            {
                infos.Insert(0, "已处理表" + tablename);
                MyClassLibrary.helper.refreshScreen();
            }
        }



        System.ComponentModel.BindingList<string> infos = new System.ComponentModel.BindingList<string>();
        private void Window_Initialized(object sender, EventArgs e)
        {
            //初始化四参数转换
            geohelper.m_DX = 4118585.75733497;
            geohelper.m_DY = 38942245.9657318;
            geohelper.m_Scale = 1.00006825902658;
            geohelper.m_RotationAngle = 1.57811204215682;

            lstinfo.ItemsSource = infos;

        }
    }
}
