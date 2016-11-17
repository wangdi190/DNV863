using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Text;

namespace DNVLibrary
{
    class zButton : System.Windows.Controls.Button
    {
        public zButton()
        {

            sp.Orientation = Orientation.Horizontal;
            sp.Margin = new System.Windows.Thickness(5, 0, 5, 0);
            txt.Margin = new System.Windows.Thickness(5, 0, 0, 0);
            rec.Width = rec.Height = 32;
            rec.Visibility = System.Windows.Visibility.Collapsed;
            txt.VerticalAlignment = rec.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            sp.Children.Add(rec);
            sp.Children.Add(txt);

            this.Background = new SolidColorBrush(Color.FromArgb(0xB0, 0x99, 0xFF, 0xFF));

            this.FontSize = 14;
            this.Content = sp;
            this.Cursor = System.Windows.Input.Cursors.Hand;
            this.Focusable = false;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.Height = 36;


            if (SystemParameters.PrimaryScreenWidth < 1300)
            {
                rec.Width = rec.Height = 16;
                this.Height = 18;
                txt.FontSize = 8;
                sp.Margin = new Thickness(1, 0, 1, 0);
            }
            else if (SystemParameters.PrimaryScreenWidth < 1900)
            {
                rec.Width = rec.Height = 24;
                this.Height = 26;
                txt.FontSize = 9;
                sp.Margin = new Thickness(3, 0, 3, 0);
            }
        }
        internal StackPanel sp = new StackPanel();
        internal Rectangle rec = new Rectangle();
        internal TextBlock txt = new TextBlock();
        Brush savebrush;

        public string group { get; set; }  //同组不失效，不同组时互斥
        public string exclusivegroup { get; set; }  //同组互斥，在exclusivegroup同组中，与上组合，可适应不同的需求

        ///<summary>指示按钮是否象普通按钮一样，没有表现选择或未选择的功能</summary>
        public bool isNormalButton;

        private bool _isChecked;
        ///<summary>是否被选择</summary>
        public bool isChecked
        {
            get { return _isChecked; }
            set
            {
                if (!_isChecked) savebrush = Background;
                _isChecked = value;
                Foreground = value ? Brushes.Orange : new SolidColorBrush(Colors.Black);
                Height = value ? 40 : 36;
                //Background = value ? (Brush)FindResource("btnselected") : savebrush;
                Background = value ? new SolidColorBrush(Color.FromArgb(0x96, 0x80, 0x80, 0xFF)) : savebrush;
            }
        }

        protected override void OnClick()
        {
            if (!isNormalButton)
                isChecked = !isChecked;
            base.OnClick();

            if (isChecked && !isNormalButton)
            {
                StackPanel panel = (StackPanel)LogicalTreeHelper.GetParent(this);
                foreach (object btn in panel.Children)
                {
                    if (btn is zButton)
                    {
                        if (((btn as zButton).group!=this.group || string.IsNullOrWhiteSpace(this.group)) && btn!=this) //非同组按钮，关闭
                            (btn as zButton).isChecked = false;
                        if ((btn as zButton).exclusivegroup == this.exclusivegroup && !string.IsNullOrWhiteSpace((btn as zButton).exclusivegroup) && btn != this)  //第二组，同组互斥
                            (btn as zButton).isChecked = false;
                    }
                }
            }
        }


        private string _text;
        ///<summary>文字</summary>
        public string text
        {
            get { return _text; }
            set { _text = value; txt.Text = value; }
        }


        private Brush _icon;
        ///<summary>前置图标</summary>
        public Brush icon
        {
            get { return _icon; }
            set
            {
                _icon = value; rec.Fill = value; rec.Visibility = System.Windows.Visibility.Visible;
            }
        }

                
     

    }
}
