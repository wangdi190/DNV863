using System;
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

namespace DNVLibrary.Planning
{
    /// <summary>
    /// PAllPrj.xaml 的交互逻辑
    /// </summary>
    public partial class PAllPrj : UserControl
    {
        public PAllPrj()
        {
            InitializeComponent();
        }

        public string name
        {
            set { txtName.Text = value; }
        }

        public string note
        {
            set { txtNote.Text = value; }
        }
        public Color color
        {
            set
            {
            	rect.Fill = new SolidColorBrush(value);
            }
        }

        System.Windows.Media.Animation.DoubleAnimation animx = new System.Windows.Media.Animation.DoubleAnimation() { Duration = TimeSpan.FromSeconds(0.3) };
        System.Windows.Media.Animation.DoubleAnimation animy = new System.Windows.Media.Animation.DoubleAnimation() { Duration = TimeSpan.FromSeconds(0.3) };
        int _idx;
        public int idx
        {
            get { return _idx; }
            set { 
                //transform.X = value * 10;
                //transform.Y = value * -20;
                brd.BeginAnimation(Border.OpacityProperty, null);

                animx.To = value * 10;
                animy.To = value * -20;

                transform.BeginAnimation(TranslateTransform.XProperty, animx);
                transform.BeginAnimation(TranslateTransform.YProperty, animy);

                Panel.SetZIndex(this, 10 - value);
                _idx = value; }
        }



        System.Windows.Media.Animation.ColorAnimation anic = new System.Windows.Media.Animation.ColorAnimation() { Duration = TimeSpan.FromSeconds(0.5) };
        bool _isSelected;
        public bool isSelected
        {
            get { return _isSelected; }
            set {
                if (_isSelected != value)
                {
                    //grdHeader.Background = value ? Brushes.RoyalBlue : new SolidColorBrush(Colors.PaleTurquoise);
                    grdHeader.Background = new SolidColorBrush();
                    anic.To = value ? Colors.RoyalBlue : Colors.PaleTurquoise;
                    grdHeader.Background.BeginAnimation(SolidColorBrush.ColorProperty, anic);

                    txtName.Foreground = value ? new SolidColorBrush(Colors.Yellow) : Brushes.Black;
                    _isSelected = value;

                    brd.Opacity = value ? 1 : 0.5;
                    txtNote.Opacity = value ? 1 : 0;
                    grdHeader.Cursor = value ? Cursors.Arrow : Cursors.Hand;

                }
            }
        }


        private void grdHeader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isSelected) return;


            RaiseprojectChangedEvent();
        }

        
        public event EventHandler projectChanged;
        protected virtual void RaiseprojectChangedEvent()
        {
            if (projectChanged != null)
                projectChanged(this, null);
        }

        System.Windows.Media.Animation.DoubleAnimation ani = new System.Windows.Media.Animation.DoubleAnimation() {Duration=TimeSpan.FromSeconds(0.15)};
        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isSelected) return;

            ani.FillBehavior = System.Windows.Media.Animation.FillBehavior.HoldEnd;
            ani.To = 1;
            brd.BeginAnimation(Border.OpacityProperty, ani);
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isSelected) return;

            ani.FillBehavior = System.Windows.Media.Animation.FillBehavior.Stop;
            ani.To = 0.5;
            brd.BeginAnimation(Border.OpacityProperty, ani);

        }
      

    }
}
