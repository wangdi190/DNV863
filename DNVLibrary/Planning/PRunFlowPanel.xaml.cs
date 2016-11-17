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
    /// PFlowPanel.xaml 的交互逻辑
    /// </summary>
    public partial class PRunFlowPanel : UserControl, BaseIPanel
    {
        public PRunFlowPanel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
        }
        public void load()
        {

            root.earth.config.tooltipMoveEnable = true;

            cmbMode.SelectedIndex = 0;

            root.earth.VisualRangeChanged += new EventHandler(earth_VisualRangeChanged);


            //===附加表格
            grid = new PShareGrid { Width = root.grdContent.ActualWidth - this.Width, Height = 210, Margin = new System.Windows.Thickness(0, 0, 0, 24), VerticalAlignment = System.Windows.VerticalAlignment.Bottom, HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
            root.grdContent.Children.Add(grid);
        }

        EViewStatus viewstatus;
        enum EViewStatus { 初始态, 显示变电站, 显示变压器 };
        ///<summary>视图状态变化，改变列表显示内容</summary>
        void earth_VisualRangeChanged(object sender, EventArgs e)
        {
            double curdistance = root.earth.camera.curCameraDistanceToGround;
            EViewStatus newviewstatus = (curdistance < root.visualdistance) ? EViewStatus.显示变压器 : EViewStatus.显示变电站;
            if (viewstatus != newviewstatus)
            {
                viewstatus = newviewstatus;
                refreshTranformerlist();
            }
        }
        ///<summary>刷新变电数据源</summary>
        void refreshTranformerlist()
        {
            if (viewstatus == EViewStatus.显示变电站)
            {
                var lst2 = (from e0 in root.earth.objManager.zLayers.Values
                            from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).objCategory == EObjectCategory.变电设施类 && p.busiRunData != null)
                            orderby (e1.busiRunData as RunDataTransformFacilityBase).rateOfLoad descending
                            select e1).Take(50);
                lstStation.ItemsSource = lst2;
            }
            else if (viewstatus == EViewStatus.显示变压器)
            {
                var lst2 = (from e0 in root.earth.objManager.zLayers.Values
                            from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).objCategory == EObjectCategory.变压器类 && p.busiRunData != null)
                            orderby (e1.busiRunData as RunDataTransformerBase).rateOfLoad descending
                            select e1).Take(50);
                lstStation.ItemsSource = lst2;
            }

        }


        public void unload()
        {
            root.distnet.clearFlow();
            root.distnet.clearLoadCol();
            root.distnet.clearVLContour();

            root.earth.config.tooltipMoveEnable = false;


            ////关闭潮流，清理tooltip
            //foreach (pPowerLine item in root.earth.objManager.getObjList("线路"))
            //{
            //    item.isFlow = false;
            //    item.lineColor = Colors.Cyan;
            //    item.tooltipMoveTemplate = null;
            //    item.tooltipMoveContent = null;
            //}

            ////清除附加子对象
            //foreach (pLayer layer in root.earth.objManager.zLayers.Values)
            //{
            //    foreach (PowerBasicObject obj in layer.pModels.Values)
            //    {
            //        obj.submodels.Clear();
            //    }
            //}

            root.earth.UpdateModel();

            root.grdContent.Children.Remove(grid);
        }


        Random rd = new Random();
        UCDNV863 root;

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            lstLine.MouseDoubleClick += new MouseButtonEventHandler(lst_MouseDoubleClick);
            lstStation.MouseDoubleClick += new MouseButtonEventHandler(lst_MouseDoubleClick);



            cmbMode.Items.Add("典型运行方式");
            cmbMode.Items.Add("最大运行方式");
            cmbMode.Items.Add("最小运行方式");




        }


        private void cmbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            datachange(true);
        }

        void lst_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListBox).SelectedItem != null)
            {
                PowerBasicObject selobj = (sender as ListBox).SelectedItem as WpfEarthLibrary.PowerBasicObject;
                root.earth.camera.aniLook(selobj.VecLocation, 5);  //动画定位
                //改变外观
                if (selobj is pPowerLine)
                {
                    (selobj as pPowerLine).aniTwinkle.doCount = 20;
                    (selobj as pPowerLine).AnimationBegin(pPowerLine.EAnimationType.闪烁);
                }
                else if (selobj is pSymbolObject)
                {
                    (selobj as pSymbolObject).aniTwinkle.doCount = 20;
                    (selobj as pSymbolObject).AnimationBegin(pSymbolObject.EAnimationType.闪烁);
                }


                propObj.SelectedObject = selobj.busiAccount;
            }



        }


        bool isFlowShow, isLoadShow, isVLShow;

        ///<summary>显示或隐藏潮流</summary>
        internal void ShowFlow(bool isshow)
        {
            isFlowShow = isshow;
            datachange(false);
        }
        ///<summary>显示或隐藏节点负载</summary>
        internal void ShowLoad(bool isshow)
        {
            isLoadShow = isshow;
            datachange(false);
        }
        ///<summary>显示或隐藏电压等值图</summary>
        internal void ShowVL(bool isshow)
        {
            isVLShow = isshow;
            datachange(false);
            //showcontour(isVLShow);
            //showNodeVL(isVLShow);
        }


        ///<summary>路由命令，规划日期改变</summary>
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            datachange(true);
        }

        bool isloadinited; //是否节点数据已初始化
        bool isflowinited; //是否线路负载已初始化

        ///<summary>isChangeDate为false时，不更新数据，仅控制显示或隐藏</summary>
        void datachange(bool isChangeData)
        {

            root.distnet.clearFlow();
            if (isFlowShow)
                root.distnet.showFlow();
            //else
            //    root.distnet.clearFlow();


            //var lst = (from e0 in root.earth.objManager.zLayers.Values
            //           from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).objCategory == EObjectCategory.导线类)
            //           orderby e1.busiData.busiPercentValue descending
            //           select e1).Take(50);
            var lst = (from e0 in root.earth.objManager.zLayers.Values
                       from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).objCategory == EObjectCategory.导线类 && p.busiRunData != null)
                       orderby (e1.busiRunData as RunDataACLineBase).rateOfLoad descending
                       select e1).Take(50);
            lstLine.ItemsSource = lst;


            //---- 变电站


            if (isLoadShow)
                root.distnet.showLoadCol();
            else
                root.distnet.clearLoadCol();

            if (isVLShow)
                root.distnet.showVLContour();
            else
                root.distnet.hideVLContour();
            //changeContour();
            //showNodeVL(isVLShow);

            root.earth.UpdateModel();

            earth_VisualRangeChanged(null, null);
            refreshTranformerlist();
            
        }

        #region ============ 等高线 ================
        pLayer containerLayer;
        List<ContourGraph.ValueDot> dots;
        ContourGraph.Contour con;
        pContour gcon;
        Dictionary<string, pSymbolObject> objs = new Dictionary<string, pSymbolObject>();
        void showcontour(bool isShow)
        {
            if (isShow)
            {
                if (!root.earth.objManager.zLayers.TryGetValue("等高图层", out containerLayer))
                {
                    root.earth.objManager.AddLayer("等高图层", "等高图层", "等高图层");
                    containerLayer = root.earth.objManager.zLayers["等高图层"];
                    containerLayer.deepOrder = -1;


                    //导入和重新计算点位置
                    pSymbolObject ps;
                    dots = new List<ContourGraph.ValueDot>();

                    IEnumerable<PowerBasicObject> tmpobjs = root.earth.objManager.getAllObjListBelongtoCategory("变压器");
                    foreach (PowerBasicObject obj in tmpobjs)//.Where(p => p.busiRunData != null))
                    {
                        ps = obj as pSymbolObject;
                        double tmpvalue = 0.85 + 0.3 * rd.NextDouble();//(ps.busiRunData as RunDataNodeP).voltPUV;
                        dots.Add(new ContourGraph.ValueDot() { id = obj.id, location = Point.Parse(ps.location), value = tmpvalue });
                        objs.Add(ps.id, ps);
                        obj.busiData.busiValue1 = tmpvalue; //存储电压标幺值 , busiValue已被变电站负载占用
                        obj.busiData.busiValue2 = (tmpvalue - 0.85) / 0.3 * 100;  //存储模板用位置信息，避免写转换
                        //更复杂应用应使用busiData自定义类来处理
                        obj.busiData.busiStr2 = tmpvalue.ToString("f2");
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
                    con.gridXCount = 300;
                    con.gridYCount = 300;
                    con.Span = 30;
                    con.maxvalue = 1.15;
                    con.minvalue = 0.85;
                    con.dataFillValue = 1;
                    con.dataFillMode = ContourGraph.Contour.EFillMode.单点包络填充;
                    con.dataFillDictance = 100;
                    con.dataFillSpan = 10;
                    con.isDrawGrid = false;
                    con.isDrawLine = false;
                    con.isFillLine = true;
                    //con.isShowData = true;

                    //计算
                    //con.GenContour();
                    //创建图形
                    gcon = new pContour(containerLayer) { id = "等值图" };// { minJD = minx, maxJD = maxx, minWD = miny, maxWD = maxy };
                    gcon.setRange(minx, maxx, miny, maxy);
                    gcon.brush = con.ContourBrush;
                    containerLayer.AddObject("等值线", gcon);

                    //contourtimer.Start();  //timer模拟刷新

                    con.GenCompleted += new EventHandler(con_GenCompleted);
                    con.GenContourAsync(); //异步开始生成
                }

                containerLayer.logicVisibility = true;
            }
            else
            {
                if (root.earth.objManager.zLayers.TryGetValue("等高图层", out containerLayer))
                {
                    containerLayer.logicVisibility = false;
                }
                //contourtimer.Stop();

            }
            root.earth.UpdateModel();
        }



        void con_GenCompleted(object sender, EventArgs e) //异步完成
        {
            gcon.brush = con.ContourBrush;
        }

        void changeContour()
        {
            //if (!(bool)chkContour.IsChecked) return;
            if (!isVLShow) return;

            foreach (var item in dots)
            {
                if (item.id != null)
                {
                    item.value = 0.85 + 0.3 * rd.NextDouble();
                }

            }
            con.ReGenContourAsync();
        }


        #region 节点电压，扩散着色
        void showNodeVL(bool isShow)
        {
            pSymbolObject pso, nobj;
            double topvalue = 0.15;
            double warningvalue = 0.1;
            IEnumerable<PowerBasicObject> tmpobjs = root.earth.objManager.getAllObjListBelongtoCategory("节点");
            if (isShow)
            {
                foreach (PowerBasicObject obj in tmpobjs.Where(p => p.busiRunData != null))
                {
                    pso = obj as pSymbolObject;
                    double v = (0.85 + 0.3 * rd.NextDouble()) - 1;//(pso.busiRunData as RunDataNodeP).voltPUV - 1;
                    Color color;
                    color = v < 0 ? Colors.Blue : Colors.Red;
                    v = Math.Abs(v);
                    if (v > warningvalue)
                    {
                        v = v > topvalue ? topvalue : v;
                        v = v / topvalue;
                        color = MyClassLibrary.Share2D.MediaHelper.getColorSaturation(v, color);

                        if (pso.submodels.Count == 0)
                        {
                            nobj = new pSymbolObject(pso.parent) { id = pso.id + "node", location = pso.location, symbolid = "渐变圆", isH = true, groundHeight = WpfEarthLibrary.Para.SymbolHeight * 0.9 };
                            pso.submodels.Add(nobj.id, nobj);
                        }
                        else
                            nobj = pso.submodels.Values.First() as pSymbolObject;
                        nobj.color = color;
                        nobj.scaleX = 0.0007f;
                        nobj.scaleY = nobj.scaleZ = 0.001f;
                    }
                    else
                    {
                        pso.submodels.Clear();
                    }

                }

            }
            else
            {
                foreach (PowerBasicObject obj in tmpobjs.Where(p => p.busiRunData != null))
                {
                    obj.submodels.Clear();
                }
            }
        }

        #endregion


        //private void chkContour_Checked(object sender, RoutedEventArgs e)
        //{
        //    showcontour(true);
        //}

        //private void chkContour_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    showcontour(false);
        //}
        #endregion


        #region 规划运行之tooltip数据定义
        public class TooltipLineData
        {
            public string name { get; set; }

            public List<TooltipLineItemData> items { get; set; }
        }

        public class TooltipLineItemData
        {
            public string name { get; set; }

            public Color color { get; set; }

            public double len { get; set; }

            public double len2 { get { return 100 - len; } }

            public string strvalue { get; set; }

            public Brush textbrush { get; set; }
        }

        #endregion


        #region ===== 结论表格相关 =====
        PShareGrid grid;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button).Content.ToString();
            grid.ButtonClick(tag);
            uncheckall();
            if (grid.curtag != null)
                (sender as MyClassLibrary.MyButton).IsChecked = true;

        }

        void uncheckall()
        {
            foreach (MyClassLibrary.MyButton item in panelbutton.Children)
            {
                item.IsChecked = false;
            }
        }

        #endregion
  

    }






}
