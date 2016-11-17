using System;
using System.Windows.Media;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Run
{
    static class RFullViewStatData
    {
        static RFullViewStatData()
        {
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from dict_statdata where isshow=1 order by ord");

            foreach (DataRow dr in dt.Rows)
            {
                StatData data = new StatData()
                {
                    name = dr.getString("name"),
                    header = dr.getString("header"),
                    format = dr.getString("format"),
                    warningType = dr.getString("warningType"),
                    warningMinValue = dr.getDouble("warningMinValue"),
                    warningMaxValue = dr.getDouble("warningMaxValue"),
                    isRefresh = dr.getBool("isRefresh"),
                    ord=dr.getInt("ord")
                };
                datas.Add(data.name, data);
            }


            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += new EventHandler(timer_Tick);
        }

        static void timer_Tick(object sender, EventArgs e)
        {
            refreshDatas((sender as System.Windows.Threading.DispatcherTimer).Tag as DistNetLibrary.DistNet);
        }

        #region 可视属性
        public static double fontSize = 22;
        public static Brush headerBrush = Brushes.Cyan;
        public static Brush valueBrush = Brushes.Yellow;
        public static Brush warningBrush = Brushes.Red;
        public static Brush realheaderBrush = Brushes.Cyan;
        public static Brush readvalueBrush = Brushes.Lime;


        public static int topSpan = 150;
        public static int headerWidth = 200;
        //public static int valueWidth = 200;
        public static int rowHeight = 60;
        #endregion


        //临时
        static Random rd = new Random();
        static System.Windows.Threading.DispatcherTimer timer=new System.Windows.Threading.DispatcherTimer();

        public static Dictionary<string, StatData> datas = new Dictionary<string, StatData>();


        ///<summary>初始化数据</summary>
        public static void initDatas(DistNetLibrary.DistNet distnet)
        {
            foreach (KeyValuePair<string, StatData> item in datas)
            {
                    readData(item.Value, distnet);
            }

            timer.Tag = distnet;
            timer.Start();
        }


        ///<summary>刷新isRefresh为真的实时数据</summary>
        public static void refreshDatas(DistNetLibrary.DistNet distnet)
        {
            foreach (KeyValuePair<string, StatData> item in datas)
            {
                if (item.Value.isRefresh)
                    readData(item.Value, distnet);
            }
        }

        public static void stopRefresh()
        {
            timer.Stop();
            timer.Tag = null;
        }


        ///<summary>读取指定数据项数据</summary>
        static void readData(StatData data, DistNetLibrary.DistNet distnet)
        {

            if (data.name == "变电站数")
            {
                data.value = distnet.getAllObjListByObjType(DistNetLibrary.EObjectType.变电站).Count();
            }
            if (data.name == "配电室数")
            {
                data.value = distnet.getAllObjListByObjType(DistNetLibrary.EObjectType.配电室).Count();
            }
            else if (data.name == "当年累计供电量")
            {
                data.value = 65145;
            }
            else if (data.name == "当年清洁能源累计发电量")
            {
                data.value = 5347;
            }
            else if (data.name == "当年最大供电负荷")
            {
                data.value = 2352;
            }
            else if (data.name == "供电面积")
            {
                data.value = 238;
            }
            else if (data.name == "开关站数")
            {
                data.value = distnet.getAllObjListByObjType(DistNetLibrary.EObjectType.开关站).Count();
            }
            else if (data.name == "线路总长")
            {
                data.value = 256;
            }
            else if (data.name == "主变容量")
            {
                data.value = distnet.getAllObjListByObjType(DistNetLibrary.EObjectType.两卷主变).Where(p => p.busiAccount != null).Sum(p => (p as DistNetLibrary.DNMainTransformer2W).thisAcntData.cap) +
                    distnet.getAllObjListByObjType(DistNetLibrary.EObjectType.三卷主变).Where(p => p.busiAccount != null).Sum(p => (p as DistNetLibrary.DNMainTransformer3W).thisAcntData.cap);
                data.value2 = distnet.getAllObjListByObjType(DistNetLibrary.EObjectType.两卷主变).Count() +
                    distnet.getAllObjListByObjType(DistNetLibrary.EObjectType.三卷主变).Count();
            }
            else if (data.name == "配变容量")
            {
                data.value = distnet.getAllObjListByObjType(DistNetLibrary.EObjectType.配变).Where(p=>p.busiAccount!=null).Sum(p => (p as DistNetLibrary.DNDistTransformer).thisAcntData.cap);
                data.value2 = distnet.getAllObjListByObjType(DistNetLibrary.EObjectType.配变).Count();
            }
            else if (data.name == "清洁能源总装机")
            {
                data.value = 23.5;
            }
            else if (data.name == "清洁能源渗透率")
            {
                data.value = 5347.0 / 65145;
            }
            else if (data.name == "供电可靠率")
            {
                data.value = 0.998;
            }
            else if (data.name == "综合电压合格率")
            {
                data.value = 0.977;
            }
            else if (data.name == "当前供电负荷")
            {
                data.value = 100.0 + 100.0 * rd.NextDouble();
            }
            else if (data.name == "当前清洁能源出力")
            {
                data.value = 10.0 + 10.0 * rd.NextDouble();
            }

        }

    }


    class StatData:MyClassLibrary.MVVM.NotificationObject
    {
        public string name { get; set; }
        public string header { get; set; }

        
        private double _value;
        public double value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(() => info); }
        }
      
        public double value2 { get; set; }
        public string format { get; set; }
        public int ord { get; set; }

        public string warningType { get; set; }
        public double warningMinValue { get; set; }
        public double warningMaxValue { get; set; }
        public bool isRefresh { get; set; }
        public string info { get { return string.Format(format, value, value2); } }


        public double fontsize { get { return RFullViewStatData.fontSize; } }
        public Brush headerbrush { get { return isRefresh?RFullViewStatData.realheaderBrush: RFullViewStatData.headerBrush; } }
        public Brush valuebrush { get { return isWarning ? RFullViewStatData.warningBrush : (isRefresh?RFullViewStatData.readvalueBrush: RFullViewStatData.valueBrush); } }
        public int headerwidth { get { return RFullViewStatData.headerWidth; } }
        //public int valuewidth { get { return RFullViewStatData.valueWidth; } }
        public int rowheight { get { return RFullViewStatData.rowHeight; } }

        public bool isWarning
        {
            get
            {
                switch (warningType)
                {
                    case ">":
                        return value > warningMaxValue;
                    case "<":
                        return value < warningMinValue;
                    case "><":
                        return value > warningMaxValue || value < warningMinValue;
                    case "<>":
                        return value < warningMaxValue && value > warningMinValue;
                    default:
                        return false;
                }
            }
        }

        public System.Windows.Visibility warnningVisual { get { return isWarning ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; } }
    }

}
