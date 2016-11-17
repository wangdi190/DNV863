using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using WpfEarthLibrary;


namespace DistNetLibrary
{
    ///<summary>配网模型</summary>
    public class DistNet
    {
        public DistNet()
        {
            SymbolAndGeomery.CreateSymbol(scene);  //创建图元资源
            SymbolAndGeomery.CreateGeometry(scene);  //创建几何体资源

        }


        public Earth scene = new Earth();

        #region ========== 编辑相关 ==========
        ///<summary>编辑模式面板</summary>
        public Edit.EditPanel editpanel;

        //private Edit.DBDesc _dbdesc;
        /////<summary>存于xml的数据库定义描述</summary>
        //public Edit.DBDesc dbdesc
        //{
        //    get
        //    {
        //        if (_dbdesc == null)
        //        {
        //            _dbdesc = Edit.DBDesc.ReadFromXml();
        //        }
        //        return _dbdesc;
        //    }
        //}
        ///<summary>存于xml的数据库定义描述字典</summary>
        public Dictionary<string, Edit.DBDesc> dbdesc { get; set; }



        private Edit.DNObjectDesc _DNObjectDesces;
        ///<summary>可用的已描述对象集合，可以同对象不同数据表</summary>
        public Edit.DNObjectDesc DNObjectDesces
        {
            get
            {
                if (_DNObjectDesces == null)
                    _DNObjectDesces = new Edit.DNObjectDesc(dbdesc);
                return _DNObjectDesces;
            }
        }



        ///<summary>是否编辑模式</summary>
        public bool isEditMode
        {
            get { return scene.config.isEditMode; }
            set { scene.config.isEditMode = value; showEditPanel(value); }
        }

        void showEditPanel(bool isshow)
        {
            if (isshow)
            {
                if (dbdesc == null)
                {
                    isEditMode = false;
                    //System.Windows.MessageBox.Show("没有数据库操作描述信息，不能进行编辑！");
                }
                else
                {
                    if (editpanel == null)
                        editpanel = new Edit.EditPanel(this);
                    if (!scene.gridAddition.Children.Contains(editpanel))
                        scene.gridAddition.Children.Add(editpanel);
                }
            }
            else
            {
                if (scene.gridAddition.Children.Contains(editpanel))
                    scene.gridAddition.Children.Remove(editpanel);
            }
        }

        ///<summary>场景基础度量单位，建议以最小线宽的值为基础度量单位，所有对象均以基础度量单位倍数来计算, 缺省0.0001f</summary>
        public float UnitMeasure = 0.0001f;





        #endregion

        #region ========== 拓扑相关方法 ==========


        ///<summary>根据填写的拓扑数据建立双向完整关联拓扑关系和从属包容拓扑关系，暂不支持单向关联，一定是双向关联</summary>
        public void buildTopoRelation()
        {
            //注：设备向设施的所属关系，后一步建立，先建立纯设备连接关系



            //单向连接补充为双向连接
            Dictionary<string, PowerBasicObject> all =
                (from e0 in scene.objManager.zLayers.Values
                 from e1 in e0.pModels.Values
                 select e1).ToDictionary(p => p.id);
            TopoData td;
            PowerBasicObject dobj;
            List<KeyValuePair<string, string>> newrelations = new List<KeyValuePair<string, string>>();
            foreach (KeyValuePair<string, PowerBasicObject> obj in all)
            {
                if (obj.Value.busiTopo != null)
                {
                    td = obj.Value.busiTopo as TopoData;
                    foreach (string relationid in td.relationObjs.Select(p => p.id))
                    {
                        if (all.TryGetValue(relationid, out dobj))
                        {
                            if (dobj.busiTopo == null) dobj.busiTopo = new TopoData(dobj);
                            if (!(dobj.busiTopo as TopoData).relationObjs.Select(p => p.id).Contains(relationid))
                                newrelations.Add(new KeyValuePair<string, string>(dobj.id, obj.Key));
                        }


                    }
                }
            }

            foreach (KeyValuePair<string, string> item in newrelations)
            {
                dobj = all[item.Key];
                (dobj.busiTopo as TopoData).relationObjs.Add(new TopoObjDesc() { id = item.Value });
            }





        }

        ///<summary>根据图形对象位置建立拓扑关系，不准确，暂未实现</summary>
        public void buildTopoRelationByLocation()
        {
            //推断线路首末端
            IEnumerable<PowerBasicObject> lines = getAllObjListByObjType(EObjectType.输电线路);

            IEnumerable<PowerBasicObject> dots = getAllObjListByCategory(EObjectCategory.变电设施类);
            dots = dots.Union(getAllObjListByObjType(EObjectType.开关站));
            Point pf, pt, pd;
            double minf1, mint1, minf2, mint2, lenf, lent;
            string minfid1, minfid2, mintid1, mintid2;
            foreach (DNACLineBase lin in lines)
            {
                pf = lin.points.First();
                pt = lin.points.Last();
                minf1 = mint1 = minf2 = mint2 = double.PositiveInfinity;
                minfid1 = mintid1 = mintid2 = minfid2 = null;
                foreach (pSymbolObject dot in dots)
                {
                    pd = Point.Parse(dot.location);
                    lenf = (pd - pf).Length;
                    lent = (pd - pt).Length;
                    if (lenf < minf1)
                    { minf1 = lenf; minfid1 = dot.id; }
                    else
                    { if (lenf < minf2) { minf2 = lenf; minfid2 = dot.id; } }
                    if (lent < mint1)
                    { mint1 = lent; mintid1 = dot.id; }
                    else
                    { if (lent < mint2) { mint2 = lent; mintid2 = dot.id; } }
                }
                if (minfid1 == mintid1)//若from和to相同，分取最近和次近
                {
                    lin.fromID = minf1 < mint1 ? minfid1 : minfid2;
                    lin.toID = minf1 < mint1 ? mintid2 : mintid1;
                }
                else
                {
                    lin.fromID = minfid1;
                    lin.toID = mintid1;
                }
                (lin.busiTopo as TopoData).relationObjs.Add(new TopoObjDesc() { id = lin.fromID });
                (lin.busiTopo as TopoData).relationObjs.Add(new TopoObjDesc() { id = lin.toID });
            }
            //依据线路推断，填入节点关联线路对象ID
            foreach (pSymbolObject dot in dots)
            {
                dot.relationID = lines.Where(p => (p as DNACLineBase).fromID == dot.id || (p as DNACLineBase).toID == dot.id).Select(p => p.id).ToList<string>();
                foreach (string item in dot.relationID)
                    (dot.busiTopo as TopoData).relationObjs.Add(new TopoObjDesc() { id = item });

            }

        }

        ///<summary>根据图形对象位置建立变电站与主变、配电室与配变、开关站与开关的从属关系，当无法直接获得从属关系时可使用此方法, limitXXX为限制有效距离</summary>
        public void buildEquipmentFacilityRelationByLocation(double limitMainDistance, double limitDistDistance, double limitSwichDistance)
        {
            MyBaseControls.Screen.ScreenProgress.info = "正在根据坐标建立设施与设备的从属拓扑关系...";

            double minDistance;
            foreach (DNDistTransformer item in getAllObjListByObjType(EObjectType.配变))
            {


                Point ps = item.center; //Point.Parse(item.location);
                minDistance = double.PositiveInfinity;
                DNSwitchHouse selobj = null;
                foreach (DNSwitchHouse dot in getAllObjListByObjType(EObjectType.配电室))
                {
                    Point pd = dot.center;// Point.Parse(dot.location);
                    double len = (pd - ps).Length;
                    if (len < minDistance)
                    { minDistance = len; selobj = dot; }
                }
                if (selobj != null && minDistance < limitDistDistance)
                {
                    if (item.thisTopoData == null) item.createTopoData();
                    item.thisTopoData.belontToFacilityID = new TopoObjDesc() { id = selobj.id };
                    selobj.thisTopoData.containEquipments.Add(new TopoObjDesc() { id = item.id });
                }
            }

            foreach (DNTransformerBase item in getAllObjListByObjType(EObjectType.两卷主变).Union(getAllObjListByObjType(EObjectType.三卷主变)))
            {
                Point ps = item.center; //Point.Parse(item.location);
                minDistance = double.PositiveInfinity;
                DNSubStation selobj = null;
                foreach (DNSubStation dot in getAllObjListByObjType(EObjectType.变电站))
                {
                    Point pd = dot.center;// Point.Parse(dot.location);
                    double len = (pd - ps).Length;
                    if (len < minDistance)
                    { minDistance = len; selobj = dot; }
                }
                if (selobj != null && minDistance < limitMainDistance)
                {
                    item.thisTopoData.belontToFacilityID = new TopoObjDesc() { id = selobj.id };
                    if (selobj.thisTopoData == null) selobj.createTopoData();
                    selobj.thisTopoData.containEquipments.Add(new TopoObjDesc() { id = item.id });
                }
            }

            foreach (PowerBasicObject item in getAllObjListByCategory(EObjectCategory.开关类))
            {
                //排除母联
                if (item is DNSwitchBase)
                {
                    Point ps = item.center;// Point.Parse(item.location);
                    minDistance = double.PositiveInfinity;
                    DNSwitchStation selobj = null;
                    foreach (DNSwitchStation dot in getAllObjListByObjType(EObjectType.开关站))
                    {
                        Point pd = dot.center;// Point.Parse(dot.location);
                        double len = (pd - ps).Length;
                        if (len < minDistance)
                        { minDistance = len; selobj = dot; }
                    }
                    if (selobj != null && minDistance < limitSwichDistance)
                    {
                        (item as DNSwitchBase).thisTopoData.belontToFacilityID = new TopoObjDesc() { id = selobj.id };
                        selobj.thisTopoData.containEquipments.Add(new TopoObjDesc() { id = item.id });
                    }
                }
            }
        }



        ///<summary>获得电源追踪路径。
        ///startobj：搜索起始对象；
        ///stopType：搜索目标对象类型列表，搜索到后终止并加入到有效路径列表中；
        ///cancelType：放弃搜索对象类型列表，搜索到此对象后取消此条路径的搜索；
        ///isSwitch为真时，考虑开关状态，缺省为真，当为真时，搜索到开关为开状态时，取消此条路径的搜索
        ///</summary>
        public List<Dictionary<string, PowerBasicObject>> getTrace(PowerBasicObject startobj, List<EObjectType> findTypes, List<EObjectType> cancelTypes, bool isSwitch = true)
        {
            if (startobj.busiTopo == null) return null;
            List<Dictionary<string, PowerBasicObject>> result = new List<Dictionary<string, PowerBasicObject>>();
            DescData desc = startobj.busiDesc as DescData;

            foreach (string id in (startobj.busiTopo as TopoData).relationObjs.Select(p => p.id))
            {
                Dictionary<string, PowerBasicObject> lst = new Dictionary<string, PowerBasicObject>();
                lst.Add(startobj.id, startobj);
                findtrace(id, findTypes, cancelTypes, isSwitch, lst, ref result);
            }
            return result;
        }
        ///<summary>跟踪的递归方法</summary>
        void findtrace(string nextid, List<EObjectType> findTypes, List<EObjectType> cancelTypes, bool isSwitch, Dictionary<string, PowerBasicObject> lst, ref List<Dictionary<string, PowerBasicObject>> result)
        {
            if (result.Count > 10)
                return;

            PowerBasicObject dobj = scene.objManager.find(nextid);
            if (dobj != null)
            {
                lst.Add(dobj.id, dobj);

                DescData desc = dobj.busiDesc as DescData;
                if (desc.objCategory == EObjectCategory.开关类 && isSwitch)
                {
                    if (dobj.busiRunData == null) return;
                    if (!(dobj.busiRunData as RunDataSwitchBase).isClose) return; //开关未闭合，放弃
                }
                if (cancelTypes != null && cancelTypes.Contains(desc.objType))  //找到取消对象，终止该条路径搜索
                {
                    return;
                }

                if (findTypes.Contains(desc.objType)) //找到目标对象，终止该条路径搜索并加入有效路径列表
                {
                    result.Add(lst);
                    return;
                }
                else if (!isSwitch && dobj.busiTopo != null && (dobj.busiTopo as TopoData).belongToEquipmentID != null) //静态搜索中，若有所属设备，还需检查所属设备
                {
                    PowerBasicObject tmp = scene.objManager.find((dobj.busiTopo as TopoData).belongToEquipmentID.id);
                    if (tmp != null && (findTypes.Contains((tmp.busiDesc as DescData).objType)))//所属设备满足终止条件
                    {
                        lst.Add(tmp.id, tmp);
                        result.Add(lst);
                        return;
                    }
                }
                else
                {
                    if (dobj.busiTopo == null) return;
                    foreach (string id in (dobj.busiTopo as TopoData).relationObjs.Select(p => p.id)) //下一递归
                    {
                        //排除已走通的对象，注：若排除，将不一定是最短路径，若要查最优路径，需专门写算法，多线程搜索
                        bool isWalk = false;
                        foreach (var item in result)
                        {
                            if (item.ContainsKey(id))
                            {
                                isWalk = true;
                                break;
                            }
                        }

                        if (!isWalk && !lst.ContainsKey(id)) //排除之前走过路径中已有对象
                        {
                            findtrace(id, findTypes, cancelTypes, isSwitch, lst.Values.ToDictionary(p => p.id), ref result);
                        }
                        else
                        {
                            //终止了的序表

                        }
                    }
                }
            }
            else
                return;

        }



        //================================ 新拓扑搜索 =====================================

        /// <summary>
        /// 按路径长度进行拓扑搜索
        /// </summary>
        /// <param name="startID">搜索起始设备ID</param>
        /// <param name="endType">搜索要发现的设备列表</param>
        /// <param name="maxLength">搜索的最大终止长度</param>
        /// <returns>路径列表</returns>
        public List<TopoFindResult> findByMaxLength(string startID, List<EObjectType> findType, double maxLength)
        {
            Dictionary<string, PowerBasicObject> allobj = getAllObjDict();

            List<TopoFindResult> result = new List<TopoFindResult>();
            List<TopoFindResult> result2 = new List<TopoFindResult>();
            Dictionary<string, string> findids = new Dictionary<string, string>();//已发现对象
            PowerBasicObject obj;
            //添加开始节点
            TopoFindResult r, r2, r3;
            r = new TopoFindResult();
            r.path.Add(startID);
            r.surplusLoad = double.PositiveInfinity;
            r.lastobj = allobj[startID];
            findids.Add(startID, null);
            result.Add(r);
            int count = 0;
            while (count < 10000)  //最大循环
            {
                count++;
                //查找当前未完成的最短路径者，同则按最小节点，同则按最大余量
                r = (from e in result where !e.isCompleted orderby e.lineLength, e.nodesCount, e.surplusLoad descending select e).FirstOrDefault();
                if (r == null) break;

                result2.Clear();
                foreach (TopoObjDesc item in (r.lastobj.busiTopo as TopoData).relationObjs)
                {
                    if (findids.ContainsKey(item.id)) continue;  //若已在遍历对象列表中，跳过

                    //遍历记录长度和余量，以便比较
                    obj = allobj[item.id];
                    TopoFindResult tmp = new TopoFindResult();
                    if (obj.busiAccount != null && obj.busiAccount is ILength)
                        tmp.lineLength = (obj.busiAccount as ILength).len;
                    else
                        tmp.lineLength = 0;
                    if (obj.busiAccount != null && obj.busiAccount is ICapcity && obj.busiRunData != null && obj.busiRunData is ILoad)
                    {
                        tmp.surplusLoad = (obj.busiAccount as ICapcity).cap - (obj.busiRunData as ILoad).apparentPower;
                    }
                    else
                        tmp.surplusLoad = double.PositiveInfinity;
                    tmp.lastobj = obj;
                    result2.Add(tmp);
                }

                if (result2.Count > 0) //有可选路径
                {
                    r2 = (from e in result2 orderby e.lineLength, e.surplusLoad descending select e).First(); //采纳最短路径
                    if (r.lineLength + r2.lineLength > maxLength)  //若最短路径大于限制长度，终止该路径的搜索
                    {
                        result.Remove(r);
                    }
                    findids.Add(r2.lastobj.id, null);  //加入途经列表
                    //增加路径
                    r3 = new TopoFindResult();
                    foreach (var item in r.path)
                        r3.path.Add(item);
                    r3.path.Add(r2.lastobj.id);
                    r3.lineLength = r.lineLength + r2.lineLength;
                    r3.surplusLoad = Math.Min(r.surplusLoad, r2.surplusLoad);
                    r3.nodesCount = r.nodesCount + 1;
                    r3.lastobj = r2.lastobj;
                    if (findType.Contains((r3.lastobj.busiDesc as DescData).objType)) //若找到目标对象
                        r3.isCompleted = true;
                    result.Add(r3);
                }
                else //无可选路径，从结果列中这去除
                {
                    result.Remove(r);
                }

            }

            return result;
        }

        /// <summary>
        /// 按负荷余量进行拓扑搜索
        /// </summary>
        /// <param name="startID">搜索起始设备ID</param>
        /// <param name="endType">搜索要发现的设备列表</param>
        /// <param name="minSurplusLoad">搜索的最小终止负荷余量, 当路径中任一设备的负荷余量小于终止负荷余量，该路径中止搜索</param>
        /// <returns>路径列表</returns>
        public List<TopoFindResult> findByMinSurplusLoad(string startID, List<EObjectType> findType, double minSurplusLoad)
        {
            Dictionary<string, PowerBasicObject> allobj = getAllObjDict();

            List<TopoFindResult> result = new List<TopoFindResult>();
            List<TopoFindResult> result2 = new List<TopoFindResult>();
            Dictionary<string, string> findids = new Dictionary<string, string>();//已发现对象
            PowerBasicObject obj;
            //添加开始节点
            TopoFindResult r, r2, r3;
            r = new TopoFindResult();
            r.path.Add(startID);
            r.surplusLoad = double.PositiveInfinity;
            r.lastobj = allobj[startID];
            findids.Add(startID, null);
            result.Add(r);
            int count = 0;
            while (count < 10000)  //最大循环
            {
                count++;
                //查找当前未完成的最大负荷余量者，同则按最短长度，同则按最小节点
                r = (from e in result where !e.isCompleted orderby e.surplusLoad descending, e.lineLength, e.nodesCount select e).FirstOrDefault();
                if (r == null) break;


                result2.Clear();
                foreach (TopoObjDesc item in (r.lastobj.busiTopo as TopoData).relationObjs)
                {
                    if (findids.ContainsKey(item.id)) continue;  //若已在遍历对象列表中，跳过

                    //遍历记录长度和余量，以便比较
                    obj = allobj[item.id];
                    TopoFindResult tmp = new TopoFindResult();
                    if (obj.busiAccount != null && obj.busiAccount is ILength)
                        tmp.lineLength = (obj.busiAccount as ILength).len;
                    else
                        tmp.lineLength = 0;
                    if (obj.busiAccount != null && obj.busiAccount is ICapcity && obj.busiRunData != null && obj.busiRunData is ILoad)
                    {
                        tmp.surplusLoad = (obj.busiAccount as ICapcity).cap - (obj.busiRunData as ILoad).apparentPower;
                    }
                    else
                        tmp.surplusLoad = double.PositiveInfinity;

                    if (tmp.surplusLoad < minSurplusLoad) //若该设备余量小于最小余量，跳过
                        continue;

                    tmp.lastobj = obj;
                    result2.Add(tmp);
                }

                if (result2.Count > 0) //有可选路径
                {
                    r2 = (from e in result2 orderby e.surplusLoad, e.lineLength descending select e).First(); //采纳最大余量

                    findids.Add(r2.lastobj.id, null);  //加入途经列表
                    //增加路径
                    r3 = new TopoFindResult();
                    foreach (var item in r.path)
                        r3.path.Add(item);
                    r3.path.Add(r2.lastobj.id);
                    r3.lineLength = r.lineLength + r2.lineLength;
                    r3.surplusLoad = Math.Min(r.surplusLoad, r2.surplusLoad);
                    r3.nodesCount = r.nodesCount + 1;
                    r3.lastobj = r2.lastobj;
                    if (findType.Contains((r3.lastobj.busiDesc as DescData).objType)) //若找到目标对象
                        r3.isCompleted = true;
                    result.Add(r3);
                }
                else //无可选路径，从结果列中这去除
                {
                    result.Remove(r);
                }

            }

            return result;
        }


        /// <summary>
        /// 按节点数进行拓扑搜索
        /// </summary>
        /// <param name="startID">搜索起始设备ID</param>
        /// <param name="endType">搜索要发现的设备列表</param>
        /// <param name="maxNodesCount">搜索的最大途经节点数, 当路经过节点数大于最大途经节点数，该路径中止搜索</param>
        /// <returns>路径列表</returns>
        public List<TopoFindResult> findByMaxNodesCount(string startID, List<EObjectType> findType, int maxNodesCount)
        {
            Dictionary<string, PowerBasicObject> allobj = getAllObjDict();

            List<TopoFindResult> result = new List<TopoFindResult>();
            List<TopoFindResult> result2 = new List<TopoFindResult>();
            Dictionary<string, string> findids = new Dictionary<string, string>();//已发现对象
            PowerBasicObject obj;
            //添加开始节点
            TopoFindResult r, r2, r3;
            r = new TopoFindResult();
            r.path.Add(startID);
            r.surplusLoad = double.PositiveInfinity;
            r.lastobj = allobj[startID];
            findids.Add(startID, null);
            result.Add(r);
            int count = 0;
            while (count < 10000)  //最大循环
            {
                count++;
                //查找当前未完成的最小节点数者，同则按最大余量，同则按最短长度
                r = (from e in result where !e.isCompleted orderby e.nodesCount, e.surplusLoad descending, e.lineLength select e).FirstOrDefault();
                if (r == null) break;

                result2.Clear();
                foreach (TopoObjDesc item in (r.lastobj.busiTopo as TopoData).relationObjs)
                {
                    if (findids.ContainsKey(item.id)) continue;  //若已在遍历对象列表中，跳过


                    //遍历记录长度和余量，以便比较
                    obj = allobj[item.id];
                    TopoFindResult tmp = new TopoFindResult();
                    if (obj.busiAccount != null && obj.busiAccount is ILength)
                        tmp.lineLength = (obj.busiAccount as ILength).len;
                    else
                        tmp.lineLength = 0;
                    if (obj.busiAccount != null && obj.busiAccount is ICapcity && obj.busiRunData != null && obj.busiRunData is ILoad)
                    {
                        tmp.surplusLoad = (obj.busiAccount as ICapcity).cap - (obj.busiRunData as ILoad).apparentPower;
                    }
                    else
                        tmp.surplusLoad = double.PositiveInfinity;

                    tmp.lastobj = obj;
                    result2.Add(tmp);
                }

                if (result2.Count > 0) //有可选路径
                {
                    r2 = (from e in result2 orderby e.surplusLoad, e.lineLength descending select e).First(); //采纳最大余量

                    findids.Add(r2.lastobj.id, null);  //加入途经列表
                    //增加路径
                    r3 = new TopoFindResult();
                    foreach (var item in r.path)
                        r3.path.Add(item);
                    r3.path.Add(r2.lastobj.id);
                    r3.lineLength = r.lineLength + r2.lineLength;
                    r3.surplusLoad = Math.Min(r.surplusLoad, r2.surplusLoad);
                    r3.nodesCount = r.nodesCount + 1;
                    r3.lastobj = r2.lastobj;
                    result.Add(r3);
                    if (findType.Contains((r3.lastobj.busiDesc as DescData).objType)) //若找到目标对象
                        r3.isCompleted = true;
                    else
                        if (r3.nodesCount + 1 > maxNodesCount)
                            result.Remove(r3);


                }
                else //无可选路径，从结果列中这去除
                {
                    result.Remove(r);
                }

            }

            return result;
        }

        #endregion

        #region 辅助方法

        ///<summary>添加并返回层对象，若同名层对象已存在，则返回已有层对象</summary>
        public pLayer addLayer(string layername)
        {
            return scene.objManager.AddLayer(layername, layername, layername);
        }
        ///<summary>删除图层及图层所含对象</summary>
        public void delLayer(string layername)
        {
            pLayer layer;
            if (scene.objManager.zLayers.TryGetValue("layername", out layer))
            {
                layer.pModels.Clear();
                scene.objManager.zLayers.Remove(layername);
            }

        }

        ///<summary>查找指定ID的对象</summary>
        public PowerBasicObject findObj(string id)
        {
            foreach (var layer in scene.objManager.zLayers.Values)
            {
                if (layer.pModels.ContainsKey(id))
                    return layer.pModels[id];
            }

            return null;
        }

        /////<summary>查找第一个指定类型的对象</summary>
        //public T findFirstObj<T>()
        //{
        //    foreach (var layer in scene.objManager.zLayers.Values)
        //    {
        //        PowerBasicObject result = layer.pModels.Values.FirstOrDefault(p => typeof(T) == p.GetType());
        //        if (result!=null)
        //            return (T)(object)result;
        //    }

        //    return default(T);
        //}
        ///<summary>查找第一个指定类型的对象</summary>
        public PowerBasicObject findFirstObj(Type findtype)
        {
            foreach (var layer in scene.objManager.zLayers.Values)
            {
                PowerBasicObject result = layer.pModels.Values.FirstOrDefault(p => findtype == p.GetType());
                if (result != null)
                    return result;
            }

            return null;
        }


        ///<summary>获取busiDesc业务数据描述中指定【类别】的【可见】对象，返回【枚举】列表</summary>
        public IEnumerable<PowerBasicObject> getObjListByCategory(EObjectCategory category)
        {
            return from e0 in scene.objManager.zLayers.Values
                   from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).objCategory == category && p.isShowObject)
                   select e1;
        }
        ///<summary>获取busiDesc业务数据描述中指定【类别】的【可见】对象，返回【字典】列表</summary>
        public Dictionary<string, PowerBasicObject> getObjDictByCategory(EObjectCategory category)
        {
            return getObjListByCategory(category).ToDictionary(p => p.id);
        }

        ///<summary>获取busiDesc业务数据描述中指定【类别】的【所有】对象，返回【枚举】列表</summary>
        public IEnumerable<PowerBasicObject> getAllObjListByCategory(EObjectCategory category)
        {
            return from e0 in scene.objManager.zLayers.Values
                   from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).objCategory == category)
                   select e1;
        }
        ///<summary>获取busiDesc业务数据描述中指定【类别】的【所有】对象，返回【字典】列表</summary>
        public Dictionary<string, PowerBasicObject> getAllObjDictByCategory(EObjectCategory category)
        {
            return getAllObjListByCategory(category).ToDictionary(p => p.id);
        }



        ///<summary>获取busiDesc业务数据描述中指定【对象类型】的【可见】对象，返回【枚举】列表</summary>
        public IEnumerable<PowerBasicObject> getObjListByObjType(EObjectType objtype)
        {
            return from e0 in scene.objManager.zLayers.Values
                   from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).objType == objtype && p.isShowObject)
                   select e1;
        }
        ///<summary>获取busiDesc业务数据描述中指定【对象类型】的【可见】对象，返回【字典】列表</summary>
        public Dictionary<string, PowerBasicObject> getObjDictByObjType(EObjectType objtype)
        {
            return getObjListByObjType(objtype).ToDictionary(p => p.id);
        }

        ///<summary>获取【可见】对象，返回【枚举】列表</summary>
        public IEnumerable<PowerBasicObject> getObjList()
        {
            return from e0 in scene.objManager.zLayers.Values
                   from e1 in e0.pModels.Values.Where(p => p.isShowObject)
                   select e1;
        }
        ///<summary>获取【可见】对象，返回【字典】列表</summary>
        public Dictionary<string, PowerBasicObject> getObjDict()
        {
            return getObjList().ToDictionary(p => p.id);
        }

        ///<summary>获取busiDesc业务数据描述中指定【对象类型】的【所有】对象，返回【枚举】列表</summary>
        public IEnumerable<PowerBasicObject> getAllObjListByObjType(EObjectType objtype)
        {
            return from e0 in scene.objManager.zLayers.Values
                   from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).objType == objtype)
                   select e1;
        }
        ///<summary>获取busiDesc业务数据描述中指定【对象类型】的【所有】对象，返回【字典】列表</summary>
        public Dictionary<string, PowerBasicObject> getAllObjDictByObjType(EObjectType objtype)
        {
            return getAllObjListByObjType(objtype).ToDictionary(p => p.id);
        }

        ///<summary>获取【所有】对象，返回【枚举】列表</summary>
        public IEnumerable<PowerBasicObject> getAllObjList()
        {
            return from e0 in scene.objManager.zLayers.Values
                   from e1 in e0.pModels.Values
                   select e1;
        }
        ///<summary>获取【所有】对象，返回【字典】列表</summary>
        public Dictionary<string, PowerBasicObject> getAllObjDict()
        {
            return getAllObjList().ToDictionary(p => p.id);
        }
        ///<summary>获取【所有】对象，返回结果为object的【枚举】列表</summary>
        public IEnumerable<object> getAllObjListAsObject()
        {
            return from e0 in scene.objManager.zLayers.Values
                   from e1 in e0.pModels.Values
                   select e1 as object;
        }
        ///<summary>获取【所有】对象，返回结果为object的【字典】列表</summary>
        public Dictionary<string, object> getAllObjDictAsObject()
        {
            return getAllObjListAsObject().ToDictionary(p => (p as PowerBasicObject).id);
        }


        ///<summary>获取busiDesc业务数据描述中指定【对象类型】的【所有】对象，返回【枚举】列表</summary>
        public IEnumerable<PowerBasicObject> getAllObjListByObjFlag(string flag)
        {
            return from e0 in scene.objManager.zLayers.Values
                   from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).Flags.ContainsKey(flag))
                   select e1;
        }

        #endregion


        #region ========== 业务逻辑 ==========
        #region ---------- 潮流 -----------

        Color? saveLineColor = null;
        float? saveArrowSize = null;

        ///<summary>当前是否正在显示潮流</summary>
        bool isShowingFlow { get; set; }
        ///<summary>显示导线类对象潮流，
        ///可指定busiData中busiSort和busiCategory来限定范围, 
        ///以运行数据中的有功activePower判断是否有潮流,
        ///activePower为负则反向
        ///isChangeLineColor是否根据负载改变线路颜色
        ///isChangeArrowSize是否根据负载改变箭头大小
        ///重复调用则依据busiRunData数据刷新显示
        /// </summary>
        public void showFlow(string busiSort = null, string busiCategory = null, bool isChangeLineColor = true, bool isChangeArrowSize = true)
        {
            isShowingFlow = true;
            IEnumerable<pPowerLine> objs = from e0 in scene.objManager.zLayers.Values
                                           from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).objCategory == EObjectCategory.导线类 && p.busiRunData != null)
                                           select e1 as pPowerLine;
            if (busiSort != null)
                objs = objs.Where(p => p.busiData.busiSort == busiSort);
            if (busiCategory != null)
                objs = objs.Where(p => p.busiData.busiCategory == busiCategory);

            if (objs.Count() > 0 && saveLineColor == null)
            {
                saveLineColor = (objs.First() as pPowerLine).color;
                saveArrowSize = (objs.First() as pPowerLine).arrowSize;
            }

            foreach (pPowerLine obj in objs)
            {
                RunDataACLineBase data = obj.busiRunData as RunDataACLineBase;
                if (data.activePower == 0)
                    obj.isFlow = false;
                else
                {
                    if (isChangeArrowSize)
                        obj.arrowSize = (float)(saveArrowSize * ((1 + data.rateOfLoad) > 2 ? 2 : (1 + data.rateOfLoad)));
                    if (isChangeLineColor)
                        obj.color = MyClassLibrary.Share2D.MediaHelper.getColorBetween(data.rateOfLoad, Colors.Blue, Colors.Red);

                    obj.isInverse = data.activePower < 0;
                    obj.isFlow = true;
                }
            }
        }

        ///<summary>清除潮流显示</summary>
        public void clearFlow()
        {
            if (isShowingFlow && saveLineColor!=null)
            {
                IEnumerable<pPowerLine> lines = from e0 in scene.objManager.zLayers.Values
                                                from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && (p.busiDesc as DescData).objCategory == EObjectCategory.导线类 && p.busiRunData != null)
                                                select e1 as pPowerLine;
                foreach (pPowerLine obj in lines)
                {
                    obj.isFlow = false;
                    obj.color = (Color)saveLineColor;
                    obj.arrowSize = (float)saveArrowSize;
                }
                isShowingFlow = false;
            }
        }

        #endregion

        #region ---------- 变电站或变压器负载 -----------

        bool isShowingLoadCol { get; set; }
        ///<summary>显示变电站和变压器负载，
        ///可指定busiData中busiSort和busiCategory来限定范围, 
        ///colRadiusScale指定半径为图元scalex的倍数, 
        ///colHeightScale指定高为半径的倍数 ,
        ///minSpan和maxSpan指定负载分色界限,
        ///重复调用则依据busiRunData数据刷新显示
        ///</summary>
        public void showLoadCol(float colRadiusScale = 0.3f, float colHeightScale = 10f, float minSpan = 0.1f, float maxSpan = 0.9f, string busiSort = null, string busiCategory = null)
        {
            isShowingLoadCol = true;
            IEnumerable<pSymbolObject> objs = from e0 in scene.objManager.zLayers.Values
                                              from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && ((p.busiDesc as DescData).objCategory == EObjectCategory.变电设施类 || (p.busiDesc as DescData).objCategory == EObjectCategory.变压器类) && p.busiRunData != null)
                                              select e1 as pSymbolObject;
            if (busiSort != null)
                objs = objs.Where(p => p.busiData.busiSort == busiSort);
            if (busiCategory != null)
                objs = objs.Where(p => p.busiData.busiCategory == busiCategory);

            foreach (pSymbolObject obj in objs)
            {
                double sv;
                if (obj is DNTransformerBase)
                    sv = (obj.busiRunData as RunDataTransformerBase).rateOfLoad;
                else if (obj is DNTransformFacilityBase)
                    sv = (obj.busiRunData as RunDataTransformFacilityBase).rateOfLoad;
                else
                    continue;

                Color sc;
                if (sv > maxSpan)
                    sc = Colors.Red;
                else if (sv < minSpan)
                    sc = Color.FromRgb(0x00, 0x66, 0x00);
                else
                    sc = Colors.Orange;



                PowerBasicObject tmp;
                pData pd;
                if (!obj.submodels.TryGetValue(obj.id + "负载柱", out tmp))  //若未创建数据对象
                {
                    pd = new pData(obj.parent)
                    {
                        id = obj.id + "负载柱",
                        location = obj.location,
                        radScale = obj.scaleX * colRadiusScale,
                        valueScale = obj.scaleX * colRadiusScale * colHeightScale,
                        labelScaleX = (float)obj.scaleX * colRadiusScale * 50,
                        labelScaleY = (float)obj.scaleX * colRadiusScale * 50,
                    };
                    pd.datas.Add(new Data() { id = obj.id + "负载值", argu = "负载", value = sv, color = sc, geokey = "圆柱体", format = "{2:p0}" });
                    pd.datas.Add(new Data() { id = obj.id + "备用值", argu = "空", value = 1 - sv, color = Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF), geokey = "圆柱体", isShowLable = false });
                    obj.AddSubObject(pd.id, pd);

                }
                else
                {
                    pd = tmp as pData;
                    pd.datas[0].color = sc;
                    pd.datas[0].value = sv;
                    pd.datas[1].value = 1 - sv;
                }

                pd.isShowLabel = true;
                obj.isShowSubObject = true;


            }
            scene.UpdateModel();
        }


        ///<summary>清除变电站变压器负载柱显示</summary>
        public void clearLoadCol()
        {
            if (isShowingLoadCol)
            {
                IEnumerable<pSymbolObject> objs = from e0 in scene.objManager.zLayers.Values
                                                  from e1 in e0.pModels.Values.Where(p => p.busiDesc != null && ((p.busiDesc as DescData).objCategory == EObjectCategory.变电设施类 || (p.busiDesc as DescData).objCategory == EObjectCategory.变压器类) && p.busiRunData != null)
                                                  select e1 as pSymbolObject;

                foreach (PowerBasicObject obj in objs)
                {
                    PowerBasicObject tmp;
                    if (obj.submodels.TryGetValue(obj.id + "负载柱", out tmp))
                        obj.submodels.Remove(obj.id + "负载柱");
                }
                scene.UpdateModel();
                isShowingLoadCol = false;
            }
        }
        #endregion


        #region ---------- 变电站或变压器电压等值图 -----------
        bool isShowingVLContour { get; set; }
        pLayer contourLayer;
        List<ContourGraph.ValueDot> dots;
        ///<summary>电压等值图对象，可直接对其设置参数</summary>
        public ContourGraph.Contour con;
        pContour gcon;
        Dictionary<string, pSymbolObject> objs = new Dictionary<string, pSymbolObject>();
        ///<summary>显示变电设施或变电设备的电压等值图
        ///isFaciliy，以变电设施(false变压器)高压侧电压标幺值构建电压等值图
        ///可使用distnet.con对象，设置生成等值线的参数
        ///</summary>
        public void showVLContour(bool isFacility = true)
        {
            if (!scene.legendManager.legends.ContainsKey("电压等值图例"))
            {
                GradientBrushLegend legend = scene.legendManager.createGradientBrushLegend("电压等值图例");
                legend.header = " 电压标幺值 ";
                legend.isShow = true;
                legend.panel.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                legend.panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                legend.panel.Margin = new Thickness(120, 0, 0, 30);
                legend.panel.Background = new SolidColorBrush(Color.FromArgb(0xCC, 0x00, 0x00, 0x00));
                legend.headerBackground = Brushes.Black;
                legend.headerForeground = Brushes.Aqua;
                legend.headerBorderBrush = new SolidColorBrush(Colors.White);
                legend.setText("0.9", "1", "1.1");

                GradientStopCollection gsc = new GradientStopCollection();
                gsc.Add(new GradientStop(Colors.Blue, 0));
                gsc.Add(new GradientStop(Color.FromArgb(0x34, 0x00, 0xFF, 0xFF), 0.25));
                gsc.Add(new GradientStop(Color.FromArgb(0x00, 0x00, 0xFF, 0x00), 0.5));
                gsc.Add(new GradientStop(Color.FromArgb(0x34, 0xFF, 0xFF, 0x00), 0.75));
                gsc.Add(new GradientStop(Colors.Red, 1));
                legend.setGradientStop(gsc);
            }
            scene.legendManager.legends["电压等值图例"].isShow = true;


            isShowingVLContour = true;
            if (!scene.objManager.zLayers.TryGetValue("等高图层", out contourLayer))
            {
                contourLayer = scene.objManager.AddLayer("等高图层", "等高图层", "等高图层");
                contourLayer.deepOrder = -1;


                //导入和重新计算点位置
                dots = new List<ContourGraph.ValueDot>();
                if (isFacility)
                    foreach (pSymbolObject obj in getAllObjListByCategory(EObjectCategory.变电设施类).Where(p => p.busiRunData != null))
                    {
                        double tmpvalue = (obj.busiRunData as RunDataTransformFacilityBase).HVoltPUV;
                        //dots.Add(new ContourGraph.ValueDot() { id = obj.id, location = Point.Parse(obj.location), value = tmpvalue });
                        dots.Add(new ContourGraph.ValueDot() { id = obj.id, location = obj.center, value = tmpvalue });
                        objs.Add(obj.id, obj);
                    }
                else
                    foreach (pSymbolObject obj in getAllObjListByCategory(EObjectCategory.变压器类).Where(p => p.busiRunData != null))
                    {
                        double tmpvalue = (obj.busiRunData as RunDataTransformFacilityBase).HVoltPUV;
                        //dots.Add(new ContourGraph.ValueDot() { id = obj.id, location = Point.Parse(obj.location), value = tmpvalue });
                        dots.Add(new ContourGraph.ValueDot() { id = obj.id, location = obj.center, value = tmpvalue });
                        objs.Add(obj.id, obj);
                    }
                double minx, miny, maxx, maxy;
                //根据scene.coordinateManager.isXAsLong确定原始坐标的横纵方向
                miny = dots.Min(p => scene.coordinateManager.isXAsLong ? p.location.Y : p.location.X);
                maxy = dots.Max(p => scene.coordinateManager.isXAsLong ? p.location.Y : p.location.X);
                minx = dots.Min(p => scene.coordinateManager.isXAsLong ? p.location.X : p.location.Y);
                maxx = dots.Max(p => scene.coordinateManager.isXAsLong ? p.location.X : p.location.Y);


                double w = maxx - minx; double h = maxy - miny;
                minx = minx - w * 0.2; maxx = maxx + w * 0.2;
                miny = miny - h * 0.2; maxy = maxy + h * 0.2;
                w = maxx - minx; h = maxy - miny;
                //经纬换为屏幕坐标
                int size = 1024;
                foreach (ContourGraph.ValueDot dot in dots)
                {
                    if (scene.coordinateManager.isXAsLong)
                        dot.location = new Point((dot.location.X - minx) / w * size, (maxy - dot.location.Y) / h * size);
                    else
                        dot.location = new Point((dot.location.Y - minx) / w * size, (maxy - dot.location.X) / h * size);  //重新赋与新的平面点位置, 注，纬度取反，仅适用北半球
                }

                //设置计算参数
                if (con == null) con = new ContourGraph.Contour();
                con.dots = dots;
                con.opacityType = ContourGraph.Contour.EOpacityType.倒梯形;
                con.canvSize = new Size(size, size);
                con.gridXCount = 200;
                con.gridYCount = 200;
                con.Span = 30;
                con.maxvalue = 1.1;
                con.minvalue = 0.9;
                con.dataFillValue = 1;
                con.dataFillMode = ContourGraph.Contour.EFillMode.单点包络填充;
                con.isDrawGrid = false;
                con.isDrawLine = false;
                con.isFillLine = true;
                if (gcon == null) gcon = new pContour(contourLayer) { id = "等值图" };// { minJD = minx, maxJD = maxx, minWD = miny, maxWD = maxy };
                gcon.setRange(minx, maxx, miny, maxy);
                gcon.brush = con.ContourBrush;
                contourLayer.AddObject("等值线", gcon);
                con.GenCompleted += new EventHandler(con_GenCompleted);
                con.GenContourAsync(); //异步开始生成
            }
            else  //刷新数据
            {
                foreach (var item in dots)
                {
                    if (item.id != null)
                    {
                        item.value = (objs[item.id].busiRunData as RunDataTransformFacilityBase).HVoltPUV;
                    }

                }
                con.ReGenContourAsync();
            }
            if (scene.objManager.zLayers.TryGetValue("等高图层", out contourLayer))
                contourLayer.logicVisibility = true;


        }
        void con_GenCompleted(object sender, EventArgs e) //异步完成
        {
            gcon.brush = con.ContourBrush;
            scene.UpdateModel();

        }

        ///<summary>隐藏电压等值图</summary>
        public void hideVLContour()
        {
            if (scene.objManager.zLayers.TryGetValue("等高图层", out contourLayer))
                contourLayer.logicVisibility = false;
            if (scene.legendManager.legends.ContainsKey("电压等值图例"))
                scene.legendManager.legends["电压等值图例"].isShow = false;

            scene.UpdateModel();
        }

        ///<summary>清理电压等值图</summary>
        public void clearVLContour()
        {
            if (isShowingVLContour)
            {
                if (scene.objManager.zLayers.Keys.Contains("等高图层"))
                {
                    contourLayer = scene.objManager.zLayers["等高图层"];
                    scene.objManager.zLayers.Remove("等高图层");
                    contourLayer.pModels.Clear();
                }
                isShowingVLContour = false;
                contourLayer = null;
                dots.Clear();
                objs.Clear();
                con = null;
                gcon = null;
                if (scene.legendManager.legends.ContainsKey("电压等值图例"))
                    scene.legendManager.legends["电压等值图例"].isShow = false;
                scene.UpdateModel();

            }
        }





        #endregion

        #region ---------- 节点电压点图 -----------

        #endregion


        #region ---------- 电源追溯与供电范围 -----------

        pLayer supplyrangeLayer;
        RangeGenerator rangegen;
        pContour pRange;


        ///<summary>恢复保存的状态（电源追溯和供电范围），并清除保存列表, 清除范围效果</summary>
        public void clearTraceSupplyRange()
        {
            scene.objManager.restoreVisionProperty(); //恢复之前保存的视觉属性

            if (scene.objManager.zLayers.TryGetValue("供电范围图层", out supplyrangeLayer))
            {
                supplyrangeLayer.logicVisibility = false;
                supplyrangeLayer.pModels.Clear();
                scene.UpdateModel();
            }
        }

        ///<summary>电源追溯，当前配变与配电室有效</summary>
        public void showSourceTrace(PowerBasicObject selectedObj)
        {
            clearTraceSupplyRange();
            if (selectedObj is DNDistTransformer)  //配变追溯
            {
                traceFromDistTransformer(selectedObj);
            }
            else if (selectedObj is DNSwitchHouse)  //配电室追溯
            {
                DNSwitchHouse sh = selectedObj as DNSwitchHouse;
                foreach (string id in sh.thisTopoData.containEquipments.Select(p => p.id))
                {
                    PowerBasicObject tmpobj = scene.objManager.find(id);
                    if (tmpobj is DNDistTransformer)
                    {
                        traceFromDistTransformer(tmpobj);
                    }
                }

            }
        }
        ///<summary>配变追溯</summary>
        void traceFromDistTransformer(PowerBasicObject selectedObj)
        {
            DNDistTransformer obj = selectedObj as DNDistTransformer;
            List<EObjectType> findtypes = new List<EObjectType>() { EObjectType.两卷主变, EObjectType.三卷主变 };
            List<Dictionary<string, PowerBasicObject>> tmp = getTrace(obj, findtypes, null, false);
            foreach (var path in tmp)
            {
                foreach (PowerBasicObject item in path.Values)
                {
                    setTraceEffect(item);
                    //若从属于设施，设施也加效果
                    if ((item.busiDesc as DescData).isEquipment && !(item.busiDesc as DescData).isFacility && (item.busiTopo as TopoData).belontToFacilityID != null) //纯设备，有从属设施
                    {
                        PowerBasicObject tmpobj2 = scene.objManager.find((item.busiTopo as TopoData).belontToFacilityID.id);
                        setTraceEffect(tmpobj2);
                    }
                }


            }
        }
        ///<summary>设置每一对象追踪效果</summary>
        void setTraceEffect(PowerBasicObject item)
        {
            scene.objManager.saveVisionProperty(item);

            if (item is pSymbolObject)
            {
                (item as pSymbolObject).color = Colors.Red;
                (item as pSymbolObject).AnimationBegin(pSymbolObject.EAnimationType.闪烁);
            }
            else if (item is pPowerLine)
            {
                (item as pPowerLine).color = Colors.Red;
                (item as pPowerLine).AnimationBegin(pPowerLine.EAnimationType.闪烁);
            }
        }


        ///<summary>
        ///使用缺省的颜色显示供电范围
        ///isUseGrid，真：使用小区网格表现范围, 假: 使用线路扩散表现范围
        ///expandRad：扩散半径
        ///</summary>
        public void showSupplyRange(PowerBasicObject selectedObj, bool isUseGrid = false, double expandRad = 15)
        {
            showSupplyRange(selectedObj, Color.FromArgb(0x37, 0xff, 0xff, 0xff), isUseGrid);
        }
        ///<summary>
        ///使用指定的颜色显示供电范围
        ///isUseAreaGrid，真：使用小区网格表现范围, 假: 使用线路扩散表现范围
        ///rangeColor: 范围颜色
        ///expandRad：扩散半径
        ///</summary>
        public void showSupplyRange(PowerBasicObject selectedObj, Color rangeColor, bool isUseAreaGrid = false, double expandRad = 15)
        {
            clearTraceSupplyRange();
            if (selectedObj is DNSubStation)  //变电站供电范围
            {
                DNSubStation sh = selectedObj as DNSubStation;
                foreach (string id in sh.thisTopoData.containEquipments.Select(p => p.id))
                {
                    PowerBasicObject tmpobj = scene.objManager.find(id);
                    if (tmpobj is DNMainTransformerBase)
                    {
                        SupplyRangeByMainTransformer(tmpobj, rangeColor, isUseAreaGrid, expandRad);
                    }

                }

            }
            else
                SupplyRangeByMainTransformer(selectedObj, rangeColor, isUseAreaGrid, expandRad);

        }
        ///<summary>主变供电范围</summary>
        void SupplyRangeByMainTransformer(PowerBasicObject selectedObj, Color rangeColor, bool isUseAreaGrid, double expandRad)
        {
            //DNMainTransformer obj = selectedObj as DNMainTransformer;
            List<EObjectType> findtypes = new List<EObjectType>() { EObjectType.配变 };
            List<EObjectType> canceltypes = new List<EObjectType>() { EObjectType.两卷主变, EObjectType.三卷主变 };
            List<Dictionary<string, PowerBasicObject>> tmp = getTrace(selectedObj, findtypes, canceltypes, true);
            if (tmp.Count == 0) return;
            foreach (var path in tmp)
            {
                foreach (PowerBasicObject item in path.Values)
                {
                    setSupplyEffect(item);
                    //若从属于设施，设施也加效果
                    if ((item.busiDesc as DescData).isEquipment && !(item.busiDesc as DescData).isFacility && (item.busiTopo as TopoData).belontToFacilityID != null) //纯设备，有从属设施
                    {
                        PowerBasicObject tmpobj2 = scene.objManager.find((item.busiTopo as TopoData).belontToFacilityID.id);
                        setTraceEffect(tmpobj2);
                    }
                }


            }


            //范围效果
            if (isUseAreaGrid)  //使用小区网格表现
            {

            }
            else  //使用线路扩散表现
            {
                supplyrangeLayer = addLayer("供电范围图层");
                supplyrangeLayer.logicVisibility = true;
                supplyrangeLayer.pModels.Clear();
                if (rangegen == null) rangegen = new RangeGenerator();
                rangegen.drawObjects.Clear();
                rangegen.rad = expandRad;
                rangegen.brush = new SolidColorBrush(rangeColor);
                foreach (var path in tmp)
                    foreach (PowerBasicObject item in path.Values)
                    {
                        if (item is pPowerLine)
                            rangegen.drawObjects.Add(new RangeGenerator.StruDrawObjDesc((item as pPowerLine).strPoints, 0));
                    }
                rangegen.layerCount = 6;
                rangegen.GenRangeBrush();

                //创建图形对象
                pRange = new pContour(supplyrangeLayer) { id = selectedObj.id + "范围图" };
                pRange.setRange(rangegen.minx, rangegen.maxx, rangegen.miny, rangegen.maxy);
                pRange.brush = rangegen.RangeBrush;
                supplyrangeLayer.AddObject(pRange.id, pRange);
                scene.UpdateModel();
            }


        }

        ///<summary>设置供电范围每一对象效果</summary>
        void setSupplyEffect(PowerBasicObject item)
        {

            scene.objManager.saveVisionProperty(item); //保存对象的原有视觉属性

            if (item is pSymbolObject)
            {
                (item as pSymbolObject).color = Colors.Red;
                (item as pSymbolObject).AnimationBegin(pSymbolObject.EAnimationType.闪烁);
            }
            else if (item is pPowerLine)
            {
                (item as pPowerLine).color = Colors.Red;
                (item as pPowerLine).AnimationBegin(pPowerLine.EAnimationType.闪烁);
            }
        }

        #endregion


        #region ---------- 变电站配电室等容器对象处理 ----------
        ///<summary>清除变电站配电室容器对象的运行数据，在读数据之前调用</summary>
        public void clearContainerRundata()
        {
            IEnumerable<PowerBasicObject> objs = getAllObjListByObjType(EObjectType.变电站);
            foreach (var obj in objs)
                obj.busiRunData = null;
            objs = getAllObjListByObjType(EObjectType.配电室);
            foreach (var obj in objs)
                obj.busiRunData = null;


        }
        ///<summary>根据从属拓扑统计变电站配电室容器对象的运行数据，若有该对象运行数据，说明从数据库中读取，则不统计</summary>
        public void statContainerRundata() //zhh注：未完成
        {
            IEnumerable<PowerBasicObject> objs = getAllObjListByObjType(EObjectType.变电站);
            foreach (var obj in objs)
            {
                if (obj.busiRunData == null)
                {
                    DNSubStation dnobj = obj as DNSubStation;
                    dnobj.createRunData();
                    foreach (var item in dnobj.thisTopoData.containEquipments)
                    {
                        DNTransformerBase tf = findObj(item.id) as DNTransformerBase;
                        if (tf.busiRunData != null)
                        {
                            RunDataTransformerBase rd = tf.busiRunData as RunDataTransformerBase;
                            dnobj.thisRunData.activePower += rd.activePower;
                            dnobj.thisRunData.reactivePower += rd.reactivePower;
                        }
                    }
                    

                    
                }
            }
            objs = getAllObjListByObjType(EObjectType.配电室);
            foreach (var obj in objs)
                obj.busiRunData = null;


        }


        #endregion



        #endregion
    }





}
