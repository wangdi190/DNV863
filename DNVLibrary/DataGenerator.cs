using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;

namespace DNVLibrary
{
    ///<summary>模拟数据生成器 </summary>
    internal static class DataGenerator
    {
        static System.Windows.Threading.DispatcherTimer tmrLine = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(10000) };
        static System.Windows.Threading.DispatcherTimer tmrSwitch = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(10000) };
        static System.Windows.Threading.DispatcherTimer tmrStation = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(10000) };
        static System.Windows.Threading.DispatcherTimer tmrVL = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(10000) };
        static DataGenerator()
        {
            tmrLine.Tick += new EventHandler(tmrLine_Tick);
            tmrStation.Tick += new EventHandler(tmrStation_Tick);
            tmrSwitch.Tick += new EventHandler(tmrSwitch_Tick);
            tmrVL.Tick += new EventHandler(tmrVL_Tick);
        }

        public static WpfEarthLibrary.Earth earth;
        static Random rd = new Random();

        public delegate void UpdateData(bool isShow);
        public static UpdateData UpdateLineData { get; set; }
        public static UpdateData UpdateStationData { get; set; }
        public static UpdateData UpdateVLData { get; set; }
        public static UpdateData UpdateSwitchData { get; set; }
        public static UpdateData UpdateSectionData { get; set; }

        public static void clearAll()
        {
            tmrLine.Stop();
            tmrStation.Stop();
            tmrSwitch.Stop();
            tmrVL.Stop();
            UpdateLineData = UpdateStationData = UpdateSwitchData = UpdateVLData =UpdateSectionData= null;
        }




        ///<summary>产生实时开关站数据</summary>
        static void tmrSwitch_Tick(object sender, EventArgs e)
        {
            foreach (pLayer layer in earth.objManager.zLayers.Values)
            {
                foreach (PowerBasicObject obj in layer.pModels.Values)
                {
                    if (obj.busiData.busiSort == "开关站")
                    {
                        double dv = rd.NextDouble();
                        obj.busiData.busiBool = dv < 0.2;  //以busiBool表示开合
                        (obj as pSymbolObject).symbolid = obj.busiData.busiBool ? "SwitchStationOpen" : "SwitchStationOpen2";


                    }
                }
            }
            if (UpdateSwitchData != null) UpdateSwitchData(true);
        }

        ///<summary>实时电压数据</summary>
        static void tmrVL_Tick(object sender, EventArgs e)
        {
            foreach (pLayer layer in earth.objManager.zLayers.Values)
            {
                foreach (PowerBasicObject obj in layer.pModels.Values)
                {
                    if (obj.busiData.busiSort == "变电站")
                    {
                        //电压标夭值
                        double tmpvalue = 0.85 + 0.3 * rd.NextDouble();
                        obj.busiData.busiValue1 = tmpvalue; //存储电压标幺值 , busiValue已被变电站负载占用
                        obj.busiData.busiValue2 = (tmpvalue - 0.85) / 0.3 * 100;  //存储模板用位置信息，避免写转换
                        obj.busiData.busiStr2 = tmpvalue.ToString("f2");

                    

                    }
                }
            }
            if (UpdateVLData != null) UpdateVLData(true);
        }


        ///<summary>产生实时变电站数据</summary>
        static void tmrStation_Tick(object sender, EventArgs e)
        {
            foreach (pLayer layer in earth.objManager.zLayers.Values)
            {
                foreach (PowerBasicObject obj in layer.pModels.Values)
                {
                    if (obj.busiData.busiSort == "变电站")
                    {

                        //负载数据
                        double dv = rd.NextDouble(); //负载率
                        obj.busiData.busiPercentValue = dv;
                        obj.busiData.busiCurValue = dv * obj.busiData.busiRatingValue; //有功功率
                        foreach (pData pd in obj.submodels.Values)
                        {
                            foreach (Data dd in pd.datas)
                            {
                                if (dd.argu == "负载")
                                {
                                    if (dv > 0.9)
                                        dd.color = Colors.Red;
                                    else if (dv < 0.2)
                                        dd.color = Color.FromRgb(0x00, 0x66, 0x00);
                                    else
                                        dd.color = Colors.Orange;
                                    dd.value = dv; //色在前改，只有值变，更新事件

                                    obj.busiData.busiColor1 = dd.color;
                                }
                                else
                                {
                                    dd.value = 1 - dv;
                                }


                            }

                        }

                        obj.busiData.busiStr1 = obj.busiData.busiPercentValue.ToString("p1"); 

                        obj.busiData.busiColor2 = obj.busiData.busiPercentValue > 0.9 ? Color.FromRgb(0xFF, 0x92, 0x92) : Color.FromRgb(0xFF, 0xDD, 0xA0);

                    }
                }
            }
            if (UpdateStationData != null) UpdateStationData(true);
        }

        ///<summary>产生实时线路数据</summary>
        static void tmrLine_Tick(object sender, EventArgs e)
        {
            foreach (pLayer layer in earth.objManager.zLayers.Values)
            {
                foreach (PowerBasicObject obj in layer.pModels.Values)
                {
                    if (obj.busiData.busiSort == "线路")
                    {
                        double sv = rd.NextDouble(); //模拟负载率
                        obj.busiData.busiPercentValue = sv;
                        obj.busiData.busiCurValue = obj.busiData.busiRatingValue * sv;
                        (obj as pPowerLine).color = sv > 0.9 ? Colors.Red : Colors.Cyan;
                        obj.busiData.busiStr1 = sv.ToString("p1"); //借用busitype为模板所用
                        obj.busiData.busiColor1 = (obj as pPowerLine).color;
                        obj.busiData.busiColor2 = sv > 0.9 ? Color.FromRgb(0xFF, 0x92, 0x92) : Color.FromRgb(0xB0, 0xFF, 0xFF);
                    }
                }
            }
            if (UpdateLineData != null) UpdateLineData(true);
            if (UpdateSectionData != null) UpdateSectionData(true);
        }


        public static void genLineData(bool isRun)
        {
            if (earth == null) return;
            if (isRun)
                tmrLine.Start();
            else
                tmrLine.Stop();
        }

        public static void genStationData(bool isRun)
        {
            if (earth == null) return;
            if (isRun)
                tmrStation.Start();
            else
                tmrStation.Stop();
        }
        public static void genVLData(bool isRun)
        {
            if (earth == null) return;
            if (isRun)
                tmrVL.Start();
            else
                tmrVL.Stop();
        }
        public static void genSwitchData(bool isRun)
        {
            if (earth == null) return;
            if (isRun)
                tmrSwitch.Start();
            else
                tmrSwitch.Stop();
        }


    }
}
