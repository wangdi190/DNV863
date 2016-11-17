using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Interact
{
    public class RadarChart : Grid
    {
        public RadarChart()
        {
            this.Loaded += new System.Windows.RoutedEventHandler(RadarChart_Loaded);
        }


        void RadarChart_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            build();
        }



        private List<RadarDataItem> _dataSource;
        ///<summary>数据源</summary>
        public List<RadarDataItem> dataSource
        {
            get { return _dataSource; }
            set { _dataSource = value; build(); }
        }

        ///<summary>表示系列的属性名称</summary>
        public string serialName {get;set;}

        ///<summary>表示argu的属性名称</summary>
        public string argumentName {get;set;}

        ///<summary>表示值属性名称</summary>
        public string valueName {get;set;}

        private List<Pen> _pens=new List<Pen>();
        ///<summary>系统画笔</summary>
        public List<Pen> pens
        {
            get { return _pens; }
            set { _pens = value; }
        }
      

                
        private int _padding=80;
        ///<summary>雷达图边距</summary>
        public int padding
        {
            get { return _padding; }
            set { _padding = value; }
        }
                
        private TextBlock _title=new TextBlock(){FontSize=16, Foreground=Brushes.Yellow, HorizontalAlignment= HorizontalAlignment.Center, VerticalAlignment= VerticalAlignment.Top, Margin=new Thickness(0,5,0,0)};
        ///<summary>标题对象</summary>
        public TextBlock title
        {
            get { return _title; }
            set { _title = value; }
        }

        
        private Border _panel=new Border(){ Background = new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0x00, 0x00)), CornerRadius = new CornerRadius(10), BorderBrush=Brushes.Silver, BorderThickness=new Thickness(1) };
        ///<summary>面板对象</summary>
        public Border panel
        {
            get { return _panel; }
            set { _panel = value; }
        }
      


        DrawingGroup drawingGroup;
        DrawingBrush drawingBrush;
        Grid rect = new Grid();

        public void build()
        {
            if (string.IsNullOrWhiteSpace(serialName) || string.IsNullOrWhiteSpace(argumentName) || string.IsNullOrWhiteSpace(valueName) || dataSource == null || !this.IsLoaded) return;

            this.Children.Clear();

            //背景板
            this.Children.Add(panel);
            //标题
            this.Children.Add(title);

            this.Children.Add(rect);
            drawingGroup = new DrawingGroup();
            drawingBrush = new DrawingBrush(drawingGroup);
            rect.Background = drawingBrush;
            rect.IsHitTestVisible = false;

            double edgeLength = Math.Min(this.ActualHeight, this.ActualWidth)-padding;
            Vector orgpoint = new Vector(edgeLength / 2, edgeLength / 2);
            rect.Width = rect.Height = edgeLength;

            List<string> argues = dataSource.GroupBy(p => p.argu).Select(p => p.Key).ToList(); //所有的argu
            foreach (var item in dataSource) //计算项点位置
            {
                item.idx = argues.IndexOf(item.argu);
                double angle = Math.PI * 2 * item.idx / argues.Count;
                item.pnt = new Point(edgeLength * item.value / item.maxvalue * Math.Cos(angle), edgeLength * item.value / item.maxvalue * Math.Sin(angle)) + orgpoint;
            }

            List<string> sorts = dataSource.GroupBy(p => p.sort).Select(p => p.Key).ToList();

            //绘制网格
            GeometryDrawing gd;
            PathGeometry geo;

            gd = new GeometryDrawing();
            RectangleGeometry geo2 = new RectangleGeometry();
            geo2.Rect = new Rect(0, 0, edgeLength, edgeLength);
            gd.Brush = Brushes.Transparent;
            gd.Geometry = geo2;
            drawingGroup.Children.Add(gd);

            //=== 背板
            gd = new GeometryDrawing();
            geo = new PathGeometry();
            gd.Geometry = geo;
            {
                PathFigure pf = new PathFigure();
                geo.Figures.Add(pf);
                PolyLineSegment ps = new PolyLineSegment();
                pf.Segments.Add(ps);

                for (int j = 0; j < argues.Count; j++)
                {
                    double angle = Math.PI * 2 * j / argues.Count + Math.PI / 2;
                    Point pnt = new Point(edgeLength / 2 * Math.Cos(angle), edgeLength / 2 * Math.Sin(angle)) + orgpoint;
                    if (j == 0)
                        pf.StartPoint = pnt;
                    else
                        ps.Points.Add(pnt);
                }
                ps.Points.Add(pf.StartPoint);
            }
            gd.Pen = new Pen() { Thickness = 1, Brush = Brushes.Green };
            gd.Brush = new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0x34, 0x00));
            drawingGroup.Children.Add(gd);
            //=== 网格线
            gd = new GeometryDrawing();
            geo = new PathGeometry();
            gd.Geometry = geo;
            for (int i = 2; i < 3; i++)
            {
                PathFigure pf = new PathFigure();
                geo.Figures.Add(pf);
                PolyLineSegment ps = new PolyLineSegment();
                pf.Segments.Add(ps);

                for (int j = 0; j < argues.Count; j++)
                {
                    double angle = Math.PI * 2 * j / argues.Count + Math.PI / 2;
                    Point pnt = new Point(edgeLength / 2 * i * 0.25 * Math.Cos(angle), edgeLength / 2 * i * 0.25 * Math.Sin(angle)) + orgpoint;
                    if (j == 0)
                        pf.StartPoint = pnt;
                    else
                        ps.Points.Add(pnt);
                }
                ps.Points.Add(pf.StartPoint);

            }
            gd.Pen = new Pen() { Thickness = 1, Brush = Brushes.Green };
            drawingGroup.Children.Add(gd);
            //===放射线
            {
                gd = new GeometryDrawing();
                geo = new PathGeometry();
                gd.Geometry = geo;

                for (int j = 0; j < argues.Count; j++)
                {
                    PathFigure pf = new PathFigure();
                    geo.Figures.Add(pf);
                    LineSegment ps = new LineSegment();
                    pf.Segments.Add(ps);
                    pf.StartPoint = (Point)orgpoint;
                    double angle = Math.PI * 2 * j / argues.Count + Math.PI / 2;
                    ps.Point = new Point(edgeLength / 2 * Math.Cos(angle), edgeLength / 2 * Math.Sin(angle)) + orgpoint;

                    //----- 添加标签 -----
                    TextBlock txt = new TextBlock() {
                        Text=argues[j], 
                        Foreground=new SolidColorBrush(Colors.LawnGreen), 
                        IsHitTestVisible=false 
                    };
                    txt.Measure(new Size(0,0));
                    txt.Arrange(new System.Windows.Rect(0, 0, 0, 0));
                    this.Children.Add(txt);
                    var tmp = dataSource.First(p => p.argu == argues[j]);
                    TextBlock txt2 = new TextBlock()
                    {
                        Text =string.Format(tmp.format,tmp.maxvalue),
                        Foreground = Brushes.Green,
                        IsHitTestVisible = false
                    };
                    txt2.Measure(new Size(0, 0));
                    txt2.Arrange(new System.Windows.Rect(0, 0, 0, 0));
                    this.Children.Add(txt2);
                    double offsetx, offsety;
                    offsetx=offsety=0;
                    double maxtextlen = Math.Max(txt.ActualWidth, txt2.ActualWidth);
                    if (ps.Point.Y < orgpoint.Y - (edgeLength / 10))
                        offsety = -txt.ActualHeight - 1;
                    else if (ps.Point.Y > orgpoint.Y + (edgeLength / 10))
                        offsety = txt.ActualHeight + 1;
                    if (ps.Point.X < orgpoint.X - (edgeLength / 10))
                        offsetx = -maxtextlen/2 - 1;
                    else if (ps.Point.X > orgpoint.X + (edgeLength / 10))
                        offsetx = maxtextlen/2 + 1;
                    txt.Margin = new Thickness(ps.Point.X - txt.ActualWidth / 2 + (this.ActualWidth - edgeLength) / 2+offsetx, ps.Point.Y + (this.ActualHeight - edgeLength) / 2+offsety, 0, 0);
                    txt2.Margin = new Thickness(ps.Point.X - txt2.ActualWidth / 2 + (this.ActualWidth - edgeLength) / 2+offsetx, ps.Point.Y - txt2.ActualHeight + (this.ActualHeight - edgeLength) / 2+offsety, 0, 0);
                    //----- 添加热线以显示tooltips -----
                    string tip = "【" + argues[j] + "】\r\n";
                    foreach (string sort in sorts)
                    {
                        RadarDataItem rad= dataSource.First(p => p.sort == sort && p.argu == argues[j]);
                        tip+= (sort+"："+string.Format(rad.format,rad.value));
                        if (sort != sorts.Last())
                            tip += "\r\n";
                    }
                    Line lin = new Line()
                    {
                        Stroke = Brushes.Transparent,
                        StrokeThickness = 20,
                        X1 = orgpoint.X + (this.ActualWidth - edgeLength) / 2,
                        Y1 = orgpoint.Y + (this.ActualHeight - edgeLength) / 2,
                        X2 = ps.Point.X + (this.ActualWidth - edgeLength) / 2,
                        Y2 = ps.Point.Y + (this.ActualHeight - edgeLength) / 2,
                        ToolTip = tip,
                    };
                    this.Children.Add(lin);
                }


                gd.Pen = new Pen() { Thickness = 1, Brush = Brushes.Green };
                drawingGroup.Children.Add(gd);
            }
            //===业务图形
            StackPanel spanel = new StackPanel() //图例容器
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                HorizontalAlignment= System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5),
            };
            this.Children.Add(spanel);


            int idx=0;
            foreach (string sort in sorts)
            {
                gd = new GeometryDrawing();
                geo = new PathGeometry();
                gd.Geometry = geo;
                PathFigure pf = new PathFigure();
                geo.Figures.Add(pf);
                PolyLineSegment ps = new PolyLineSegment();
                pf.Segments.Add(ps);
                foreach (var item in dataSource.Where(p => p.sort == sort).OrderBy(p=>p.idx))
                {
                    double angle = Math.PI * 2 * item.idx / argues.Count + Math.PI / 2;
                    Point pnt = new Point(edgeLength / 2 * item.value / item.maxvalue * Math.Cos(angle), edgeLength / 2 * item.value / item.maxvalue * Math.Sin(angle)) + orgpoint;
                    if (item.idx == 0)
                        pf.StartPoint = pnt;
                    else
                        ps.Points.Add(pnt);
                }

                ps.Points.Add(pf.StartPoint);
                gd.Pen = idx < pens.Count ? pens[idx] :(new Pen() { Thickness = 1, Brush = Brushes.Lime });
                drawingGroup.Children.Add(gd);
                //----- 图例
                spanel.Children.Add(new Rectangle() 
                {
                    Width = 10, 
                    Height = 10, 
                    VerticalAlignment = System.Windows.VerticalAlignment.Center, 
                    Fill = idx < pens.Count ? pens[idx].Brush : Brushes.Lime ,
                    Stroke=Brushes.Silver,
                    Margin=new Thickness(12,0,3,0)
                });
                spanel.Children.Add(new TextBlock()
                {
                    Text=sort,
                    Foreground=Brushes.Silver,
                });

                

                idx++;
            }
            //----- 标志点 -----
            idx = 0;
            foreach (string sort in sorts)
            {
                foreach (var item in dataSource.Where(p => p.sort == sort).OrderBy(p => p.idx))
                {
                    double angle = Math.PI * 2 * item.idx / argues.Count + Math.PI / 2;
                    Point pnt = new Point(edgeLength / 2 * item.value / item.maxvalue * Math.Cos(angle), edgeLength / 2 * item.value / item.maxvalue * Math.Sin(angle)) + orgpoint;
                    
                    GeometryDrawing gddot = new GeometryDrawing();
                    EllipseGeometry geodot = new EllipseGeometry();
                    gddot.Geometry = geodot;
                    geodot.Center = pnt;
                    geodot.RadiusX = geodot.RadiusY = 2;
                    gddot.Brush = Brushes.Red;
                    gddot.Pen = new Pen() { Thickness = 1, Brush = Brushes.White };
                    drawingGroup.Children.Add(gddot);
                }
            }
          


        }



        

        
    }


    public class RadarDataItem
    {
        public string sort { get; set; }
        public string argu { get; set; }
        public double minvalue { get; set; }
        public double maxvalue { get; set; }
        public double value { get; set; }
        public string format { get; set; }

        internal int idx;
        internal Point pnt;

    }



}
