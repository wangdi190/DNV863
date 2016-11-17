using MyClassLibrary.DevShare;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadInput
{
    class LoadClassData:MyClassLibrary.MVVM.NotificationObject
    {
        //industrial business people
        public BindingList<MyChartDataPoint> gyLoads { get; set; } //工业
        public BindingList<MyChartDataPoint> syLoads { get; set; } //商业
        public BindingList<MyChartDataPoint> jmLoads { get; set; } //居民

        Random rd = new Random();

        public LoadClassData()
        {
            gyLoads = new BindingList<MyChartDataPoint>();
            syLoads = new BindingList<MyChartDataPoint>();
            jmLoads = new BindingList<MyChartDataPoint>();
        }

        public void start()
        {
            DateTime d = DateTime.Now;
            d = new DateTime(d.Year, d.Month, d.Day);
            for (int i=0; i<24; i++)
            {
                MyChartDataPoint cp = new MyChartDataPoint() { species=ElecSpecies.工业, nPos=i, argudate = d.AddHours(i), value = MyClassLibrary.MyFunction.simHourData((int)(i / 4)) * 200 + rd.Next(30) };
                gyLoads.Add(cp);
            }

            for (int i = 0; i < 24; i++)
            {
                MyChartDataPoint cp = new MyChartDataPoint() { species = ElecSpecies.商业, nPos = i, argudate = d.AddHours(i), value = MyClassLibrary.MyFunction.simHourData((int)(i / 4)) * 200 + rd.Next(30) };
                syLoads.Add(cp);
            }

            for (int i = 0; i < 24; i++)
            {
                MyChartDataPoint cp = new MyChartDataPoint() { species = ElecSpecies.居民, nPos = i, argudate = d.AddHours(i), value = MyClassLibrary.MyFunction.simHourData((int)(i / 4)) * 200 + rd.Next(30) };
                jmLoads.Add(cp);
            }

            RaisePropertyChanged(() => gyLoads);
            RaisePropertyChanged(() => syLoads);
            RaisePropertyChanged(() => jmLoads);
        }

        public void modify(ElecSpecies species, int nPos, double value)
        {
            if(species == ElecSpecies.工业)
            {
                gyLoads.ElementAt(nPos).value = value;
                RaisePropertyChanged(() => gyLoads);
            }
            else if(species == ElecSpecies.商业)
            {
                syLoads.ElementAt(nPos).value = value;
                RaisePropertyChanged(() => syLoads);
            }
            else if (species == ElecSpecies.居民)
            {
                jmLoads.ElementAt(nPos).value = value;
                RaisePropertyChanged(() => jmLoads);
            }

        }
    }

    public class MyChartDataPoint : ChartDataPoint
    {
        public int nPos { get; set; }
        public ElecSpecies species { get; set; }//
    }

    public enum ElecSpecies { 工业,商业,居民}

    class HitInfo
    {
        public ElecSpecies species { get; set; }//
        public int nPos { get; set; }//
        public double dValue { get; set; }

    }
}
