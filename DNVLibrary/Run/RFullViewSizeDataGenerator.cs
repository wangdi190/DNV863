using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.Xpf.Charts;
using System.Threading;

namespace DNVLibrary.Run
{
    class RFullViewSizeDataGenerator
    {
        static Random rd = new Random();
        public static List<SeriesPoint> SimuData(int cnst,int max,int year,int num)
        {
            List<SeriesPoint> lst = new List<SeriesPoint>();
            
            for (int i=0;i<num;i++)
            {
                int val = cnst + rd.Next(max);
                SeriesPoint sp = new SeriesPoint(year + i, val);
                lst.Add(sp);
            }
            return lst;
        }
    }
}
