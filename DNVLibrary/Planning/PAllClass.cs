using System;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Planning
{
    public class YearData
    {
        public YearData()
        {
            projects = new List<ProjectData>();
        }
        public int year { get; set; }
        public List<ProjectData> projects { get; set; }


        public Path path { get; set; }
        public PAllYearPorjects view { get; set; }
    }

    public class ProjectData
    {
        public ProjectData()
        {
            idx1 = new Index1();
            idx2 = new Index2();
            idx3 = new Index3();
            idxes = new Indexes();
        }
        public int year { get; set; }
        public int parentid { get; set; }
        ///<summary>实例ID</summary>
        public int prjid { get; set; }
        public string name { get; set; }
        public string note { get; set; }
        ///<summary>容载比</summary>
        public Index1 idx1 { get; set; }
        ///<summary>清洁能源渗透率</summary>
        public Index2 idx2 { get; set; }
        ///<summary>供电可靠性</summary>
        public Index3 idx3 { get; set; }

        public System.Windows.Media.Color color { get; set; }
        ///<summary>其它指标</summary>
        public Indexes idxes { get; set; }

        public List<WpfEarthLibrary.PowerBasicObject> addobjs = new List<WpfEarthLibrary.PowerBasicObject>();
    }


    #region ----- 指标定义 -----
    public abstract class IndexBase
    {
        public string name { get; set; }
        public string unit { get; set; }
        public double value { get; set; }
        public string note { get; set; }

        ///<summary>参数序为name,value,unit,note, 缺省为{0}{1:f0}{2}</summary>
        public string format { get; set; }

        public string strvalue { get { return string.Format(gaugeformat, value); } }

        public double min { get; set; }
        public double max { get; set; }

        public string gaugeformat { get; set; }  //仪表值格式
        public string labformat { get; set; }  //仪表标签格式
        public string lab0 { get { return string.Format(labformat, min + (max - min) / 5 * 0); } }  //仪表用标签
        public string lab1 { get { return string.Format(labformat, min + (max - min) / 5 * 1); } }
        public string lab2 { get { return string.Format(labformat, min + (max - min) / 5 * 2); } }
        public string lab3 { get { return string.Format(labformat, min + (max - min) / 5 * 3); } }
        public string lab4 { get { return string.Format(labformat, min + (max - min) / 5 * 4); } }
        public string lab5 { get { return string.Format(labformat, min + (max - min) / 5 * 5); } }
        public double loc0 { get { return min + 0.02 * (max - min); } }
        public double loc1 { get { return min + 0.22 * (max - min); } }
        public double loc2 { get { return min + 0.385 * (max - min); } }
        public double loc3 { get { return min + 0.57 * (max - min); } }
        public double loc4 { get { return min + 0.78 * (max - min); } }
        public double loc5 { get { return min + 1.02 * (max - min); } }

        public string info { get { return string.Format(format, name, value, unit, note); } }

    }

    
    public class Index1 : IndexBase
    {
        public Index1()
        {
            name = "容载比";
            unit = "";
            note = "指配网变电容量（kVA）在满足供电可靠性基础上与对应的负荷（kW）之比，是反映电网供电能力的重要技术经济指标之一，是宏观控制变电总容量和规划安排变电容量的依据。";
            format = "{0} {1:f1} ";
            min = 1;
            max = 3;
            labformat = "{0:f1}";
            gaugeformat = "{0:f1}";
        }
    }

    
    public class Index2 : IndexBase
    {
        public Index2()
        {
            name = "清洁能源渗透率";
            unit = "%";
            note = "清洁能源占供电负荷的比例";
            format = "{0} {1:p1} ";
            min = 0;
            max = 0.5;
            labformat = "{0:p0}";
            gaugeformat = "{0:p0}";
        }
    }

   
    public class Index3 : IndexBase
    {
        public Index3()
        {
            name = "供电可靠性";
            unit = "%";
            note = "通过计算设备可靠性和故障转供能力得到的综合供电可靠性指标。";
            format = "{0} {1:p1} ";
            min = 0.99;
            max = 1;
            labformat = "{0:p1}";
            gaugeformat = "{0:p2}";
        }
    }

    public class Index
    {


        public string name { get; set; }
        public string shortname { get {return name.IndexOf("（") < 0 ? name : name.Substring(0,name.IndexOf("（")); } }
        public double value { get; set; }
        public string format { get; set; }

        public double min { get; set; }
        public double max { get; set; }


        public void simData(double xs)
        {
            value = min + (max - min) * xs;
        }
    }

    public class Indexes
    {


        public Indexes()
        {
            indexes = new Dictionary<string, Index>();
            indexes.Add("主变容量（kW）", new Index() { format = "{V:f1}", min=1200,max=3500 });
            indexes.Add("配变容量（kW）", new Index() { format = "{V:f1}", min = 1200, max = 3500 });
            indexes.Add("DG容量（kW）", new Index() { format = "{V:f1}", min = 10, max = 300 });
            indexes.Add("线路长度（km）", new Index() { format = "{V:f1}", min = 150, max = 800 });
            indexes.Add("主变台数", new Index() { format = "{V:f0}", min = 2, max = 20 });
            indexes.Add("配变台数", new Index() { format = "{V:f0}", min = 22, max = 100 });

            foreach (var item in indexes)
            {
                item.Value.name = item.Key;
            }
        }

        public Dictionary<string,Index> indexes { get; set; }
    }

    #endregion


}
