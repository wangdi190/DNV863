using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MyClassLibrary;

namespace WpfEarthLibrary
{


    public class ColorManager
    {
        public ColorManager()
        {
            
        }

        public enum ECMapBackground { 卫星和无底图, 道路和地形 }
        public enum ECObjectType { 折线, 图元, 区域 }

        public ColorDatas colors = null;

        
        private bool _isEnabled;
        ///<summary>是否使用色彩管理器</summary>
        public bool isEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                if (value && colors==null)
                    loadColor();
            }
        }
      


        void init()
        {
            initline(); initsymbol(); initarea();
        }
        void initline()
        {
            Color color = Colors.Red;
            foreach (ECMapBackground mtype in Enum.GetValues(typeof(ECMapBackground)))
            {
                foreach (pPowerLine.ECType ctype in Enum.GetValues(typeof(pPowerLine.ECType)))
                {
                    foreach (pPowerLine.ECStatus cstatus in Enum.GetValues(typeof(pPowerLine.ECStatus)))
                    {
                        if (mtype == ECMapBackground.卫星和无底图)
                        {
                            #region 线路卫星
                            switch (cstatus)
                            {
                                case pPowerLine.ECStatus._正常:
                                    color = Colors.Cyan;
                                    break;
                                case pPowerLine.ECStatus.选择:
                                    color = Colors.Purple;
                                    break;
                                case pPowerLine.ECStatus.过载:
                                    color = Color.FromRgb(0xFF, 0x66, 0x00);
                                    break;
                                case pPowerLine.ECStatus.轻载:
                                    color = Colors.Lime;
                                    break;
                                case pPowerLine.ECStatus.检修:
                                    color = Colors.Yellow;
                                    break;
                                case pPowerLine.ECStatus.故障:
                                    color = Colors.Red;
                                    break;
                                case pPowerLine.ECStatus.停电:
                                    color = Colors.White;
                                    break;
                                case pPowerLine.ECStatus.测试:
                                    color = Color.FromRgb(0xFF, 0xFF, 0x99);
                                    break;
                                case pPowerLine.ECStatus.建设:
                                    color = Color.FromRgb(0xFF, 0xCC, 0xFF);
                                    break;
                                case pPowerLine.ECStatus.规划:
                                    color = Color.FromRgb(0x00, 0x66, 0xFF);
                                    break;
                                case pPowerLine.ECStatus.退运:
                                    color = Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF);
                                    break;
                            }

                            #endregion
                        }
                        else
                        {
                            #region 线路其它地图
                            switch (cstatus)
                            {
                                case pPowerLine.ECStatus._正常:
                                    color = Colors.Purple;
                                    break;
                                case pPowerLine.ECStatus.选择:
                                    color = Colors.Blue;
                                    break;
                                case pPowerLine.ECStatus.过载:
                                    color = Color.FromRgb(0xFF, 0x66, 0x00);
                                    break;
                                case pPowerLine.ECStatus.轻载:
                                    color = Color.FromRgb(0x00, 0x99, 0x00);
                                    break;
                                case pPowerLine.ECStatus.检修:
                                    color = Color.FromRgb(0xFF, 0x99, 0x00);
                                    break;
                                case pPowerLine.ECStatus.故障:
                                    color = Colors.Red;
                                    break;
                                case pPowerLine.ECStatus.停电:
                                    color = Colors.Gray;
                                    break;
                                case pPowerLine.ECStatus.测试:
                                    color = Color.FromRgb(0xFF, 0x99, 0x66);
                                    break;
                                case pPowerLine.ECStatus.建设:
                                    color = Color.FromRgb(0xFF, 0x99, 0x00);
                                    break;
                                case pPowerLine.ECStatus.规划:
                                    color = Colors.Blue;
                                    break;
                                case pPowerLine.ECStatus.退运:
                                    color = Color.FromArgb(0x80, 0x31, 0x31, 0x31);
                                    break;
                            }
                            #endregion

                        }
                        setColor(mtype, ECObjectType.折线, ctype.ToString(), cstatus.ToString(), color);
                    }
                }
            }
        }
        void initsymbol()
        {
            Color color = Colors.Red;
            foreach (ECMapBackground mtype in Enum.GetValues(typeof(ECMapBackground)))
            {
                foreach (pSymbolObject.ECType ctype in Enum.GetValues(typeof(pSymbolObject.ECType)))
                {
                    foreach (pSymbolObject.ECStatus cstatus in Enum.GetValues(typeof(pSymbolObject.ECStatus)))
                    {
                        if (mtype == ECMapBackground.卫星和无底图)
                        {
                            #region 卫星
                            switch (cstatus)
                            {
                                case pSymbolObject.ECStatus._正常:
                                    color = Colors.Blue;
                                    break;
                                case pSymbolObject.ECStatus.选择:
                                    color = Colors.Fuchsia;
                                    break;
                                case pSymbolObject.ECStatus.过载:
                                    color = Color.FromRgb(0xFF, 0x66, 0x00);
                                    break;
                                case pSymbolObject.ECStatus.轻载:
                                    color = Colors.Lime;
                                    break;
                                case pSymbolObject.ECStatus.检修:
                                    color = Colors.Yellow;
                                    break;
                                case pSymbolObject.ECStatus.故障:
                                    color = Colors.Red;
                                    break;
                                case pSymbolObject.ECStatus.停电:
                                    color = Colors.White;
                                    break;
                                case pSymbolObject.ECStatus.测试:
                                    color = Colors.MistyRose;
                                    break;
                                case pSymbolObject.ECStatus.建设:
                                    color = Colors.DarkGray;
                                    break;
                                case pSymbolObject.ECStatus.规划:
                                    color = Color.FromRgb(0x00, 0x99, 0xCC);
                                    break;
                                case pSymbolObject.ECStatus.退运:
                                    color = Color.FromArgb(0x7F, 0xFF, 0xFF, 0xFF);
                                    break;
                                case pSymbolObject.ECStatus.闭合:
                                    color = Colors.Lime;
                                    break;
                                case pSymbolObject.ECStatus.断开:
                                    color = Colors.Red;
                                    break;
                            }

                            #endregion
                        }
                        else
                        {
                            #region 图元其它地图
                            switch (cstatus)
                            {
                                case pSymbolObject.ECStatus._正常:
                                    color = Color.FromRgb(0x00, 0x66, 0x00);
                                    break;
                                case pSymbolObject.ECStatus.选择:
                                    color = Colors.DarkMagenta;
                                    break;
                                case pSymbolObject.ECStatus.过载:
                                    color = Color.FromRgb(0xFF, 0x66, 0x00);
                                    break;
                                case pSymbolObject.ECStatus.轻载:
                                    color = Color.FromRgb(0x00, 0x99, 0x00);
                                    break;
                                case pSymbolObject.ECStatus.检修:
                                    color = Color.FromRgb(0xFF, 0xCC, 0x00);
                                    break;
                                case pSymbolObject.ECStatus.故障:
                                    color = Colors.Red;
                                    break;
                                case pSymbolObject.ECStatus.停电:
                                    color = Colors.Gray;
                                    break;
                                case pSymbolObject.ECStatus.测试:
                                    color = Colors.RosyBrown;
                                    break;
                                case pSymbolObject.ECStatus.建设:
                                    color = Colors.DarkGray;
                                    break;
                                case pSymbolObject.ECStatus.规划:
                                    color = Color.FromRgb(0x00, 0x99, 0xCC);
                                    break;
                                case pSymbolObject.ECStatus.退运:
                                    color = Color.FromArgb(0x7F, 0x38, 0x38, 0x38);
                                    break;
                                case pSymbolObject.ECStatus.闭合:
                                    color = Color.FromRgb(0x00, 0x99, 0x00);
                                    break;
                                case pSymbolObject.ECStatus.断开:
                                    color = Colors.Red;
                                    break;
                            }
                            #endregion

                        }
                        setColor(mtype, ECObjectType.图元, ctype.ToString(), cstatus.ToString(), color);
                    }
                }
            }
        }
        void initarea()
        {
            Color color = Colors.Red;
            foreach (ECMapBackground mtype in Enum.GetValues(typeof(ECMapBackground)))
            {
                foreach (pArea.ECType ctype in Enum.GetValues(typeof(pArea.ECType)))
                {
                    foreach (pArea.ECStatus cstatus in Enum.GetValues(typeof(pArea.ECStatus)))
                    {
                        if (mtype == ECMapBackground.卫星和无底图)
                        {
                            #region 卫星
                            switch (cstatus)
                            {
                                case pArea.ECStatus._正常:
                                    color = Color.FromArgb(0x80, 0xFF, 0xFF, 0xE0);
                                    break;
                                case pArea.ECStatus.选择:
                                    color = Colors.Plum;
                                    break;
                                case pArea.ECStatus.供电:
                                    color = Color.FromArgb(0x7F, 0xF0, 0xE6, 0x8C);
                                    break;
                                case pArea.ECStatus.断电:
                                    color = Color.FromArgb(0x80, 0x00, 0x00, 0x00);
                                    break;
                            }

                            #endregion
                        }
                        else
                        {
                            #region 图元其它地图
                            switch (cstatus)
                            {
                                case pArea.ECStatus._正常:
                                    color = Color.FromArgb(0x7E, 0xFF, 0x48, 0x00);
                                    break;
                                case pArea.ECStatus.选择:
                                    color = Colors.MediumOrchid;
                                    break;
                                case pArea.ECStatus.供电:
                                    color = Color.FromArgb(0x7F, 0xF0, 0xE6, 0x8C);
                                    break;
                                case pArea.ECStatus.断电:
                                    color = Color.FromArgb(0x80, 0x00, 0x00, 0x00);
                                    break;
                            }
                            #endregion

                        }
                        setColor(mtype, ECObjectType.区域, ctype.ToString(), cstatus.ToString(), color);
                    }
                }
            }
        }




        ///<summary>获取预定义的颜色</summary>
        public Color getColor(EMapType mtype, ECObjectType otype, string ctype, string cstatus)
        {
            Color color = Colors.Black;
            //if (string.IsNullOrWhiteSpace(ctype) || string.IsNullOrWhiteSpace(cstatus)) return color;

            SerializableDictionary<string, Color> dic;
            ECMapBackground enummap;
            if (mtype == EMapType.卫星 || mtype == EMapType.无)
                enummap = ECMapBackground.卫星和无底图;
            else
                enummap = ECMapBackground.道路和地形;

            if (colors.cs[enummap][otype].TryGetValue(ctype, out dic))
                dic.TryGetValue(cstatus, out color);
            return color;
        }

        /////<summary>设置预定义颜色</summary>
        //public void setColor(EMapType mtype,ECObjectType otype , string ctype, string cstatus, Color color)
        //{

        //    ECMapBackground enummap;
        //    if (mtype== EMapType.卫星 || mtype== EMapType.无)
        //        enummap= ECMapBackground.卫星和无底图;
        //    else
        //        enummap= ECMapBackground.道路和地形;

        //    setColor(enummap, otype, ctype, cstatus, color);
        //}

        ///<summary>设置预定义颜色</summary>
        public void setColor(ECMapBackground enummap, ECObjectType otype, string ctype, string cstatus, Color color)
        {
            Color oldcolor;

            SerializableDictionary<string, Color> dic;
            if (!colors.cs.Keys.Contains(enummap))
                colors.cs.Add(enummap, new SerializableDictionary<ECObjectType, SerializableDictionary<string, SerializableDictionary<string, Color>>>());
            if (!colors.cs[enummap].Keys.Contains(otype))
                colors.cs[enummap].Add(otype, new SerializableDictionary<string, SerializableDictionary<string, Color>>());
            if (!colors.cs[enummap][otype].TryGetValue(ctype, out dic))
            {
                dic = new SerializableDictionary<string, Color>();
                colors.cs[enummap][otype].Add(ctype, dic);
            }
            if (!dic.TryGetValue(cstatus, out oldcolor))
            {
                dic.Add(cstatus, color);
            }
            else
                dic[cstatus] = color;
        }


        public void saveColor()
        {
            XmlHelper.saveToXml(".\\xml\\colorcfg.xml", colors);
        }

        public void loadColor()
        {
            colors = (ColorDatas)XmlHelper.readFromXml(".\\xml\\colorcfg.xml", typeof(ColorDatas));
            if (colors == null)
            {
                colors = new ColorDatas() { cs = new SerializableDictionary<ECMapBackground, SerializableDictionary<ECObjectType, SerializableDictionary<string, SerializableDictionary<string, Color>>>>() };
                init();
                saveColor();
            }
        }

    }



    [Serializable]
    public class ColorDatas
    {
        ///地图-图形大类-对象型号-状态
        public SerializableDictionary<ColorManager.ECMapBackground, SerializableDictionary<ColorManager.ECObjectType, SerializableDictionary<string, SerializableDictionary<string, Color>>>> cs { get; set; }

    }



}
