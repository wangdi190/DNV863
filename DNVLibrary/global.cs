using System;
using System.Data;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;

namespace DNVLibrary
{
    /// <summary>
    /// 静态类管理所有方案
    /// </summary>
    internal static class global
    {
        static global()
        {
            instances = new ObservableCollection<InstanceData>();
            readProjects();

            objects = new Dictionary<string, ObjectData>();
            readObjects();
        }
        public static ObservableCollection<InstanceData> instances { get; set; }  //所有实例信息列表
        public static InstanceData curInstance { get; set; }
        ///<summary>对象字典，以对象id为key</summary>
        public static Dictionary<string,ObjectData> objects { get; set; }  //所有对象信息列表


        ///<summary>读取实例信息</summary>
        internal static void readProjects()
        {
            instances.Clear();
            string sql = "select * from all_instance";
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(sql);
            foreach (DataRow dr in dt.Rows)
            {
                instances.Add(new InstanceData()
                {
                    instanceID = dr.Field<int>("id"),
                    parentID = dr["parentid"] is DBNull ? null : (int?)dr.Field<int>("parentid"),
                    instanceName = dr.Field<string>("instancename"),
                    note = dr.Field<string>("note"),
                    idlist = dr.Field<string>("idlist"),
                    year=dr.Field<int>("year")
                });
            }

            curInstance = instances[0];
        }

        ///<summary>读取对象信息，为尽可能不改动以前的构架（工具构建），对象信息读了两次，以后有必要时调整构架只读一次</summary>
        internal static void readObjects()
        {
            objects.Clear();
            string sql = "select id,instanceid,objecttypeid,objoperatemode,objoperateid from all_object";
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(sql);
            foreach (DataRow dr in dt.Rows)
            {
                objects.Add(dr.Field<string>("id"),new ObjectData()
                {
                    instanceID = dr.Field<int>("instanceid"),
                     operateID=dr.getString("objoperateid"),
                     operateMode=(EObjectOperateMode)dr.getInt("objoperatemode"),
                     objtypeid=dr.getInt("objecttypeid")
                });
            }
        }
        ///<summary>清理未加载的对象项，退运项除外</summary>
        internal static void clearNoLoadObject()
        {
            //清除不是退运项且未加载的项
            List<string> tmp = (from e in objects
                                where e.Value.obj == null && e.Value.operateMode != EObjectOperateMode.退运
                                select e.Key).ToList();
            foreach (string id in tmp)
                objects.Remove(id);

            //清除退运所创建图形对象
            tmp = (from e in objects
                   where e.Value.operateMode == EObjectOperateMode.退运
                   select e.Key).ToList();
            foreach (var item in tmp)
                objects[item].obj = null;


        }

        ///<summary>重构显示对象，仅显示指定实例的对象，所有电力对象均位于layer层, 重置后应另行加载运行数据</summary>
        internal static void showInstanceObject(List<pLayer> layers, int instanceid)
        {
            foreach (pLayer layer in layers)
            {
                layer.pModels.Clear();    
            }

            
            //遍历组成实例的所有实例
            foreach (int instid in instances.First(p => p.instanceID == instanceid).listID.OrderBy(p => p))
            {

                foreach (var od in objects.Where(p => p.Value.instanceID == instid))
                {
                    if (od.Value.operateMode == EObjectOperateMode.新增)//增加设备对象
                       od.Value.obj.parent.pModels.Add(od.Key, od.Value.obj);
                    else if (od.Value.operateMode == EObjectOperateMode.退运)//退运设备
                        od.Value.obj.parent.pModels.Remove(od.Value.operateID);
                    else if (od.Value.operateMode == EObjectOperateMode.改造)//改造对象被系统视为退运旧对象，新加新对象，具有不同的ID
                    {
                        od.Value.obj.parent.pModels.Remove(od.Value.operateID);
                        od.Value.obj.parent.pModels.Add(od.Key, od.Value.obj);
                    }
                        
                }
            }
        }
      

        ///<summary>获取指定实例、指定标志的所有对象</summary>
        internal static Dictionary<string, PowerBasicObject> getObjDictByFlag(int instanceid, string flag)
        {
            Dictionary<string, PowerBasicObject> result = new Dictionary<string, PowerBasicObject>();
            //遍历组成实例的所有实例
            foreach (int instid in instances.First(p => p.instanceID == instanceid).listID.OrderBy(p => p))
            {

                foreach (var od in objects.Where(p => p.Value.instanceID == instid && (p.Value.obj.busiDesc as DistNetLibrary.DescData).Flags.ContainsKey(flag)))
                {
                    if (od.Value.operateMode == EObjectOperateMode.新增)//增加设备对象
                        result.Add(od.Key, od.Value.obj);
                    else if (od.Value.operateMode == EObjectOperateMode.退运)//退运设备
                        result.Remove(od.Value.operateID);
                    else if (od.Value.operateMode == EObjectOperateMode.改造)//改造对象被系统视为退运旧对象，新加新对象，具有不同的ID
                    {
                        result.Remove(od.Value.operateID);
                        result.Add(od.Key, od.Value.obj);
                    }

                }
            }
            return result;
        }

        ///<summary>获取指定实例的相对根的所有增量对象</summary>
        internal static Dictionary<string, PowerBasicObject> getAllAdditionObjects(int instanceid)
        {
            Dictionary<string, PowerBasicObject> result = new Dictionary<string, PowerBasicObject>();
            
            InstanceData inst = instances.FirstOrDefault(p => p.instanceID == instanceid);
            if (inst != null)
            {
                int rootid = inst.listID.Min();
                //遍历组成实例的所有实例
                foreach (int instid in instances.First(p => p.instanceID == instanceid).listID.OrderBy(p => p)) //无根的树路径
                {
                    if (instid == rootid) continue;
                    foreach (var od in objects.Where(p => p.Value.instanceID == instid))
                        result.Add(od.Key, od.Value.obj);
                }
            }
            return result;
        }

        ///<summary>获取指定实例的相对父的所有增量对象</summary>
        internal static Dictionary<string, PowerBasicObject> getParentAdditionObjects(int instanceid)
        {
            Dictionary<string, PowerBasicObject> result = new Dictionary<string, PowerBasicObject>();

            foreach (var od in objects.Where(p => p.Value.instanceID == instanceid))
                        result.Add(od.Key, od.Value.obj);
            return result;
        }


        ///<summary>读取规划运行数据</summary>
        internal static void readPlanningData(DistNetLibrary.DistNet distnet)
        {
            //distnet.clearContainerRundata();  //未完成

            List<PowerBasicObject> objs;
            objs = distnet.dbdesc["基础数据"].DictSQLS["主变2卷"].batchLoadRunData(distnet, false);
            objs = distnet.dbdesc["基础数据"].DictSQLS["变电站"].batchLoadRunData(distnet, false);
            objs = distnet.dbdesc["基础数据"].DictSQLS["线路"].batchLoadRunData(distnet, false);
            objs = distnet.dbdesc["基础数据"].DictSQLS["连接线"].batchLoadRunData(distnet, false);
            objs = distnet.dbdesc["基础数据"].DictSQLS["开关站"].batchLoadRunData(distnet, false);
            objs = distnet.dbdesc["基础数据"].DictSQLS["配电室"].batchLoadRunData(distnet, false);
            objs = distnet.dbdesc["基础数据"].DictSQLS["配变"].batchLoadRunData(distnet, false);
            objs = distnet.dbdesc["基础数据"].DictSQLS["母线"].batchLoadRunData(distnet, false);
            objs = distnet.dbdesc["基础数据"].DictSQLS["断路器"].batchLoadRunData(distnet, false);
            objs = distnet.dbdesc["基础数据"].DictSQLS["节点"].batchLoadRunData(distnet, false);

            distnet.statContainerRundata(); //未完成
        }

    }




    internal class InstanceData
    {
        public int instanceID { get; set; }
        public int? parentID { get; set; }
        public string instanceName { get; set; }
        public string note { get; set; }
        public int year { get; set; }

        private string _idlist;
        public string idlist  //所有父实例列表
        {
            get { return _idlist; }
            set
            {
                _idlist = value;

                string[] ss = value.Split(',');
                listID = new List<int>();
                for (int i = 0; i < ss.Count(); i++)
                {
                    listID.Add(int.Parse(ss[i]));
                }
            }
        }

        public List<int> listID { get; set; }

    }

    internal enum EObjectOperateMode { 新增, 退运, 改造 }
    internal class ObjectData
    {
        public int instanceID { get; set; }
        public EObjectOperateMode operateMode { get; set; }
        public string operateID { get; set; }
        public PowerBasicObject obj { get; set; }
        public int objtypeid { get; set; }
    }

}
