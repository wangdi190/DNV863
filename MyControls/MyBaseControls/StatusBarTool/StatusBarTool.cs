using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using System.Xml.Serialization;

namespace MyBaseControls.StatusBarTool
{
    public enum EIcon { 无, 坐标, 提示, 告警, 信息 }
    public enum EEffect { 无, 淡入, 持续闪烁, 闪烁5次, 粗字体, 斜字体 }

    public static class StatusBarTool
    {
        static StatusBarTool()
        {
            statusBar = new StatusBar() { Height = 24, VerticalAlignment = System.Windows.VerticalAlignment.Bottom, Visibility = System.Windows.Visibility.Collapsed };
            LinearGradientBrush background = new LinearGradientBrush() { EndPoint = new System.Windows.Point(0, 1) };
            background.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#d7e6f9"), 0));
            background.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#c7dcf8"), 0.3));
            background.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#b3d0f5"), 0.3));
            background.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#cde0f7"), 1));
            statusBar.Background = background;
            statusBar.Effect = new System.Windows.Media.Effects.DropShadowEffect() { Direction = 90, ShadowDepth = 0, BlurRadius = 6 };


            infos = new List<InfoBase>();
            statusBar.Loaded += new System.Windows.RoutedEventHandler(statusBar_Loaded);

        }

        static void statusBar_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (isEnable)
                adjust();
        }

        ///<summary>是否使状态栏生效</summary>
        public static bool isEnable
        {
            get { return statusBar.Visibility == System.Windows.Visibility.Visible; }
            set
            {
                statusBar.Visibility = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                if (value)
                    adjust();
            }
        }


        ///<summary>状态栏控件，放置于界面中</summary>
        public static StatusBar statusBar { get; set; }


        ///<summary>状态栏信息类的列类集合，用于用户添加自定义状态栏信息</summary>
        public static List<InfoBase> infos { get; set; }

        private static RealTimeInfo _realTimeInfo;
        ///<summary>实时性信息，用于显示如时间、坐标</summary>
        public static RealTimeInfo realTimeInfo
        {
            get { if (_realTimeInfo == null) { _realTimeInfo = new RealTimeInfo(); infos.Add(_realTimeInfo); } return _realTimeInfo; }
            set { _realTimeInfo = value; }
        }

        private static ReportInfo _reportInfo;
        ///<summary>程序报告信息，用于显示如操作结果、载入等待、异步完成等，支持时效特性</summary>     
        public static ReportInfo reportInfo
        {
            get { if (_reportInfo == null) { _reportInfo = new ReportInfo(); infos.Add(_reportInfo); } return _reportInfo; }
            set { _reportInfo = value; }
        }


        private static TipsInfo _tipsInfo;
        ///<summary>提示性信息，用于显示界面操作提示、即时帮助</summary>
        public static TipsInfo tipsInfo
        {
            get { if (_tipsInfo == null) { _tipsInfo = new TipsInfo(); infos.Add(_tipsInfo); } return _tipsInfo; }
            set { _tipsInfo = value; }
        }

        private static StatusInfo _statusInfo;
        ///<summary>状态信息，用于显示状态，适用于实时监视</summary>
        public static StatusInfo statusInfo
        {
            get { if (_statusInfo == null) { _statusInfo = new StatusInfo(); infos.Add(_statusInfo); } return _statusInfo; }
            set { _statusInfo = value; }
        }

        private static DebugInfo _debugInfo;
        ///<summary>调试信息，给开发者使用</summary>
        public static DebugInfo debugInfo
        {
            get { if (_debugInfo == null) { _debugInfo = new DebugInfo(); infos.Add(_debugInfo); } return _debugInfo; }
            set { _debugInfo = value; }
        }

        ///<summary>调整设置状态栏内容和分布</summary>
        internal static void adjust()
        {
            statusBar.Items.Clear();
            if (statusBar.ActualWidth == 0) return;
            if (infos.Count(p => p.isVisible && p.content != null) == 0) return;

            //加入状态栏
            var tmp = infos.Where(p => p.isVisible && p.content != null);
            System.Windows.Controls.Separator sep = new System.Windows.Controls.Separator();
            foreach (var item in tmp.OrderBy(p => p.order))
            {
                if (!item.Equals(tmp.First()))
                {
                    sep = new System.Windows.Controls.Separator();
                    statusBar.Items.Add(sep);
                }
                statusBar.Items.Add(item.statusbaritem);
            }
            int count = tmp.Count();
            //设置固定宽度项
            tmp = infos.Where(p => p.isVisible && p.width >= 1 && p.content != null);
            foreach (var item in tmp)
                item.panel.Width = item.width;
            //设置动态宽度项
            double dynlen = statusBar.ActualWidth - tmp.Sum(p => p.width) - (count - 1) * 20;//sep.ActualWidth;
            if (dynlen <= 0) return;
            double all = infos.Where(p => p.isVisible && p.width < 1 && p.content != null).Sum(p => p.width);
            tmp = infos.Where(p => p.isVisible && p.width < 1 && p.content != null);
            foreach (var item in tmp)
                item.panel.Width = item.width / all * dynlen;


        }
    }

    public class InfoBase
    {

        private bool _isVisible;
        ///<summary>是否显示在状态栏中</summary>
        public bool isVisible
        {
            get { return _isVisible; }
            set
            {
                if (StatusBarTool.isEnable && _isVisible != value)
                {
                    _isVisible = value;
                    StatusBarTool.adjust();
                }
                else
                    _isVisible = value;
            }
        }


        private double _width = 0.2;
        ///<summary>该信息栏宽度，若大于等于1，表示固定宽度，若小于1则与其余动态宽度共同决定实际宽度</summary>
        public double width
        {
            get { return _width; }
            set { _width = value; }
        }

        ///<summary>在状态栏中的位置序号</summary>
        public int order { get; set; }

        System.Windows.FrameworkElement _content;
        ///<summary>状态栏内容</summary>
        public System.Windows.FrameworkElement content { get { return _content; } set { _content = value; buildcontent(); } }


        internal StatusBarItem statusbaritem = new StatusBarItem();
        internal StackPanel panel = new StackPanel() { Orientation = Orientation.Horizontal };

        #region ----- 图标处理 -----

        private EIcon _iconselect;
        ///<summary>内置图标</summary>
        public EIcon iconselect
        {
            get { return _iconselect; }
            set
            {
                _iconselect = value;
                switch (value)
                {
                    case EIcon.无:
                        icon = null;
                        break;
                    case EIcon.坐标:
                        icon = geticonbrush("point.png");
                        break;
                    case EIcon.提示:
                        icon = geticonbrush("tips.png");
                        break;
                    case EIcon.告警:
                        icon = geticonbrush("warning.png");
                        break;
                    case EIcon.信息:
                        icon = geticonbrush("info.png");
                        break;
                }
            }
        }
        Brush geticonbrush(string name)
        {
            BitmapImage bi = new BitmapImage();
            Uri overlayuri = new Uri("pack://application:,,,/MyBaseControls;component/StatusBarTool/icon/" + name);
            bi.BeginInit();
            bi.UriSource = overlayuri;
            bi.EndInit();
            return new ImageBrush(bi);
        }


        private Brush _icon;
        ///<summary>图标，可直接赋与brush</summary>
        public Brush icon
        {
            get { return _icon; }
            set { _icon = value; if (value != null) recticon.Fill = value; buildcontent(); }
        }

        System.Windows.Shapes.Rectangle recticon = new System.Windows.Shapes.Rectangle()
        {
            Width = 16,
            Height = 16,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            Effect = new System.Windows.Media.Effects.DropShadowEffect() { ShadowDepth = 0, BlurRadius = 3 },
            Margin = new System.Windows.Thickness(0, 0, 5, 0)
        };


        void buildcontent()
        {
            statusbaritem.Content = panel;
            panel.Children.Clear();
            if (icon != null)
                panel.Children.Add(recticon);
            panel.Children.Add(content);
        }
        #endregion


        #region ----- 效果动画 -----
        DoubleAnimation anieffect = new DoubleAnimation() { Duration = TimeSpan.FromSeconds(1) };
        public EEffect effect { get; set; }

        protected void doeffect()
        {
            switch (effect)
            {
                case EEffect.无:
                    break;
                case EEffect.淡入:
                    anieffect.From = 0;
                    anieffect.To = 1;
                    anieffect.FillBehavior = FillBehavior.Stop;
                    content.BeginAnimation(System.Windows.FrameworkElement.OpacityProperty, anieffect);
                    break;
                case EEffect.持续闪烁:
                    anieffect.From = 0;
                    anieffect.To = 1;
                    anieffect.FillBehavior = FillBehavior.Stop;
                    anieffect.RepeatBehavior = RepeatBehavior.Forever;
                    anieffect.AutoReverse = true;
                    content.BeginAnimation(System.Windows.FrameworkElement.OpacityProperty, anieffect);
                    break;
                case EEffect.闪烁5次:
                    anieffect.From = 0;
                    anieffect.To = 1;
                    anieffect.FillBehavior = FillBehavior.Stop;
                    anieffect.RepeatBehavior = new RepeatBehavior(5);
                    anieffect.AutoReverse = true;
                    content.BeginAnimation(System.Windows.FrameworkElement.OpacityProperty, anieffect);
                    break;
                case EEffect.粗字体:
                    break;
                case EEffect.斜字体:
                    break;
                default:
                    break;
            }
        }
        #endregion
    }

    public class RealTimeInfo : InfoBase
    {
        public RealTimeInfo()
        {
            content = txt;
            order = 0;
        }

        TextBlock txt = new TextBlock() { Padding = new System.Windows.Thickness(0), Foreground = Brushes.Navy };
        public void showInfo(string text)
        {
            if (!content.HasAnimatedProperties)
                doeffect();
            txt.Text = text;
        }

    }

    ///<summary>支持具有时效性的信息</summary>
    public class ReportInfo : InfoBase
    {
        public ReportInfo()
        {
            content = txt;
            tmr.Tick += new EventHandler(tmr_Tick);
            panel.Visibility = System.Windows.Visibility.Hidden;
            order = 10;
        }

        void tmr_Tick(object sender, EventArgs e)
        {
            txt.Text = "";
            panel.Visibility = System.Windows.Visibility.Hidden;
        }

        TextBlock txt = new TextBlock() { Padding = new System.Windows.Thickness(0), Foreground = Brushes.Navy };
        System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer();

        ///<summary>timeout信息有效时间（秒），小于等于0为永久直到被下一条信息冲掉</summary>
        public void showInfo(string text, int timeout)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                txt.Text = "";
                panel.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                tmr.Stop();
                txt.Text = text;
                panel.Visibility = System.Windows.Visibility.Visible;
                doeffect();
                if (timeout > 0)
                {
                    tmr.Interval = TimeSpan.FromSeconds(timeout);
                    tmr.Start();

                }
            }
        }

    }

    ///<summary>集中填写的信息集合，多条轮显</summary>
    public class TipsInfo : InfoBase
    {
        public TipsInfo()
        {
            order = 20;
            content = new Grid();
            (content as Grid).Children.Add(view);

            view.Content = spanel;


            spanel.Children.Add(txt);
            spanel.Children.Add(txt2);
            spanel.Loaded += new System.Windows.RoutedEventHandler(panel_Loaded);
            spanel.RenderTransform = transform;

            tmr.Interval = TimeSpan.FromSeconds(10);
            tmr.Tick += new EventHandler(tmr_Tick);

            ani.Duration = TimeSpan.FromSeconds(1);
            ani.FillBehavior = FillBehavior.Stop;
            ani.Completed += new EventHandler(ani_Completed);

            panel.Visibility = System.Windows.Visibility.Hidden;
        }

        void panel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            content.Height = txt.ActualHeight;
            view.Height = txt.ActualHeight * 2;
        }

        void tmr_Tick(object sender, EventArgs e)
        {
            txt.Text = infos[curidx].info;
            txt2.Text = infos[curidx == infos.Count - 1 ? 0 : curidx + 1].info;
            ani.To = -1 * txt.ActualHeight;
            transform.BeginAnimation(TranslateTransform.YProperty, ani);
        }

        void ani_Completed(object sender, EventArgs e)
        {
            txt.Text = txt2.Text;
            curidx = (curidx == infos.Count - 1 ? 0 : curidx + 1);
        }

        DoubleAnimation ani = new DoubleAnimation();
        ScrollViewer view = new ScrollViewer() { VerticalScrollBarVisibility = ScrollBarVisibility.Hidden };
        StackPanel spanel = new StackPanel();
        TranslateTransform transform = new TranslateTransform();
        TextBlock txt = new TextBlock() { Padding = new System.Windows.Thickness(0), Foreground = Brushes.Navy };
        TextBlock txt2 = new TextBlock() { Padding = new System.Windows.Thickness(0), Foreground = Brushes.Navy };
        System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer();
        int curidx;

        List<Info> allinfos = new List<Info>();
        List<Info> infos = new List<Info>();
        System.Collections.Stack stack = new System.Collections.Stack();

        private string _curDomain;
        ///<summary>程序当前域,设置程序当前域可更新为显示指定域的提示信息，若无指定域信息，则tips为空</summary>
        public string curDomain
        {
            get { return _curDomain; }
            set { _curDomain = value; setCurDomain(value); }
        }

        void setCurDomain(string domain)
        {
            _curDomain = domain;
            infos.Clear();
            var tmp = allinfos.Where(p => p.domain == domain);
            foreach (var item in tmp)
                infos.Add(item);
            if (infos.Count == 0)
            {
                tmr.Stop();
                txt.Text = "";
                txt2.Text = "";
                panel.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                tmr.Stop();
                curidx = 0;
                txt.Text = infos[0].info;
                panel.Visibility = System.Windows.Visibility.Visible;
                if (infos.Count > 1)
                    tmr.Start();
            }
        }


        private int _looptime;
        ///<summary>信息轮换时间（秒）</summary>
        public int timeout
        {
            get { return _looptime; }
            set { _looptime = value; tmr.Interval = TimeSpan.FromSeconds(value); }
        }

        ///<summary>添加提示信息，需指定所属程序域</summary>
        public void addInfo(string info, string domain)
        {
            allinfos.Add(new Info() { domain = domain, info = info });
        }


        ///<summary>清除指定域的tips，若不指定则全部清除</summary>
        public void clearInfo(string domain = null)
        {
            if (string.IsNullOrWhiteSpace(domain))
                allinfos.Clear();
            else
                allinfos.RemoveAll(p => p.domain == domain);
        }

        ///<summary>将当前程序域压到堆栈备用。push和pop适用于保存域信息，当返回上级界面时可pop恢复上级域提示信息</summary>
        public void push()
        {
            stack.Push(curDomain);
        }
        ///<summary>将堆栈中的程序域取回，请确保push和pop配对使用。</summary>
        public void pop()
        {
            curDomain = (string)stack.Pop();
        }

        public void saveToXml()
        {
            MyClassLibrary.XmlHelper.saveToXml(".\\xml\\tips.xml", allinfos);
        }

        public void loadFromXml()
        {
            //allinfos = (List<Info>)MyClassLibrary.XmlHelper.readFromXml(".\\xml\\tips.xml", allinfos.GetType());
            allinfos = (List<Info>)MyClassLibrary.XmlHelper.readFromXml(AppDomain.CurrentDomain.BaseDirectory+ "\\xml\\tips.xml", allinfos.GetType()); //xbap
        }

    }

    public class Info
    {
        [XmlAttribute]
        public string domain { get; set; }
        [XmlAttribute]
        public string info { get; set; }
    }



    public class DebugInfo : InfoBase
    {
        public DebugInfo()
        {
            order = 100;
            content = spanel;
            spanel.Children.Add(btn);
            spanel.Children.Add(txt);
            btn.Click += new System.Windows.RoutedEventHandler(btn_Click);
        }

        void btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            winDebug win = new winDebug();
            win.txt.Text = txt.Text;
            win.ShowDialog();
        }

        StackPanel spanel = new StackPanel() { Orientation = Orientation.Horizontal };
        Button btn = new Button() { Content = "..." };
        TextBlock txt = new TextBlock() { Padding = new System.Windows.Thickness(0), Foreground = Brushes.Navy };


        public void showInfo(string text)
        {
            txt.Text = text;
            panel.Visibility = System.Windows.Visibility.Visible;
        }

    }




    public class StatusInfo : InfoBase
    {
        public StatusInfo()
        {
            order = 1000;
            content = spanel;
            width = 80;
        }

        internal StackPanel spanel = new StackPanel() { Orientation = Orientation.Horizontal };


        private CalStatus _calStatus;
        ///<summary>计算状态</summary>
        public CalStatus calStatus
        {
            get { if (_calStatus == null) _calStatus = new CalStatus(); return _calStatus; }
            set { _calStatus = value; }
        }

        private WarningStatus _warningStatus;
        ///<summary>告警状态，若有后台监视程序，可使用告警状态</summary>
        public WarningStatus warningStatus
        {
            get { if (_warningStatus == null) _warningStatus = new WarningStatus(); return _warningStatus; }
            set { _warningStatus = value; }
        }

        private OnlineStatus _onlineStatus;
        ///<summary>连接状态，暂无应用未完成</summary>
        public OnlineStatus onlineStatus
        {
            get { if (_onlineStatus == null) _onlineStatus = new OnlineStatus(); return _onlineStatus; }
            set { _onlineStatus = value; }
        }

    }

    public abstract class StatusBase
    {
        public StatusBase()
        {
            rect = new Border() { Width = 18, Height = 18, Margin = new System.Windows.Thickness(2, 0, 2, 0) };
        }

        internal Border rect { get; set; }
        internal Brush brush { get; set; }

        private bool _isVisible;
        internal bool isVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value; if (isVisible && !StatusBarTool.statusInfo.spanel.Children.Contains(rect))
                    StatusBarTool.statusInfo.spanel.Children.Add(rect);
                else if (!isVisible && StatusBarTool.statusInfo.spanel.Children.Contains(rect))
                    StatusBarTool.statusInfo.spanel.Children.Remove(rect);
            }
        }



        protected Brush geticonbrush(string name)
        {
            BitmapImage bi = new BitmapImage();
            Uri overlayuri = new Uri("pack://application:,,,/MyBaseControls;component/StatusBarTool/icon/" + name);
            bi.BeginInit();
            bi.UriSource = overlayuri;
            bi.EndInit();
            return new ImageBrush(bi);
        }

    }


    public class CalStatus : StatusBase
    {
        public CalStatus()
            : base()
        {
            rect.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            rect.RenderTransform = transform;
            eps.Fill = Brushes.DarkGreen;
            rect.Child = eps;

            status = EStatus.隐藏;
            rect.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(rect_MouseLeftButtonUp);
        }

        ///<summary>单击图标, 执行计算完成后的委托</summary>
        void rect_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            foreach (var item in tasks.Where(p => p.Value.status == EStatus.计算完成))
            {
                if (item.Value.completed != null)
                {
                    item.Value.completed();
                    item.Value.status = EStatus.无计算;
                }
            }
        }

        System.Windows.Shapes.Ellipse eps = new System.Windows.Shapes.Ellipse() { Width = 8, Height = 8 };

        public enum EStatus { 隐藏, 无计算, 计算中, 计算完成 }

        ///<summary>添加并返回计算任务，若已存在，将返回已存在任务</summary>
        public CalTask addCalTask(string calkey, CalTask.CompletedDelegate delegatemothod)
        {
            CalTask task;
            if (!tasks.TryGetValue(calkey, out task))
            {
                task = new CalTask(delegatemothod);
                tasks.Add(calkey, task);
            }
            refresh();
            return task;
        }

        internal void refresh()
        {
            if (tasks.Values.FirstOrDefault(p => p.status == EStatus.计算完成) != null)
                status = EStatus.计算完成;
            else if (tasks.Values.FirstOrDefault(p => p.status == EStatus.计算中) != null)
                status = EStatus.计算中;
            else if (tasks.Values.FirstOrDefault(p => p.status == EStatus.无计算) != null)
                status = EStatus.无计算;
            else
                status = EStatus.隐藏;

            string tip = "";
            foreach (var item in tasks.Where(p => p.Value.status == EStatus.计算完成))
                tip += (String.Format("{0}★{1}{2}", (string.IsNullOrWhiteSpace(tip) ? "" : Environment.NewLine), item.Key, item.Value.status));
            foreach (var item in tasks.Where(p => p.Value.status == EStatus.计算中))
                tip += (String.Format("{0}☆{1}{2}", (string.IsNullOrWhiteSpace(tip) ? "" : Environment.NewLine), item.Key, item.Value.status));
            tip = string.IsNullOrWhiteSpace(tip) ? "无计算" : tip;
            rect.ToolTip = tip;
        }

        public Dictionary<string, CalTask> tasks = new Dictionary<string, CalTask>();


        private EStatus _status;
        EStatus status
        {
            get { return _status; }
            set
            {
                _status = value;
                isVisible = value != EStatus.隐藏;
                transform.BeginAnimation(RotateTransform.AngleProperty, null);
                eps.BeginAnimation(System.Windows.Shapes.Rectangle.OpacityProperty, null);
                eps.Visibility = System.Windows.Visibility.Collapsed;
                switch (value)
                {
                    case EStatus.无计算:
                        brush = geticonbrush("cal3.png");
                        break;
                    case EStatus.计算中:
                        brush = geticonbrush("cal1.png");
                        ani.Duration = TimeSpan.FromSeconds(10);
                        ani.AutoReverse = false;
                        ani.From = 0;
                        ani.To = 360;
                        transform.BeginAnimation(RotateTransform.AngleProperty, ani);
                        break;
                    case EStatus.计算完成:
                        brush = geticonbrush("cal1.png");
                        ani.Duration = TimeSpan.FromSeconds(1);
                        ani.AutoReverse = true;
                        ani.From = 0.1;
                        ani.To = 1;
                        eps.BeginAnimation(System.Windows.Shapes.Rectangle.OpacityProperty, ani);
                        eps.Visibility = System.Windows.Visibility.Visible;
                        break;
                }
                rect.Background = brush;
                rect.ToolTip = value.ToString();
            }

        }


        DoubleAnimation ani = new DoubleAnimation() { RepeatBehavior = RepeatBehavior.Forever, From = 0 };
        RotateTransform transform = new RotateTransform();

    }
    public class CalTask
    {
        public CalTask(CompletedDelegate delegatemothod)
        {
            completed = delegatemothod;
            status = CalStatus.EStatus.无计算;
        }


        private CalStatus.EStatus _status;
        ///<summary>计算状态</summary>
        public CalStatus.EStatus status
        {
            get { return _status; }
            set { _status = value; StatusBarTool.statusInfo.calStatus.refresh(); }
        }


        public delegate void CompletedDelegate();
        public CompletedDelegate completed { get; set; }
    }



    public class WarningStatus : StatusBase
    {
        public WarningStatus()
            : base()
        {
            rect.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(rect_MouseLeftButtonUp);
        }

        void rect_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((status == EStatus.一般告警 || status == EStatus.严重告警) && gotoWarning != null)
                gotoWarning();
        }

        public enum EStatus { 隐藏, 无告警, 一般告警, 严重告警 }
        private EStatus _status;
        public EStatus status
        {
            get { return _status; }
            set
            {
                _status = value;
                isVisible = value != EStatus.隐藏;
                rect.Cursor = System.Windows.Input.Cursors.Arrow;
                rect.BeginAnimation(System.Windows.Shapes.Rectangle.OpacityProperty, null);
                switch (value)
                {
                    case EStatus.无告警:
                        brush = geticonbrush("warn1.png");
                        break;
                    case EStatus.一般告警:
                        brush = geticonbrush("warn2.png");
                        ani.Duration = TimeSpan.FromSeconds(1.5);
                        rect.BeginAnimation(System.Windows.Shapes.Rectangle.OpacityProperty, ani);
                        if (gotoWarning != null)
                            rect.Cursor = System.Windows.Input.Cursors.Hand;
                        break;
                    case EStatus.严重告警:
                        brush = geticonbrush("warn3.png");
                        ani.Duration = TimeSpan.FromSeconds(0.5);
                        rect.BeginAnimation(System.Windows.Shapes.Rectangle.OpacityProperty, ani);
                        if (gotoWarning != null)
                            rect.Cursor = System.Windows.Input.Cursors.Hand;
                        break;
                }
                rect.Background = brush;
                rect.ToolTip = value.ToString();
            }
        }

        DoubleAnimation ani = new DoubleAnimation() { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever, From = 0.2, To = 1 };

        public delegate bool GotoWarning();
        ///<summary>调用程序委托，点击图标调用,以前往处理或查看告警</summary>
        public GotoWarning gotoWarning { get; set; }

    }

    public class OnlineStatus : StatusBase
    {
        public enum EStatus { 隐藏, 连接, 断开 }
        private EStatus _status;
        public EStatus status
        {
            get { return _status; }
            set { _status = value; }
        }

    }


}
