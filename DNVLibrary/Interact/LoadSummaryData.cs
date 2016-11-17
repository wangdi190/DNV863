using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadInput
{
    class LoadSummaryData: MyClassLibrary.MVVM.NotificationObject
    {
        public LoadSummaryData()
        {
            gyInfo = new PerInfo() { species=ElecSpecies.工业, weight=0.50, isEnable=true};
            syInfo = new PerInfo() { species = ElecSpecies.商业, weight = 0.25, isEnable = true };
            jmInfo = new PerInfo() { species = ElecSpecies.居民, weight = 0.25, isEnable = true };

            Loads = new BindingList<MyChartDataPoint>();
        }

        Random rd = new Random();

        public PerInfo gyInfo { get; set; }
        public PerInfo syInfo { get; set; }
        public PerInfo jmInfo { get; set; }

        public BindingList<MyChartDataPoint> Loads { get; set; } //负荷

        public void start()
        {
            DateTime d = DateTime.Now;
            d = new DateTime(d.Year, d.Month, d.Day);
            for (int i = 1; i <=24; i++)
            {
                MyChartDataPoint cp = new MyChartDataPoint() { nPos = i, argudate = d.AddHours(i), value = MyClassLibrary.MyFunction.simHourData((int)(i / 4)) * 200 + rd.Next(30) };
                Loads.Add(cp);
            }
        }
    }

    class PerInfo: MyClassLibrary.MVVM.NotificationObject
    {
        //private LoadCurveData parent;
        //public PerInfo(LoadCurveData p)
        //{
        //    parent = p;
        //}

        private double _weight;
        public double weight
        { get { return _weight; }
          set
            {
                _weight = Math.Round(value, 2);
                RaisePropertyChanged(() => weight);
            }
        }

        private bool _isEnable;
        public bool isEnable
        {
            get { return _isEnable; }
            set
            {
                _isEnable = value;
                RaisePropertyChanged(() => isEnable); 
            }
        }

        public ElecSpecies species { get; set; }
    }
}
