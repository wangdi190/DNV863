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
using WpfEarthLibrary;
using DistNetLibrary;

namespace DNVLibrary.Planning
{
    /// <summary>
    /// PLoadForcastPanel.xaml 的交互逻辑
    /// </summary>
    public partial class PRunLoadForcastPanel : UserControl, BaseIPanel
    {
        public PRunLoadForcastPanel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
        }

        Random rd = new Random();
        public void load()
        {
            //初始化负荷图例
            for (int i = 0; i < 5; i++)
            {
                double scale = (float)(i / 4.0);
                linegradbrush.GradientStops.Add(new GradientStop(MyClassLibrary.Share2D.MediaHelper.getColorBetweenRedBlue(scale), scale));
            }


            List<PowerBasicObject> objs = root.distnet.dbdesc["基础数据"].DictSQLS["网格"].batchLoadRunData(root.distnet, false, 1);
            double maxLoadDensity = objs.Max(p => (p.busiRunData as RunDataGridArea).loadDensity);
            foreach (var obj in objs)
            {
                obj.tooltipMoveTemplate = "PlanningGridAreaTemplate";
                obj.tooltipMoveContent = obj.busiRunData;

                obj.color = MyClassLibrary.Share2D.MediaHelper.getColorBetweenRedBlue((obj.busiRunData as RunDataGridArea).loadDensity/maxLoadDensity);
            }

            //foreach (DNGridArea area in root.distnet.getAllObjListByObjType(EObjectType.网格))
            //{
            //    area.busiData = new busiBase(area);
            //    area.busiData.busiValue1 = rd.Next(100);
            //    area.busiData.busiValue2 = rd.NextDouble();
            //    area.busiData.busiColor1 = MyClassLibrary.Share2D.MediaHelper.getColorBetweenRedBlue(area.busiData.busiValue2);//.getColorBetween(area.busiData.busiValue2 , Colors.Blue, Colors.Red);
                
            //    area.color = area.busiData.busiColor1;
            //}
            //root.distnet.scene.objManager.zLayers[EObjectCategory.区域类.ToString()].logicVisibility = true;


            root.distnet.scene.objManager.zLayers["网格"].logicVisibility = true;
            root.earth.UpdateModel();
            root.earth.MouseDoubleClick += new MouseButtonEventHandler(earth_MouseDoubleClick);

            ////===== 色例
            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from d_gridareatype");
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("SELECT Sort,MIN(color) color FROM Dic_RegionUseType where InstanceID=1 group by sort");
            List<ColorDesc> colorlist = new List<ColorDesc>();
            foreach (DataRow dr in dt.Rows)
                colorlist.Add(new ColorDesc() { name = dr["sort"].ToString(), brush = new SolidColorBrush((Color)MyClassLibrary.MyColorConverter.Default.ConvertFrom(dr.Field<string>("color"))) });
            lstColor.ItemsSource = colorlist;


            //var tmp = from e in root.distnet.getAllObjListByObjType(EObjectType.网格)
            //          group e by (e.busiAccount as AcntGridArea).useType into g
            //          select new 
            //          {
            //              rtype=g.Key,
            //              load = g.Sum(p => (p.busiRunData as RunDataGridArea).load)
            //              //load=g.Sum(p=>p.busiData.busiValue1)
            //          };
            var tmp= from e in objs
                     group e by (e.busiAccount as AcntGridArea).useType into g
                      select new 
                      {
                          rtype=g.Key,
                          load = g.Sum(p => (p.busiRunData as RunDataGridArea).load)
                          //load=g.Sum(p=>p.busiData.busiValue1)
                      };
            cht2.DataSource = tmp;

            //var tmp2 = from e in root.distnet.getAllObjListByObjType(EObjectType.网格)
            //           select new
            //           {
            //               名称 = e.name,
            //               负荷=e.busiData.busiValue1,
            //               负荷密度=e.busiData.busiValue2,
            //               建筑面积=(e.busiAccount as AcntGridArea).area,
            //               容积率 = (e.busiAccount as AcntGridArea).rjl,
            //           };
            var tmp2 = from e in objs
                       select new
                       {
                           名称 = e.name,
                           负荷 = (e.busiRunData as RunDataGridArea).load,
                           负荷密度 = (e.busiRunData as RunDataGridArea).loadDensity,
                           建筑面积 = (e.busiAccount as AcntGridArea).area,
                           容积率 = (e.busiAccount as AcntGridArea).rjl,
                       };
            griddata.ItemsSource = tmp2;


            root.distnet.scene.config.tooltipMoveEnable = true;
        }
        public void unload()
        {
            foreach (DNGridArea area in root.distnet.getAllObjListByObjType(EObjectType.网格))
            {
                area.color = area.typeColor;
            }
            root.distnet.scene.objManager.zLayers["网格"].logicVisibility = false;
            root.earth.UpdateModel();

            root.earth.MouseDoubleClick -= new MouseButtonEventHandler(earth_MouseDoubleClick);
            //loadlayer.pModels.Clear();
            //root.earth.objManager.zLayers.Remove("负荷预测图层");

            //root.earth.UpdateModel();

            root.distnet.scene.config.tooltipMoveEnable = false;
        }
        UCDNV863 root;


        ///<summary>路由命令，规划日期改变</summary>
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        //pLayer loadlayer;
        //Stack<string> stackAreaLayer=new Stack<string>();
        //Stack<string> stackBH = new Stack<string>();
        //Stack<PowerBasicObject> stackObj = new Stack<PowerBasicObject>();
        //string curAreaLayer, curBH;
        //PowerBasicObject curObj;
        //enum EColorType {用地类型着色,负荷密度着色}
        //EColorType colortype = EColorType.负荷密度着色;
        //Rect initRange; //初始范围
        private void UserControl_Initialized(object sender, EventArgs e)
        {


            //tooltips之实现

            

        }
      

        void earth_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PowerBasicObject selobj = null;
            selobj = root.earth.objManager.pick(e.GetPosition(sender as UserControl));


            propObj.SelectedObject = selobj == null ? null : selobj.busiAccount;


            //if (selobj==null) 
            //{
            //    if (stackAreaLayer.Count>0)
            //    {
            //        curAreaLayer = stackAreaLayer.Pop();
            //        curBH = stackBH.Pop();
            //        curObj = stackObj.Pop();
            //        propObj.SelectedObject =curObj==null?null: curObj.busiAccount;
            //        createLayer(curAreaLayer, curBH);
            //    }
            //}
            //else
            //{
            //    if (curAreaLayer!="小区图层")
            //    {
            //        propObj.SelectedObject = selobj.busiAccount;
            //        stackAreaLayer.Push(curAreaLayer);
            //        stackBH.Push(curBH);
            //        stackObj.Push(curObj);
            //        if (curAreaLayer == "大区图层")
            //            curAreaLayer = "中区图层";
            //        else
            //            curAreaLayer = "小区图层";
            //        curBH = selobj.id;
            //        curObj = selobj;
            //        createLayer(curAreaLayer, curBH);
            //    }
            //    else
            //        propObj.SelectedObject = selobj.busiAccount;
            //}
            //if (curObj!=null)
            //{
            //    root.earth.camera.adjustCameraRange(curObj.bounds);
            //}
            //else
            //    root.earth.camera.adjustCameraRange(initRange);

        }


        
        void createLayer(string arealayername, string bh)
        {
            //loadlayer.pModels.Clear();

            //DataTable dtshareobject = DataLayer.DataProvider.getDataTableFromSQL(string.Format("select * from map_share_object where prjid=11 and layername='{0}' and belongto='{1}'",arealayername, bh));
            //DataTable dtdata = DataLayer.DataProvider.getDataTableFromSQL(string.Format("select objid,header,cast(value as float) value from map_data where prjid=11 and sort='负荷预测' and objid in (select bh from map_share_object where prjid=11 and layername='{0}' and belongto='{1}')", arealayername, bh));

            //pArea area; AcntArea acnt;
            //double value, value2;
            //double max = arealayername=="大区图层"? 4: 40000;
            //if (arealayername == "小区图层")
            //{
            //    btnOrg.IsEnabled = true;
            //    btnOrg.Foreground = Brushes.Black;
            //}
            //else
            //{
            //    btnOrg.IsEnabled = false;
            //    btnLoad.Foreground = Brushes.Blue;
            //    btnOrg.Foreground = Brushes.DarkGray;
            //    colortype = EColorType.负荷密度着色;
            //    grdAreaTypeLegend.Visibility = System.Windows.Visibility.Collapsed;

            //}
            //foreach (DataRow dr in dtshareobject.Rows)
            //{
            //    area = new pArea(loadlayer){id=dr["bh"].ToString(), strPoints=dr["points"].ToString(), name=dr["objname"].ToString()};
            //    if (arealayername != "小区图层")
            //    {
            //        area.Label = area.name; area.LabelColor = Colors.Yellow;
            //        area.LabelSizeX = area.LabelSizeY = arealayername == "大区图层" ? 20 : 10;
            //        area.isShowLabel = true; //isShowLabel暂最后传递
            //    }

            //    value = dtdata.AsEnumerable().FirstOrDefault(p => p.Field<string>("objid") == area.id && p.Field<string>("header") == "负荷").Field<double>("value");
            //    value2 = dtdata.AsEnumerable().FirstOrDefault(p => p.Field<string>("objid") == area.id && p.Field<string>("header") == "负荷密度").Field<double>("value");
            //    area.busiData.busiCurValue = value2;
            //    area.busiData.busiValue1 = value;
            //    area.busiData.busiColor1 = MyClassLibrary.Share2D.MediaHelper.getColorBetween(value2 / max, Colors.Blue, Color.FromRgb(0xFA, 0x00, 0x05));
            //    area.busiData.busiColor2 = ((SolidColorBrush)new MyClassLibrary.MyBrushConverter().ConvertFromString(dr["fill"].ToString())).Color;
            //    area.areaColor = area.busiData.busiColor1;

            //    acnt = new AcntArea() {id=area.id, name=area.name, belongto=dr["belongto"].ToString(), };
            //    double tmp;
            //    if (double.TryParse(dr["area"].ToString(), out tmp)) acnt.area = tmp;
            //    if (double.TryParse(dr["rjl"].ToString(), out tmp)) acnt.rjl = tmp;
            //    area.busiAccount = acnt;
            //    loadlayer.AddObject(area.id, area);
            //}

            //updatePie();
            //updateTable();
            //root.earth.UpdateModel();
        }


        ///<summary>饼图</summary>
        void updatePie()
        {
//            string sql = @"select rtype,SUM(cast(value as float)) load from map_share_object t1 ,map_data t2 
//	where t1.prjid=11 and layername='小区图层' and bh like '{1}%' and t2.header='负荷' and t2.objid=t1.bh and t2.prjid=t1.prjid 
//	group by rtype";
//            sql = string.Format(sql, curAreaLayer, curBH);
//            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(sql);
//            cht2.DataSource = dt;
        }
        ///<summary>表格</summary>
        void updateTable()
        {
//            string sql = @"select objname 名称,fh 负荷,md 负荷密度,mj 建筑面积 from
//(select bh,objname from map_share_object where prjid=11 and layername='{0}' and belongto='{1}') t1 join 
//(select objid,header,cast(value as float) fh from map_data where prjid=11 and sort='负荷预测' and header='负荷') t2 on t1.bh=t2.objid join
//(select objid,header,cast(value as float) md from map_data where prjid=11 and sort='负荷预测' and header='负荷密度') t3 on t1.bh=t3.objid join
//(select objid,header,cast(value as float) mj from map_data where prjid=11 and sort='负荷预测' and header='建筑面积') t4 on t1.bh=t4.objid
//";
//            sql = string.Format(sql, curAreaLayer, curBH);
//            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(sql);
//            griddata.ItemsSource = dt;
        }

        private void btnOrg_Click(object sender, RoutedEventArgs e)
        {
            //foreach (pArea item in loadlayer.pModels.Values)
            //{
            //    item.areaColor = item.busiData.busiColor2;
            //}
            btnOrg.Foreground = Brushes.Blue;
            btnLoad.Foreground = Brushes.Black;
            grdAreaTypeLegend.Visibility = System.Windows.Visibility.Visible;

            foreach (DNGridArea area in root.distnet.getAllObjListByObjType(EObjectType.网格))
            {
                //area.color = area.typeColor;
                area.color = (area.busiAccount as AcntGridArea).areaColor;
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            //foreach (pArea item in loadlayer.pModels.Values)
            //{
            //    item.areaColor = item.busiData.busiColor1;
            //}
            btnLoad.Foreground = Brushes.Blue;
            btnOrg.Foreground = Brushes.Black;
            grdAreaTypeLegend.Visibility = System.Windows.Visibility.Collapsed;
            foreach (DNGridArea area in root.distnet.getAllObjListByObjType(EObjectType.网格))
            {
                area.color = area.busiData.busiColor1;
            }
        }
     
   

        public class ColorDesc
        {
            public string name { get; set; }
            public Brush brush { get; set; }
        }
    }
}
