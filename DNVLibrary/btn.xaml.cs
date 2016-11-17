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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DNVLibrary
{
    /// <summary>
    /// btn.xaml 的交互逻辑
    /// </summary>
    public partial class BTN : ContentPresenter
    {
        public BTN()
        {
            InitializeComponent();
        }
        DoubleAnimation aniT = new DoubleAnimation(){Duration=TimeSpan.FromSeconds(0.5)};
        DoubleAnimation aniR = new DoubleAnimation() { Duration = TimeSpan.FromSeconds(0.5) };

        public string Text
        {
            get 
            {
                return txt.Text;
            }
            set
            {
                txt.Text = value;
            }
        }

        bool _isSelected = false;
        public bool isSelected {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                btnBackground.Fill= (Brush)FindResource(value?"btnSelected":"btnUnselected");
            } 
        }

        public int index { get; set; }

        public bool isExpand { get; set; }

        private void btnBackground_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Grid panel = (Grid)LogicalTreeHelper.GetParent(this);
            bool tmp = !isExpand;
            foreach (BTN btn in panel.Children)
            {
                btn.isSelected = btn == this;
                Panel.SetZIndex(btn, btn == this ? 1 : 0);
                btn.expand(tmp);
            }

        }        

        public void expand(bool IsExpand)
        {
            isExpand = IsExpand;
            if (isExpand)
            {
                aniR.To = -index * 30;
                aniT.To = -120;
            }
            else
            {
                aniR.To = -index * 0;
                aniT.To = -0;
            }
            translate.BeginAnimation(TranslateTransform.XProperty, aniT);
            rotate.BeginAnimation(RotateTransform.AngleProperty, aniR);
        }



        public Brush icon { get; set; }      

    }
}
