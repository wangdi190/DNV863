using System;
using System.IO;
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
using System.Data;
using System.Text.RegularExpressions;
using WpfEarthLibrary;


namespace DNV863
{
    /// <summary>
    /// 以北京经研院项目数据为示例数据
    /// 所有图形对象从数据库中读取
    /// 示例潮流、电压等高线、区域数据、对象数据
    /// </summary>
    public partial class UCSample1 : UserControl
    {
        public UCSample1()
        {
            InitializeComponent();
        }

        Earth uc;
        private void UserControl_Initialized(object sender, EventArgs e)
        {
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (uc==null)
            {
                uc = new Earth();

                uc.config.tooltipClickEnable = true;
                uc.config.tooltipMoveEnable = true;

                grdContent.Children.Add(uc);

                uc.earthManager.earthpara.ArrowSpan = 1.5f; //控制潮流箭头密度

                loadModel();
                uc.camera.XRotationScale = 0.8;

            }
        }




        #region ======================= 基础网架载入 ========================
        //------------------------------------------------------------------------------------------
        // 说明：从数据库中载入北京主城区配网数据为示例，经纬度坐标
        //------------------------------------------------------------------------------------------


        Random rd = new Random();

        protected DataTable dtsymbol, dtsymbolitem, dttext, dtlayer, dtstyle, dtshareobject; //dtsymbol为symbol和symbolitem的合集
        protected DataTable dtproject, dtallproject, dtdata;
        protected DataTable dtdictype, dtdicproperty, dtobject, dtproperty;
        protected DataRow curprj = null;
        int curprjkey;
        void loadModel()
        {

            //读取数据
            curprjkey = 13;
            dtallproject = DataLayer.DataProvider.getDataTableFromSQL("select * from map_project");
            dtproject = DataLayer.DataProvider.getDataTableFromSQL(string.Format("select * from map_project where id={0}", curprjkey));
            curprj = dtproject.Rows[0];
            string sqlwhere = PrjHelper.genProjectsExpress(curprjkey, dtallproject);
            string sqlwhere2 = PrjHelper.genProjectsExpress(curprjkey, dtallproject, "t1.");
            dtobject = DataLayer.DataProvider.getDataTableFromSQL(string.Format("select t1.*,t2.layer from map_object t1 join map_svg_layer t2 on t1.layerid=t2.id where points is not null and {0}", sqlwhere2));
            dtlayer = DataLayer.DataProvider.getDataTableFromSQL(string.Format("select * from map_svg_layer where {0}", sqlwhere));
            dttext = DataLayer.DataProvider.getDataTableFromSQL(string.Format("select t1.*,t2.layer from map_svg_text t1 join map_svg_layer t2 on t1.layerid=t2.id where points is not null and {0}", sqlwhere2));
            dtsymbol = DataLayer.DataProvider.getDataTableFromSQL(string.Format("select t2.svgsymbolid,t2.name,t2.viewbox,t2.preserveAspectRatio,t1.shapetype,t1.data,t1.fill,t1.stroke,t1.width from map_svg_symbolitem t1 join map_svg_symbol t2 on t1.symbolid=t2.id where t2.svgsymbolid in (select distinct replace(symbolid,'#','') from map_object where points is not null and ({0})) and ({0})", sqlwhere)); //仅使用了的图元

            dtstyle = DataLayer.DataProvider.getDataTableFromSQL(string.Format("select * from map_svg_style where {0}", sqlwhere));
            int KGID = 11;
            dtshareobject = DataLayer.DataProvider.getDataTableFromSQL(string.Format("select * from map_share_object where prjid={0} or layername='区县图层'", KGID));

            //生成图元字典，图元字典内容会被传送到d3d生成公用材质，下面的厂站，将使用图元字典中的键值来决定在d3d中的材质
            genSymbolBrush();
            //另外直接添加材质, SubstationEntityDisH2为键值
            //uc.objManager.AddSymbol("","SubstationEntityDisH2", "SubstationEntityDisH2.dds");


            //生成几何体资源字典，几何体资源字典内容会被传送到d3d生成公用几何体数据，下面的数据呈现用的柱体，将使用几何体字典中的键值来决定在d3d中的形态
            genGeomeries();


            //添加对象层
            foreach (DataRow item in dtlayer.Rows)
                uc.objManager.AddLayer(item["layer"].ToString(), item["id"].ToString(), item["layer"].ToString()); //示例中以层的名字为键值


            int idx = 0;
            //对象
            bool isfind;
            PowerBasicObject obj;
            pLayer containerLayer;



            foreach (DataRow item in dtobject.AsEnumerable().OrderBy(p => p.Field<int>("prjid")))
            {
                obj = null;
                if (item["shapetype"].ToString() == "dot")
                {
                    isfind = uc.objManager.zLayers.TryGetValue(item["layer"].ToString(), out containerLayer);  //查找是否有对象所属层
                    if (isfind)
                    {
                        isfind = containerLayer.pModels.TryGetValue(item["id"].ToString(), out obj);
                        if (!isfind)
                        {
                            obj = new pSymbolObject(containerLayer)
                            {
                                id = item["id"].ToString(),
                                name = item["objname"].ToString(),
                                location = item["points"].ToString(),
                                symbolid = item["symbolid"].ToString().Replace("#", ""),  //材质Key
                                isH = true,
                                color=Colors.Lime
                            };

                            if ((obj as pSymbolObject).symbolid == "SwitchStationOpen")
                            {
                                if (obj.busiData == null) obj.busiData = new busiBase(obj);
                                obj.busiData.busiSort = "开关站";
                            }
                            else if ((obj as pSymbolObject).symbolid == "SubstationEntityDisH")
                            {
                                if (obj.busiData == null) obj.busiData = new busiBase(obj);
                                obj.busiData.busiSort = "变电站";

                                //测试 tooltip
                                //mousemove无自定义模板示例
                                {
                                    obj.tooltipMoveContent = obj.name;
                                }
                                //mouseclick自定义模板示例
                                {
                                    List<MyClassLibrary.DevShare.ChartDataPoint> ds = new List<MyClassLibrary.DevShare.ChartDataPoint>();
                                    ds.Add(new MyClassLibrary.DevShare.ChartDataPoint("夏季高峰", "", 100 + rd.Next(1000)));
                                    ds.Add(new MyClassLibrary.DevShare.ChartDataPoint("夏季低谷", "", 100 + rd.Next(700)));
                                    ds.Add(new MyClassLibrary.DevShare.ChartDataPoint("冬季高峰", "", 100 + rd.Next(800)));
                                    ds.Add(new MyClassLibrary.DevShare.ChartDataPoint("冬季低谷", "", 100 + rd.Next(500)));
                                    obj.tooltipClickContent = ds;
                                    obj.tooltipClickTemplate = "transformerModeTemplate";
                                }

                            }
                            else if ((obj as pSymbolObject).symbolid == "Pole")
                            {
                                if (obj.busiData == null) obj.busiData = new busiBase(obj);
                                obj.busiData.busiSort = "杆塔";
                            }

                            //(obj as pSymbolObject).aniTwinkle.isDoAni = true; 


                            Regex regex = new Regex("translate\\(.*\\) ?rotate\\(.*\\) ?scale\\((\\d*.?\\d*), ?(\\d*.?\\d*)\\)", RegexOptions.Multiline);
                            Match m = regex.Match(item["data"].ToString());
                            if (m.Success)
                            {
                                (obj as pSymbolObject).scaleX =(float)(uc.objManager.zSymbols[(obj as pSymbolObject).symbolid].sizeX * Math.Pow(double.Parse(m.Groups[1].Value), 0.7) / 400);
                                (obj as pSymbolObject).scaleY =(float)( uc.objManager.zSymbols[(obj as pSymbolObject).symbolid].sizeY * Math.Pow(double.Parse(m.Groups[1].Value), 0.7) / 400);
                            }
                            else
                                (obj as pSymbolObject).scaleX = (obj as pSymbolObject).scaleY = 0.025f;


                            containerLayer.AddObject(item["id"].ToString(), obj);//也可直接加如：containerLayer.pModels.Add(item["id"].ToString(), obj);


                        }
                    }
                    if (idx == 0)
                    {
                        obj = new pModel3D(containerLayer)  //测试实物模型用，暂无效
                        {
                            id = item["id"].ToString() + "3d",
                            name = item["objname"].ToString(),
                            location = item["points"].ToString(),
                            Model3DType = EPowerModel3DType.风电
                        };
                        containerLayer.AddObject(item["id"].ToString() + "3d", obj);
                        idx++;
                    }

                }
                else if (item["shapetype"].ToString() == "path")
                {
                    isfind = uc.objManager.zLayers.TryGetValue(item["layer"].ToString(), out containerLayer);
                    if (isfind)
                    {
                        isfind = containerLayer.pModels.TryGetValue(item["id"].ToString(), out obj);
                        if (!isfind)
                        {
                            obj = new pPowerLine(containerLayer)
                            {
                                id = item["id"].ToString(),
                                name = item["objname"].ToString(),
                                strPoints = item["points"].ToString(),
                                color = Color.FromRgb(0xFF, 0xCC, 0x00),
                                arrowColor = Colors.Blue,
                                isFlow = true,
                                thickness = 0.002f
                            };
                            //(obj as pPowerLine).aniDraw.aniType=EAniType.绘制; //注意：只有aniDraw属性可以是擦除或绘制两者之一，其它的动画属性大多没有可选性，请不要更改它们的动画类型
                            //(obj as pPowerLine).aniDraw.isDoAni = true; 
                            //(obj as pPowerLine).aniTwinkle.isDoAni = true; 

                            //zh注：对不合理的过近的相邻点进行处理，这部分应放入位置数据入库前
                            PointCollection pc = PointCollection.Parse(item["points"].ToString());
                            PointCollection newpc = new PointCollection();
                            newpc.Add(pc[0]);
                            for (int i = 1; i < pc.Count; i++)
                            {
                                if ((pc[i] - pc[i - 1]).Length > 0.00001)
                                {
                                    if (i < pc.Count - 1)
                                    {
                                        Vector v1 = pc[i] - pc[i - 1]; v1.Normalize();
                                        Vector v2 = pc[i + 1] - pc[i]; v2.Normalize();
                                        if (v1 != v2)
                                            newpc.Add(pc[i]);
                                    }
                                    else
                                        newpc.Add(pc[i]);
                                }
                            }
                            (obj as pPowerLine).strPoints = newpc.ToString();


                            if (newpc.Count > 1)
                                containerLayer.pModels.Add(item["id"].ToString(), obj);





                        }
                    }


                    idx++;
                }


            }



        }
        #endregion


        List<pData> datas = new List<pData>();
        bool isBuildData = false; //在此示例中用于控制数据对象不再新建
        ///<summary>显示表现数据的图形，随机以几何体和文字表现</summary>
        void showData(bool isShow)
        {
            if (isShow && !isBuildData)
            {
                isBuildData = true;
                pData obj; Data zd;
                foreach (pLayer layer in uc.objManager.zLayers.Values)
                {
                    foreach (PowerBasicObject pobj in layer.pModels.Values)
                    {
                        if (pobj is pSymbolObject && rd.NextDouble() > 0.5) //示例从图元取位置以生成数据几何体
                        {
                            if (pobj.submodels.Count == 0)
                            {
                                //创建数据几何体，几何体可包含多个数据堆叠在一起
                                obj = new pData(pobj.parent)
                                {
                                    id = pobj.id + "数据", //id加入数据避免id不唯一
                                    valueScale = 0.0002,  //值转高度的系数，高度=值*valueScale
                                    radScale = 0.02,  //底部尺寸
                                    location = (pobj as pSymbolObject).location, //经纬坐标
                                    isShowLabel = true,  //是否显示标签,
                                    //dataLabelColor=Colors.Red
                                };

                                zd = new Data() { argu = "负荷", id = pobj.id + "1", value = rd.NextDouble() * 1000, color = Colors.Gainsboro, geokey = "圆柱体"}; //id需唯一, geokey与先期添加在几何体资源字典中一至
                                (obj as pData).datas.Add(zd);
                                zd = new Data() { argu = "备用", id = pobj.id + "2", value = rd.NextDouble() * 1000, color = Color.FromArgb(0x98, 0xFF, 0xFF, 0xFF), geokey = "圆锥体" };
                                (obj as pData).datas.Add(zd);

                                datas.Add(obj);
                                pobj.AddSubObject("数据对象1", obj); //数据对象依附于图形对象
                            }
                        }
                        else if (pobj is pPowerLine && rd.NextDouble() > 0.9)//坐线路创建数据对象，示例文字模式
                        {
                            if (pobj.submodels.Count == 0)
                            {
                                obj = new pData(pobj.parent) { id = pobj.id + "数据", valueScale = 0.0002, labelScaleX = 0.8f, labelScaleY = 0.8f, location = (pobj as pPowerLine).location, isH = true, showType = pData.EShowType.文字 };
                                zd = new Data() { argu = pobj.name, id = pobj.id + "1", value = rd.NextDouble() * 1000, color = Colors.Red, unit = "WM" }; //id需唯一
                                (obj as pData).datas.Add(zd);
                                datas.Add(obj);
                                pobj.AddSubObject("数据对象", obj);
                                //数据对象依附于图形对象, 删除图形对象时将自动清除数据对象
                                //数据对象也可以单独放入一个层中，但需要自行管理数据对象删除
                            }
                        }
                        pobj.isShowSubObject = true;  //设置为显示数据对象

                    }
                }

                datatimer.Tick += new EventHandler(datatimer_Tick);
                datatimer.Start();
            }
            else
            {
                foreach (pLayer layer in uc.objManager.zLayers.Values)
                {
                    foreach (PowerBasicObject pobj in layer.pModels.Values)
                    {
                        pobj.isShowSubObject = isShow; //设置为不显示数据对象，数据对象并不删除
                    }
                }                
                datatimer.Stop();
            }

            uc.UpdateModel();
        }

        void datatimer_Tick(object sender, EventArgs e)
        {

            foreach (pLayer layer in uc.objManager.zLayers.Values)
            {
                foreach (PowerBasicObject pobj in layer.pModels.Values)
                {
                    foreach (pData pd in pobj.submodels.Values)
                    {
                        foreach (Data dd in pd.datas)
                        {
                            
                            double dv = rd.NextDouble() * 1000;

                            if (pobj is pPowerLine)  //根据值， 改变线路色彩示例
                            {
                                if (dv > 500)
                                    (pobj as pPowerLine).color=Colors.Red;
                                else
                                    (pobj as pPowerLine).color = Colors.Blue;
                            }
                            else if (pobj is pSymbolObject)
                            {
                                if (dd.argu=="负荷")
                                {
                                    if (dv > 500)
                                        dd.color=Color.FromRgb(0xFF, 0x64, 0x64);
                                    else
                                        dd.color = Color.FromRgb(0x5E, 0x5E, 0xFF);
                                }
                            }

                            dd.value = dv; //色在前改，只有值变，更新事件
                            
                        }


                    }
                }
            }                


        }
        System.Windows.Threading.DispatcherTimer datatimer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };





        ///<summary>示例：显示区域</summary>
        void showArea(bool isShow)
        {
            //====================  载入区域测试  ======================
            //objManager.zLayers.Add("区县图层", new zLayer(objManager) { id = "区县图层", name = "区县图层" });  //zh注：按深度写，层加到前面，以后在层中加深度索引，立柱若有问题，再在对象中加深度索引
            pLayer containerLayer;
            PowerBasicObject obj;

            if (isShow)
            {
                if (!uc.objManager.zLayers.TryGetValue("区县图层", out containerLayer))
                {
                    uc.objManager.AddLayer("区县图层", "区县图层", "区县图层");
                    containerLayer = uc.objManager.zLayers["区县图层"];
                    containerLayer.deepOrder = -1;
                    foreach (DataRow item in dtshareobject.AsEnumerable().Where(p => p.Field<string>("layername") == "区县图层"))
                    {

                        obj = new pArea(containerLayer)
                        {
                            id = item["bh"].ToString(),
                            name = item["objname"].ToString(),
                            strPoints = item["points"].ToString(),
                            color = Color.FromArgb(0x01, 0x00, 0x46, 0x5A)
                            //areaColor = (Color)ColorConverter.ConvertFromString(item["fill"].ToString())
                        };

                        //处理颜色
                        Color tmpcolor = Colors.AliceBlue;
                        string[] ss = (item["fill"].ToString()).Split('|');
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        foreach (string se in ss)
                        {
                            string sn = se.Substring(0, se.IndexOf('='));
                            string sv = se.Substring(se.IndexOf('=') + 1);
                            dic.Add(sn, sv);
                        }
                        if (dic["type"] == "SolidColorBrush")
                        {
                            if (dic["string"].Contains('#'))
                                tmpcolor = (Color)ColorConverter.ConvertFromString(dic["string"]);//  (SolidColorBrush)(new System.Windows.Media.BrushConverter().ConvertFromString(dic["string"]));
                            else
                            {
                                string[] scolor = dic["string"].Split(',');
                                tmpcolor = Color.FromRgb(byte.Parse(scolor[0]), byte.Parse(scolor[1]), byte.Parse(scolor[2]));
                            }
                        }
                        tmpcolor.A = 80;
                        (obj as pArea).color = tmpcolor;

                        containerLayer.pModels.Add(item["bh"].ToString(), obj);
                        //break;
                    }
                }
                containerLayer.logicVisibility = true;
            }
            else
            {
                if (uc.objManager.zLayers.TryGetValue("区县图层", out containerLayer))
                {
                    containerLayer.logicVisibility = false;
                }
            }

            uc.UpdateModel();
        }

        ///<summary>示例：显示潮流</summary>
        void showFlow(bool isShow)
        {
            if (uc != null)
                foreach (pLayer layer in uc.objManager.zLayers.Values)
                {
                    foreach (PowerBasicObject obj in layer.pModels.Values)
                    {
                        if (obj is pPowerLine)
                            (obj as pPowerLine).isFlow = isShow;
                    }
                }

        }



        List<ContourGraph.ValueDot> dots;
        ContourGraph.Contour con;
        pContour gcon;
        ///<summary>示例：等高线</summary>
        void showContour(bool isShow)
        {
            pLayer containerLayer;

            if (isShow)
            {
                if (!uc.objManager.zLayers.TryGetValue("等高图层", out containerLayer))
                {
                    uc.objManager.AddLayer("等高图层", "等高图层", "等高图层");
                    containerLayer = uc.objManager.zLayers["等高图层"];
                    containerLayer.deepOrder = -1;


                    //导入和重新计算点位置
                    pSymbolObject ps;
                    dots = new List<ContourGraph.ValueDot>();
                    foreach (pLayer layer in uc.objManager.zLayers.Values)
                    {
                        foreach (PowerBasicObject obj in layer.pModels.Values)
                        {
                            if (obj is pSymbolObject)
                            {
                                ps = obj as pSymbolObject;
                                dots.Add(new ContourGraph.ValueDot() { location = Point.Parse(ps.location), value = rd.Next(2200) });
                            }
                        }
                    }
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

                    //设置计算参数
                    con = new ContourGraph.Contour();
                    con.dots = dots;
                    con.opacityType = ContourGraph.Contour.EOpacityType.倒梯形;
                    con.canvSize = new Size(size, size);
                    con.gridXCount = 200;
                    con.gridYCount = 200;
                    con.Span = 30;
                    con.maxvalue = 2000;
                    con.minvalue = 200;
                    con.dataFillValue = 1000;
                    con.dataFillMode = ContourGraph.Contour.EFillMode.八角点包络填充;
                    con.isDrawGrid = false;
                    con.isDrawLine = false;
                    con.isFillLine = true;



                    //计算
                    //con.GenContour();
                    //创建图形
                    gcon = new pContour(containerLayer) { id = "等值图" };// { minJD = minx, maxJD = maxx, minWD = miny, maxWD = maxy };
                    gcon.setRange(minx, maxx, miny, maxy);
                    gcon.brush = con.ContourBrush;
                    containerLayer.AddObject("等值线", gcon);

                    contourtimer.Tick += new EventHandler(contourtimer_Tick);
                    contourtimer.Start();

                    con.GenCompleted += new EventHandler(con_GenCompleted);
                    con.GenContourAsync();
                }
                containerLayer.logicVisibility = true;
            }
            else
            {
                if (uc.objManager.zLayers.TryGetValue("等高图层", out containerLayer))
                {
                    containerLayer.logicVisibility = false;
                }
                contourtimer.Stop();

            }
            uc.UpdateModel();
        }

        void con_GenCompleted(object sender, EventArgs e) //异步完成
        {
            gcon.brush =con.ContourBrush;
        }

        void contourtimer_Tick(object sender, EventArgs e)
        {
            foreach (var item in dots)
            {
                item.value = rd.Next(2200);
            }
            con.ReGenContourAsync();
            //con.ReGenContour();
            //gcon.brush = con.ContourBrush;

        }
        System.Windows.Threading.DispatcherTimer contourtimer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };




        ///<summary>生成几何体资源</summary>
        void genGeomeries()
        {
            uc.objManager.AddBoxResource("立方体", 1, 1, 1);
            uc.objManager.AddCylinderResource("圆柱体", 1, 1, 1, 16, 1);
            uc.objManager.AddCylinderResource("圆锥体", 0, 1, 1, 16, 1);
            uc.objManager.AddSphereResource("球体", 1, 16, 16);
        }


        ///<summary>生成图元Brush</summary>
        protected void genSymbolBrush()
        {
            SolidColorBrush defaultbrush = new SolidColorBrush(Colors.Lime);
            Brush brush;



            foreach (IGrouping<string, DataRow> g in dtsymbol.AsEnumerable().GroupBy(p => p.Field<string>("svgsymbolid")))
            {
                string sid = g.Key;
                string name = g.First().Field<string>("name");

                DrawingGroup drawgroup = new DrawingGroup();
                GeometryDrawing aDrawing;


                double sizeX = 10;
                double sizeY = 10;
                int idx = 0;
                foreach (DataRow item in dtsymbol.AsEnumerable().Where(p => p.Field<string>("svgsymbolid") == sid))
                {
                    if (idx == 0)//获取尺寸
                    {
                        Rect tmp = Rect.Parse(item["viewbox"].ToString());
                        sizeX = tmp.Width;
                        sizeY = tmp.Height;
                    }


                    string shapetype = item["shapetype"].ToString();
                    string data = item["data"].ToString();
                    if (shapetype == "circle")
                    {
                        Regex regex = new Regex("(\\d*.?\\d*,\\d*.?\\d*)\\|(\\d*.?\\d*)", RegexOptions.Multiline);
                        Match m = regex.Match(data);
                        if (m.Success)
                        {
                            Point pc = Point.Parse(m.Groups[1].Value);
                            double r = double.Parse(m.Groups[2].Value);

                            EllipseGeometry geo = new EllipseGeometry(pc, r, r);
                            aDrawing = new GeometryDrawing();
                            aDrawing.Geometry = geo;

                            Pen pen = new Pen();
                            double thickness = double.Parse(item["width"].ToString());
                            thickness = thickness < 2 ? 2 : thickness;
                            pen.Thickness = thickness;
                            brush = defaultbrush;//强制缺省用黄色//brush = anaBrush(item["fill"].ToString());
                            pen.Brush = brush;
                            aDrawing.Pen = pen;
                            drawgroup.Children.Add(aDrawing);
                        }
                    }
                    else if (shapetype == "path")
                    {
                        Geometry geo = PathGeometry.Parse(data);
                        aDrawing = new GeometryDrawing();
                        aDrawing.Geometry = geo;

                        brush = defaultbrush;//强制缺省用黄色//brush = anaBrush(item["fill"].ToString());
                        aDrawing.Brush = brush;
                        Pen pen = new Pen();
                        pen.Thickness = double.Parse(item["width"].ToString());
                        brush = defaultbrush;
                        pen.Brush = brush;
                        aDrawing.Pen = pen;
                        drawgroup.Children.Add(aDrawing);
                    }
                    idx++;
                }

                DrawingBrush myDrawingBrush = new DrawingBrush();
                myDrawingBrush.Drawing = drawgroup;


                pSymbol sym = new pSymbol() { id = sid, sizeX = sizeX, sizeY = sizeY, brush = myDrawingBrush, name = name };
                //可选以文件生成材质, 否则以brush生成材质
                //if (sid == "SubstationEntityDisH")
                //    sym.texturefile = "SubstationEntityDisH.dds";
                //if (sid == "SwitchStationOpen")
                //    sym.texturefile = "SwitchStationOpen.dds";
                //if (sid == "Pole")
                //    sym.texturefile = "Pole.dds";

                uc.objManager.zSymbols.Add(sid, sym);


            }

        }



        #region 界面控件
        //控制显示潮流
        private void chkFlow_Checked(object sender, RoutedEventArgs e)
        {
            showFlow(true);
        }

        private void chkFlow_Unchecked(object sender, RoutedEventArgs e)
        {
            showFlow(false);
        }

        //控制显示区域
        private void chkArea_Checked(object sender, RoutedEventArgs e)
        {
            showArea(true);
        }
        private void chkArea_Unchecked(object sender, RoutedEventArgs e)
        {
            showArea(false);
        }


        //控制显示等高线
        private void chkContour_Checked(object sender, RoutedEventArgs e)
        {
            showContour(true);
        }

        private void chkContour_Unchecked(object sender, RoutedEventArgs e)
        {
            showContour(false);
        }

        //控制显示地球表面
        private void chkEarth_Checked(object sender, RoutedEventArgs e)
        {
            if (uc != null)
                uc.earthManager.mapType = EMapType.卫星;
        }

        private void chkEarth_Unchecked(object sender, RoutedEventArgs e)
        {
            if (uc != null)
                uc.earthManager.mapType = EMapType.无;
        }

        private void chkData_Checked(object sender, RoutedEventArgs e)
        {
            showData(true);
        }

        private void chkData_Unchecked(object sender, RoutedEventArgs e)
        {
            showData(false);
        }

        #endregion


        #region 数据标签
        private void chkDataLabel_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in datas)
            {
                pData obj = item as pData;
                if (rd.NextDouble()>0.8)  //标签影响性能较大，只显示重要的，或告警的
                    obj.isShowLabel = true;

            }
            //注：数据标签涉及到文字标签模型的增删，所以应用updatemodel来刷新
            uc.UpdateModel();
        }

        private void chkDataLabel_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in datas)
            {
                pData obj = item as pData;
                obj.isShowLabel = false;

            }
            //注：数据标签涉及到文字标签模型的增删，所以应用updatemodel来刷新
            uc.UpdateModel();
        }

        #endregion



        //  测试载入后，动画控制
        List<PowerBasicObject> aniobj = new List<PowerBasicObject>();
        private void chkAni_Checked(object sender, RoutedEventArgs e)
        {
            #region  线路动画测试
            aniobj.Clear();
            if (uc != null)
                foreach (pLayer layer in uc.objManager.zLayers.Values)
                {
                    foreach (PowerBasicObject obj in layer.pModels.Values)
                    {
                        if (obj is pPowerLine && rd.NextDouble() < 0.1)
                        {
                            aniobj.Add(obj);
                            (obj as pPowerLine).color = Colors.Red;
                            (obj as pPowerLine).thickness = 0.005f;


                            //(obj as pPowerLine).aniTwinkle.duration=200;  //结构内参数可控制动画
                            (obj as pPowerLine).AnimationBegin(pPowerLine.EAnimationType.闪烁);
                        }
                    }
                }
            #endregion

            #region 图元动画测试
            //aniobj.Clear();
            //if (uc != null)
            //    foreach (pLayer layer in uc.objManager.zLayers.Values)
            //    {
            //        foreach (PowerBasicObject obj in layer.pModels.Values)
            //        {
            //            if (obj is pSymbolObject && rd.NextDouble() < 0.5)
            //            {
            //                if ((obj as pSymbolObject).symbolid == "SubstationEntityDisH")
            //                {
            //                    aniobj.Add(obj);
            //                    (obj as pSymbolObject).aniTwinkle.duration = 500;
            //                    //(obj as pSymbolObject).AnimationBegin(pSymbolObject.EAnimationType.闪烁);
            //                    //(obj as pSymbolObject).symbolid = "SubstationEntityDisH2"; //更换图元
                                
            //                    //(obj as pSymbolObject).AnimationBegin(pSymbolObject.EAnimationType.缩放);
            //                }
            //            }
            //        }
            //    }
            #endregion

        }

        private void chkAni_Unchecked(object sender, RoutedEventArgs e)
        {
            #region  线路闪烁测试清理
            foreach (var obj in aniobj)
            {
                (obj as pPowerLine).thickness = 0.002f;
                (obj as pPowerLine).color = Color.FromRgb(0xFF, 0xCC, 0x00);
                (obj as pPowerLine).AnimationStop(pPowerLine.EAnimationType.闪烁);
            }
            #endregion

            #region  图元闪烁测试清理
            //foreach (var obj in aniobj)
            //{
            //    (obj as pSymbolObject).AnimationStop(pSymbolObject.EAnimationType.闪烁);
            //    (obj as pSymbolObject).symbolid = "SubstationEntityDisH";
            //    (obj as pSymbolObject).AnimationStop(pSymbolObject.EAnimationType.缩放);
            //}
            #endregion

        }

        private void btnCamAni_Click(object sender, RoutedEventArgs e)
        {

            var tmp=from e0 in uc.objManager.zLayers.Values
                    from e1 in e0.pModels.Values
                    where e1 is pSymbolObject
                    select e1;
            pSymbolObject obj =(pSymbolObject)tmp.ElementAt(rd.Next(tmp.Count()));

            uc.camera.aniLook(obj.VecLocation);

        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            var tmp = uc.objManager.getObjList();

            foreach (PowerBasicObject item in tmp)
            {
                if (item is pSymbolObject)
                {
                    item.submodels.Clear();
                    if (rd.NextDouble()>0.8)
                    {
                        pData pd = new pData(item.parent) { id = item.id + "数据", valueScale = 0.0002, radScale=0.02, location = (item as pSymbolObject).location};
                        pd.datas.Add(new Data(){id=pd.id+"test", argu="test", value= rd.NextDouble() * 1000, color=Colors.Red, geokey="圆柱体"});
                        item.AddSubObject("数据对象", pd);

                        (item as pSymbolObject).isShowSubObject = true;
                    }


                }
            }


            uc.UpdateModel();
        }




    }
}
