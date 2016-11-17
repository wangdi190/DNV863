using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace DNVLibrary
{
    class zMenuButton : Button
    {
        public zMenuButton()
        {
            sp.Orientation = Orientation.Horizontal;
            sp.Height=32;
            sp.Width=300;
            sp.Margin = new System.Windows.Thickness(5);
            txt.Margin = new System.Windows.Thickness(5, 0, 0, 0);
            rec.Width = rec.Height = 32;
            txt.VerticalAlignment=rec.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            sp.Children.Add(rec);
            sp.Children.Add(txt);

            this.FontSize = 20;
            this.FontFamily=new FontFamily("YouYuan");
            this.Background=Brushes.Transparent;
            this.Foreground=Brushes.White;
            this.Content = sp;
            this.Cursor = System.Windows.Input.Cursors.Hand;
            this.Focusable = false;

        }

        internal StackPanel sp = new StackPanel();
        internal Rectangle rec = new Rectangle();
        internal TextBlock txt = new TextBlock();

        
        private bool _enable;
        public bool enable
        {
            get { return _enable; }
            set {
                _enable = value;
                if (!enable)
                {
                    this.IsHitTestVisible = false;
                    txt.Foreground = Brushes.DarkGray;
                }
            }
        }
      
        
        private string _head;
        public string head
        {
            get { return _head; }
            set { _head = value; txt.Text = value; }
        }

        
        private Brush _icon;
        public Brush icon
        {
            get { return _icon; }
            set { _icon = value; rec.Fill = value; }
        }

        
        private bool _isSelected;
        public bool isSelected
        {
            get { return _isSelected; }
            set { _isSelected = value;
            if (value)
                this.Background = new SolidColorBrush(Colors.RoyalBlue);
            else
                this.Background = Brushes.Transparent;
            }
        }
      
    }
}
