using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Text;
using System.Xml.Serialization;

namespace MyClassLibrary.DevShare
{

    public class ChartDataPoint:MyClassLibrary.MVVM.NotificationObject
    {
        public ChartDataPoint() { }
        public ChartDataPoint(string a, string s, double v)
        {
            argu = a; sort = s; value = v;
        }
        public ChartDataPoint(DateTime a, string s, double v)
        {
            argudate = a; sort = s; value = v;
        }

        
        private string _argu;
        public string argu
        {
            get { return _argu; }
            set { _argu = value; RaisePropertyChanged(() => argu); RaisePropertyChanged(() => info); }
        }

        
        private DateTime? _argudate;
        public DateTime? argudate
        {
            get { return _argudate; }
            set { _argudate = value; RaisePropertyChanged(() => argudate); }
        }

        
        private double _argudouble;
        public double argudouble
        {
            get { return _argudouble; }
            set { _argudouble = value; RaisePropertyChanged(() => argudouble); }
        }

        
        private string _sort="";
        public string sort
        {
            get { return _sort; }
            set { _sort = value; RaisePropertyChanged(() => sort); RaisePropertyChanged(() => info); }
        }
      

        
        private double _value;
        public double value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(() => value); RaisePropertyChanged(() => info); }
        }

        
        private string _label;
        public string label
        {
            get { return _label; }
            set { _label = value; RaisePropertyChanged(() => label); RaisePropertyChanged(() => info); }
        }


        
        private Color _color;
        [XmlIgnore]
        public Color color
        {
            get { return _color; }
            set { _color = value; RaisePropertyChanged(() => color); }
        }


        
        private string _tooltip;
        public string tooltip
        {
            get { return _tooltip; }
            set { _tooltip = value; RaisePropertyChanged(() => tooltip); }
        }

        
        private bool _isInclude=true;
        ///<summary>用于过滤的附加信息</summary>
        public bool isInclude
        {
            get { return _isInclude; }
            set { _isInclude = value; RaisePropertyChanged(() => isInclude); RaisePropertyChanged(() => info); }
        }

        public Visibility visibility
        {
            get { return isInclude ? Visibility.Visible : Visibility.Collapsed; }
        }

        public string info
        {
            get
            {
                return sort + "　" + argu + "　" + value.ToString() + "　" + (label == null ? "" : label) + " " + isInclude.ToString();
            }
        }
    }
}
