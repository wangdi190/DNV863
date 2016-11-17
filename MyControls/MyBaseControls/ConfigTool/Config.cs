using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyClassLibrary;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace MyBaseControls.ConfigTool
{
    public static class Config
    {
        static Config()
        {
            load();
        }


        private static bool _isMemory;
        ///<summary>是否记忆设置过的属性，若为真，配合refreshObjects方法，配置参数的修改将可以即时刷新到屏幕</summary>
        public static bool isMemory
        {
            get { return _isMemory; }
            set { _isMemory = value; }
        }

        ///<summary>最大记忆条数，缺省为10000, 若超出些数目，系统删除按时间戳排序的后5%条</summary>
        public static int maxMemoryCount = 10000;

        static Dictionary<string, memo> memoes = new Dictionary<string, memo>();

        public static ConfigData cfgData { get; set; }

        public static void save() { XmlHelper.saveToXml(".\\xml\\config.xml", cfgData); }

        public static void load() { cfgData = (ConfigData)XmlHelper.readFromXml(".\\xml\\config.xml", typeof(ConfigData)); }


        ///<summary>为obj对象的prop【 lamda写法示例：()=>obj.lineColor 】属性，设置cfgkey键指定的值, </summary>
        public static bool setValue<T>(object obj, string objid, Expression<Func<T>> prop, string cfgkey)
        {
            var expression = (MemberExpression)prop.Body;
            string propertyName = expression.Member.Name;
            return setValue(obj, objid, propertyName, cfgkey);
        }

        ///<summary>为obj对象的propertyName的属性，设置cfgkey键指定的值</summary>
        public static bool setValue(object obj, string objid, string propertyName, string cfgkey)
        {

            Type objType = obj.GetType();
            PropertyInfo pinfo = objType.GetProperty(propertyName);

            object value;
            if (pinfo.PropertyType == typeof(int))
                value = Convert.ToInt16(cfgData.items[cfgkey].value);
            else if (pinfo.PropertyType == typeof(float))
                value = Convert.ToSingle(cfgData.items[cfgkey].value);
            else if (pinfo.PropertyType == typeof(double))
                value = Convert.ToDouble(cfgData.items[cfgkey].value);
            else if (pinfo.PropertyType == typeof(bool))
                value = Convert.ToBoolean(cfgData.items[cfgkey].value);
            else if (pinfo.PropertyType == typeof(Color))
                value = ColorConverter.ConvertFromString(cfgData.items[cfgkey].value);
            else if (pinfo.PropertyType == typeof(Brush))
                value =new SolidColorBrush((Color)ColorConverter.ConvertFromString(cfgData.items[cfgkey].value));
            else
                return false;


            pinfo.SetValue(obj, value, null);

            if (isMemory)
            {
                if (memoes.Count > maxMemoryCount)
                    clearMemoes();

                memo m = new memo() { objid=objid, objtype=objType, propertyname=propertyName, cfgkey= cfgkey, lastwrite=DateTime.Now };
                if (memoes.ContainsKey(m.key))
                {
                    m = memoes[m.key];
                    m.lastwrite = DateTime.Now;
                }
                else
                    memoes.Add(m.key, m);

            }


            return true;
        }

        static void clearMemoes()
        {
            List<string> dellist = memoes.Values.OrderBy(p => p.lastwrite).Take((int)0.05 * maxMemoryCount).Select(p => p.key).ToList();
            foreach (string key in dellist)
            {
                memoes.Remove(key);
            }
        }

        ///<summary>刷新给定对象列表中的曾用cfgkey赋过值的对象, Config.isMemory必须为真，才能记录下曾经的赋值</summary>
        public static void refreshObjects(Dictionary<string,object> objdict, string cfgkey)
        {
            IEnumerable<memo> listcfg = memoes.Values.Where(p => p.cfgkey == cfgkey);
            foreach (var item in listcfg)
            {
                object obj;
                if (objdict.TryGetValue(item.objid,out obj))
                {
                    setValue(obj, item.objid, item.propertyname, cfgkey);
                }
            }

        }

    }


    internal struct memo
    {
        public string key { get { return string.Format("{0}.{1}.{2}", objid, propertyname, cfgkey); } }

        public Type objtype { get; set; }
        public string objid { get; set; }
        public string propertyname { get; set; }
        public string cfgkey { get; set; }
        public DateTime lastwrite { get; set; }
    }


}
