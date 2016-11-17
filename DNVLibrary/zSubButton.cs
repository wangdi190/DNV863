using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary
{
    class zSubButton: Button
    {
        public zSubButton()
        {
            Padding = new Thickness(5,0,5,0);
            Background = new SolidColorBrush(Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF));
            Foreground = new SolidColorBrush(Colors.Black);
            Cursor = System.Windows.Input.Cursors.Hand;
            Height = 24;
            this.Focusable = false;

            if (SystemParameters.PrimaryScreenWidth < 1300)
            {
                this.FontSize = 7;
            }
            else if (SystemParameters.PrimaryScreenWidth < 1900)
            {
                this.FontSize = 9;
            }

        }


        Brush savebrush;

        private bool _isChecked;
        ///<summary>是否被选择</summary>
        public bool isChecked
        {
            get { return _isChecked; }
            set
            {
                if (!_isChecked) savebrush = Background;
                _isChecked = value;
                Foreground = value ? new SolidColorBrush(Colors.Gold) : new SolidColorBrush(Colors.Blue);
                Background = value ? new SolidColorBrush(Color.FromArgb(0x96, 0x00, 0x00, 0xFF)) : savebrush;
            }
        }


    }
}
