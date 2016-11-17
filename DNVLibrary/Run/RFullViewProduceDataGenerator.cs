using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.Xpf.Charts;
namespace DNVLibrary.Run
{
    class RFullViewProduceDataGenerator
    {
        static Random rd = new Random();

        public static List<SeriesPoint> SimulateData(int cnst,int var,int year)
        {
            List<SeriesPoint> points = new List<SeriesPoint>();

            for (int i = 1; i <= 12;i++ )
            {
                int vol = cnst + rd.Next(var);
                string arg= string.Format("{0}/{1}", year, i);
                points.Add(new SeriesPoint(arg, vol));
            }
            return points;
        }
    }
}
