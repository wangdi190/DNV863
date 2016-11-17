using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Interact
{
    class IEdit : BaseMain
    {
        public IEdit(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {

        }


        protected override void load()  //进入时装载数据
        {
            if (IEditController.uctop == null) { IEditController.uctop = new Top(); IEditController.uctop.btnPrjManage.Visibility = System.Windows.Visibility.Visible; }
            toolboxSub.Children.Add(IEditController.uctop);

            root.distnet.scene.camera.adjustCameraAngle(0);
            root.distnet.isEditMode = true;
            root.distnet.editpanel.Margin = new System.Windows.Thickness(0, 80, 0, 0);

            if (IEditController.ucpanel == null)
                IEditController.ucpanel = new IEditPanel(root);
            root.grdContent.Children.Add(IEditController.ucpanel);

            //DNVLibrary.Run.DataGenerator.initRunData(root.distnet);
            //DNVLibrary.Run.DataGenerator.StartGenData(root.distnet);


            root.distnet.editpanel.SaveMethod = save;
        }
        protected override void unload()  //退出时卸载数据
        {
            root.distnet.isEditMode = false;

            if (root.grdContent.Children.Contains(IEditController.ucpanel))
                root.grdContent.Children.Remove(IEditController.ucpanel);

            //DNVLibrary.Run.DataGenerator.StopGenData();

            root.distnet.editpanel.SaveMethod = null;
        }

        ///<summary>编辑结果保存</summary>
        void save()
        {
            //所有被修改对象
            var allobjs = from e0 in root.distnet.scene.objManager.zLayers.Values
                          from e1 in e0.pModels.Values
                          where e1.modifyStatus != WpfEarthLibrary.EModifyStatus.未修改
                          select e1;

            int curinstanceid=IEditController.uctop.instanceviewmodel.curItem.id;

            foreach (var obj in allobjs)
            {

                if (obj.modifyStatus == WpfEarthLibrary.EModifyStatus.删除)
                {
                    #region ---删除操作---
                    //逻辑: 1.对象在当前实例中，直接删除对象
                    //      2.对象为实例继承，增加删除记录
                    //      3.同时应删除相关计算分析数据
                    int objinstanceid;
                    ObjectData objdata;
                    if (global.objects.TryGetValue(obj.id, out objdata))
                    {
                        objinstanceid = objdata.instanceID;
                        if (curinstanceid == objinstanceid)  //当前实例对象：直接删除
                        {
                            string dbopkey = obj.DBOPKey;
                            DistNetLibrary.Edit.SQL sqldesc = root.distnet.dbdesc.Values.SelectMany(p => p.SQLS.Where(pp => pp.key == dbopkey)).FirstOrDefault();
                            if (sqldesc != null)
                            {
                                sqldesc.delKeyAcnt(obj.id);
                                //处理global

                            }
                        }
                        else   //父类实例对象：增加为删除记录
                        {

                            string sql = @"insert all_object (instanceid, id, objtypeid, objcreatedate,objoperatemode,objoperateid) values ({0},'{1}',{2},'{3}',2,'{4}')";
                            sql = string.Format(sql, curinstanceid, MyClassLibrary.helper.getGUID(), objdata.objtypeid, DateTime.Now, obj.id);
                            DataLayer.DataProvider.ExecuteSQL(sql);
                            //处理global


                        }
                    }
                    else  //不存在else这种情况
                    { }

                    #endregion
                }
                else if (obj.modifyStatus == WpfEarthLibrary.EModifyStatus.新增)
                {
                    #region ---新增操作---
                    string dbopkey = obj.DBOPKey;

                    DistNetLibrary.Edit.SQL sqldesc = root.distnet.dbdesc.Values.SelectMany(p => p.SQLS.Where(pp => pp.key == dbopkey)).FirstOrDefault();
                    if (sqldesc != null)
                    {
                        obj.id = (obj.busiAccount as DistNetLibrary.AcntDataBase).id;
                        obj.name = (obj.busiAccount as DistNetLibrary.AcntDataBase).name;
                        sqldesc.saveKeyAcnt(curinstanceid, null, obj);
                        //处理global
                    }


                    #endregion

                }
                else if (obj.modifyStatus == WpfEarthLibrary.EModifyStatus.修改)
                {
                    #region ---修改操作---
                    //逻辑: 1.对象在当前实例中，直接更新对象
                    //      2.对象为实例继承，增加为改造操作记录
                    //      3.同时应删除相关计算分析数据

                    int objinstanceid;
                    ObjectData objdata;
                    if (global.objects.TryGetValue(obj.id, out objdata))
                    {
                        objinstanceid = objdata.instanceID;
                        if (curinstanceid == objinstanceid)  //当前实例对象：直接更新
                        {
                            string dbopkey = obj.DBOPKey;
                            DistNetLibrary.Edit.SQL sqldesc = root.distnet.dbdesc.Values.SelectMany(p => p.SQLS.Where(pp => pp.key == dbopkey)).FirstOrDefault();
                            if (sqldesc != null)
                            {
                                sqldesc.saveKeyAcnt(curinstanceid,obj.id, obj);
                                //处理global

                            }
                        }
                        else   //父类实例对象：增加改造记录
                        {
                            string oldid = obj.id; //旧的ID
                            obj.id = MyClassLibrary.helper.getGUID(); //新的改造后ID
                            string dbopkey = obj.DBOPKey;
                            DistNetLibrary.Edit.SQL sqldesc = root.distnet.dbdesc.Values.SelectMany(p => p.SQLS.Where(pp => pp.key == dbopkey)).FirstOrDefault();
                            if (sqldesc != null)
                            {
                                sqldesc.saveKeyAcnt(curinstanceid, null, obj); //新建改造记录
                                //更新为改造


                            }


                            //处理global


                        }
                    }
                    else  //不存在else这种情况
                    { }
                    #endregion

                }
            }



        }
    }

    //class IEdit : AppBase
    //{
    //    public IEdit(UCDNV863 Root)
    //        : base(Root)
    //    {
    //    }

    //    internal override void load()
    //    {
    //        root.distnet.scene.camera.adjustCameraAngle(0);
    //        root.distnet.isEditMode = true;
    //        root.distnet.editpanel.Margin = new System.Windows.Thickness(0, 80, 0, 0);
    //    }

    //    internal override void unload()
    //    {
    //        root.distnet.isEditMode = false;
    //    }

    //    internal void isshowedit(bool isedit)
    //    {
    //        root.distnet.isEditMode = isedit;
    //    }
    //}

}
