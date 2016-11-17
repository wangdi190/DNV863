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
using System.Text.RegularExpressions;
using WpfEarthLibrary;

namespace DNV863
{
    /// <summary>
    /// UCSample4.xaml 的交互逻辑
    /// </summary>
    public partial class UCSample4 : UserControl
    {
        public UCSample4()
        {
            InitializeComponent();
        }

        Earth uc; //1.定义Earth对象
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (uc == null)
            {
                uc = new Earth(); //2 .创建Earth对象
                uc.config.isShowDebugInfo = true;  //在屏下方显示相机信息
                uc.config.isDynShow = true;  //根据相机视界动态显示图形对象
                grdMain.Children.Add(uc);//3.Earth对象添加入容器中
                uc.earthManager.earthpara.ArrowSpan = 1.5f; //控制潮流箭头密度
                loadModel();//4.装载模型对象
                uc.camera.XRotationScale = 0.8;

                uc.MouseMove += new MouseEventHandler(uc_MouseMove);  //tooltip示例
                tooltiptimer.Interval = TimeSpan.FromMilliseconds(200);
                tooltiptimer.Tick += new EventHandler(tooltiptimer_Tick);
            }     
        }



        #region ===================  tooltips 示例 ==============================
        //------------------------------------------------------------------------------------------
        // 说明：mouse移动不停的进行拾取测试将严重影响性能，所以只在mouse停下一段时间后才进行一次拾取测试，若单击双击，可直接调用拾取方法
        //------------------------------------------------------------------------------------------
        System.Windows.Threading.DispatcherTimer tooltiptimer = new System.Windows.Threading.DispatcherTimer();
        Point mouseposition;
        void tooltiptimer_Tick(object sender, EventArgs e)
        {
            //测试拾取
            WpfEarthLibrary.PowerBasicObject obj = uc.objManager.pick(mouseposition);  // 拾取方法，返回拾取到的对象
            if (obj != null && (obj is pData || obj is pPowerLine || obj is pSymbolObject))
            {

                string txt = "";
                if (obj is pData)
                    txt = "拾取" + (obj as pData).id + "\r\n" + (obj as pData).dataLabel;
                else if (obj is pPowerLine)
                    txt = (obj as pPowerLine).name;
                else if (obj is pSymbolObject)
                    txt =string.Format("{0}:{1},{2},{3}", obj.name,obj.VecLocation.x,obj.VecLocation.y,obj.VecLocation.z);

                txtToolTips.Text = txt;


                double ToolTipOffset = 5;
                Tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
                Tooltip.PlacementTarget = uc;
                Tooltip.HorizontalOffset = mouseposition.X + ToolTipOffset + 5;
                Tooltip.VerticalOffset = mouseposition.Y + ToolTipOffset;
                Tooltip.IsOpen = true;

            }
            else
                Tooltip.IsOpen = false;
            tooltiptimer.Stop();
        }





        void uc_MouseMove(object sender, MouseEventArgs e)
        {
            Tooltip.IsOpen = false;
            mouseposition = e.GetPosition(uc);
            if (tooltiptimer.IsEnabled)
                tooltiptimer.Stop();
            tooltiptimer.Start();

        }

        #endregion


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
            genSymbolBrush();//4.1 创建图元所使用的纹理资源
            //另外直接添加材质, SubstationEntityDisH2为键值
            //uc.objManager.AddSymbol("","SubstationEntityDisH2", "SubstationEntityDisH2.dds");


            //生成几何体资源字典，几何体资源字典内容会被传送到d3d生成公用几何体数据，下面的数据呈现用的柱体，将使用几何体字典中的键值来决定在d3d中的形态
            genGeomeries();//4.2 创建几何体Mesh资源


            //添加对象层
            foreach (DataRow item in dtlayer.Rows) //4.3 循环创建层
                uc.objManager.AddLayer(item["layer"].ToString(), item["id"].ToString(), item["layer"].ToString()); //示例中以层的名字为键值


            int idx = 0;
            //对象
            bool isfind;
            PowerBasicObject obj;
            pLayer containerLayer;



            foreach (DataRow item in dtobject.AsEnumerable().OrderBy(p => p.Field<int>("prjid"))) //4.4 循环创建模型对象
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
                                isH = true
                            };
                            if ((obj as pSymbolObject).symbolid == "SwitchStationOpen")
                            {
                                if (obj.busiData == null) obj.busiData = new busiBase(obj);
                                obj.busiData.busiSort = "开关站";
                                (obj as pSymbolObject).color = Colors.Aqua;
                                obj.visualMaxDistance = 10;
                            }
                            else if ((obj as pSymbolObject).symbolid == "SubstationEntityDisH")
                            {
                                if (obj.busiData == null) obj.busiData = new busiBase(obj);
                                obj.busiData.busiSort = "变电站";
                                (obj as pSymbolObject).color = Colors.Lime;
                            }
                            else if ((obj as pSymbolObject).symbolid == "Pole")
                            {
                                if (obj.busiData == null) obj.busiData = new busiBase(obj);
                                obj.busiData.busiSort = "杆塔";
                                (obj as pSymbolObject).color = Color.FromRgb(0xCC, 0xFF, 0xFF);
                                obj.visualMaxDistance = 5;
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

        
        #endregion




        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //uc.checkrange();
        }



    }
}
