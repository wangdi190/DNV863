using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfEarthLibrary
{
    ///<summary>业务数据基类：
    ///可包含大多数简单应用可存放的业务数据，
    ///复杂应用应以此为基类派生新类，并替换powerbasicobject对象中的busiData
    ///</summary>
    public class busiBase
    {
        //public busiBase(){}
        public busiBase(PowerBasicObject Parent)
        {
            parent = Parent;
        }
        protected PowerBasicObject parent;

        ///<summary>业务数据扩展，类别(业务数据扩展属性只提供数据存取方法，本身不执行任何业务逻辑)</summary>
        public string busiCategory { get; set; }
        ///<summary>业务数据扩展，分类(业务数据扩展属性只提供数据存取方法，本身不执行任何业务逻辑)</summary>
        public string busiSort { get; set; }
        ///<summary>业务数据扩展，类型(业务数据扩展属性只提供数据存取方法，本身不执行任何业务逻辑)</summary>
        public string busiType { get; set; }
        ///<summary>业务数据扩展，日期(业务数据扩展属性只提供数据存取方法，本身不执行任何业务逻辑)</summary>
        public DateTime busiDatetime { get; set; }
        ///<summary>业务数据扩展，标志(业务数据扩展属性只提供数据存取方法，本身不执行任何业务逻辑)</summary>
        public int busiFlag { get; set; }
        ///<summary>业务数据扩展，真假(业务数据扩展属性只提供数据存取方法，本身不执行任何业务逻辑)</summary>
        public bool busiBool { get; set; }
        ///<summary>业务数据扩展，颜色1(业务数据扩展属性只提供数据存取方法，本身不执行任何业务逻辑)</summary>
        public Color busiColor1 { get; set; }
        ///<summary>业务数据扩展，颜色2(业务数据扩展属性只提供数据存取方法，本身不执行任何业务逻辑)</summary>
        public Color busiColor2 { get; set; }
        ///<summary>业务数据扩展，图标(业务数据扩展属性只提供数据存取方法，本身不执行任何业务逻辑)</summary>
        public Brush busiBrush { get; set; }
        ///<summary>业务数据扩展，电压等级(业务数据扩展属性只提供数据存取方法，本身不执行任何业务逻辑)</summary>
        public int busiVL { get; set; }
        ///<summary>业务数据扩展，状态(业务数据扩展属性只提供数据存取方法，本身不执行任何业务逻辑)</summary>
        public int busiStatus { get; set; }

        ///<summary>额定值</summary>
        public double busiRatingValue { get; set; }
        ///<summary>当前值</summary>
        public double busiCurValue { get; set; }
        ///<summary>旧值</summary>
        public double busiOldValue { get; set; }
        ///<summary>最大值</summary>
        public double busiMaxValue { get; set; }
        ///<summary>最小值</summary>
        public double busiMinValue { get; set; }
        ///<summary>平均值</summary>
        public double busiAvgValue { get; set; }
        ///<summary>合计值</summary>
        public double busiSumValue { get; set; }
        ///<summary>百分比值</summary>
        public double busiPercentValue { get; set; }

        ///<summary>值1</summary>
        public double busiValue1 { get; set; }
        ///<summary>值2</summary>
        public double busiValue2 { get; set; }
        ///<summary>值3</summary>
        public double busiValue3 { get; set; }

        ///<summary>值1</summary>
        public string busiStr1 { get; set; }
        ///<summary>值2</summary>
        public string busiStr2 { get; set; }
        ///<summary>值3</summary>
        public string busiStr3 { get; set; }

        ///<summary>附加对象</summary>
        public object busiAddition { get; set; }

        ///<summary>附加值集合</summary>
        public System.ComponentModel.BindingList<double> busiValues { get; set; }
        ///<summary>附加字串集合</summary>
        public System.ComponentModel.BindingList<string> busiStrings { get; set; }

    }

  


}
