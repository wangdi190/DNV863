using System;
using System.Windows;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;

namespace DNVLibrary.Planning
{
    /// <summary>
    /// 无面板附加负荷等值线
    /// </summary>
    class PRunLoadContour : AppBase
    {
        public PRunLoadContour(UCDNV863 Root)
            : base(Root)
        {
        }


        internal override void load()
        {
            createLoadContour();

            //状态栏提示的程序域保存
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.push();
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "规划_模拟运行_负荷等值图";
        }

        internal override void unload()
        {
            con.GenCompleted -= new EventHandler(con_GenCompleted);

            if (root.earth.objManager.zLayers.Keys.Contains("负荷等值图层"))
            {
                root.earth.objManager.zLayers.Remove("负荷等值图层");
                containerLayer.pModels.Clear();
            }
            dots.Clear();

            root.earth.UpdateModel();
            //状态栏提示恢复
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.pop();
        }

        internal void showhide(bool isshow)
        {
            if (root.earth.objManager.zLayers.Keys.Contains("负荷等值图层"))
            {
                root.earth.objManager.zLayers["负荷等值图层"].logicVisibility = isshow;
                root.earth.UpdateModel();
            }
        }

        //=======================================================================================================================
        pLayer containerLayer;
        List<ContourGraph.ValueDot> dots;
        ContourGraph.Contour con;
        pContour gcon;

        ///<summary>创建负荷等值图</summary>
        void createLoadContour()
        {
            createDots();
            initContour();
        }

        Random rd = new Random();
        ///<summary>初始化区块，并从区块创建等值线用dots</summary>
        void createDots()
        {
            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select bh,center,cast(value as float) value from map_share_object t1 join map_data t2 on t1.bh=t2.objid where t1.prjid=11 and layername='小区图层' and sort='负荷预测' and header='负荷密度'");

            //dots = new List<ContourGraph.ValueDot>();
            //foreach (DataRow dr in dt.Rows)
            //{
            //    dots.Add(new ContourGraph.ValueDot() { id = dr.Field<string>("bh"), location =System.Windows.Point.Parse(dr.Field<string>("center")), value = dr.Field<double>("value") });
            //}
            dots = new List<ContourGraph.ValueDot>();
            foreach (pArea item in root.distnet.getAllObjListByObjType(DistNetLibrary.EObjectType.网格))
            {
                dots.Add(new ContourGraph.ValueDot() { id = item.id, location = item.center, value = 30 + rd.Next(200) });
            }

        }

        void initContour()
        {
            if (!root.earth.objManager.zLayers.TryGetValue("负荷等值图层", out containerLayer))
            {
                root.earth.objManager.AddLayer("负荷等值图层", "负荷等值图层", "负荷等值图层");
                containerLayer = root.earth.objManager.zLayers["负荷等值图层"];
            }
            //=========
            double minx, miny, maxx, maxy;
            miny = dots.Min(p => p.location.X); maxy = dots.Max(p => p.location.X);  //将经度换为X坐标, 纬度换为Y坐标
            minx = dots.Min(p => p.location.Y); maxx = dots.Max(p => p.location.Y);
            double w = maxx - minx; double h = maxy - miny;
            minx = minx - w * 0.2; maxx = maxx + w * 0.2;
            miny = miny - h * 0.2; maxy = maxy + h * 0.2;
            w = maxx - minx; h = maxy - miny;
            //经纬换为屏幕坐标
            int size = 1024;
            foreach (ContourGraph.ValueDot dot in dots)
            {
                dot.location = new Point((dot.location.Y - minx) / w * size, (maxy - dot.location.X) / h * size);  //重新赋与新的平面点位置, 注，纬度取反，仅适用北半球
            }
            double maxvalue = dots.Max(p => p.value);

            //设置计算参数
            con = new ContourGraph.Contour();
            con.dots = dots;
            con.opacityType = ContourGraph.Contour.EOpacityType.正坡形;
            con.opacityRange = 0.9;
            con.minOpacity = 0.3;
            con.maxOpacity = 1;
            con.canvSize = new Size(size, size);
            con.gridXCount = 200;
            con.gridYCount = 200;
            con.Span = 30;
            con.maxvalue = maxvalue * 0.9;
            con.minvalue = 0;
            con.dataFillValue = 0;
            con.dataFillMode = ContourGraph.Contour.EFillMode.八角点包络填充;
            con.isDrawGrid = false;
            con.isDrawLine = false;
            con.isFillLine = true;

            gcon = new pContour(containerLayer) { id = "负荷等值图" };// { minJD = minx, maxJD = maxx, minWD = miny, maxWD = maxy };
            gcon.setRange(minx, maxx, miny, maxy);
            gcon.brush = con.ContourBrush;
            containerLayer.AddObject("负荷等值图", gcon);

            con.GenCompleted += new EventHandler(con_GenCompleted);
            con.GenContourAsync(); //异步开始生成

            root.earth.UpdateModel();
        }

        ///<summary>负荷变化</summary>
        void changeData()
        {
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select bh,cast(value as float) value from map_share_object t1 join map_data t2 on t1.bh=t2.objid where t1.prjid=11 and layername='小区图层' and sort='负荷预测' and header='负荷'");

            foreach (DataRow dr in dt.Rows)
            {
                string id = dr.Field<string>("bh");
                float value = dr.Field<float>("value");
                ContourGraph.ValueDot dot = dots.First(p => p.id == id);
                if (dot != null)
                    dot.value = value;
            }

            con.ReGenContourAsync();
        }

        void con_GenCompleted(object sender, EventArgs e) //异步完成
        {
            gcon.brush = con.ContourBrush;
        }



    }

}
