using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistNetLibrary;

namespace DNVLibrary.Run
{
    /// <summary>
    /// 面板数据类
    /// </summary>
    class PanelData:MyClassLibrary.MVVM.NotificationObject
    {
        public PanelData(UCDNV863 Root)
        {
            root = Root;
            tmr.Tick += new EventHandler(tmr_Tick);
            loadrates = new System.ComponentModel.BindingList<MyClassLibrary.DevShare.VListItem>();
            volts = new System.ComponentModel.BindingList<MyClassLibrary.DevShare.VListItem>();
            realLoads = new System.ComponentModel.BindingList<MyClassLibrary.DevShare.ChartDataPoint>();
            planLoads = new System.ComponentModel.BindingList<MyClassLibrary.DevShare.ChartDataPoint>();
            greenpowers = new System.ComponentModel.BindingList<MyClassLibrary.DevShare.VListItem>();
            greenscales = new System.ComponentModel.BindingList<MyClassLibrary.DevShare.ChartDataPoint>();
            cars = new System.ComponentModel.BindingList<MyClassLibrary.DevShare.VListItem>();
            customs = new System.ComponentModel.BindingList<MyClassLibrary.DevShare.VListItem>();
            cuts = new System.ComponentModel.BindingList<MyClassLibrary.DevShare.VListItem>();
        }
        UCDNV863 root;

        #region 数据定义
        //配网生产
        public double curLoad { get; set; }
        public double dayElectricity  { get; set; }
        public System.ComponentModel.BindingList<MyClassLibrary.DevShare.ChartDataPoint> realLoads { get; set; }
        public System.ComponentModel.BindingList<MyClassLibrary.DevShare.ChartDataPoint> planLoads { get; set; }
        public System.ComponentModel.BindingList<MyClassLibrary.DevShare.VListItem> loadrates { get; set; }
        public System.ComponentModel.BindingList<MyClassLibrary.DevShare.VListItem> volts { get; set; }

        //清洁能源
        public double greenPower { get; set; }
        public System.ComponentModel.BindingList<MyClassLibrary.DevShare.ChartDataPoint> greenscales { get; set; }
        public System.ComponentModel.BindingList<MyClassLibrary.DevShare.VListItem> greenpowers { get; set; }

        //电动汽车
        public double carLoad { get; set; }
        public System.ComponentModel.BindingList<MyClassLibrary.DevShare.VListItem> cars { get; set; }
        //大客户
        public double customLoad { get; set; }
        public System.ComponentModel.BindingList<MyClassLibrary.DevShare.VListItem> customs { get; set; }

        //停电信息
        public double cutLoad { get; set; }
        public System.ComponentModel.BindingList<MyClassLibrary.DevShare.VListItem> cuts { get; set; }

        //事件
        public System.ComponentModel.BindingList<EventData> lstEvent { get; set; }
        #endregion

        #region 读取数据
        System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };
        Random rd = new Random();

        void tmr_Tick(object sender, EventArgs e)
        {
            readData();
        }
        
        public void readData()
        {
            //配网生产
            curLoad = (int)(500 + rd.Next(1000));
            RaisePropertyChanged(() => curLoad);
            dayElectricity += (int)rd.Next(20);
            RaisePropertyChanged(() => dayElectricity);
            loadrates.Clear(); //负载率

            List<MyClassLibrary.DevShare.VListItem> lst = new List<MyClassLibrary.DevShare.VListItem>();
            foreach (var item in root.distnet.getAllObjListByCategory(EObjectCategory.变压器类).OrderByDescending(p=>(p.busiRunData as RunDataTransformerBase).rateOfLoad).Take(10))
            {
                RunDataTransformerBase rundata = item.busiRunData as RunDataTransformerBase;
                MyClassLibrary.DevShare.VListItem vi = new MyClassLibrary.DevShare.VListItem()
                { id = item.id, name = item.name, value = rundata.rateOfLoad, format = "{0:p0}", brush=rundata.rateOfLoadBrush, };
                lst.Add(vi);
            }
            foreach (var item in root.distnet.getAllObjListByObjType(EObjectType.输电线路).OrderByDescending(p => (p.busiRunData as RunDataACLine).rateOfLoad).Take(10))
            {
                RunDataACLine rundata = item.busiRunData as RunDataACLine;
                MyClassLibrary.DevShare.VListItem vi = new MyClassLibrary.DevShare.VListItem() { id = item.id, name = item.name, value = rundata.rateOfLoad, format = "{0:p0}", brush = rundata.rateOfLoadBrush };
                lst.Add(vi);
            }
            foreach (var item in lst.OrderByDescending(p=>p.value))
                loadrates.Add(item);
            RaisePropertyChanged(() => loadrates);

            volts.Clear();
            foreach (var item in root.distnet.getAllObjListByObjType(EObjectType.节点).OrderByDescending(p => Math.Abs((p.busiRunData as RunDataNode).voltPUV-1)).Take(10))
            {
                RunDataNode rundata = item.busiRunData as RunDataNode;
                MyClassLibrary.DevShare.VListItem vi = new MyClassLibrary.DevShare.VListItem() { id = item.id, name = item.name, value = rundata.voltPUV, format = "{0:f2}", brush = rundata.voltPUVBrush };
                vi.value2 = 1 + 2 * (vi.value - 1);
                volts.Add(vi);
            }
            RaisePropertyChanged(() => volts);

            //清洁能源
            greenPower = 0;
            greenpowers.Clear();
            foreach (var item in root.distnet.getAllObjListByCategory(EObjectCategory.电厂设施类))
            {
                RunDataPlantBase rundata = item.busiRunData as RunDataPlantBase;
                greenPower += rundata.power;
                MyClassLibrary.DevShare.VListItem vi = new MyClassLibrary.DevShare.VListItem() { id = item.id, name = item.name, value = rundata.rateOfLoad, format = "{0:p0}", brush = rundata.rateOfLoadBrush };
                greenpowers.Add(vi);
            }
            greenPower = ((int)(greenPower*10))/10;
            RaisePropertyChanged(() => greenPower);
            RaisePropertyChanged(() => greenpowers);
            greenscales.Clear();
            greenscales.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argu = "清洁能源", value = greenPower/curLoad });
            greenscales.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argu = " ", value =(curLoad- greenPower)/curLoad });
            RaisePropertyChanged(() => greenscales);


            //电动汽车
            carLoad = (int)(40 + rd.Next(80));
            RaisePropertyChanged(() => carLoad);
            //大客户
            customLoad = (int)(200 + rd.Next(300));
            RaisePropertyChanged(() => customLoad);
            //停电
            cutLoad = (int)(10 + rd.Next(20));
            RaisePropertyChanged(() => cutLoad);
            //事件
            lstEvent = DataGenerator.events;
            RaisePropertyChanged(() => lstEvent);
        }

        public void start()
        {
            //初始模拟
            dayElectricity = 301;

            //负荷曲线
            DateTime d = DateTime.Now;
            d = new DateTime(d.Year, d.Month, d.Day);
            for (int i = 0; i < 96; i++)
            {
                MyClassLibrary.DevShare.ChartDataPoint cp= new MyClassLibrary.DevShare.ChartDataPoint(){argudate=d.AddMinutes(15*i), value=MyClassLibrary.MyFunction.simHourData((int)(i/4))*200+rd.Next(30)};
                if (i<66)
                    realLoads.Add(cp);
                planLoads.Add(new MyClassLibrary.DevShare.ChartDataPoint(){argudate=d.AddMinutes(15*i), value=cp.value+rd.Next(20)});
            }
            RaisePropertyChanged(() => planLoads); RaisePropertyChanged(() => realLoads);

            readData();
            tmr.Start();
        }

        public void stop()
        {
            tmr.Stop();
        }

        #endregion

    }


  

}
