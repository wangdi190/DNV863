using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace WpfEarthLibrary
{

    /// <summary>
    /// 设置类  ******************** 未完成，暂保留源码 *******************
    /// 1.从xml读取或保存在xml文件中
    /// 2.不被平台其它对象直接调用，而是本类的属性设置去修改以前的对象属性以生效，以便尽可能与以前版本兼容
    /// </summary>
    public class Settings
    {
        public Settings()
        {
            basicSettings = new BasicSettings();
            mapSettings = new MapSettings();
            lightSettings = new LightSettings();
            effectSettings = new EffectSettings();
        }

        ///<summary>静态方式，从指定xml中读取设置</summary>
        public static Settings ReadFromXml(string xmlfile)
        {
            try
            {
                Settings settings=(Settings)MyClassLibrary.XmlHelper.readFromXml(xmlfile, typeof(Settings));
                settings.xmlfile = xmlfile;
                return settings;
            }
            catch 
            {
                return null;
            }
        }
        public static void SaveToXml(Settings settings)
        {
            MyClassLibrary.XmlHelper.saveToXml(settings.xmlfile, settings);
        }

        [XmlIgnore]
        public string xmlfile { get; set; }


        public BasicSettings basicSettings { get; set; }

        public MapSettings mapSettings { get; set; }

        public LightSettings lightSettings { get; set; }

        public EffectSettings effectSettings { get; set; }
    }

    public class BasicSettings
    {
        ///<summary>设置内置工具栏是否可见</summary>
        public bool enableToolbox {get;set;}


    }

    public class MapSettings
    {

    }

    public class LightSettings
    {

    }

    public class EffectSettings
    {
        
    }

}
