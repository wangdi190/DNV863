using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;

namespace DistNetLibrary
{
    /// <summary>
    /// 本拓扑数据以863数据为基础的配网拓扑数据：
    /// 1. 应填写至少单向的连接关系，通distnet.buildTopoRelation可补充为双向连接关系
    /// 2. 某些设备如配变，主变需填写它所属设施（配电室、变电站）等信息，方可得到设施的拓扑关系
    /// 3. 当前假定主变直接连接关系为母线，静态搜索拓扑关系时，搜索到主变的母线就中止搜索，动态搜按开关状态确定
    /// </summary>
    public class TopoData
    {
        public TopoData(PowerBasicObject Parent)
        {
            parent = Parent;
        }

        internal PowerBasicObject parent;

        ///<summary>设施对象包含的设备对象ID，以实现从设备拓扑到设施拓扑的转换</summary>
        public List<TopoObjDesc> containEquipments = new List<TopoObjDesc>();


        ///<summary>从属于的对象ID，用于存储配网关系，</summary>
        public List<TopoObjDesc> subordinateObjs = new List<TopoObjDesc>();

        ///<summary>填写(如主变母线所属的主变)：对象所属设备，用于提前中止以避免脏搜索，比如属于主变的母线，避免避开主变通过母线转连接到其它主变上</summary>
        public TopoObjDesc belongToEquipmentID;

        ///<summary>填写：对象所属设施ID</summary>
        public TopoObjDesc belontToFacilityID;

        ///<summary>填写：有直接关联关系的ID对象列表</summary>
        public List<TopoObjDesc> relationObjs = new List<TopoObjDesc>();

    }


    public class TopoObjDesc
    {
        ///<summary>有拓扑关系的对象的ID</summary>
        public string id { get; set; }

        ///<summary>来源数据表</summary>
        public string table { get; set; }

        ///<summary>来源字段</summary>
        public string field { get; set; }

        ///<summary>是否是扩展关联</summary>
        public bool isExpand { get; set; }
    }


    ///<summary>拓扑搜索结果类</summary>
    public class TopoFindResult
    {
        public TopoFindResult()
        {
            path = new List<string>();
        }
        ///<summary>路径</summary>
        public List<string> path { get; set; }
        ///<summary>路径长度</summary>
        public double lineLength { get; set; }
        ///<summary>路径中最小负荷余量</summary>
        public double surplusLoad { get; set; }
        ///<summary>途经节点数</summary>
        public int nodesCount { get; set; }

        public PowerBasicObject lastobj { get; set; }

        public bool isCompleted { get; set; }
    }
}
