using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;

namespace DistNetLibrary.Edit
{
    ///<summary>描述可使用的对象的集合，需在代码中进行可使用项注册</summary>
    public class DNObjectDesc
    {
        public DNObjectDesc(Dictionary<string,DBDesc> dbdesc )
        {
            foreach (string item in Enum.GetNames(typeof(EObjectCategory)))  //初始化类别
            {
                dndescs.Add(new DNDesc() { name = item, info=item });
            }

            foreach (var e0 in dbdesc.Values)
            {
                foreach (var item in e0.SQLS) //遍历所有描述定义
                {
                    reg(item.DNObjTypeFullName, item.key);
                }
            }

        }

        public List<DNDesc> dndescs = new List<DNDesc>();

        /////<summary>注册可用项，注:已不使用，改为自动获取</summary>
        //public void regCanCreateItem(Type type, string dbopkey)
        //{
        //    pLayer tmplayer = new pLayer(null);
        //    DNDesc dn = new DNDesc() { type = type };
        //    PowerBasicObject o = dn.CreateByType(tmplayer);
        //    DescData dd = o.busiDesc as DescData;
        //    dn.sort = dd.objCategory.ToString();
        //    dn.name = type.Name;
        //    dn.dbopkey = dbopkey;
        //    dn.icon = dd.icon;

        //    dn.info = string.Format("{0}({1})", dbopkey, dn.name);
        //    dndescs.Add(dn);
      
        //}

      
        void reg(string fulltypename,string dbopkey)
        {
            pLayer tmplayer = new pLayer(null);
            DNDesc dn = new DNDesc() { typefullname=fulltypename };
            PowerBasicObject o = dn.CreateByTypeName(tmplayer);
            DescData dd = o.busiDesc as DescData;
            dn.sort = dd.objCategory.ToString();
            dn.type = o.GetType();
            dn.name = dbopkey; //dn.type.Name;
            dn.dbopkey = dbopkey;
            dn.icon = dd.icon;

            dn.info = string.Format("{0}({1})", dbopkey, dn.type.Name);//dn.name);
            dndescs.Add(dn);
        }

    }


    public class DNDesc
    {
        public string sort { get; set; }
        public string name { get; set; }
        public System.Windows.Media.Brush icon { get; set; }
        public string dbopkey { get; set; }
        public Type type { get; set; }
        public string typefullname { get; set; }

        public string info { get; set; }

        public WpfEarthLibrary.PowerBasicObject CreateByType(pLayer layer)
        {

            Object[] parameters = new Object[1]; // 定义构造函数需要的参数，所有参数都必须为Object
            parameters[0] = layer;
            return (PowerBasicObject)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(type.FullName, false,
                System.Reflection.BindingFlags.Default, null, parameters, null, null);
        }
        public WpfEarthLibrary.PowerBasicObject CreateByTypeName(pLayer layer)
        {

            Object[] parameters = new Object[1]; // 定义构造函数需要的参数，所有参数都必须为Object
            parameters[0] = layer;
            return (PowerBasicObject)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(typefullname, false,
                System.Reflection.BindingFlags.Default, null, parameters, null, null);
        }
    }

}
