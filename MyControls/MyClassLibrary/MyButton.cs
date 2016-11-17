using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MyClassLibrary
{
    ///<summary>自定义带IsChecked属性的按钮，应配合template使用</summary>
    public class MyButton:System.Windows.Controls.Button
    {


        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(MyButton), new UIPropertyMetadata(false));

        
    }
}
