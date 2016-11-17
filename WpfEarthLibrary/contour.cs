using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace ContourGraph
{
    /// <summary>
    /// 等值线绘图，使用方法：
    /// 1.设定参数(canvSize必须设定，其余可用缺省)
    /// 2.输入数据
    /// 3.调用GenContour
    /// 4.数据变化后可直接调用ReGenContour
    /// </summary>
    public class Contour
    {
        public Contour()
        {
            canv = new Canvas();

            bworker.DoWork += new DoWorkEventHandler(bworker_DoWork);
            bworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bworker_RunWorkerCompleted);

        }





        ///<summary>透明度处理模式</summary>
        public enum EOpacityType { 无, 正坡形, 逆坡形, 倒梯形 }
        ///<summary>填充模式</summary>
        public enum EFillMode {无, 八角点包络填充, 单点包络填充}
        #region 后台线程
        BackgroundWorker bworker = new BackgroundWorker();
        ///<summary>后台扫描和预计算</summary>
        void bworker_DoWork(object sender, DoWorkEventArgs e)
        {
            InsertValue();
            Trace();
        }

        void bworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            canv.Children.Clear();
            canv.Children.Add(new Rectangle() { Width = canvSize.Width, Height = canvSize.Height, StrokeThickness = 10, Stroke = Brushes.Transparent });


            Draw();
            DrawGrid();
            ContourBrush = new VisualBrush();
            ContourBrush.Visual = canv;

            canv.Measure(canvSize);
            canv.Arrange(new System.Windows.Rect(0, 0, canvSize.Width, canvSize.Height));

            RaiseGenCompletedEvent();
        }
        /// <summary>
        /// 异步方式生成全新等值图，完成后触发GenCompleted事件
        /// </summary>
        public void GenContourAsync()
        {
            canv.Width = canvSize.Width;
            canv.Height = canvSize.Height;
            isPreHandled = false;
            ReGenContourAsync();
        }
        /// <summary>
        /// 异步方式生成仅数据改变的等值图，完成后触发GenCompleted事件
        /// </summary>
        public void ReGenContourAsync()
        {
            //canv.Children.Clear();
            //canv.Children.Add(new Rectangle() { Width = canvSize.Width, Height = canvSize.Height, StrokeThickness = 10, Stroke = Brushes.Transparent });
            if (!bworker.IsBusy)
                bworker.RunWorkerAsync();
        }
        public event EventHandler GenCompleted;
        protected virtual void RaiseGenCompletedEvent()
        {

            if (GenCompleted != null)
                GenCompleted(this, null);
        }



        #endregion

        #region 公开参数
        /// <summary>
        /// 网格横向切分数，数字越大，横向越平滑，缺省100
        /// </summary>
        [Description("网格横向切分数，数字越大，横向越平滑，缺省100")]
        public int gridXCount = 100;
        /// <summary>
        /// 网格纵向切分数，数字越大，纵向越平滑，缺省100
        /// </summary>
        public int gridYCount = 100;
        /// <summary>
        /// 画布大小，应与输入数据坐标相匹配, ，缺省1000*1000
        /// </summary>
        public Size canvSize = new Size(1000, 1000);
        /// <summary>
        /// 反距离幂，缺省为2
        /// </summary>
        public double pow = 2;//反距离幂
        /// <summary>
        /// 等值线层数，数字越大，等值线越密，缺省20
        /// </summary>
        public int Span = 20;  //点扩散层数

        /// <summary>
        /// 是否绘制网格，缺省false
        /// </summary>
        public bool isDrawGrid = false;


        /// <summary>
        /// 最大值，缺省为负无穷，若不填此值，系统以输入数据的最大值为绘制等高线的最大值
        /// </summary>
        public double maxvalue = double.NegativeInfinity;


        /// <summary>
        /// 最小值，缺省为0，若填写此值，系统以此数据绘制最小等值线
        /// </summary>
        public double minvalue = 0;


        /// <summary>
        /// 是否绘制等值线，缺省false
        /// </summary>
        public bool isDrawLine = false;

        /// <summary>
        /// 是否填充等值线，缺省true
        /// </summary>
        public bool isFillLine = true;

        /// <summary>
        /// 最小值填充色
        /// </summary>
        public Color minColor = Colors.Blue;

        /// <summary>
        /// 最大色填充色
        /// </summary>
        public Color maxColor = Colors.Red;

        /// <summary>
        /// 最小透明系数
        /// </summary>
        public double minOpacity = 0;

        /// <summary>
        /// 最大透明系数
        /// </summary>
        public double maxOpacity = 1;

        /// <summary>
        /// 透明度图样类型，如坡形：值越低透明度高；凹形：中间透明度高
        /// </summary>
        public EOpacityType opacityType = EOpacityType.无;

        /// <summary>
        /// 如当值为缺省值0.6时：
        /// 坡形：可视范围为0.4-1
        /// 倒坡形：可视范围为0-0.6
        /// 倒梯形:可视范围为0-0.3和0.7-1
        /// </summary>
        public double opacityRange = 0.6;

        ///<summary>面积判断阈值, 0-1，用于判断是否区域应取反</summary>
        public double areaThreshold = 0.5;


        ///<summary>空白区域预填充的值</summary>
        public double dataFillValue=0;

        ///<summary>空白区域数据预填充模式，缺省无。</summary>
        public EFillMode dataFillMode;

        ///<summary>空白区域数据预填充时，填充点与数据点的最小距离，缺省50</summary>
        public double dataFillDictance=50;

        ///<summary>空白区域数据预填充时，填充点与填充点的间距，缺省30</summary>
        public double dataFillSpan = 30;


        /// <summary>
        /// 数据导入，包含数据值和坐标
        /// </summary>
        public List<ValueDot> dots = new List<ValueDot>();




        public VisualBrush ContourBrush { get; set; }
        #endregion

        #region 内部参数
        List<ValueLine> vlines = new List<ValueLine>();
        GridNode[,] nodes;
        GridEdge[,] edgesH; //横边
        GridEdge[,] edgesV; //竖边
        GridCell[,] cells;
        List<GridEdge> edges = new List<GridEdge>();

        bool isPreHandled = false;

        public Canvas canv { get; set; }

        //=========下为方便调试用参数
        ///<summary>是否绘制网格节点数据</summary>
        public bool isShowGridData = false;
        ///<summary>是否绘制原始数据</summary>
        public bool isShowData = false; 
        bool isDebug = false;
        bool isStep = false;//是否单步绘制以便调试
        bool isRepeat = false; //是否重复上一次单步调试

        #endregion

        /// <summary>
        /// 生成等值线图形
        /// </summary>
        /// <returns>VisualBrush返回等值线图形</returns>
        public void GenContour()
        {
            canv.Width = canvSize.Width;
            canv.Height = canvSize.Height;
            isPreHandled = false;


            ReGenContour();
        }


        /// <summary>
        /// 变化数据值后，直接重新绘制等值线图，不再处理网格节点与数据点节关系，速度更快
        /// </summary>
        /// <returns>VisualBrush返回等值线图形</returns>
        public void ReGenContour()
        {

            canv.Children.Clear();
            canv.Children.Add(new Rectangle() { Width = canvSize.Width, Height = canvSize.Height, StrokeThickness = 10, Stroke = Brushes.Transparent });
            InsertValue();
            Trace();
            Draw();
            DrawGrid();
            ContourBrush = new VisualBrush();
            ContourBrush.Visual = canv;

            canv.Measure(canvSize);
            canv.Arrange(new System.Windows.Rect(0, 0, canvSize.Width, canvSize.Height));
            //RaiseGenCompletedEvent();
        }


        List<Point> pts = new List<Point>();
        //List<Point> tmpdots = new List<Point>();
        ///<summary>插值</summary>
        private void InsertValue()
        {
            if (!isPreHandled)
            {
                if (dataFillMode == EFillMode.八角点包络填充) 
                {
                    // ========== 求出包络图形，简单凸包，提高性能 ==========
                    //PointCollection pts = new PointCollection();
                    pts.Clear();
                    //tmpdots.Clear();

                    for (int i = 0; i < 8; i++)
                        pts.Add(dots[0].location);

                    double maxx = dots.Max(p => p.location.X);
                    double maxy = dots.Max(p => p.location.Y);

                    foreach (ValueDot dot in dots) //遍历找出八个角点
                    {
                        if (dot.location.Y < pts[0].Y)
                            pts[0] = dot.location;
                        if (dot.location.X < pts[2].X)
                            pts[2] = dot.location;
                        if (dot.location.Y > pts[4].Y)
                            pts[4] = dot.location;
                        if (dot.location.X > pts[6].X)
                            pts[6] = dot.location;
                        if (dot.location.X * dot.location.X + dot.location.Y * dot.location.Y < pts[1].X * pts[1].X + pts[1].Y * pts[1].Y)
                            pts[1] = dot.location;
                        if (dot.location.X * dot.location.X + dot.location.Y * dot.location.Y > pts[5].X * pts[5].X + pts[5].Y * pts[5].Y)
                            pts[5] = dot.location;
                        if (dot.location.X * dot.location.X + (maxy - dot.location.Y) * (maxy - dot.location.Y) < pts[3].X * pts[3].X + (maxy - pts[3].Y) * (maxy - pts[3].Y))
                            pts[3] = dot.location;
                        if ((maxx - dot.location.X) * (maxx - dot.location.X) + dot.location.Y * dot.location.Y < (maxx - pts[7].X) * (maxx - pts[7].X) + pts[7].Y * pts[7].Y)
                            pts[7] = dot.location;
                    }




                    PointCollection pcs = new PointCollection();
                    foreach (Point pnt in pts)
                        pcs.Add(pnt);

                    //求八角点多边形中心
                    Point center = MyClassLibrary.Share2D.MyGeometryHelper.getCenterFromPolygon(pcs);
                    for (int i = 0; i < 8; i++)
                    {
                        double scale = ((pts[i] - center).Length + dataFillDictance) / (pts[i] - center).Length;
                        ScaleTransform transform = new ScaleTransform(scale, scale, center.X, center.Y);
                        pts[i] = transform.Transform(pts[i]);
                    }
                    double len;
                    Vector vec;
                    for (int i = 0; i < 8; i++)
                    {
                        if (i == 7)
                        {
                            vec = (pts[0] - pts[i]);
                            len = vec.Length;
                            vec.Normalize();
                        }
                        else
                        {
                            vec = (pts[i + 1] - pts[i]);
                            len = vec.Length;
                            vec.Normalize();
                        }
                        dots.Add(new ValueDot() { value = dataFillValue, location = pts[i] });
                        int devcount = (int)(len / dataFillSpan);
                        if (devcount > 0)
                        {
                            double step = len / devcount;
                            for (int j = 0; j < devcount; j++)
                            {
                                dots.Add(new ValueDot() { value = dataFillValue, location = pts[i] + j * step * vec });
                                //tmpdots.Add(pts[i] + j * step * vec);
                            }
                        }

                    }

                }
                else if (dataFillMode == EFillMode.单点包络填充) 
                {
                    int devcount = (int)(dataFillDictance * 2 * Math.PI / dataFillSpan);
                    List<ValueDot> adddots = new List<ValueDot>();
                    List<ValueDot> tmpdots = new List<ValueDot>();
                    foreach (ValueDot dot in dots) //遍历值点
                    {
                        tmpdots.Clear();
                        foreach (ValueDot dot2 in dots) //遍历值点
                        {
                            if (dot.Equals(dot2)) //同一点
                                continue;
                            if ((dot.location - dot2.location).Length < 2 * dataFillDictance) //半径相交
                                tmpdots.Add(dot2);
                        }

                        for (int i = 0; i < devcount; i++) //圆周分段
                        {
                            double angle=Math.PI*2*i/devcount;
                            Vector vec=new Vector(dataFillDictance*Math.Cos(angle) ,dataFillDictance*Math.Sin(angle));
                            Point pt = dot.location + vec;
                            bool iscontain = false;
                            foreach (ValueDot tmpdot in tmpdots)
                            {
                                if ((tmpdot.location-pt).Length<dataFillDictance)
                                {
                                    iscontain = true;
                                    break;
                                }
                            }
                            if (!iscontain)
                                adddots.Add(new ValueDot() { value = dataFillValue, location = pt });

                        }

                    }
                    dots = dots.Union(adddots).ToList();



                }

                if (dataFillMode!= EFillMode.无)  //四边填充
                {
                    for (int i = 0; i < gridXCount + 1; i = i + 2)
                    {
                        for (int j = 0; j < gridYCount + 1; j = j + 2)
                        {
                            if (i == 0 || i == gridXCount || j == 0 || j == gridYCount)
                            {
                                Point location = new Point(canvSize.Width * i / gridXCount, canvSize.Height * j / gridYCount);
                                dots.Add(new ValueDot() { value = dataFillValue, location = location });
                            }
                        }
                    }
                }
            }

            //////////

            double value;
            if (!isPreHandled)
            {
                nodes = new GridNode[gridXCount + 1, gridYCount + 1];
                edgesH = new GridEdge[gridXCount, gridYCount + 1];
                edgesV = new GridEdge[gridXCount + 1, gridYCount];
                cells = new GridCell[gridXCount, gridYCount];
                edges.Clear();
                prehandlegrid();
            }

            for (int i = 0; i < gridXCount + 1; i++)
            {
                for (int j = 0; j < gridYCount + 1; j++)
                {
                    value = calvalue(nodes[i, j]);
                    nodes[i, j].value = value;
                }
            }


            ////横边
            for (int i = 0; i < gridXCount; i++)
                for (int j = 0; j < gridYCount + 1; j++)
                {
                    GridNode unit1 = nodes[i, j].value < nodes[i + 1, j].value ? nodes[i, j] : nodes[i + 1, j];  //unit设定为更小值的节点
                    GridNode unit2 = nodes[i, j].value < nodes[i + 1, j].value ? nodes[i + 1, j] : nodes[i, j];
                    edgesH[i, j].unit1 = unit1;
                    edgesH[i, j].unit2 = unit2;
                }
            //竖边
            for (int i = 0; i < gridXCount + 1; i++)
                for (int j = 0; j < gridYCount; j++)
                {
                    GridNode unit1 = nodes[i, j].value < nodes[i, j + 1].value ? nodes[i, j] : nodes[i, j + 1];
                    GridNode unit2 = nodes[i, j].value < nodes[i, j + 1].value ? nodes[i, j + 1] : nodes[i, j];
                    edgesV[i, j].unit1 = unit1;
                    edgesV[i, j].unit2 = unit2;
                }



        }

        //预处理网格节点和数据节点关系
        void prehandlegrid()
        {
            for (int i = 0; i < gridXCount + 1; i++)
            {
                for (int j = 0; j < gridYCount + 1; j++)
                {
                    Point location = new Point(canvSize.Width * i / gridXCount, canvSize.Height * j / gridYCount);

                    nodes[i, j] = new GridNode() { idxx = i, idxy = j, location = location };
                    if (i == 0 || i == gridXCount || j == 0 || j == gridYCount)
                        nodes[i, j].nodeType = GridNode.ENodeType.边缘节点;

                    //nodes[i, j].dots=(dots.OrderBy(p=>(p.location-location).Length)).Take(10).ToList();

                    asslendot[] ad = new asslendot[5];
                    for (int m = 0; m < 5; m++)
                    {
                        ad[m] = new asslendot() { len = double.PositiveInfinity };
                    }
                    foreach (ValueDot dot in dots)
                    {
                        double len = (dot.location - location).Length;
                        if (ad[4].len > len)
                        {
                            int ins;
                            if (ad[2].len > len)
                            {
                                if (ad[1].len > len)
                                {
                                    if (ad[0].len > len)
                                        ins = 0;
                                    else
                                        ins = 1;
                                }
                                else
                                {
                                    ins = 2;
                                }

                            }
                            else
                            {
                                if (ad[3].len > len)
                                {
                                    ins = 3;
                                }
                                else
                                {
                                    ins = 4;
                                }
                            }
                            for (int m = 4; m >= ins; m--)
                            {
                                if (m == ins)
                                {
                                    ad[m].dot = dot;
                                    ad[m].len = len;
                                }
                                else
                                {
                                    ad[m].dot = ad[m - 1].dot;
                                    ad[m].len = ad[m - 1].len;
                                }
                            }

                        }
                    }
                    for (int m = 0; m < 5; m++)
                    {
                        nodes[i, j].dots.Add(ad[m].dot);
                    }

                }
            }

            //横边
            for (int i = 0; i < gridXCount; i++)
                for (int j = 0; j < gridYCount + 1; j++)
                {
                    //GridNode unit1 = nodes[i, j].value < nodes[i + 1, j].value ? nodes[i, j] : nodes[i + 1, j];  //unit设定为更小值的节点
                    //GridNode unit2 = nodes[i, j].value < nodes[i + 1, j].value ? nodes[i + 1, j] : nodes[i, j];
                    edgesH[i, j] = new GridEdge() { idxx = i, idxy = j, isEdge = (j == 0 || j == gridYCount) ? true : false };
                    edgesH[i, j].edgetype = j == 0 ? EdgeType.上 : (j == gridYCount ? EdgeType.下 : EdgeType.无);
                    edges.Add(edgesH[i, j]);
                }
            //竖边
            for (int i = 0; i < gridXCount + 1; i++)
                for (int j = 0; j < gridYCount; j++)
                {
                    //GridNode unit1 = nodes[i, j].value < nodes[i, j + 1].value ? nodes[i, j] : nodes[i, j + 1];
                    //GridNode unit2 = nodes[i, j].value < nodes[i, j + 1].value ? nodes[i, j + 1] : nodes[i, j];
                    edgesV[i, j] = new GridEdge() { idxx = i, idxy = j, isEdge = (i == 0 || i == gridXCount) ? true : false };
                    edgesV[i, j].edgetype = i == 0 ? EdgeType.左 : (i == gridXCount ? EdgeType.右 : EdgeType.无);
                    edges.Add(edgesV[i, j]);
                }

            for (int i = 0; i < gridXCount; i++)
                for (int j = 0; j < gridYCount; j++)
                {
                    cells[i, j] = new GridCell();
                    cells[i, j].Edges[0] = edgesV[i, j]; edgesV[i, j].celles.Add(cells[i, j]);
                    cells[i, j].Edges[1] = edgesH[i, j]; edgesH[i, j].celles.Add(cells[i, j]);
                    cells[i, j].Edges[2] = edgesV[i + 1, j]; edgesV[i + 1, j].celles.Add(cells[i, j]);
                    cells[i, j].Edges[3] = edgesH[i, j + 1]; edgesH[i, j + 1].celles.Add(cells[i, j]);
                }

            isPreHandled = true;
        }
        //计算插值
        double calvalue(GridNode node)
        {

            double alldis = 0;
            double allval = 0;

            foreach (ValueDot dot in node.dots)
            {
                //if ((dot.location - node.location).Length < canvSize.Width / 5)
                {
                    double tmp = Math.Pow((dot.location - node.location).Length, pow);
                    if (tmp == 0)
                        return dot.value;

                    alldis += 1.0 / tmp;
                    allval += dot.value / tmp;
                }
            }
            return allval / alldis;
        }


        ///<summary>绘制网格</summary>
        private void DrawGrid()
        {
            if (isDrawGrid)
            {
                for (int i = 0; i < gridXCount + 1; i++)
                {
                    GridNode gu1 = nodes[i, 0];
                    GridNode gu2 = nodes[i, gridYCount];
                    Line lin = new Line() { X1 = gu1.location.X, X2 = gu2.location.X, Y1 = gu1.location.Y, Y2 = gu2.location.Y };
                    lin.StrokeThickness = 0.1;
                    lin.Stroke = Brushes.Black;
                    canv.Children.Add(lin);
                }
                for (int i = 0; i < gridYCount + 1; i++)
                {
                    GridNode gu1 = nodes[0, i];
                    GridNode gu2 = nodes[gridXCount, i];
                    Line lin = new Line() { X1 = gu1.location.X, X2 = gu2.location.X, Y1 = gu1.location.Y, Y2 = gu2.location.Y };
                    lin.StrokeThickness = 0.1;
                    lin.Stroke = Brushes.Black;
                    canv.Children.Add(lin);
                }
            }
            ////网格值
            if (isShowGridData)
            {
                int step = (int)Math.Round(1f * gridXCount / 50);
                for (int i = 0; i < gridXCount + 1; i = i + step)
                {
                    for (int j = 0; j < gridYCount + 1; j++)
                    {
                        GridNode item = nodes[i, j];
                        TextBlock txt = new TextBlock() { Text = item.value.ToString("f1"), Margin = new Thickness(item.location.X, item.location.Y, 0, 0), FontSize = 9 };
                        canv.Children.Add(txt);
                    }
                }
            }
            //原始数据
            if (isShowData)
                foreach (ValueDot item in dots)
                {
                    TextBlock txt = new TextBlock() { Text = item.value.ToString("f1"), Margin = new Thickness(item.location.X, item.location.Y, 0, 0), Foreground = Brushes.Blue };
                    canv.Children.Add(txt);
                }



        }




        ///<summary>追踪等高线</summary>
        private void Trace()
        {

            //初始化等值层
            vlines.Clear();
            if (maxvalue == double.NegativeInfinity)
                maxvalue = dots.Max(p => p.value);

            double div; //层间距代表的值差
            div = (maxvalue - minvalue) / Span;
            for (int i = 0; i < Span; i++)
            {
                ValueLine vl = new ValueLine();
                vl.value = (i + 0) * div + minvalue;
                vlines.Add(vl);

                //判断可否不进行计算以提升性能
                double scale = 1f * i / (Span - 1);
                switch (opacityType)
                {
                    case EOpacityType.无:
                        vl.vltype = ValueLine.EVLType.计算并且绘制;
                        break;
                    case EOpacityType.正坡形:
                        if (scale < 1 - opacityRange)
                            vl.vltype = ValueLine.EVLType.不计算;
                        else
                            vl.vltype = ValueLine.EVLType.计算并且绘制;
                        break;
                    case EOpacityType.逆坡形:
                        if (scale < opacityRange)
                            vl.vltype = ValueLine.EVLType.计算并且绘制;
                        else
                            if (i > 0 && vlines[i - 1].vltype == ValueLine.EVLType.计算并且绘制)
                                vl.vltype = ValueLine.EVLType.仅计算;
                            else
                                vl.vltype = ValueLine.EVLType.不计算;
                        break;
                    case EOpacityType.倒梯形:
                        if (scale < opacityRange / 2)
                            vl.vltype = ValueLine.EVLType.计算并且绘制;
                        else if (scale > 1 - opacityRange / 2)
                            vl.vltype = ValueLine.EVLType.计算并且绘制;
                        else
                            if (i > 0 && vlines[i - 1].vltype == ValueLine.EVLType.计算并且绘制)
                                vl.vltype = ValueLine.EVLType.仅计算;
                            else
                                vl.vltype = ValueLine.EVLType.不计算;

                        break;
                }

                if (vl.vltype == ValueLine.EVLType.计算并且绘制)
                {
                    vl.stroke = new SolidColorBrush(getColorBetween(1.0 * (i + 1) / Span, minColor, maxColor));
                    vl.stroke.Opacity = getOpacity(scale);
                    if (vl.stroke.CanFreeze) vl.stroke.Freeze(); //zhadd
                }
            }

            //等值线追踪
            foreach (ValueLine vline in vlines)
            {
                if (vline.vltype == ValueLine.EVLType.不计算) continue;

                foreach (GridEdge edg in edges)  //计算等值点
                {
                    edg.isHandle = false;
                    if (edg.unit1.value <= vline.value && edg.unit2.value > vline.value)
                    {
                        edg.hasEquPoint = true;
                        Vector vec = edg.unit2.location - edg.unit1.location;
                        edg.equLocation = edg.unit1.location + vec * (vline.value - edg.unit1.value) / (edg.unit2.value - edg.unit1.value);
                    }
                    else
                        edg.hasEquPoint = false;
                }
                //非闭合线
                List<GridEdge> equedgs = edges.Where(p => p.hasEquPoint).ToList();

                List<GridEdge> edgedgs = equedgs.Where(p => p.isEdge).ToList();
                while (edgedgs.Count(p => p.isHandle == false) > 0)
                {
                    ZLine line = new ZLine();
                    vline.lines.Add(line);

                    GridEdge curedg = edgedgs.FirstOrDefault(p => p.isHandle == false);
                    line.points.Add(curedg.equLocation);
                    line.startedge = curedg;
                    bool isClockwise = (curedg.edgetype == EdgeType.上 && curedg.unit2.idxx > curedg.unit1.idxx) || (curedg.edgetype == EdgeType.下 && curedg.unit2.idxx < curedg.unit1.idxx) || (curedg.edgetype == EdgeType.左 && curedg.unit2.idxy < curedg.unit1.idxy) || (curedg.edgetype == EdgeType.右 && curedg.unit2.idxy > curedg.unit1.idxy);
                    while (true) //沿边界搜
                    {
                        GridEdge nextedg = getNextEdge(curedg, isClockwise);

                        if (nextedg.edgetype != curedg.edgetype)  //增加角点
                            line.points.Add(getConner(curedg, nextedg));

                        if (nextedg.hasEquPoint) //有等值点
                        {
                            nextedg.isHandle = true;
                            line.points.Add(nextedg.equLocation);

                            GridCell cell = nextedg.celles[0];
                            while (true)
                            {
                                nextedg = cell.Edges.Where(p => p != nextedg && p.hasEquPoint && !p.isHandle).FirstOrDefault();
                                nextedg.isHandle = true;
                                line.points.Add(nextedg.equLocation);
                                if (nextedg.isEdge)
                                {
                                    curedg = nextedg;
                                    isClockwise = (curedg.edgetype == EdgeType.上 && curedg.unit2.idxx > curedg.unit1.idxx) || (curedg.edgetype == EdgeType.下 && curedg.unit2.idxx < curedg.unit1.idxx) || (curedg.edgetype == EdgeType.左 && curedg.unit2.idxy < curedg.unit1.idxy) || (curedg.edgetype == EdgeType.右 && curedg.unit2.idxy > curedg.unit1.idxy);
                                    break;
                                }
                                cell = nextedg.celles.Where(p => p != cell).FirstOrDefault();
                            }

                        }
                        else  //进到下一边
                            curedg = nextedg;


                        if (curedg == line.startedge) //闭合退出
                            break;



                    }




                }


                //闭合线
                edgedgs = equedgs.Where(p => !p.isHandle).ToList();
                while (edgedgs.Count(p => p.isHandle == false) > 0)
                {
                    ZLine line = new ZLine() { isClosed = true };
                    //double minleft = double.PositiveInfinity, mintop = double.PositiveInfinity;
                    GridEdge minleftedge = null, mintopedge = null;
                    vline.lines.Add(line);
                    GridEdge edg = edgedgs.FirstOrDefault(p => p.isHandle == false);
                    if (edg.unit1.idxy == edg.unit2.idxy)//横边
                        minleftedge = edg;
                    else
                        mintopedge = edg;
                    edg.isHandle = true;
                    line.points.Add(edg.equLocation);

                    GridCell cell = edg.celles[0];
                    while (true)
                    {
                        edg = cell.Edges.Where(p => p != edg && p.hasEquPoint && !p.isHandle).FirstOrDefault();
                        if (edg == null)
                        {
                            line.points.Add(line.points.First());
                            line.startedge = minleftedge;
                            line.endedge = mintopedge;
                            break;
                        }
                        if (edg.unit1.idxy == edg.unit2.idxy)//横边
                        {
                            if (minleftedge == null || edg.equLocation.X < minleftedge.equLocation.X)
                                minleftedge = edg;
                        }
                        else
                            if (mintopedge == null || edg.equLocation.Y < mintopedge.equLocation.Y)
                                mintopedge = edg;

                        edg.isHandle = true;
                        line.points.Add(edg.equLocation);
                        cell = edg.celles.Where(p => p != cell).FirstOrDefault();
                    }

                }
                //闭合线填充属性，外围还是内部

                foreach (ZLine zline in vline.lines.Where(p => p.isClosed))
                {
                    if (zline.startedge != null)
                    {
                        zline.fillouter = zline.startedge.unit1.idxx > zline.startedge.unit2.idxx;
                    }
                    else if (zline.endedge != null)
                    {
                        zline.fillouter = zline.endedge.unit1.idxy > zline.endedge.unit2.idxy;
                    }
                }


            }

        }

        int drawcount = -1;
        //绘制等值线图
        private void Draw()
        {

            if (isStep)
                isDebug = true;


            if (!isRepeat)
                drawcount++;
            int count = 0;
            foreach (ValueLine vline in vlines)
            {
                if (vline.lines.Count == 0) continue;
                if (vline.vltype != ValueLine.EVLType.计算并且绘制) continue;

                GeometryGroup gg = new GeometryGroup();
                foreach (ZLine zline in vline.lines)
                {
                    PathGeometry path = new PathGeometry();
                    gg.Children.Add(path);
                    path.Figures.Add(new PathFigure());
                    path.Figures[0].Segments.Add(new PolyLineSegment());
                    path.Figures[0].StartPoint = zline.points[0];
                    //(path.Figures[0].Segments[0] as PolyLineSegment).Points = zline.points;
                    PointCollection pc = new PointCollection();
                    foreach (Point pnt in zline.points)
                        pc.Add(pnt);
                    (path.Figures[0].Segments[0] as PolyLineSegment).Points = pc;


                    zline.line = path;
                }

                //if (count == drawcount)
                //{ }

                vline.geoGroup = gg;






            }

            //最低值的填充
            ValueLine tmpLine = null;
            for (int i = 0; i < vlines.Count; i++)
            {
                tmpLine = vlines[i];
                if (tmpLine.vltype == ValueLine.EVLType.计算并且绘制 && tmpLine.geoGroup != null)
                    break;
            }
            if (tmpLine != null && tmpLine.vltype == ValueLine.EVLType.计算并且绘制 && tmpLine.geoGroup != null)
            {
                Path tmppath = new Path();
                GeometryGroup ggtmp = new GeometryGroup();
                ggtmp.Children.Add(tmpLine.geoGroup);
                if (ggtmp.GetArea() > areaThreshold * canvSize.Width * canvSize.Height)
                {
                    RectangleGeometry rg = new RectangleGeometry();
                    ggtmp.Children.Add(rg);
                    rg.Rect = new Rect(nodes[0, 0].location, nodes[gridXCount, gridYCount].location);
                }
                tmppath.Data = ggtmp;
                if (count == drawcount && isDebug)
                    tmppath.Stroke = new SolidColorBrush(Colors.Lime);
                else
                    tmppath.Stroke = Brushes.Blue;

                if (isDrawLine)
                    tmppath.StrokeThickness = 0.5;
                else
                    tmppath.StrokeThickness = 0;

                if (isFillLine)
                {
                    if (count == drawcount && isDebug)
                        tmppath.Fill = new SolidColorBrush(Color.FromArgb(0xC2, 0xFF, 0x00, 0xFA));
                    else
                    {
                        tmppath.Fill = tmpLine.stroke;
                        //tmppath.Fill.Opacity = getOpacity(0);

                    }
                }
                else
                    tmppath.Stroke = new SolidColorBrush(minColor);
                canv.Children.Add(tmppath);
            }



            count = 0;
            ValueLine prevVLine = null;
            foreach (ValueLine vline in vlines.OrderByDescending(p => p.value))
            {
                if (vline.geoGroup == null) continue;
                Path p = new Path();

                if (prevVLine == null)
                    p.Data = vline.geoGroup;
                else
                {
                    GeometryGroup gg = new GeometryGroup();
                    gg.Children.Add(vline.geoGroup);
                    gg.Children.Add(prevVLine.geoGroup);
                    if (gg.GetArea() > areaThreshold * canvSize.Width * canvSize.Height)
                    {
                        RectangleGeometry rg = new RectangleGeometry();
                        gg.Children.Add(rg);
                        rg.Rect = new Rect(nodes[0, 0].location, nodes[gridXCount, gridYCount].location);
                    }
                    p.Data = gg;
                }
                prevVLine = vline;

                //p.Data = vline.geoGroup;
                if (count == drawcount && isDebug)
                    p.Stroke = new SolidColorBrush(Colors.Lime);
                else
                    p.Stroke = Brushes.Blue;

                if (isDrawLine)
                    p.StrokeThickness = 0.5;
                else
                    p.StrokeThickness = 0;

                if (isFillLine)
                {
                    if (count == drawcount && isDebug)
                        p.Fill = new SolidColorBrush(Color.FromArgb(0xC2, 0xFF, 0x00, 0xFA));
                    else
                        p.Fill = vline.stroke;
                }
                else
                    p.Stroke = vline.stroke;
                canv.Children.Add(p);

                count++;
                if (isStep)
                {
                    if (count > drawcount)
                    {
                        DrawGrid();
                        return;
                    }
                }
            }


        }

        /// <summary>
        /// 获得边界的下一条边
        /// </summary>
        /// <param name="curEdge">当前边</param>
        /// <param name="isClockwise">true按顺时针, false按逆时针</param>
        /// <returns></returns>
        GridEdge getNextEdge(GridEdge curEdge, bool isClockwise)
        {
            int x, y;
            if (curEdge.edgetype == EdgeType.上)
            {
                x = curEdge.idxx + (isClockwise ? 1 : -1);
                y = curEdge.idxy;
                if (x < 0)
                {
                    return edgesV[0, 0];
                }
                else if (x >= gridXCount)
                    return edgesV[gridXCount, 0];
                else
                    return edgesH[x, y];
            }
            if (curEdge.edgetype == EdgeType.下)
            {
                x = curEdge.idxx + (isClockwise ? -1 : 1);
                y = curEdge.idxy;
                if (x < 0)
                    return edgesV[0, gridYCount - 1];
                else if (x >= gridXCount)
                    return edgesV[gridXCount, gridYCount - 1];
                else
                    return edgesH[x, y];
            }
            if (curEdge.edgetype == EdgeType.左)
            {
                x = curEdge.idxx;
                y = curEdge.idxy + (isClockwise ? -1 : 1);
                if (y < 0)
                    return edgesH[0, 0];
                else if (y >= gridYCount)
                    return edgesH[0, gridYCount];
                else
                    return edgesV[x, y];
            }
            if (curEdge.edgetype == EdgeType.右)
            {
                x = curEdge.idxx;
                y = curEdge.idxy + (isClockwise ? 1 : -1);
                if (y < 0)
                    return edgesH[gridXCount - 1, 0];
                else if (y >= gridYCount)
                    return edgesH[gridXCount - 1, gridYCount];
                else
                    return edgesV[x, y];
            }

            return null;
        }
        Point getConner(GridEdge edg1, GridEdge edg2)
        {
            int x, y;
            if (edg1.edgetype == EdgeType.上 || edg2.edgetype == EdgeType.上)
                y = 0;
            else
                y = gridYCount;
            if (edg1.edgetype == EdgeType.左 || edg2.edgetype == EdgeType.左)
                x = 0;
            else
                x = gridXCount;

            return nodes[x, y].location;
        }


        double getOpacity(double scale)
        {
            double result = 1;
            switch (opacityType)
            {
                case EOpacityType.无:
                    result = 1;
                    break;
                case EOpacityType.正坡形:
                    if (scale < 1 - opacityRange)
                        result = minOpacity;
                    else
                        result = minOpacity + (scale - (1 - opacityRange)) / opacityRange * (maxOpacity - minOpacity);
                    break;
                case EOpacityType.逆坡形:
                    if (scale < opacityRange)
                        result = minOpacity + scale / opacityRange * (maxOpacity - minOpacity);
                    else
                        result = minOpacity;
                    break;
                case EOpacityType.倒梯形:
                    if (scale < opacityRange / 2)
                        result = minOpacity + (opacityRange / 2 - scale) / (opacityRange / 2) * (maxOpacity - minOpacity);
                    else if (scale > 1 - opacityRange / 2)
                        result = minOpacity + (scale - (1 - opacityRange / 2)) / (opacityRange / 2) * (maxOpacity - minOpacity);
                    else
                        result = minOpacity;
                    break;
            }
            return result;
        }

        Color getColorBetween(double scale, Color colorStart, Color colorEnd)
        {
            Color color;
            System.Drawing.Color cStart = System.Drawing.Color.FromArgb(colorStart.A, colorStart.R, colorStart.G, colorStart.B);
            System.Drawing.Color cEnd = System.Drawing.Color.FromArgb(colorEnd.A, colorEnd.R, colorEnd.G, colorEnd.B);
            float hstart = cStart.GetHue();
            float hend = cEnd.GetHue();
            float hue = hstart + (hend - hstart) * (float)scale;
            int a = (int)(colorStart.A + 1.0 * (colorEnd.A - colorStart.A) * scale);
            System.Drawing.Color dc = HSBColor.FromHSB(new HSBColor(a, hue * 255 / 360, 255f, 255f));
            color = Color.FromArgb(dc.A, dc.R, dc.G, dc.B);
            return color;
        }

    }


    #region 辅助类

    class GridCell
    {
        public GridEdge[] Edges = new GridEdge[4]; //左上右下序
    }

    enum EdgeType { 无, 左, 上, 右, 下 }

    class GridEdge
    {
        public int idxx;
        public int idxy;
        public GridNode unit1;
        public GridNode unit2;
        public double equvalue;
        public Point equLocation;
        public bool hasEquPoint;
        public bool isHandle;
        public bool isEdge;
        public EdgeType edgetype;
        public List<GridCell> celles = new List<GridCell>();
    }

    class GridNode
    {
        public enum ENodeType { 内部节点, 边缘节点 }

        public ENodeType nodeType = ENodeType.内部节点;
        public int idxx;
        public int idxy;
        public Point location;
        public double value;

        public List<ValueDot> dots = new List<ValueDot>();
    }

    public class ValueDot  //值点
    {
        public string id;

        /// <summary>
        /// 坐标
        /// </summary>
        public Point location;
        /// <summary>
        /// 值
        /// </summary>
        public double value;
    }

    class ValueLine //等高线
    {
        public enum EVLType { 计算并且绘制, 仅计算, 不计算 }

        public EVLType vltype = EVLType.计算并且绘制;
        public double value;
        public List<ZLine> lines = new List<ZLine>();
        public Brush fill;
        public Brush stroke;
        public GeometryGroup geoGroup;
    }

    class ZLine
    {
        public ValueDot dot;
        public Geometry line;
        //public PointCollection points = new PointCollection();
        public List<Point> points = new List<Point>();
        public bool isCombined;
        public GridEdge startedge;  //非闭合的开始边，闭合的最左横边
        public GridEdge endedge;
        public bool isClosed;
        public bool fillouter; //填充外围
    }

    class asslendot
    {
        public ValueDot dot;
        public double len = double.PositiveInfinity;
    }



    public struct HSBColor
    {
        float h;
        float s;
        float b;
        int a;

        public HSBColor(float h, float s, float b)
        {
            this.a = 0xff;
            this.h = Math.Min(Math.Max(h, 0), 255);
            this.s = Math.Min(Math.Max(s, 0), 255);
            this.b = Math.Min(Math.Max(b, 0), 255);
        }

        public HSBColor(int a, float h, float s, float b)
        {
            this.a = a;
            this.h = Math.Min(Math.Max(h, 0), 255);
            this.s = Math.Min(Math.Max(s, 0), 255);
            this.b = Math.Min(Math.Max(b, 0), 255);
        }

        public HSBColor(System.Drawing.Color color)
        {
            HSBColor temp = FromColor(color);
            this.a = temp.a;
            this.h = temp.h;
            this.s = temp.s;
            this.b = temp.b;
        }

        public float H
        {
            get { return h; }
        }

        public float S
        {
            get { return s; }
        }

        public float B
        {
            get { return b; }
        }

        public int A
        {
            get { return a; }
        }

        public System.Drawing.Color Color
        {
            get
            {
                return FromHSB(this);
            }
        }

        public static System.Drawing.Color ShiftHue(System.Drawing.Color c, float hueDelta)
        {
            HSBColor hsb = HSBColor.FromColor(c);
            hsb.h += hueDelta;
            hsb.h = Math.Min(Math.Max(hsb.h, 0), 255);
            return FromHSB(hsb);
        }

        public static System.Drawing.Color ShiftSaturation(System.Drawing.Color c, float saturationDelta)
        {
            HSBColor hsb = HSBColor.FromColor(c);
            hsb.s += saturationDelta;
            hsb.s = Math.Min(Math.Max(hsb.s, 0), 255);
            return FromHSB(hsb);
        }


        public static System.Drawing.Color ShiftBrighness(System.Drawing.Color c, float brightnessDelta)
        {
            HSBColor hsb = HSBColor.FromColor(c);
            hsb.b += brightnessDelta;
            hsb.b = Math.Min(Math.Max(hsb.b, 0), 255);
            return FromHSB(hsb);
        }

        public static System.Drawing.Color FromHSB(HSBColor hsbColor)
        {
            float r = hsbColor.b;
            float g = hsbColor.b;
            float b = hsbColor.b;
            if (hsbColor.s != 0)
            {
                float max = hsbColor.b;
                float dif = hsbColor.b * hsbColor.s / 255f;
                float min = hsbColor.b - dif;

                float h = hsbColor.h * 360f / 255f;

                if (h < 60f)
                {
                    r = max;
                    g = h * dif / 60f + min;
                    b = min;
                }
                else if (h < 120f)
                {
                    r = -(h - 120f) * dif / 60f + min;
                    g = max;
                    b = min;
                }
                else if (h < 180f)
                {
                    r = min;
                    g = max;
                    b = (h - 120f) * dif / 60f + min;
                }
                else if (h < 240f)
                {
                    r = min;
                    g = -(h - 240f) * dif / 60f + min;
                    b = max;
                }
                else if (h < 300f)
                {
                    r = (h - 240f) * dif / 60f + min;
                    g = min;
                    b = max;
                }
                else if (h <= 360f)
                {
                    r = max;
                    g = min;
                    b = -(h - 360f) * dif / 60 + min;
                }
                else
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }

            return System.Drawing.Color.FromArgb
                (
                    (byte)hsbColor.a,
                    (byte)Math.Round(Math.Min(Math.Max(r, 0), 255)),
                    (byte)Math.Round(Math.Min(Math.Max(g, 0), 255)),
                    (byte)Math.Round(Math.Min(Math.Max(b, 0), 255))
                    );
        }


        public static HSBColor FromColor(System.Drawing.Color color)
        {
            HSBColor ret = new HSBColor(0f, 0f, 0f);
            ret.a = color.A;

            float r = color.R;
            float g = color.G;
            float b = color.B;

            float max = Math.Max(r, Math.Max(g, b));

            if (max <= 0)
            {
                return ret;
            }

            float min = Math.Min(r, Math.Min(g, b));
            float dif = max - min;

            if (max > min)
            {
                if (g == max)
                {
                    ret.h = (b - r) / dif * 60f + 120f;
                }
                else if (b == max)
                {
                    ret.h = (r - g) / dif * 60f + 240f;
                }
                else if (b > g)
                {
                    ret.h = (g - b) / dif * 60f + 360f;
                }
                else
                {
                    ret.h = (g - b) / dif * 60f;
                }
                if (ret.h < 0)
                {
                    ret.h = ret.h + 360f;
                }
            }
            else
            {
                ret.h = 0;
            }

            ret.h *= 255f / 360f;
            ret.s = (dif / max) * 255f;
            ret.b = max;

            return ret;
        }
    }
    #endregion
}
