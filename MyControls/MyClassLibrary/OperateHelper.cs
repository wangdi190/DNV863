using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyClassLibrary
{
    /// <summary>
    /// 快捷操作常用代码段的方法
    /// </summary>
    public static class OperateHelper
    {
        public static void bind(object src, string srcpath, DependencyObject obj, DependencyProperty objp)
        {
            Binding bind = new Binding();
            bind.Source = src;
            bind.Path = new PropertyPath(srcpath);
            bind.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(obj, objp, bind);
        }
        public static void bind(object src, DependencyProperty srcpath, DependencyObject obj, DependencyProperty objp, BindingMode bindmode)
        {
            Binding bind = new Binding();
            bind.Source = src;
            bind.Path = new PropertyPath(srcpath);
            bind.Mode = bindmode;
            BindingOperations.SetBinding(obj, objp, bind);
        }
        public static void bind(object src, string srcpath, DependencyObject obj, DependencyProperty objp, BindingMode bindmode)
        {
            Binding bind = new Binding();
            bind.Source = src;
            bind.Path = new PropertyPath(srcpath);
            bind.Mode = bindmode;
            BindingOperations.SetBinding(obj, objp, bind);
        }
        public static void bind(object src, DependencyProperty srcp, DependencyObject obj, DependencyProperty objp)
        {
            Binding bind = new Binding();
            bind.Source = src;
            bind.Path = new PropertyPath(srcp);
            bind.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(obj, objp, bind);
        }

        public static void bind(object src, string srcpath, DependencyObject obj, DependencyProperty objp, Type converttype, object convertpara, BindingMode bindmode)
        {
            Binding bind;
            bind = new Binding();
            bind.Converter = (IValueConverter)Activator.CreateInstance(converttype); ;
            bind.ConverterParameter = convertpara;
            bind.Source = src;
            bind.Path = new PropertyPath(srcpath);
            bind.Mode = bindmode;
            BindingOperations.SetBinding(obj, objp, bind);
        }

        public static void bind(object src, DependencyProperty srcp, DependencyObject obj, DependencyProperty objp, Type converttype, object convertpara, BindingMode bindmode)
        {
            Binding bind;
            bind = new Binding();
            bind.Converter = (IValueConverter)Activator.CreateInstance(converttype); ;
            bind.ConverterParameter = convertpara;
            bind.Source = src;
            bind.Path = new PropertyPath(srcp);
            bind.Mode = bindmode;
            BindingOperations.SetBinding(obj, objp, bind);
        }


        public static childItem FindVisualChild<childItem>(DependencyObject obj)
      where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

    }

}
