using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;

namespace MyClassLibrary
{

    /// <summary>
    /// 列表selectindex与是否选择的转换器
    /// </summary>
    [ValueConversion(typeof(int), typeof(bool))]
    public class SelectIndexToIsSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int selectindex = (int)value;
            return selectindex > -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = (bool)value;
            return isSelected ? 0 : -1;
        }

    }

    /// <summary>
    /// Visibility取反转换器
    /// </summary>
    [ValueConversion(typeof(Visibility), typeof(Visibility))]
    public class VisibilityReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

    }

    /// <summary>
    /// bool取反转换器
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BoolNotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

    }

    /// <summary>
    /// bool 运算
    /// </summary>
    public class BoolOperationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            bool result = (values[0] == System.Windows.DependencyProperty.UnsetValue) ? false : (bool)values[0];
            string operation = (string)parameter;
            for (int i = 1; i < values.Count(); i++)
            {
                switch (operation)
                {
                    case "and":
                        result = result && ((values[i] == System.Windows.DependencyProperty.UnsetValue) ? false : (bool)values[i]);
                        break;
                    case "or":
                        result = result || ((values[i] == System.Windows.DependencyProperty.UnsetValue) ? false : (bool)values[i]);
                        break;
                }

            }

            return result;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }


    }


    /// <summary>
    /// 枚举整型转换器
    /// </summary>
    public class EnumIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.ToObject(targetType, value);
        }

    }


    /// <summary>
    /// 布尔Visibility转换器
    /// </summary>
    public class BoolvisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (System.Windows.Visibility)value == System.Windows.Visibility.Visible;
        }

    }

    /// <summary>
    /// string转double转换器
    /// </summary>
    public class StringToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.Parse(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

    }


    /// <summary>
    /// + 固定数运算
    /// </summary>
    public class PlusFixedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value + double.Parse(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value - double.Parse(parameter.ToString());
        }

    }


    /// <summary>
    /// +多值 运算
    /// </summary>
    public class PlusMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            double result = 0;
            for (int i = 0; i < values.Count(); i++)
            {
                result += (double)values[i];
            }
            if (parameter!=null)
                result += double.Parse(parameter.ToString());

            return result;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }


    }

    /// <summary>
    /// -1 运算
    /// </summary>
    public class NavgateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return -1*(double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value - double.Parse(parameter.ToString());
        }

    }



    /// <summary>
    /// Brush类型转换器，支持string与solid,linearGradient等的转换
    /// </summary>
    public class MyBrushConverter : TypeConverter
    {

        public static MyBrushConverter Default = new MyBrushConverter();

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is SolidColorBrush || value is ImageBrush)
            {
                string s = String.Format("type={0}|string={1}", value.GetType().Name, (new System.Windows.Media.BrushConverter().ConvertToString(value)));
                return s;
            }
            else if (value is LinearGradientBrush || value is RadialGradientBrush)
            {
                string s = String.Format("type={0}", value.GetType().Name);

                if (value is LinearGradientBrush)
                {
                    s += "|StartPoint=" + (value as LinearGradientBrush).StartPoint.ToString();
                    s += "|EndPoint=" + (value as LinearGradientBrush).EndPoint.ToString();
                }
                else
                {
                    s += "|GradientOrigin=" + (value as RadialGradientBrush).GradientOrigin.ToString();
                    s += "|Center=" + (value as RadialGradientBrush).Center.ToString();
                }
                string s2 = "";
                int idx = 0;
                foreach (GradientStop item in (value as GradientBrush).GradientStops)
                {
                    if (idx > 0)
                        s2 += "^";
                    s2 += item.ToString();
                    idx++;
                }

                s += "|GradientStops=" + s2;
                return s;
            }
            else
                return null;


        }


        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            Brush brush=null;

            if (value is string)
            {
                string[] ss = (value as string).Split('|');
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (string item in ss)
                {
                    string sn=item.Substring(0,item.IndexOf('='));
                    string sv = item.Substring(item.IndexOf('=')+1);
                    dic.Add(sn, sv);
                }

                if (dic["type"] == "SolidColorBrush")
                {
                    if (dic["string"].Contains('#'))
                        brush = (SolidColorBrush)(new System.Windows.Media.BrushConverter().ConvertFromString(dic["string"]));
                    else
                    {
                        string[] scolor = dic["string"].Split(',');

                        brush = new SolidColorBrush(Color.FromRgb(byte.Parse(scolor[0]),byte.Parse(scolor[1]),byte.Parse(scolor[2])));
                    }
                }
                else if (dic["type"] == "ImageBrush")
                    brush = (ImageBrush)(new System.Windows.Media.BrushConverter().ConvertFromString(dic["string"]));
                else if (dic["type"] == "LinearGradientBrush" || dic["type"] == "RadialGradientBrush")
                {
                    if (dic["type"] == "LinearGradientBrush")
                    {
                        brush = new LinearGradientBrush();
                        (brush as LinearGradientBrush).StartPoint = System.Windows.Point.Parse(dic["StartPoint"]);
                        (brush as LinearGradientBrush).EndPoint = System.Windows.Point.Parse(dic["EndPoint"]);
                    }
                    else
                    {
                        brush = new RadialGradientBrush();
                        (brush as RadialGradientBrush).GradientOrigin = System.Windows.Point.Parse(dic["GradientOrigin"]);
                        (brush as RadialGradientBrush).Center = System.Windows.Point.Parse(dic["Center"]);
                    }
                    string[] steps = dic["GradientStops"].Split('^');
                    foreach (string item in steps)
                    {
                        string[] sss = item.Split(',');
                        (brush as GradientBrush).GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(sss[0]), double.Parse(sss[1])));
                    }


                }
            }


            return brush;



        }




    }




    /// <summary>
    /// Color类型转换器，支持string与color的转换
    /// </summary>
    public class MyColorConverter : TypeConverter
    {

        public static MyColorConverter Default = new MyColorConverter();

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return base.CanConvertTo(context, destinationType);
        }

        ///<summary>返回r,g,b的字串</summary>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is Color)
            {
                Color c = (Color)value;
                string s = String.Format("{0},{1},{2}", c.R,c.G,c.B);
                return s;
            }
            else
                return null;
        }

        ///<summary>返回color</summary>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            Color color=Colors.Black;

            if (value is string)
            {
                string[] ss = (value as string).Split(',');
                byte r,g,b;
                byte.TryParse(ss[0],out r);
                byte.TryParse(ss[1], out g);
                byte.TryParse(ss[2], out b);
                color = Color.FromRgb(r, g, b);
            }
            return color;
        }




    }
 


}
