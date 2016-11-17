using System;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary
{
    abstract class BaseMain
    {
        public BaseMain(UCDNV863 Root, string AppName, Brush icon)
        {
            root = Root;

            //初始化工具栏
            rec.Fill = icon;
            toolbox.Children.Add(rec);
            toolbox.HorizontalAlignment = HorizontalAlignment.Right;
            title.Text = AppName;
            title.Effect = new System.Windows.Media.Effects.DropShadowEffect() { Color=Colors.White, ShadowDepth=0, BlurRadius=5 };
            toolbox.Children.Add(title);
            toolbox.Children.Add(toolboxSub);
            toolbox.RenderTransform = transform;
            transform.Y =0;//= -100;
            root.sPanel.Children.Add(toolbox);

            //初始化信息板容器
            grdPanel.RenderTransform = transformpanel;
            transformpanel.X = 300;
            root.grdInfo.Children.Add(grdPanel);
            if (SystemParameters.PrimaryScreenWidth < 1300)
            {
                rec.Width = rec.Height = 16;
                title.FontSize = 16;
            }
            else if (SystemParameters.PrimaryScreenWidth < 1900)
            {
                rec.Width = rec.Height = 24;
                title.FontSize = 11;
            }

        }

        protected UCDNV863 root; //主控件

        protected abstract void load(); //子类必须实现的装载数据
        protected abstract void unload(); //子类必须实现的装载数据

        bool isAniCompleted;
        bool isDoBeginEnd;
        public void begin()
        {
            isDoBeginEnd = true;
            if (isAniCompleted)
            {
                load();
                isDoBeginEnd = false;
            }
        }

     
        public void show(double from ,double to)
        {
            isAniCompleted = false;
            ani.Duration = TimeSpan.FromMilliseconds(800);
            ani.Completed += new EventHandler(aniBegin_Completed);
            ani.From = from;
            ani.To = to;
            //transform.BeginAnimation(TranslateTransform.YProperty, ani);
            toolbox.BeginAnimation(StackPanel.OpacityProperty, ani);

            if (grdPanel.Children.Count != 0)
            {
                anipanel.To = 0;
                transformpanel.BeginAnimation(TranslateTransform.XProperty, anipanel);
            }
        }

        public void end()
        {
            isDoBeginEnd = true;
            if (isAniCompleted)
            {
                root.sPanel.Children.Remove(toolbox);
                root.grdInfo.Children.Remove(grdPanel);
                unload();
            }
        }

        public void hide(double from ,double to)
        {
            isAniCompleted = false;
            ani.Duration = TimeSpan.FromMilliseconds(500);
            ani.Completed += new EventHandler(aniEnd_Completed);
            ani.From = from;
            ani.To = to;
            toolbox.BeginAnimation(StackPanel.OpacityProperty, ani);

            if (grdPanel.Children.Count != 0)
            {
                anipanel.To = 300;
                transformpanel.BeginAnimation(TranslateTransform.XProperty, anipanel);
            }
        }


        void aniBegin_Completed(object sender, EventArgs e)
        {
            isAniCompleted = true;
            if (isDoBeginEnd)
            {
                load();
                isDoBeginEnd = false;
            }

            ani.Completed -= aniBegin_Completed;
        }
        void aniEnd_Completed(object sender, EventArgs e)
        {
            isAniCompleted = true;
            ani.Completed -= aniEnd_Completed;
            toolbox.Opacity = 0;
            if (isDoBeginEnd)
            {
                root.sPanel.Children.Remove(toolbox);
                root.grdInfo.Children.Remove(grdPanel);
                unload();
            }
        }


        #region ========== 工具栏相关 ==============
        DoubleAnimation ani = new DoubleAnimation() { Duration=TimeSpan.FromMilliseconds(500), FillBehavior= FillBehavior.HoldEnd};
        TranslateTransform transform = new TranslateTransform();

        Rectangle rec = new Rectangle() //图标
        {
            Width=48, 
            Height=48,
            Margin=new Thickness(0,0,2,0)
        };
        TextBlock title = new TextBlock() // 标题
        { 
            FontSize=36,
            Foreground=Brushes.Orange,
            Margin=new Thickness(0,0,20,0),
            VerticalAlignment= System.Windows.VerticalAlignment.Bottom,
            FontFamily = new FontFamily("STXingkai")
        };

        public StackPanel toolbox = new StackPanel()//工具栏容器 
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
        }; 

        public StackPanel toolboxSub = new StackPanel()//工具栏子容器 
        {
            Orientation= Orientation.Horizontal, 
            VerticalAlignment= VerticalAlignment.Bottom,
        };
        #endregion


        #region ========== 信息面板相关 ==============
        protected Grid grdPanel=new Grid(); //信息面板容器
        public UserControl infoPanel; //信息面板
        DoubleAnimation anipanel = new DoubleAnimation() { Duration = TimeSpan.FromMilliseconds(500) };
        protected TranslateTransform transformpanel = new TranslateTransform();


        #endregion
    }
}
