using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary
{
    ///<summary>Tooltip所使用的简单通用类</summary>
    public class TooltipChartData: MyClassLibrary.MVVM.DependencyNotificationObject
    {
        public TooltipChartData()
        {
            items = new System.ComponentModel.BindingList<MyClassLibrary.DevShare.ChartDataPoint>();
        }

        public string name { get; set; }
        public System.ComponentModel.BindingList<MyClassLibrary.DevShare.ChartDataPoint> items { get; set; }
    }




}
