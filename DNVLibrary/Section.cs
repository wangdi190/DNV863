using System;
using System.Globalization;
using System.Windows.Data;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;



namespace DNVLibrary
{


    [Serializable]
    public class viewmodel : DependencyNotificationObject
    {
        public viewmodel()
        {
            brushCover.StartPoint = new Point(0, 1);
            brushCover.EndPoint = new Point(0, 0);
            brushCover.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 0x00, 0x00, 0x00), 0));
            brushCover.GradientStops.Add(new GradientStop(Color.FromArgb(0x30, 0x00, 0x00, 0x00), 0.5));
            brushCover.GradientStops.Add(new GradientStop(Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF), 0.5));
            brushCover.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF), 1));

        }
        #region 显示选项参数

        Para _para;
        public Para para
        {
            get { if (_para == null) _para = new Para(); return _para; }
            set
            {
                _para = value;

                _background = null;
                _fontBrush = null;
                _lineBrush = null;
                _SectionNameBrush = null;

                isFull = para.isFull;
                len = para.len;
                len2 = para.len2;
                height = para.height;
                height2 = para.height2;
                backgroundColor = para.backgroundColor;
                lightColor = para.lightColor;
                normalColor = para.normalColor;
                weightColor = para.weightColor;
                fontColor = para.fontColor;
                fontSize = para.fontSize == 0 ? 9 : para.fontSize;
                fontSize2 = para.fontSize2 == 0 ? 9 : para.fontSize2;
                lineColor = para.lineColor;
                lightScale = para.lightScale;
                weightScale = para.weightScale;
                span = para.span;
                isShowSectionName = para.isShowSectionName;
                fontSizeSectionName = para.fontSizeSectionName;
                fontColorSectionName = para.fontColorSectionName;
                isShowAllArrow = para.isShowAllArrow;
            }
        }

        public static bool isFull { get; set; } //是否填充为百分比同等长度形式
        public static double len { get; set; } //长度
        public static double len2 { get; set; } //地图中线路信息条长度
        public static double height { get; set; } //右侧列表中断面条高度
        public static double height2 { get; set; } //地图中线路条高度
        public static Color backgroundColor { get; set; } //底色
        public static Color lightColor { get; set; } //轻载色
        public static Color normalColor { get; set; } //普通色
        public static Color weightColor { get; set; } //重载色
        public static Color fontColor { get; set; }
        public static Double fontSize { get; set; }
        public static Double fontSize2 { get; set; }
        public static Color lineColor { get; set; }
        public static double lightScale { get; set; } //轻载率
        public static double weightScale { get; set; } //重载率
        public static double span { get; set; } //地图信息条间隔
        public static bool isShowSectionName { get; set; }
        public static double fontSizeSectionName { get; set; }
        public static Color fontColorSectionName { get; set; }
        public static bool isShowAllArrow { get; set; }

        static SolidColorBrush _background = null;
        internal static SolidColorBrush background { get { if (_background == null) _background = new SolidColorBrush(backgroundColor); return _background; } }
        static SolidColorBrush _fontBrush = null;
        internal static SolidColorBrush fontBrush { get { if (_fontBrush == null) _fontBrush = new SolidColorBrush(fontColor); return _fontBrush; } }
        static SolidColorBrush _lineBrush = null;
        internal static SolidColorBrush lineBrush { get { if (_lineBrush == null) _lineBrush = new SolidColorBrush(lineColor); return _lineBrush; } }
        static SolidColorBrush _SectionNameBrush = null;
        internal static SolidColorBrush SectionNameBrush { get { if (_SectionNameBrush == null) _SectionNameBrush = new SolidColorBrush(fontColorSectionName); return _SectionNameBrush; } }

        [XmlIgnore]
        public static LinearGradientBrush brushCover = new LinearGradientBrush();
        #endregion
        [XmlIgnore]
        public Grid grid = new Grid();
        [XmlIgnore]
        //public DynSceneLibrary.DynScene map;
        public WpfEarthLibrary.Earth map;

        public delegate void SetFlowDelegate();
        [XmlIgnore]
        public SetFlowDelegate setFlow { get; set; }



        BindingList<section> _sections = new BindingList<section>();
        public BindingList<section> sections { get { return _sections; } set { _sections = value; } }

        double _maxValue = double.NegativeInfinity;
        public double maxValue { get { if (_maxValue == double.NegativeInfinity) _maxValue = sections.Max(p => p.maxValue); return _maxValue; } }

        public void Initialize()
        {
            foreach (section sec in sections)
            {
                sec.root = this;
                sec.Initialize();
            }
            
            setFlow();
        }

        #region 控制
        public ICommand NewSectionCommand { get { return new DelegateCommand(OnNewSection); } }
        void OnNewSection()
        { sections.Add(new section() { root = this, name = "新断面" }); curSectionIdx = sections.Count - 1; RaisePropertyChanged(() => curSectionIdx); RaisePropertyChanged(() => sections); }

        public ICommand DelSectionCommand { get { return new DelegateCommand(OnDelSection); } }
        void OnDelSection()
        {
            section tmp = curSection;
            curSectionIdx = -1; sections.Remove(tmp); RaisePropertyChanged(() => curSectionIdx); RaisePropertyChanged(() => sections);
        }



        int _curSectionIdx = -1;
        [XmlIgnore]
        public int curSectionIdx
        {
            get { return _curSectionIdx; }
            set
            {
                if (value < sections.Count)
                    _curSectionIdx = value;
                else
                    _curSectionIdx = -1;
                RaisePropertyChanged(() => curSectionIdx);
                RaisePropertyChanged(() => curSection);
            }
        }

        [XmlIgnore]
        public section curSection
        {
            get { return curSectionIdx < 0 ? null : sections[curSectionIdx]; }
        }
        #endregion

        public void SaveToXml(string filepath)
        {
            XmlHelper.saveToXml(filepath, this);
        }

        public void refreshLocation()
        {
            foreach (section sec in sections.Where(p => p.isShow))
            {
                Brush tmp = sec.brush;
                sec.refreshLocation();
            }
        }

    }

    public class section : DependencyNotificationObject
    {
        [XmlIgnore]
        public viewmodel root;

        public string id { get; set; }

        string _name = "名称";
        public string name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(() => name); RaisePropertyChanged(() => lines); }
        }

        bool _isShow;
        public bool isShow
        {
            get
            {
                return _isShow;
            }
            set
            {
                _isShow = value;

                if (root != null)
                    refreshInfo();
            }
        }

        public void refreshInfo()
        {

            if (isShow)
            {
                if (viewmodel.isShowSectionName)
                    root.grid.Children.Add(vSectionName);
                foreach (line lin in lines)
                {
                    root.grid.Children.Add(lin.vInfo);
                    root.grid.Children.Add(lin.vLinkLine);
                }
                refreshLocation();
            }
            else
            {
                if (viewmodel.isShowSectionName)
                    root.grid.Children.Remove(vSectionName);
                foreach (line lin in lines)
                {
                    root.grid.Children.Remove(lin.vInfo);
                    root.grid.Children.Remove(lin.vLinkLine);
                }
            }

            if (!viewmodel.isShowAllArrow)
                root.setFlow();
        }


        BindingList<line> _lines = new BindingList<line>();
        public BindingList<line> lines { get { return _lines; } set { _lines = value; } }

        double _maxValue = double.NegativeInfinity;
        public double maxValue { get { if (lines.Count > 0) _maxValue = lines.Sum(p => p.maxValue); return _maxValue; } }
        public double curValue { get { return lines.Sum(p => p.curValue * p.direction); } }

        [XmlIgnore]
        public string info { get { return name + ":" + (viewmodel.isFull ? (curValue / maxValue).ToString("p1") : (curValue.ToString("f0") + "/" + maxValue.ToString("f0"))); } }

        public double len { get { return viewmodel.isFull ? viewmodel.len : viewmodel.len * maxValue / root.maxValue; } }
        public double height { get { return viewmodel.height; } }
        public double heightin { get { return viewmodel.height - 2; } }
        public SolidColorBrush background { get { return viewmodel.background; } }
        public SolidColorBrush fontBrush { get { return viewmodel.fontBrush; } }
        public double fontSize { get { return viewmodel.fontSize; } }
        public double radius { get { return height / 2; } }

        public Color valuecolor
        {
            get
            {
                if (scale <= viewmodel.lightScale)
                    return viewmodel.lightColor;
                else if (scale >= viewmodel.weightScale)
                    return viewmodel.weightColor;
                else
                    return viewmodel.normalColor;
            }
        }
        public double scale { get { return curValue / maxValue; } }

        GradientStop gstep1, gstep2;

        LinearGradientBrush _brush;
        [XmlIgnore]
        public LinearGradientBrush brush
        {
            get
            {
                if (_brush == null)
                {
                    _brush = new LinearGradientBrush();
                    _brush.EndPoint = new Point(1, 0);
                    GradientStop gs;
                    gs = new GradientStop() { Offset = 0 };
                    OperateHelper.bind(this, "valuecolor", gs, GradientStop.ColorProperty);
                    _brush.GradientStops.Add(gs);

                    gs = new GradientStop();
                    //OperateHelper.bind(this, "scale", gs, GradientStop.OffsetProperty);
                    OperateHelper.bind(this, "valuecolor", gs, GradientStop.ColorProperty);
                    _brush.GradientStops.Add(gs);
                    gstep1 = gs;

                    gs = new GradientStop() { Color = viewmodel.backgroundColor };
                    //OperateHelper.bind(this, "scale", gs, GradientStop.OffsetProperty);
                    _brush.GradientStops.Add(gs);
                    gstep2 = gs;

                    gs = new GradientStop() { Offset = 1, Color = viewmodel.backgroundColor };
                    _brush.GradientStops.Add(gs);

                }

                return _brush;
            }
        }


        public double width { get { return len * curValue / maxValue; } }


        #region 位置属性
        [XmlIgnore]
        Rect? _bound = null;
        Rect bound
        {
            get
            {
                if (_bound == null)
                    _bound = new Rect(new Point(lines.Min(p => p.center.X), lines.Min(p => p.center.Y)), new Point(lines.Max(p => p.center.X), lines.Max(p => p.center.Y)));
                return (Rect)_bound;
            }
        }
        #endregion


        #region 控制

        public ICommand DelLineCommand { get { return new DelegateCommand(OnDelLine); } }
        void OnDelLine()
        {
            line tmp = curLine;
            if (root.grid.Children.Contains(tmp.vInfo))
            {
                root.grid.Children.Remove(tmp.vInfo);
                root.grid.Children.Remove(tmp.vLinkLine);
            }
            curLineIdx = -1; lines.Remove(tmp); RaisePropertyChanged(() => curLineIdx); RaisePropertyChanged(() => lines);
        }



        int _curLineIdx = -1;
        [XmlIgnore]
        public int curLineIdx
        {
            get { return _curLineIdx; }
            set
            {
                if (value < lines.Count)
                    _curLineIdx = value;
                else
                    _curLineIdx = -1;
                RaisePropertyChanged(() => curLineIdx);
                RaisePropertyChanged(() => curLine);
            }
        }

        [XmlIgnore]
        public line curLine
        {
            get { return curLineIdx < 0 ? null : lines[curLineIdx]; }
        }
        #endregion


        public void Initialize()
        {
            foreach (line lin in lines)
            {
                lin.section = this;
                lin.Initialize();
            }
            RaisePropertyChanged(() => len);
            RaisePropertyChanged(() => brush);
            RaisePropertyChanged(() => width);
            refreshInfo();
        }


        public void refreshCollection()
        {
            curLineIdx = lines.Count - 1; RaisePropertyChanged(() => curLineIdx); RaisePropertyChanged(() => lines);
        }



        public void refreshData()
        {
            if (lines.Count == 0) return;

            RaisePropertyChanged(() => valuecolor);
            RaisePropertyChanged(() => scale);
            RaisePropertyChanged(() => info);

            DoubleAnimation ani = new DoubleAnimation(scale, TimeSpan.FromSeconds(0.2), FillBehavior.HoldEnd);
            gstep1.BeginAnimation(GradientStop.OffsetProperty, ani);
            gstep2.BeginAnimation(GradientStop.OffsetProperty, ani);

            if (isShow)
                foreach (line lin in lines)
                {
                    lin.refreshData();
                }
        }


        [XmlIgnore]
        public bool isLeft;

        public void refreshLocation()
        {
            foreach (line lin in lines)
            {
                lin.center = root.map.earthManager.transformD3DToScreen(lin.d3dLocation);
                isLeft = lin.center.X < root.grid.ActualWidth / 2;
                
                //List<DynSceneLibrary.CObjectPosition> ele = root.map.GetElementPositions("ACLineSegment_Layer", lin.id);
                //if (ele != null)
                //{
                //    GeneralTransform3DTo2D gt = root.map.GetTransform3DTo2D();
                //    Point3D org = new Point3D(ele[0].ObjectPosition.Left, 0, ele[0].ObjectPosition.Top);
                //    lin.center = gt.Transform(org);

                //    isLeft = lin.center.X < root.grid.ActualWidth / 2;
                //}
            }
            _bound = null;



            double allheight = lines.Count * viewmodel.height2 + (lines.Count - 1) * viewmodel.span;

            if (lines.Count > 0)
            {
                int idx = 0;
                double top = bound.Top + bound.Height / 2 - allheight / 2;
                foreach (line lin in lines.OrderBy(p => p.center.Y))
                {
                    double left = isLeft ? bound.Left - 50 - lin.len : bound.Right + 50;
                    lin.location = new Thickness(left, top, 0, 0);
                    top += viewmodel.height2 + viewmodel.span;
                    lin.idx = idx;
                    idx++;
                }

                // 相交互换
                int changecount = 0;
                line oldlin=null; 
                Point p1, p2;
                Point p3 = new Point();
                int allchangecount = 0;
                do
                {
                    allchangecount++;
                    changecount = 0;
                    foreach (line lin in lines.OrderBy(p => p.idx))
                    {
                        if (lin.idx == 0)
                        {
                            oldlin = lin;
                            continue;
                        }
                        else
                        {
                            p1 = new Point(oldlin.locationX, oldlin.locationY);
                            p2 = new Point(lin.locationX, lin.locationY);
                            if (OperateHelper.GetIntersection(oldlin.center, p1, lin.center, p2, ref p3) == 1)
                            {
                                Thickness tmp = lin.location;
                                lin.location = oldlin.location;
                                oldlin.location = tmp;
                                int tmpi = lin.idx;
                                lin.idx = oldlin.idx;
                                oldlin.idx = tmpi;
                                changecount++;
                            }
                            else
                                oldlin = lin;
 
                        }
                    }

                }
                while (changecount > 0 && allchangecount<20);
                 

                



                //======断面名称
                double top2 = bound.Top + bound.Height / 2 - allheight / 2 - viewmodel.fontSizeSectionName - 10;
                double left2 = isLeft ? bound.Left - 50 - viewmodel.len2 : bound.Right + 50 + 10;
                location = new Thickness(left2, top2, 0, 0);
                RaisePropertyChanged(() => location);


            }

        }

        public Thickness location { get; set; }

        TextBlock _vSectionName;
        public TextBlock vSectionName
        {
            get
            {
                if (_vSectionName == null)
                {
                    _vSectionName = new TextBlock() { Text = name, FontSize = viewmodel.fontSizeSectionName, Foreground = viewmodel.SectionNameBrush, IsHitTestVisible = false };
                    _vSectionName.Effect = new DropShadowEffect() {Color=Colors.White, ShadowDepth = 0, BlurRadius = 16 };
                    OperateHelper.bind(this, "location", _vSectionName, TextBlock.MarginProperty);

                }

                return _vSectionName;
            }
        }

    }

    public class line : DependencyNotificationObject
    {
        public line()
        {

        }

        [XmlIgnore]
        public section section;


        public string id { get; set; }
        public string name { get; set; }

        public WpfEarthLibrary.VECTOR3D d3dLocation { get; set; }

        public double maxValue { get; set; }

        double _curValue;
        public double curValue
        {
            get { return _curValue; }
            set { _curValue = value; if (section != null) section.refreshData(); }
        }

        public bool isReverse { get; set; }
        public int direction { get { return isReverse ? -1 : 1; } } //方向性，1或-1

        public SolidColorBrush background { get { return viewmodel.background; } }

        public double scale { get { return Math.Abs(curValue) / maxValue; } }

        [XmlIgnore]
        public int idx { get; set; }

        public Color valuecolor
        {
            get
            {
                if (scale <= viewmodel.lightScale)
                    return viewmodel.lightColor;
                else if (scale >= viewmodel.weightScale)
                    return viewmodel.weightColor;
                else
                    return viewmodel.normalColor;
            }
        }


        GradientStop gstep1, gstep2;
        LinearGradientBrush _brush;
        [XmlIgnore]
        public LinearGradientBrush brush
        {
            get
            {
                if (_brush == null)
                {
                    _brush = new LinearGradientBrush();
                    _brush.EndPoint = new Point(1, 0);
                    GradientStop gs;
                    gs = new GradientStop() { Offset = 0 };
                    OperateHelper.bind(this, "valuecolor", gs, GradientStop.ColorProperty);
                    _brush.GradientStops.Add(gs);

                    gs = new GradientStop();
                    OperateHelper.bind(this, "valuecolor", gs, GradientStop.ColorProperty);
                    _brush.GradientStops.Add(gs);
                    gstep1 = gs;

                    gs = new GradientStop() { Color = viewmodel.backgroundColor };
                    _brush.GradientStops.Add(gs);
                    gstep2 = gs;

                    gs = new GradientStop() { Offset = 1, Color = viewmodel.backgroundColor };
                    _brush.GradientStops.Add(gs);

                }

                return _brush;
            }
        }
        public double len { get { return viewmodel.isFull ? viewmodel.len2 : viewmodel.len2 * maxValue / section.maxValue; } }
        public double width { get { return viewmodel.len2 * curValue / maxValue; } }

        [XmlIgnore]
        public Point center { get; set; }

        public double centerX { get { return center.X; } }
        public double centerY { get { return center.Y; } }
        public double locationX { get { return location.Left + (section.isLeft ? viewmodel.len2 : 0); } }
        public double locationY { get { return location.Top + viewmodel.height2 / 2; } }


        Thickness _location;
        [XmlIgnore]
        public Thickness location
        {
            get { return _location; }
            set
            {
                _location = value;
                RaisePropertyChanged(() => location);
                RaisePropertyChanged(() => centerX);
                RaisePropertyChanged(() => centerY);
                RaisePropertyChanged(() => locationX);
                RaisePropertyChanged(() => locationY);

                RaisePropertyChanged(() => lineMargin);
                RaisePropertyChanged(() => lineWidth);
                RaisePropertyChanged(() => lineHeight);
                RaisePropertyChanged(() => lineDir);
            }
        }

        public Thickness lineMargin { get { return new Thickness(Math.Min(center.X, locationX), Math.Min(center.Y, locationY), 0, 0); } }
        public double lineWidth { get { return Math.Max(center.X, locationX) - Math.Min(center.X, locationX); } }
        public double lineHeight { get { return Math.Max(center.Y, locationY) - Math.Min(center.Y, locationY); } }
        public Microsoft.Expression.Media.CornerType lineDir
        {
            get
            {
                if (center.X < locationX)
                {
                    if (center.Y > locationY)
                        return Microsoft.Expression.Media.CornerType.TopRight;
                    else
                        return Microsoft.Expression.Media.CornerType.BottomRight;
                }
                else
                {
                    if (center.Y > locationY)
                        return Microsoft.Expression.Media.CornerType.TopLeft;
                    else
                        return Microsoft.Expression.Media.CornerType.BottomLeft;

                }
            }
        }



        Grid _vInfo;
        [XmlIgnore]
        public Grid vInfo
        {
            get
            {
                if (_vInfo == null)
                    InitVisualControl();
                return _vInfo;
            }
        }
        [XmlIgnore]
        public string info { get { return name + ":" + (viewmodel.isFull ? (curValue / maxValue).ToString("p1") : (curValue.ToString("f0") + "/" + maxValue.ToString("f0"))); } }


        //[XmlIgnore]
        //public Line vLinkLine { get; set; }

        [XmlIgnore]
        public Microsoft.Expression.Controls.LineArrow vLinkLine { get; set; }


        public void Initialize()
        {

        }

        public void InitVisualControl()
        {
            _vInfo = new Grid() { IsHitTestVisible = false };
            Grid grd = new Grid() { Height = viewmodel.height2, Width = viewmodel.len2, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
            Border brdback = new Border() { Background = viewmodel.background, Width = viewmodel.len2, Height = viewmodel.height2, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, CornerRadius = new CornerRadius(10), BorderThickness = new Thickness(0.5), BorderBrush = Brushes.Blue };
            //brdback.Effect = new System.Windows.Media.Effects.DropShadowEffect() ;
            Border brd = new Border() { Background = brush, Width = viewmodel.len2, Height = viewmodel.height2, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, CornerRadius = new CornerRadius(10), BorderThickness = new Thickness(0.5), BorderBrush = Brushes.Blue };
            OperateHelper.bind(this, "location", _vInfo, Border.MarginProperty);

            TextBlock txt = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(5, 1, 0, 0), Foreground = viewmodel.fontBrush, FontSize = viewmodel.fontSize2 };
            OperateHelper.bind(this, "info", txt, TextBlock.TextProperty);

            //vLinkLine = new Line() { StrokeThickness = 2, Stroke = viewmodel.lineBrush };
            //vLinkLine.Effect = new System.Windows.Media.Effects.DropShadowEffect();
            //OperateHelper.bind(this, "locationX", vLinkLine, Line.X1Property);
            //OperateHelper.bind(this, "locationY", vLinkLine, Line.Y1Property);
            //OperateHelper.bind(this, "centerX", vLinkLine, Line.X2Property);
            //OperateHelper.bind(this, "centerY", vLinkLine, Line.Y2Property);
            vLinkLine = new Microsoft.Expression.Controls.LineArrow() { StrokeThickness = 1, Stroke = viewmodel.lineBrush, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left, ArrowSize = 5, EndArrow = Microsoft.Expression.Media.ArrowType.StealthArrow };
            OperateHelper.bind(this, "lineMargin", vLinkLine, Microsoft.Expression.Controls.LineArrow.MarginProperty);
            OperateHelper.bind(this, "lineWidth", vLinkLine, Microsoft.Expression.Controls.LineArrow.WidthProperty);
            OperateHelper.bind(this, "lineHeight", vLinkLine, Microsoft.Expression.Controls.LineArrow.HeightProperty);
            OperateHelper.bind(this, "lineDir",vLinkLine,Microsoft.Expression.Controls.LineArrow.StartCornerProperty);
            




            Border brd2 = new Border() { Width = viewmodel.len2, Height = viewmodel.height2, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, CornerRadius = new CornerRadius(10) };
            brd2.Background = viewmodel.brushCover;
            grd.Children.Add(brdback);
            grd.Children.Add(brd);
            grd.Children.Add(txt);
            grd.Children.Add(brd2);
            _vInfo.Children.Add(grd);
            //vInfo.Child = txt;

        }


        public void refreshData()
        {
            RaisePropertyChanged(() => valuecolor);
            RaisePropertyChanged(() => scale);
            RaisePropertyChanged(() => info);

            DoubleAnimation ani = new DoubleAnimation(scale, TimeSpan.FromSeconds(0.2), FillBehavior.HoldEnd);
            gstep1.BeginAnimation(GradientStop.OffsetProperty, ani);
            gstep2.BeginAnimation(GradientStop.OffsetProperty, ani);

        }


    }


    #region 其它辅助类
    #region mvvm
    public class DependencyNotificationObject : DependencyObject, INotifyPropertyChanged
    {
        protected void RaisePropertyChanged<T>(Expression<Func<T>> action)
        {
            var propertyName = GetPropertyName(action);
            RaisePropertyChanged(propertyName);
        }

        private static string GetPropertyName<T>(Expression<Func<T>> action)
        {
            var expression = (MemberExpression)action.Body;
            var propertyName = expression.Member.Name;
            return propertyName;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    #endregion

    #region 序列化和反序列化
    public static class XmlHelper
    {
        /// <summary>
        /// XML序列化
        /// </summary>
        /// <param name="xmlfilename">保存的xml文件全名</param>
        /// <param name="obj">将xml序列化的对象</param>
        public static void saveToXml(string xmlfilename, object obj)
        {
            if (string.IsNullOrWhiteSpace(xmlfilename)) return;

            string bakfilename = xmlfilename.Replace(".xml", ".bak");
            if (File.Exists(xmlfilename))
                File.Copy(xmlfilename, bakfilename, true);
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            Stream fs = new FileStream(xmlfilename, FileMode.Create);
            //XmlWriter writer = new XmlTextWriter(fs, Encoding.Unicode);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(fs, settings);

            serializer.Serialize(writer, obj);
            writer.Close();
            fs.Close();
        }

        /// <summary>
        /// xml反序列化
        /// </summary>
        /// <param name="xmlfilename">XML文件全名</param>
        /// <param name="type">反序列化后的对象类型</param>
        /// <returns></returns>
        public static object readFromXml(string xmlfilename, Type type)
        {
            object obj = null;

            if (xmlfilename.IndexOf("http:") < 0)
            {
                if (File.Exists(xmlfilename))
                {
                    XmlSerializer serializer = new XmlSerializer(type);

                    FileStream fs = new FileStream(xmlfilename, FileMode.Open);
                    XmlReader reader = XmlReader.Create(fs);
                    obj = serializer.Deserialize(reader);
                    reader.Close();
                    fs.Close();
                }
            }
            else
            {
                try
                {
                    XmlUrlResolver resolver = new XmlUrlResolver();
                    resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.XmlResolver = resolver;
                    XmlReader reader = XmlReader.Create(xmlfilename, settings);
                    XmlSerializer serializer = new XmlSerializer(type);
                    obj = serializer.Deserialize(reader);
                    reader.Close();
                }
                catch
                { }
            }
            return obj;
        }




    }

    #endregion

    public static class helper
    {
        public static string getGUID()
        {
            System.Guid guid = new Guid();
            guid = Guid.NewGuid();
            string str = guid.ToString();
            return str;
        }
    }

    public static class OperateHelper
    {
        public static void bind(object src, string srcpath, DependencyObject obj, DependencyProperty objp)
        {
            Binding bind = new Binding();
            bind.Source = src;
            bind.Path = new PropertyPath(srcpath);
            bind.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(obj, objp, bind);
        }
        public static void bind(object src, DependencyProperty srcpath, DependencyObject obj, DependencyProperty objp, BindingMode bindmode)
        {
            Binding bind = new Binding();
            bind.Source = src;
            bind.Path = new PropertyPath(srcpath);
            bind.Mode = bindmode;
            BindingOperations.SetBinding(obj, objp, bind);
        }
        public static void bind(object src, string srcpath, DependencyObject obj, DependencyProperty objp, BindingMode bindmode)
        {
            Binding bind = new Binding();
            bind.Source = src;
            bind.Path = new PropertyPath(srcpath);
            bind.Mode = bindmode;
            BindingOperations.SetBinding(obj, objp, bind);
        }
        public static void bind(object src, DependencyProperty srcp, DependencyObject obj, DependencyProperty objp)
        {
            Binding bind = new Binding();
            bind.Source = src;
            bind.Path = new PropertyPath(srcp);
            bind.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(obj, objp, bind);
        }

        public static void bind(object src, string srcpath, DependencyObject obj, DependencyProperty objp, Type converttype, object convertpara, BindingMode bindmode)
        {
            Binding bind;
            bind = new Binding();
            bind.Converter = (IValueConverter)Activator.CreateInstance(converttype); ;
            bind.ConverterParameter = convertpara;
            bind.Source = src;
            bind.Path = new PropertyPath(srcpath);
            bind.Mode = bindmode;
            BindingOperations.SetBinding(obj, objp, bind);
        }

        public static void bind(object src, DependencyProperty srcp, DependencyObject obj, DependencyProperty objp, Type converttype, object convertpara, BindingMode bindmode)
        {
            Binding bind;
            bind = new Binding();
            bind.Converter = (IValueConverter)Activator.CreateInstance(converttype); ;
            bind.ConverterParameter = convertpara;
            bind.Source = src;
            bind.Path = new PropertyPath(srcp);
            bind.Mode = bindmode;
            BindingOperations.SetBinding(obj, objp, bind);
        }


        /// <summary>
        /// 判断两条线是否相交
        /// </summary>
        /// <param name="a">线段1起点坐标</param>
        /// <param name="b">线段1终点坐标</param>
        /// <param name="c">线段2起点坐标</param>
        /// <param name="d">线段2终点坐标</param>
        /// <param name="intersection">相交点坐标</param>
        /// <returns>是否相交 0:两线平行  -1:不平行且未相交  1:两线相交</returns>
        public static int GetIntersection(Point a, Point b, Point c, Point d, ref Point intersection)
        {
            //判断异常
            if (Math.Abs(b.X - a.Y) + Math.Abs(b.X - a.X) + Math.Abs(d.Y - c.Y) + Math.Abs(d.X - c.X) == 0)
            {
                if (c.X - a.X == 0)
                {
                    //Debug.Print("ABCD是同一个点！");
                }
                else
                {
                    //Debug.Print("AB是一个点，CD是一个点，且AC不同！");
                }
                return 0;
            }

            if (Math.Abs(b.Y - a.Y) + Math.Abs(b.X - a.X) == 0)
            {
                if ((a.X - d.X) * (c.Y - d.Y) - (a.Y - d.Y) * (c.X - d.X) == 0)
                {
                    //Debug.Print("A、B是一个点，且在CD线段上！");
                }
                else
                {
                    //Debug.Print("A、B是一个点，且不在CD线段上！");
                }
                return 0;
            }
            if (Math.Abs(d.Y - c.Y) + Math.Abs(d.X - c.X) == 0)
            {
                if ((d.X - b.X) * (a.Y - b.Y) - (d.Y - b.Y) * (a.X - b.X) == 0)
                {
                    //Debug.Print("C、D是一个点，且在AB线段上！");
                }
                else
                {
                    //Debug.Print("C、D是一个点，且不在AB线段上！");
                }
            }

            if ((b.Y - a.Y) * (c.X - d.X) - (b.X - a.X) * (c.Y - d.Y) == 0)
            {
                //Debug.Print("线段平行，无交点！");
                return 0;
            }

            intersection.X = ((b.X - a.X) * (c.X - d.X) * (c.Y - a.Y) - c.X * (b.X - a.X) * (c.Y - d.Y) + a.X * (b.Y - a.Y) * (c.X - d.X)) / ((b.Y - a.Y) * (c.X - d.X) - (b.X - a.X) * (c.Y - d.Y));
            intersection.Y = ((b.Y - a.Y) * (c.Y - d.Y) * (c.X - a.X) - c.Y * (b.Y - a.Y) * (c.X - d.X) + a.Y * (b.X - a.X) * (c.Y - d.Y)) / ((b.X - a.X) * (c.Y - d.Y) - (b.Y - a.Y) * (c.X - d.X));

            if ((intersection.X - a.X) * (intersection.X - b.X) <= 0 && (intersection.X - c.X) * (intersection.X - d.X) <= 0 && (intersection.Y - a.Y) * (intersection.Y - b.Y) <= 0 && (intersection.Y - c.Y) * (intersection.Y - d.Y) <= 0)
            {
                //Debug.Print("线段相交于点(" + intersection.X + "," + intersection.Y + ")！");
                return 1; //'相交
            }
            else
            {
                //Debug.Print("线段相交于虚交点(" + intersection.X + "," + intersection.Y + ")！");
                return -1; //'相交但不在线段上
            }
        }

    }

    public class DelegateCommand : ICommand
    {
        private readonly Action _command;
        private readonly Func<bool> _canExecute;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public DelegateCommand(Action command, Func<bool> canExecute = null)
        {
            if (command == null)
                throw new ArgumentNullException();
            _canExecute = canExecute;
            _command = command;
        }

        public void Execute(object parameter)
        {
            _command();
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

    }

    #region 转换器
    /// <summary>
    /// 列表selectindex与是否选择的转换器
    /// </summary>
    [ValueConversion(typeof(int), typeof(bool))]
    public class SelectIndexToIsSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int selectindex = (int)value;
            return selectindex > -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = (bool)value;
            return isSelected ? 0 : -1;
        }

    }
    #endregion

    #endregion
    [Serializable]
    public class Para
    {
        [CategoryAttribute("外观")]
        [DisplayNameAttribute("是否等长")]
        [Description("是否填充为百分比同等长度形式")]
        public bool isFull { get; set; } //是否填充为百分比同等长度形式

        [CategoryAttribute("断面信息外观")]
        [DisplayNameAttribute("长度")]
        [Description("断面信息条长度")]
        public double len { get; set; } //长度

        [CategoryAttribute("线路信息外观")]
        [DisplayNameAttribute("长度")]
        [Description("线路信息条长度")]
        public double len2 { get; set; } //地图中线路信息条长度

        [CategoryAttribute("断面信息外观")]
        [DisplayNameAttribute("高度")]
        [Description("断面信息条的高度")]
        public double height { get; set; } //右侧列表中断面条高度

        [CategoryAttribute("线路信息外观")]
        [DisplayNameAttribute("高度")]
        [Description("线路信息条的高度")]
        public double height2 { get; set; } //地图中线路条高度

        [CategoryAttribute("外观")]
        [DisplayNameAttribute("底色")]
        [Description("信息条底色")]
        public Color backgroundColor { get; set; } //底色

        [CategoryAttribute("业务逻辑")]
        [DisplayNameAttribute("轻载色")]
        [Description("轻载时填充的颜色")]
        public Color lightColor { get; set; } //轻载色

        [CategoryAttribute("业务逻辑")]
        [DisplayNameAttribute("普通色")]
        [Description("正常填充的颜色")]
        public Color normalColor { get; set; } //普通色

        [CategoryAttribute("业务逻辑")]
        [DisplayNameAttribute("重载色")]
        [Description("重载时填充的颜色")]
        public Color weightColor { get; set; } //重载色

        [CategoryAttribute("外观")]
        [DisplayNameAttribute("文字颜色")]
        [Description("信息条中说明文字的颜色")]
        public Color fontColor { get; set; }

        [CategoryAttribute("断面信息外观")]
        [DisplayNameAttribute("文字大小")]
        [Description("信息条中说明文字的字号")]
        public double fontSize { get; set; }

        [CategoryAttribute("线路信息外观")]
        [DisplayNameAttribute("文字大小")]
        [Description("信息条中说明文字的字号")]
        public double fontSize2 { get; set; }


        [CategoryAttribute("外观")]
        [DisplayNameAttribute("连线颜色")]
        [Description("线路与信息条之间连接线的颜色")]
        public Color lineColor { get; set; }

        [CategoryAttribute("业务逻辑")]
        [DisplayNameAttribute("轻载率")]
        [Description("低于此百分比数值表示轻载，范围0-1")]
        public double lightScale { get; set; } //轻载率

        [CategoryAttribute("业务逻辑")]
        [DisplayNameAttribute("重载率")]
        [Description("高于此百分比数值表示重载，范围0-1")]
        public double weightScale { get; set; } //重载率

        [CategoryAttribute("外观")]
        [DisplayNameAttribute("间隔")]
        [Description("断面中多条线路信息条之间的间隔")]
        public double span { get; set; } //地图信息条间隔

        [CategoryAttribute("线路信息外观")]
        [DisplayNameAttribute("显示断面名称")]
        [Description("是否在线路顶部显示断面名称")]
        public bool isShowSectionName { get; set; }

        [CategoryAttribute("线路信息外观")]
        [DisplayNameAttribute("断面标题大小")]
        [Description("断面名称文字的字号")]
        public double fontSizeSectionName { get; set; }

        [CategoryAttribute("线路信息外观")]
        [DisplayNameAttribute("断面标题颜色")]
        [Description("断面标题文字的颜色")]
        public Color fontColorSectionName { get; set; }

        [CategoryAttribute("外观")]
        [DisplayNameAttribute("显示所有潮流")]
        [Description("是否显示所有潮流，否则仅显示断面潮流")]
        public bool isShowAllArrow { get; set; }

    }

}
