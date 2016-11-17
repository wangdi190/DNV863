using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
//using System.Windows.Forms;

namespace DataLayer
{
    #region 数据模拟类
    static class Simulate
    {
        private static Dictionary<string, string> factoryDic = new Dictionary<string, string>();//
        static Simulate()
        {//当增加一个字段数据类时要在这里注册
            Simulate.AddClass("i", "DataLayer.IntFieldInfo");
            Simulate.AddClass("f", "DataLayer.DoubleFieldInfo");
            Simulate.AddClass("d", "DataLayer.DateFieldInfo");
            Simulate.AddClass("s", "DataLayer.StringFieldInfo");
        }
        public static void AddClass(string key, string className)
        {//注册类型关键字和对应的字段类
            if (null != key && "" != key && null != className && "" != className && !factoryDic.ContainsKey(key))
            {
                factoryDic.Add(key, className);
            }
        }
        private static FieldInfo ParseField(string text)
        {//解析字段描述符，如"iv1e1r"
            if (null == text || "" == text)
            {
                //MessageBox.Show("Simulate.ParseField参数为null或“”");
                return null;
            }
            string type = text[0].ToString();
            if(!factoryDic.ContainsKey(type))
            {
                string message = String.Format("Simulate.ParseField参数text='{0}'所描述的数据类型未在Simulate中注册，请检查", text);
                //MessageBox.Show(message);               
                return null;
            }
            object fieldObj = System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(factoryDic[type]);
            FieldInfo fieldInfo = fieldObj as FieldInfo;
            if (null == fieldInfo || !fieldInfo.ParseString(text))
            {
                return null;
            }

            return fieldInfo;
        }
        private static DataTable simGenEmptyDT(List<FieldInfo> lstfd)
        {//根据字段生成一张空表
            DataTable dt = new DataTable();
            foreach (FieldInfo item in lstfd)
            {
                dt.Columns.Add(new DataColumn(item.FieldName, item.FieldType));
            }
            //dt.Columns.Add(new DataColumn("zSort0", typeof(string)));   //层0   
            return dt;
        }
        private static void ParseFields(string sim, out List<FieldInfo> fields, out int lineNum)
        {//根据一条完成的sim语句解析出字段信息和数据条数
            Match match = Regex.Match(sim, @"\s*select\s*");//查找select标识符
            if (match.Success)
                sim = sim.Replace(match.Value, "");//去除select及前后的空格

            match = Regex.Match(sim, @"top\s*(\d+)\s*");//查找top标识符
            if (match.Success)
            {
                lineNum = int.Parse(match.Groups[1].Value);
                sim = sim.Replace(match.Value, "");//去除top后的数字和空格
            }
            else
            {
                Random r = new Random();
                lineNum = r.Next(10,100); ;
            }

            string tableName = null;
            match = Regex.Match(sim, @"\s*from\s*(.+)");//查找表名
            if (match.Success)
            {
                tableName = match.Groups[1].Value;
                sim = sim.Replace(match.Value, "");//去除  from 表名
            }

            fields = new List<FieldInfo>();
            string[] fieldsText = sim.Split(",".ToCharArray());//分离出各字段描述字符串
            foreach (string field in fieldsText)
            {
                FieldInfo def = ParseField(field.Trim());
                if (def != null)
                {
                    fields.Add(def);
                }
            }
        }

        private static DataTable CreateTableByFields(List<FieldInfo> fields, int lineNum)
        {
            DataTable table = simGenEmptyDT(fields);//向表中添加字段
            for (int i = 0; i < lineNum; ++i)
            {
                table.Rows.Add(table.NewRow());//添加空数据
            }
            return table;
        }

        private static bool FillID(DataTable table, List<int> ids)
        {//将ids中的值填入table中的id字段
            bool res = false;
            if (null == table || null == ids || ids.Count == 0)
            {
                return false;
            }
            
            if (table.Columns.Contains("id") && ids.Count > 0)
            {
                res = true;
                DataColumn col = table.Columns["id"];
                for (int i = 0; i < table.Rows.Count; ++i)
                {
                    if (i < ids.Count)
                    {
                        table.Rows[i][col] = (object)ids[i];
                    }
                    else
                    {
                        int id = (int)table.Rows[i - 1][col];
                        table.Rows[i][col] = (object)(id + 1);
                    }
                }
            }
            return res;
        }

        public static DataTable simData(string sim, DataTable oldTable = null)
        {
            List<FieldInfo> fields = null;
            int lineNum;
            ParseFields(sim, out fields, out lineNum);
            if (oldTable != null) lineNum = oldTable.Rows.Count;
            //当需要sim生成的DataTable和oldTable为不同的引用时使用下面的代码
            DataTable table = CreateTableByFields(fields, lineNum);
            foreach (FieldInfo field in fields)
            {
                field.FillDataTable(table, oldTable);//填充数据
            }
            return table;
            //当需要sim生成的DataTable和oldTable为同一个引用时使用下面的代码
            //foreach (FieldInfo field in fields)
            //{
            //    field.FillDataTable(oldTable, oldTable);//填充数据
            //}
            //return oldTable;
            //
        }

        public static DataTable simData(string sim, List<int> ids)
        {
            List<FieldInfo> fields = null;
            int lineNum;
            ParseFields(sim, out fields, out lineNum);
            lineNum = ids.Count;
            DataTable table = CreateTableByFields(fields, lineNum);
            foreach (FieldInfo field in fields)
            {
                if ("id" == field.FieldName && FillID(table, ids))
                {
                    continue;
                }
                field.FillDataTable(table);//填充数据
            }
            return table;
        }

        public static DataTable simData(string sim, List<string> ids)
        {
            List<int> ids2 = new List<int>();
            if(null != ids)
            {
                foreach (string id in ids)
                {
                    ids2.Add(int.Parse(id));
                }
            }
            return simData(sim, ids2);
        }
    }

    #endregion




    #region 字段信息类

    #region 字段信息基类
    internal abstract class FieldInfo
    {
        private string m_fieldName = "";
        private Type m_fieldType = null;
        protected static Random ra = new Random();
        public string FieldName
        {
            get { return m_fieldName; }
            set { m_fieldName = value; }
        }

        public Type FieldType
        {
            get { return m_fieldType; }
            set { m_fieldType = value; }
        }

        public abstract void FillDataTable(DataTable table, DataTable oldtable = null);//如果oldtable不为null，则在此表基础上变化
        public virtual bool ParseString(string text)
        {//解析字段描述字符串，如"iv1e1 as 表"
            if (null == text || "" == text) return false;
            Match match = Regex.Match(text, @"as\s*(.+)");//查找字段名称
            if (match.Success)
            {
                m_fieldName = match.Groups[1].Value;
            }
            else
            {
                m_fieldName = text;
            }
            return true;
        }
    }
    #endregion

    #region 数字型字段模板类
    internal abstract class NumFieldInfo<T> : FieldInfo
    {
        private T m_defaultValue;//由字段描述符中的v指定，v指定的值应该在minmax范围内
        private T m_Max;//由字段描述符中的max指定
        private T m_Min;//由字段描述符中的min指定
        private T m_Step;//由字段描述符中的e指定
        private bool m_bRand;//当字段描述符中指定了v时，该值为false，否则为true
        private bool m_bR;//当指定了r关键字时为true

        /*
        当m_bRand=false有2种情况
        1.指定了v而没有指定e（m_Step=0），此时该字段所有数据都为m_defaultValue
        2.1.指定了v和e（m_Step!=0,如果字段描述符中为e值指定0则按照没有指定e值处理），此时该字段第一条数据为m_defaultValue，后面的数据为前一条加m_Step
        当m_bRand=true有2中情况
        1.指定了e，此时该字段第一条数据随机，后面的数据为前一条加m_Step
        2.没有指定e，此时该字段所有数据均随机
        */

        public T DefaultValue
        {
            get { return m_defaultValue; }
            set { m_defaultValue = value; }
        }

        public T Max
        {
            get { return m_Max; }
            set { m_Max = value; }
        }

        public T Min
        {
            get { return m_Min; }
            set { m_Min = value; }
        }

        public T Step
        {
            get { return m_Step; }
            set { m_Step = value; }
        }

        public bool BRand
        {
            get { return m_bRand; }
            set { m_bRand = value; }
        }

        public bool BR
        {
            get { return m_bR; }
            set { m_bR = value; }
        }

        protected abstract T RandValue();//获得随机数据
        protected abstract T RandValue(T old);//根据原始值获得随机数据，指定了r关键字
        protected abstract T INC(T value);//根据step递增
        protected abstract T ParseT(string text);//根据文本解析T类型的值
        protected abstract bool StepValid();

        public override void FillDataTable(DataTable table, DataTable oldtable = null)
        {//如果oldtable不为null，则在此表基础上变化
            if (null == table)
            {
                //MessageBox.Show("FillDataTable函数中的table参数为null");
                return;
            }
            if (!table.Columns.Contains(FieldName))
            {
                //MessageBox.Show("FillDataTable函数中的table参数没有包含名为" + FieldName + "的列");
                return;
            }
            if (null != oldtable && !oldtable.Columns.Contains(FieldName))
            {
                //MessageBox.Show("FillDataTable函数中的oldtable参数没有包含名为" + FieldName + "的列");
                return;
            }
            if (null != oldtable && (table.Rows.Count != oldtable.Rows.Count))
            {
                //MessageBox.Show("FillDataTable函数中的table和oldtable参数的行数不一致");
                return;
            }
            DataColumn col = table.Columns[FieldName];
            if (null != col && col.DataType == FieldType)
            {
                for (int i = 0; i < table.Rows.Count; ++i)
                {
                    DataRow row = table.Rows[i];
                    if (null == oldtable)
                    {
                        if (BRand)
                        {//新生成随机值
                            if (StepValid())
                            {//指定了步进值
                                if (0 == i)
                                {//只有第一条数据随机
                                    row[col] = (object)RandValue();
                                }
                                else
                                {
                                    T value = (T)table.Rows[i - 1][col];
                                    table.Rows[i][col] = (object)INC(value);
                                }
                            }
                            else
                            {
                                row[col] = (object)RandValue();
                            }
                        }
                        else
                        {//新生成指定值
                            if (0 == i)
                            {
                                row[col] = (object)DefaultValue;
                            }
                            else
                            {
                                T value = (T)table.Rows[i - 1][col];
                                table.Rows[i][col] = (object)INC(value);
                            }
                        }
                    }
                    else
                    {//根据原有值生成数据 
                        DataColumn oldcol = oldtable.Columns[FieldName];
                        if (null != oldcol && i < oldtable.Rows.Count)
                        {
                            T oldValue = (T)oldtable.Rows[i][oldcol];
                            if(BR)
                            {
                                table.Rows[i][col] = (object)RandValue(oldValue);
                            }
                            else
                            {
                                table.Rows[i][col] = (object)INC(oldValue);
                            }
                        }
                        else
                        {//新数据数目大于旧数据数目处理
                            table.Rows[i][col] = (object)RandValue();
                        }
                    }
                }
            }
        }

        public override bool ParseString(string text)
        {//解析字段描述符，如"iv1e1"
            if (null == text || "" == text) return false;
            Match match = Regex.Match(text, @"v\s*([-.0-9]+)");//查找v后面的数字，数字包含在(-*\d+)中，\s*代表匹配任意个空白符
            if (match.Success)
            {
                DefaultValue = ParseT(match.Groups[1].Value);
                BRand = false;
            }
            match = Regex.Match(text, @"e\s*([-.0-9]+)");//查找e后面的数字，数字包含在(-*\d+)中，\s*代表匹配任意个空白符
            if (match.Success)
            {
                Step = ParseT(match.Groups[1].Value);
            }
            match = Regex.Match(text, @"min\s*([-.0-9]+)");//查找min后面的数字，数字包含在(-*\d+)中，\s*代表匹配任意个空白符
            if (match.Success)
            {
                Min = ParseT(match.Groups[1].Value);
            }
            match = Regex.Match(text, @"max\s*([-.0-9]+)");//查找min后面的数字，数字包含在(-*\d+)中，\s*代表匹配任意个空白符
            if (match.Success)
            {
                Max = ParseT(match.Groups[1].Value);
            }
            if (text.Contains("r"))
            {
                BR = true;
            }
            return base.ParseString(text);
        }
    }
    #endregion

    #region 整型字段信息类
    internal class IntFieldInfo : NumFieldInfo<int>
    {
        public IntFieldInfo()
        {
            Min = -1000000;
            Max = 1000000;
            Step = 0;
            BRand = true;
            BR = false;
            DefaultValue = RandValue();
            FieldType = typeof(int);
        }

        protected override int RandValue()
        {
            return ra.Next(Min, Max);
        }

        protected override int RandValue(int old)
        {//在old基础上浮动0.9-1.1倍
            double mul = 0.9 + (1.1 - 0.9) * ra.NextDouble();
            int inew = (int)(old * mul);
            if (inew < Min)
            {
                inew = Min;
            }
            else if (inew > Max)
            {
                inew = Max;
            }
            return inew;
        }

        protected override int INC(int value)//根据step递增
        {
            value += Step;
            if (value < Min) value = Min;
            if (value > Max) value = Max;
            return value;
        }
        protected override bool StepValid()//判断是否指定了步进值
        {
            return 0 != Step;
        }

        protected override int ParseT(string text)//根据文本解析T类型的值
        {
            return int.Parse(text);
        }

        public override bool ParseString(string text)
        {
            if (null == text || "" == text || 'i' != text[0]) return false;
            return base.ParseString(text);
        }
    }
    #endregion

    #region 浮点字段信息类
    internal class DoubleFieldInfo : NumFieldInfo<double>
    {
        public DoubleFieldInfo()
        {
            Min = -1000000;
            Max = 1000000;
            Step = 0;
            BRand = true;
            BR = false;
            FieldType = typeof(double);
            DefaultValue = RandValue();
        }

        protected override double RandValue()
        {
            return Min + (Max - Min) * ra.NextDouble();
        }

        protected override double RandValue(double old)
        {//在old基础上浮动0.9-1.1倍
            double mul = 0.9 + (1.1 - 0.9) * ra.NextDouble();
            double fnew = old * mul;
            if (fnew < Min) fnew = Min;
            if (fnew > Max) fnew = Max;
            return fnew;
        }

        protected override double INC(double value)//根据step递增
        {
            value += Step;
            if (value < Min) value = Min;
            if (value > Max) value = Max;
            return value;
        }
        protected override bool StepValid()//判断是否指定了步进值
        {
            return Math.Abs(Step) > 0.001;
        }

        protected override double ParseT(string text)//根据文本解析T类型的值
        {
            return double.Parse(text);
        }

        public override bool ParseString(string text)//根据字段描述符解析字段信息
        {
            if (null == text || "" == text || 'f' != text[0]) return false;
            return base.ParseString(text);
        }
    }
    #endregion

    #region 日期字段信息类
    internal class DateFieldInfo : FieldInfo
    {
        private int m_Step = 0;//由字段描述符中的e指定
        private DateTime m_defaultValue = DateTime.Now;
        private string m_subType = "dy";//dm,dy等

        public DateFieldInfo()
        {
            FieldType = typeof(DateTime);
        }

        public int Step
        {
            get { return m_Step; }
            set { m_Step = value; }
        }
        public DateTime DefaultValue
        {
            get { return m_defaultValue; }
            set { m_defaultValue = value; }
        }

        protected bool StepValid()//判断是否指定了步进值
        {
            return 0 != Step;
        }

        protected DateTime ParseT(string text)//根据文本解析T类型的值
        {
            return DateTime.Parse(text);
        }

        protected DateTime INC(DateTime value)//根据step递增
        {
            if (null == value) return value;
            switch (m_subType)
            {
                case "dy": { return value.AddYears(Step); } 
                case "dm": { return value.AddMonths(Step); }
                case "dd": { return value.AddDays(Step); } 
                case "dh": { return value.AddHours(Step); }
                case "dmi": { return value.AddMinutes(Step); }
                case "ds": { return value.AddSeconds(Step); } 
            }
            return value.AddYears(0);
        }

        public override bool ParseString(string text)//根据字段描述符解析字段信息
        {
            if (null == text || "" == text || 'd' != text[0]) return false;
            if (text.Length >= 3 && text.Substring(0, 3) == "dmi")
            {
                m_subType = "dmi";
            }
            else if (text.Length >= 2)
            {
                m_subType = text.Substring(0, 2);
            }
            Match match = Regex.Match(text, @"v\s*([.0-9]+)");//查找v后面的数字，数字包含在(-*\d+)中，\s*代表匹配任意个空白符
            if (match.Success)
            {
                DefaultValue = DateTime.Parse(match.Groups[1].Value);
            }
            match = Regex.Match(text, @"e\s*(-*\d+)");//查找e后面的数字，数字包含在(-*\d+)中，\s*代表匹配任意个空白符
            if (match.Success)
            {
                Step = int.Parse(match.Groups[1].Value);
            }
            return base.ParseString(text);
        }

        public override void FillDataTable(DataTable table, DataTable oldtable = null)
        {//如果oldtable不为null，则在此表基础上变化
            if (null == table)
            {
                //MessageBox.Show("FillDataTable函数中的table参数为null");
                return;
            }
            if (!table.Columns.Contains(FieldName))
            {
                //MessageBox.Show("FillDataTable函数中的table参数没有包含名为" + FieldName + "的列");
                return;
            }
            if (null != oldtable && !oldtable.Columns.Contains(FieldName))
            {
                //MessageBox.Show("FillDataTable函数中的oldtable参数没有包含名为" + FieldName + "的列");
                return;
            }
            if (null != oldtable && (table.Rows.Count != oldtable.Rows.Count))
            {
                //MessageBox.Show("FillDataTable函数中的table和oldtable参数的行数不一致");
                return;
            }
            DataColumn col = table.Columns[FieldName];
            if (null != col && col.DataType == FieldType)
            {
                for (int i = 0; i < table.Rows.Count; ++i)
                {
                    DataRow row = table.Rows[i];
                    if (null == oldtable)
                    {//新生成指定值
                        if (0 == i)
                        {
                            row[col] = (object)DefaultValue;
                        }
                        else
                        {
                            DateTime value = (DateTime)table.Rows[i - 1][col];
                            table.Rows[i][col] = (object)INC(value);
                        }
                    }
                    else
                    {//根据原有值生成数据 
                        DataColumn oldcol = oldtable.Columns[FieldName];
                        if (null != oldcol && i < oldtable.Rows.Count)
                        {
                            DateTime oldValue = (DateTime)oldtable.Rows[i][oldcol];
                            table.Rows[i][col] = (object)INC(oldValue);
                        }
                        else if (0 == i)
                        {//新数据数目大于旧数据数目处理
                            table.Rows[i][col] = (object)DefaultValue;
                        }
                        else
                        {//新数据数目大于旧数据数目处理
                            DateTime value = (DateTime)table.Rows[i - 1][col];
                            table.Rows[i][col] = (object)INC(value);
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region 字符字段信息类
    internal class StringFieldInfo : FieldInfo
    {
        private string[] m_str1 = null;//根据s{甲;乙;丙}解析得来
        private string[] m_str2 = new string[5] { "AA", "BB", "CC", "DD", "EE" };
        private string m_str3 = null;//保存“描*述”类型的字符串

        public StringFieldInfo()
        {
            FieldType = typeof(string);
        }

        public override bool ParseString(string text)//根据字段描述符解析字段信息
        {
            if (null == text || "" == text || 's' != text[0]) return false;
            Match match = Regex.Match(text, @"s{(.+)}");//查找s{甲;乙;丙} 类型字符串
            if (match.Success)
            {
                m_str1 = match.Groups[1].Value.Split(';');
            }
            match = Regex.Match(text, @"s\s*([^\s]+)");//查找"描*述"类型字符串
            if (match.Success)
            {
                m_str3 = match.Groups[1].Value;
            }
            return base.ParseString(text);
        }

        public override void FillDataTable(DataTable table, DataTable oldtable = null)
        {//oldtable=null表示新生成数据，否则在原数据基础上改动，string类型oldtable不起作用
            if (null == table) return;
            DataColumn col = table.Columns[FieldName];
            if (null != col && col.DataType == FieldType)
            {
                for (int i = 0; i < table.Rows.Count; ++i)
                {
                    DataRow row = table.Rows[i];
                    int index = 0;
                    if (null != m_str1)
                    {
                        index = ra.Next(m_str1.Length);
                        row[col] = (object)m_str1[index];
                    }
                    else if (null != m_str3)
                    {
                        index = ra.Next(m_str2.Length);
                        row[col] = (object)m_str3.Replace("*", m_str2[index]);
                    }
                }
            }
        }
    }
    #endregion

    #endregion
}
