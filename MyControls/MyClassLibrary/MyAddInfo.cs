using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyClassLibrary
{

    /// <summary>
    /// 附加属性类，用于其它程序使用附加属性转存暂存数据
    /// </summary>
    public static class MyAddInfo
    {
        public static readonly DependencyProperty AddDataProperty = DependencyProperty.RegisterAttached("AddData", typeof(object), typeof(MyAddInfo), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static void SetAddData(UIElement element, Boolean value)
        {
            element.SetValue(AddDataProperty, value);
        }
        public static Boolean GetAddData(UIElement element)
        {
            return (Boolean)element.GetValue(AddDataProperty);
        }


    }
}
