using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfEarthLibrary
{
    public class LegendManager
    {
        public LegendManager(Earth Earth)
        {
            earth = Earth;
            isShow = false;
        }

        Earth earth;

        private bool _isShow;
        ///<summary>图例面板是否可见，缺省false</summary>
        public bool isShow
        {
            get { return _isShow; }
            set { _isShow = value; earth.grdlegend.Visibility = (value ? Visibility.Visible : Visibility.Collapsed); }
        }

        ///<summary>逐项隐藏各类图例,可在要打开某单一图例前调用</summary>
        public void hideAllLegend()
        {
            foreach (var item in legends.Values)
            {
                item.isShow = false;
            }
        }

        public Dictionary<string, LegendBase> legends = new Dictionary<string, LegendBase>();

        ///<summary>创建新的画刷图例，若已有指定键值图例，返回错误</summary>
        public BrushLegend createBrushLegend(string legendkey)
        {
            BrushLegend legend = new BrushLegend(earth);
            legends.Add(legendkey, legend);
            return legend;
        }

        ///<summary>创建新的渐变画刷图例，若已有指定键值图例，返回错误</summary>
        public GradientBrushLegend createGradientBrushLegend(string legendkey)
        {
            GradientBrushLegend legend = new GradientBrushLegend(earth);
            legends.Add(legendkey, legend);
            return legend;
        }


        ///<summary>删除指定键值图例</summary>
        public void deleteLegend(string legendkey)
        {
            if (legends.ContainsKey(legendkey))
            {
                LegendBase legend = legends[legendkey];
                if (earth.grdlegend.Children.Contains(legend.panel)) 
                    earth.grdlegend.Children.Remove(legend.panel);
                legends.Remove(legendkey);
            }
        }

      



    }


    ///<summary>图例基类</summary>
    public abstract class LegendBase
    {
        public LegendBase(Earth Earth)
        {
            earth = Earth;
            panel=new GroupBox() {
                //IsHitTestVisible=false, 
                BorderBrush=Brushes.Black, 
                BorderThickness=new Thickness(0.3), 
                Background=new SolidColorBrush(Color.FromArgb(0x80, 0xff, 0xff, 0xff)),
                Foreground=Brushes.Blue,
            };
            brd = new Border() { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0.5), Padding = new Thickness(3, 2, 3, 2), Margin = new Thickness(-3, 0, -5, 0), Background = Brushes.White, CornerRadius = new CornerRadius(3), Cursor = System.Windows.Input.Cursors.Hand, Child = textblock };
            panel.Header = brd;
            earth.grdlegend.Children.Add(panel);
            brd.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(brd_MouseLeftButtonUp); 
        }
        Border brd;
        void brd_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isfold)
            {
                panel.Height = saveheight;
                ani.To = saveheight;
                ani.From = 20;
                panel.BeginAnimation(GroupBox.HeightProperty, ani);
                isfold = false;
            }
            else
            {
                saveheight = panel.ActualHeight;
                panel.Height = 20;
                ani.To = 20;
                ani.From = saveheight;
                panel.BeginAnimation(GroupBox.HeightProperty, ani);
                panel.Width = panel.ActualWidth;
                isfold = true;
            }
        }
        public void fold()
        {
            panel.Measure(new System.Windows.Size(1920, 1080));
            panel.Arrange(new Rect(0, 0, 1920, 1080));

            saveheight = panel.ActualHeight;
            panel.Height = 20;
            ani.To = 20;
            ani.From = saveheight;
            panel.BeginAnimation(GroupBox.HeightProperty, ani);
            panel.Width = panel.ActualWidth;
            isfold = true;
        }
        public void expand()
        {
            panel.Height = saveheight;
            ani.To = saveheight;
            ani.From = 20;
            panel.BeginAnimation(GroupBox.HeightProperty, ani);
            isfold = false;
        }

        protected Earth earth;

        bool isfold;//是否折叠
        double saveheight;
        System.Windows.Media.Animation.DoubleAnimation ani = new System.Windows.Media.Animation.DoubleAnimation() { Duration=TimeSpan.FromSeconds(0.3)};

        TextBlock textblock = new TextBlock();

        private string _header;
        public string header
        {
            get { return _header; }
            set { _header = value; textblock.Text = value; }
        }

        
        ///<summary>标题色</summary>
        public Brush headerForeground
        {
            set { panel.Foreground = value; }
        }
        
        ///<summary>标题背景色</summary>
        public Brush headerBackground
        {
            set { brd.Background = value; }
        }

        ///<summary>标题边框色</summary>
        public Brush headerBorderBrush
        {
            set { brd.BorderBrush = value; }
        }

        public GroupBox panel { get; set; }

        
        private bool _isShow;
        ///<summary>本图例是否可见</summary>
        public bool isShow
        {
            get { return _isShow; }
            set { _isShow = value; panel.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }
      
    }

    ///<summary>画刷图例</summary>
    public class BrushLegend :LegendBase
    {
        public BrushLegend(Earth Earth):base(Earth)
        {
            list = new ListBox() {ItemTemplate=(DataTemplate)earth.FindResource("BrushLegendTemplate"), ItemsSource=items, Background=Brushes.Transparent, BorderThickness=new Thickness(0) };
            ScrollViewer.SetVerticalScrollBarVisibility(list, ScrollBarVisibility.Hidden);
            ScrollViewer.SetHorizontalScrollBarVisibility(list, ScrollBarVisibility.Hidden);
            panel.Content = list;
        }

        ListBox list;
        System.Collections.ObjectModel.ObservableCollection<BrushLegendItem> items = new System.Collections.ObjectModel.ObservableCollection<BrushLegendItem>();

        public void clear()
        {
            items.Clear();
        }

        public void addItem(Brush brush, string text, double iconsize = 10)
        {
            items.Add(new BrushLegendItem() { brush = brush, text = text , textcolor=Brushes.Black, size=iconsize});
        }
        public void addItem(Brush brush, string text, SolidColorBrush textcolor, double iconsize=10)
        {
            items.Add(new BrushLegendItem() { brush = brush, text = text,textcolor=textcolor, size=iconsize});
        }
    }

    public struct BrushLegendItem
    {
        public Brush brush { get; set; }
        public string text { get; set; }
        public SolidColorBrush textcolor { get; set; }
        public double size { get; set; }
    }


    ///<summary>渐变画刷图例</summary>
    public class GradientBrushLegend : LegendBase
    {
        public GradientBrushLegend(Earth Earth)
            : base(Earth)
        {
            panel.Content = mainpanel;
            panel.Padding = new Thickness(5);
            panel.Height=150;
            mainpanel.Children.Add(rect);
            mainpanel.Children.Add(grd);
            grd.Children.Add(txt100);
            grd.Children.Add(txt50);
            grd.Children.Add(txt0);
            rect.Fill = brush;
            brush.GradientStops.Add(stop0);
            brush.GradientStops.Add(stop1);
        }

        LinearGradientBrush brush=new LinearGradientBrush() {EndPoint=new Point(0,1)};
        GradientStop stop0=new GradientStop(){Offset=0, Color=Colors.Blue};
        GradientStop stop1 = new GradientStop() { Offset = 1, Color = Colors.Red };
        StackPanel mainpanel=new StackPanel(){Orientation= Orientation.Horizontal};
        Grid grd=new Grid();
        System.Windows.Shapes.Rectangle rect=new System.Windows.Shapes.Rectangle(){Width=20, Margin=new Thickness(0,0,10,0)};
        TextBlock txt0=new TextBlock(){VerticalAlignment= VerticalAlignment.Top, Text="0"};
        TextBlock txt50 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, Text = "50" };
        TextBlock txt100 = new TextBlock() { VerticalAlignment = VerticalAlignment.Bottom, Text = "100" };

        ///<summary>设置起止双色</summary>
        public void setColor(Color startcolor,Color endcolor)
        {
            stop0.Color=startcolor;
            stop1.Color=endcolor;
        }
        ///<summary>替换GradientStopCollection, 以使用更复杂的色谱</summary>
        public void setGradientStop(GradientStopCollection gsc)
        {
            brush.GradientStops = gsc;
        }

        
        public void setText(string str0,string str50, string str100)
        {
            txt0.Text=str0;
            txt50.Text=str50;
            txt100.Text=str100;
        }

        public void setForeground(Brush foreground)
        {
            txt0.Foreground = txt50.Foreground = txt100.Foreground=foreground;
        }

    }



}
