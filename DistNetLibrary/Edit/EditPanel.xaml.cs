using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfEarthLibrary;

namespace DistNetLibrary.Edit
{
    /// <summary>
    /// EditPanel.xaml 的交互逻辑
    /// 提供编辑面板，编辑内容处于内存中，对象设置修改状态，调用者根据需要自行处理保存操作（如存于数据库或svg文件）
    /// </summary>
    public partial class EditPanel : UserControl
    {
        internal EditPanel(DistNet distNet)
        {
            distnet = distNet;
            dbdesc = distnet.dbdesc;
            dnobjdesc = distnet.DNObjectDesces;
            InitializeComponent();
        }

        DistNet distnet;
        DNObjectDesc dnobjdesc;
        Dictionary<string, DBDesc> dbdesc;  //数据库描述
        PowerBasicObject editobj;  //当前编辑对象
        bool isNewObject; //编辑对象是否是新增对象
        EDDot assobj; //线段编辑辅助对象
        string oldID; //当前编对象的原ID
        bool isModify;  //是否已做修改
        pLayer elayer; //编辑图层

        private void UserControl_Initialized(object sender, EventArgs e)
        {

            tree.ItemsSource = dnobjdesc.dndescs;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            distnet.scene.EditObjectSelected += new EventHandler<Earth.PickEventArgs>(scene_EditObjectSelected);
            distnet.scene.EditObjectMove += new EventHandler<Earth.PickEventArgs>(scene_EditObjectMove);
            elayer = distnet.addLayer("编辑图层");
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            distnet.scene.EditObjectSelected -= new EventHandler<Earth.PickEventArgs>(scene_EditObjectSelected);
            distnet.scene.EditObjectMove -= new EventHandler<Earth.PickEventArgs>(scene_EditObjectMove);
            distnet.delLayer("编辑图层");
            distnet.scene.UpdateModel();
        }

        public delegate void SaveMethodDelegate();
        ///<summary>委托的保存方法</summary>
        public SaveMethodDelegate SaveMethod { get; set; }



        ///<summary>选中对象</summary>
        void scene_EditObjectSelected(object sender, Earth.PickEventArgs e)
        {
            if (isTopoBrowser)  //拓扑浏览模式
            {
                handleTopoBrowsePickedObject(e.pickedObject);
            }
            else  //编辑模式
            {
                #region 编辑模式

                if (e.pickedObject == editobj) return;
                elayer.pModels.Clear();

                if (e.pickedObject is EDDot)  //选中的控制点, 服务于线段和区域
                {
                    assobj = e.pickedObject as EDDot;
                    btnDelDot.Visibility = btnAddDot.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    editobj = e.pickedObject;
                    isNewObject = false;
                    pgAcnt.SelectedObject = editobj.busiAccount;
                    //btnCreate.IsEnabled = false;
                    btnDelete.IsEnabled = true;
                    //btnSave.IsEnabled = isModify = false;
                    //btnExit.IsEnabled = true;

                    if (editobj is pPowerLine) //选中的线段
                    {
                        pPowerLine lin = editobj as pPowerLine;
                        int idx = 0;
                        foreach (Point pnt in lin.points)
                        {
                            EDDot dot = new EDDot(elayer)
                            {
                                id = MyClassLibrary.helper.getGUID(),
                                location = pnt.ToString(),
                            };
                            dot.order = idx;
                            dot.scaleX = dot.scaleY = dot.scaleZ = distnet.UnitMeasure * 5;
                            dot.symbolid = "小圆圈";
                            dot.isUseXModel = false;
                            elayer.AddObject(dot);
                            idx++;
                        }
                    }
                    else if (editobj is pArea)  //选中的区域
                    {
                        pArea lin = editobj as pArea;
                        int idx = 0;
                        foreach (Point pnt in lin.points)  //zh注，应改写为原始坐标点集
                        {
                            EDDot dot = new EDDot(elayer)
                            {
                                id = MyClassLibrary.helper.getGUID(),
                                location = pnt.ToString(),
                            };
                            dot.order = idx;
                            dot.scaleX = dot.scaleY = dot.scaleZ = distnet.UnitMeasure * 5;
                            elayer.AddObject(dot);
                            idx++;
                        }
                    }

                    distnet.scene.UpdateModel();
                    oldID = editobj.id;
                }
                #endregion
            }

            //if (editobj == null)
            //{
            //    editobj = e.pickedObject;
            //    isNewObject = false;
            //    pgAcnt.SelectedObject = editobj.busiAccount;
            //    btnCreate.IsEnabled = false;
            //    btnDelete.IsEnabled = true;
            //    btnSave.IsEnabled = isModify = false;
            //    btnExit.IsEnabled = true;

            //    if (editobj is pPowerLine) //选中的线段
            //    {
            //        pPowerLine lin = editobj as pPowerLine;
            //        int idx = 0;
            //        foreach (Point pnt in lin.points)
            //        {
            //            EDDot dot = new EDDot(elayer) 
            //            {
            //                id=MyClassLibrary.helper.getGUID(),
            //                location=pnt.ToString(),
            //            };
            //            dot.order = idx;
            //            dot.scaleX = dot.scaleY = dot.scaleZ = distnet.UnitMeasure*5;
            //            elayer.AddObject(dot);
            //            idx++;
            //        }
            //        distnet.scene.UpdateModel();
            //    }
            //    else if (editobj is pArea)  //选中的区域
            //    {
            //        pArea lin = editobj as pArea;
            //        int idx = 0;
            //        foreach (Point pnt in lin.points)  //zh注，应改写为原始坐标点集
            //        {
            //            EDDot dot = new EDDot(elayer)
            //            {
            //                id = MyClassLibrary.helper.getGUID(),
            //                location = pnt.ToString(),
            //            };
            //            dot.order = idx;
            //            dot.scaleX = dot.scaleY = dot.scaleZ = distnet.UnitMeasure * 5;
            //            elayer.AddObject(dot);
            //            idx++;
            //        }
            //        distnet.scene.UpdateModel();
            //    }

            //    oldID = editobj.id;
            //}
            //else if (e.pickedObject is EDDot)  //选中的控制点, 服务于线段和区域
            //{
            //    assobj = e.pickedObject as EDDot;
            //    btnDelDot.Visibility = btnAddDot.Visibility = System.Windows.Visibility.Visible;
            //}
        }
        ///<summary>移动</summary>
        void scene_EditObjectMove(object sender, Earth.PickEventArgs e)
        {
            btnSave.IsEnabled = isModify = true;

            if (editobj.modifyStatus == EModifyStatus.未修改)
            {
                editobj.modifyStatus = EModifyStatus.修改;
                editobj.color = Colors.Red;
            }

            if (editobj is pPowerLine && e.pickedObject is EDDot)  //线路修改，从eddot传递到线路
            {
                assobj = e.pickedObject as EDDot;
                pPowerLine lin = editobj as pPowerLine;
                lin.VecPoints[assobj.order] = assobj.VecLocation;
                lin.sendChangedLocation();
            }
            else if (editobj is pArea && e.pickedObject is EDDot)  //区域修改, 从eddot传递到区域
            {
                assobj = e.pickedObject as EDDot;
                pArea lin = editobj as pArea;
                lin.VecPoints[assobj.order] = assobj.VecLocation;
                lin.sendChangedLocation();

            }

        }
        ///<summary>增加线段或多边形控制点</summary>
        private void btnAddDot_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EDDot dot;

            if (editobj is pPowerLine)
            {
                pPowerLine lin = editobj as pPowerLine;
                int idx = assobj.order;
                int idx2 = lin.VecPoints.Count() - 1 == idx ? idx - 1 : idx + 1;
                int idxmin = Math.Min(idx, idx2); //这一点及以前的序号不变
                foreach (PowerBasicObject item in elayer.pModels.Values)  //所有后点序号+1
                {
                    if (item is EDDot)
                    {
                        dot = item as EDDot;
                        if (dot.order > idxmin)
                            dot.order = dot.order + 1;
                    }
                }

                VECTOR3D np = new VECTOR3D((lin.VecPoints[idx].x + lin.VecPoints[idx2].x) / 2, (lin.VecPoints[idx].y + lin.VecPoints[idx2].y) / 2, (lin.VecPoints[idx].z + lin.VecPoints[idx2].z) / 2);

                lin.VecPoints.Insert(idxmin + 1, np);
                lin.sendChangedLocation();

                System.Windows.Media.Media3D.Point3D p3d = new System.Windows.Media.Media3D.Point3D(np.x, np.y, np.z);
                System.Windows.Media.Media3D.Point3D jwh = Helpler.PointToJWH(p3d, distnet.scene.earthManager.earthpara);
                System.Windows.Point pnt = new System.Windows.Point(jwh.Y, jwh.X);
                if (distnet.scene.coordinateManager.Enable) pnt = distnet.scene.coordinateManager.transToOuter(pnt);  //若启用了坐标转换，转换为外部坐标

                dot = new EDDot(elayer)
                {
                    id = MyClassLibrary.helper.getGUID(),
                    location = pnt.ToString(),
                };
                dot.order = idxmin + 1;
                dot.scaleX = dot.scaleY = dot.scaleZ = distnet.UnitMeasure * 5;
                elayer.AddObject(dot);
            }
            else if (editobj is pArea)
            {
                pArea lin = editobj as pArea;
                int idx = assobj.order;
                int idx2 = lin.VecPoints.Count() - 1 == idx ? idx - 1 : idx + 1;
                int idxmin = Math.Min(idx, idx2); //这一点及以前的序号不变
                foreach (PowerBasicObject item in elayer.pModels.Values)  //所有后点序号+1
                {
                    if (item is EDDot)
                    {
                        dot = item as EDDot;
                        if (dot.order > idxmin)
                            dot.order = dot.order + 1;
                    }
                }

                VECTOR3D np = new VECTOR3D((lin.VecPoints[idx].x + lin.VecPoints[idx2].x) / 2, (lin.VecPoints[idx].y + lin.VecPoints[idx2].y) / 2, (lin.VecPoints[idx].z + lin.VecPoints[idx2].z) / 2);

                lin.VecPoints.Insert(idxmin + 1, np);
                lin.sendChangedLocation();

                System.Windows.Media.Media3D.Point3D p3d = new System.Windows.Media.Media3D.Point3D(np.x, np.y, np.z);
                System.Windows.Media.Media3D.Point3D jwh = Helpler.PointToJWH(p3d, distnet.scene.earthManager.earthpara);
                System.Windows.Point pnt = new System.Windows.Point(jwh.Y, jwh.X);
                if (distnet.scene.coordinateManager.Enable) pnt = distnet.scene.coordinateManager.transToOuter(pnt);  //若启用了坐标转换，转换为外部坐标

                dot = new EDDot(elayer)
                {
                    id = MyClassLibrary.helper.getGUID(),
                    location = pnt.ToString(),
                };
                dot.order = idxmin + 1;
                dot.scaleX = dot.scaleY = dot.scaleZ = distnet.UnitMeasure * 5;
                elayer.AddObject(dot);
            }

            distnet.scene.UpdateModel();
        }

        ///<summary>删除线段或多边形控制点</summary>
        private void btnDelDot_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EDDot dot;
            if (editobj is pPowerLine)
            {
                pPowerLine lin = editobj as pPowerLine;
                int idx = assobj.order;

                lin.VecPoints.RemoveAt(idx);
                lin.sendChangedLocation();
                elayer.pModels.Remove(assobj.id);

                foreach (PowerBasicObject item in elayer.pModels.Values)  //所有后点序号-1
                {
                    if (item is EDDot)
                    {
                        dot = item as EDDot;
                        if (dot.order > idx)
                            dot.order = dot.order - 1;
                    }
                }
            }
            else if (editobj is pArea)
            {
                pArea lin = editobj as pArea;
                int idx = assobj.order;

                lin.VecPoints.RemoveAt(idx);
                lin.sendChangedLocation();
                elayer.pModels.Remove(assobj.id);

                foreach (PowerBasicObject item in elayer.pModels.Values)  //所有后点序号-1
                {
                    if (item is EDDot)
                    {
                        dot = item as EDDot;
                        if (dot.order > idx)
                            dot.order = dot.order - 1;
                    }
                }
            }

            distnet.scene.UpdateModel();



        }

        ///<summary>创建新对象</summary>
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (tree.SelectedItem != null)
            {
                DNDesc dnd = tree.SelectedItem as DNDesc;

                //处理对象的层
                pLayer nlayer = distnet.addLayer(dnd.sort);
                PowerBasicObject obj = dnd.CreateByType(nlayer);
                obj.id = MyClassLibrary.helper.getGUID();
                obj.name = "新对象";

                //Point? pnttmp = distnet.scene.camera.getScreenCenterGeo();
                Point? pnttmp = distnet.scene.camera.getScreenCenter();

                if (pnttmp != null)
                {
                    Point pnt = (Point)pnttmp;
                    if (obj is pSymbolObject) //新增的图元
                    {
                        (obj as pSymbolObject).location = pnt.ToString();
                        PowerBasicObject refobj = distnet.findFirstObj(obj.GetType());
                        if (refobj != null)
                        {
                            (obj as pSymbolObject).scaleX = (refobj as pSymbolObject).scaleX;
                            (obj as pSymbolObject).scaleY = (refobj as pSymbolObject).scaleY;
                            (obj as pSymbolObject).scaleZ = (refobj as pSymbolObject).scaleZ;
                            (obj as pSymbolObject).color = (refobj as pSymbolObject).color;
                            (obj as pSymbolObject).isH = true;
                        }
                        obj.DBOPKey = dnd.dbopkey;
                        obj.createAcntData();
                        (obj.busiAccount as AcntDataBase).id = obj.id;
                        (obj.busiAccount as AcntDataBase).name = obj.name;
                        nlayer.AddObject(obj);
                        //(obj as pSymbolObject).aniTwinkle.doCount = 10;
                        //(obj as pSymbolObject).aniTwinkle.isDoAni = true;
                        //(obj as pSymbolObject).AnimationBegin(pSymbolObject.EAnimationType.闪烁);
                        distnet.scene.UpdateModel();
                    }
                    else if (obj is pPowerLine) //新增的线路
                    {
                        if (distnet.scene.coordinateManager.Enable) //启用了坐标转换
                            (obj as pPowerLine).strPoints = String.Format("{0} {1}", pnt, new Point(pnt.X - 50, pnt.Y - 50));
                        else  //缺省的经纬坐标
                            (obj as pPowerLine).strPoints = String.Format("{0} {1}", pnt, new Point(pnt.X - 0.005, pnt.Y - 0.005));

                        PowerBasicObject refobj = distnet.findFirstObj(obj.GetType());

                        if (refobj != null)
                        {
                            (obj as pPowerLine).thickness = (refobj as pPowerLine).thickness;
                            (obj as pPowerLine).color = (refobj as pPowerLine).color;
                            (obj as pPowerLine).arrowColor = (refobj as pPowerLine).arrowColor;
                            (obj as pPowerLine).arrowSize = (refobj as pPowerLine).arrowSize;
                        }
                        else
                        {
                            (obj as pPowerLine).thickness = 0.1f;
                            (obj as pPowerLine).color = Colors.Red; ;
                            (obj as pPowerLine).arrowColor = Colors.Blue;
                            (obj as pPowerLine).arrowSize = 0.2f;
                        }

                        obj.DBOPKey = dnd.dbopkey;
                        obj.createAcntData();
                        (obj.busiAccount as AcntDataBase).id = obj.id;
                        (obj.busiAccount as AcntDataBase).name = obj.name;
                        nlayer.AddObject(obj);
                        //(obj as pSymbolObject).aniTwinkle.doCount = 10;
                        //(obj as pSymbolObject).aniTwinkle.isDoAni = true;
                        //(obj as pSymbolObject).AnimationBegin(pSymbolObject.EAnimationType.闪烁);
                        distnet.scene.UpdateModel();
                    }
                    else if (obj is pArea) //新增的区域
                    {
                        (obj as pArea).strPoints = String.Format("{0} {1} {2}", pnt, new Point(pnt.X - 0.005, pnt.Y - 0.005), new Point(pnt.X, pnt.Y - 0.005));
                        PowerBasicObject refobj = distnet.findFirstObj(obj.GetType());
                        if (refobj != null)
                        {
                            (obj as pArea).color = (refobj as pArea).color;
                        }
                        obj.DBOPKey = dnd.dbopkey;
                        obj.createAcntData();
                        (obj.busiAccount as AcntDataBase).id = obj.id;
                        (obj.busiAccount as AcntDataBase).name = obj.name;
                        nlayer.AddObject(obj);
                        //(obj as pSymbolObject).aniTwinkle.doCount = 10;
                        //(obj as pSymbolObject).aniTwinkle.isDoAni = true;
                        //(obj as pSymbolObject).AnimationBegin(pSymbolObject.EAnimationType.闪烁);
                        distnet.scene.UpdateModel();

                    }
                    else
                        return; //提前返回

                    // 置界面
                    editobj = obj;
                    obj.modifyStatus = EModifyStatus.新增;
                    obj.color = Colors.White;
                    isNewObject = true;
                    pgAcnt.SelectedObject = editobj.busiAccount;

                    //btnCreate.IsEnabled = false;
                    btnDelete.IsEnabled = true;
                    btnSave.IsEnabled = isModify = true;
                    //btnExit.IsEnabled = true;
                }
            }
        }

        ///<summary>删除对象</summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            editobj.modifyStatus = EModifyStatus.删除;
            editobj.color = Colors.Black;


            //MessageBoxResult mbr = MessageBox.Show("操作将从数据库中真正删除该对象且不可恢复，\r\n请选择【确定】（删除）或【取消】（取消删除）？", "删除确认！", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Warning);
            //if (mbr == MessageBoxResult.OK)
            //{
            //    btnCreate.IsEnabled = true;
            //    btnDelete.IsEnabled = false;
            //    btnSave.IsEnabled = isModify = false;
            //    //btnExit.IsEnabled = false;
            //    pgAcnt.SelectedObject = null;
            //    //数据库操作
            //    if (editobj != null)
            //    {
            //        string dbopkey = editobj.DBOPKey;
            //        //SQL sql = dbdesc.SQLS.FirstOrDefault(p => p.key == dbopkey);
            //        SQL sql=dbdesc.Values.SelectMany(p => p.SQLS.Where(pp => pp.key == dbopkey)).FirstOrDefault();
            //        if (sql != null)
            //        {
            //            editobj.id = (editobj.busiAccount as AcntDataBase).id;
            //            editobj.name = (editobj.busiAccount as AcntDataBase).name;
            //            sql.delKeyAcnt(oldID);
            //        }
            //    }
            //    //对象操作
            //    pLayer pl = editobj.parent;
            //    pl.pModels.Remove(editobj.id);

            //    btnAddDot.Visibility = btnDelDot.Visibility = System.Windows.Visibility.Collapsed;
            //    btnCreate.IsEnabled = true;
            //    btnDelete.IsEnabled = false;
            //    btnSave.IsEnabled = isModify = false;
            //    //btnExit.IsEnabled = false;
            //    pgAcnt.SelectedObject = null;
            //    editobj = assobj = null;

            //    elayer.pModels.Clear();
            //    distnet.scene.UpdateModel();
            //    isModify = false;
            //}

        }

        ///<summary>保存对象</summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            btnSave.IsEnabled = isModify = false;
            //save();

            if (SaveMethod != null)
                SaveMethod();
        }
        void save() //本地保存，暂保留参考，以后删除，改由委托执行保存
        {

            //if (editobj!=null)
            //{
            //    string dbopkey = editobj.DBOPKey;
            //    //SQL sql= dbdesc.SQLS.FirstOrDefault(p => p.key == dbopkey);
            //    SQL sql = dbdesc.Values.SelectMany(p => p.SQLS.Where(pp => pp.key == dbopkey)).FirstOrDefault();
            //    if (sql!=null)
            //    {
            //        editobj.id = (editobj.busiAccount as AcntDataBase).id;
            //        editobj.name = (editobj.busiAccount as AcntDataBase).name;
            //        sql.saveKeyAcnt(oldID, editobj);
            //        isNewObject = false;
            //    }
            //}
        }

        ///<summary>退出编辑</summary>
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (isModify)
            {
                MessageBoxResult mbr = MessageBox.Show("选中对象曾进行过修改，\r\n请确认是否不保存退出？", "是否退出编辑！", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Information);
                if (mbr == MessageBoxResult.OK)
                {
                    if (isNewObject)
                    {
                        editobj.parent.pModels.Remove(editobj.id);
                    }
                    else
                    {
                        //恢复
                        //dbdesc.DictSQLS[editobj.DBOPKey].loadKeyAcnt(oldID, editobj);
                        SQL sql = dbdesc.Values.SelectMany(p => p.SQLS.Where(pp => pp.key == editobj.DBOPKey)).FirstOrDefault();
                        if (sql != null)
                            sql.loadKeyAcnt(oldID, editobj);

                        editobj.refreshLocation();
                    }
                }
                else
                    return;
            }


            //==========
            distnet.scene.clearEditObject();
            btnAddDot.Visibility = btnDelDot.Visibility = System.Windows.Visibility.Collapsed;

            btnCreate.IsEnabled = true;
            btnDelete.IsEnabled = false;
            btnSave.IsEnabled = isModify = false;
            //btnExit.IsEnabled = false;
            pgAcnt.SelectedObject = null;
            editobj = assobj = null;

            elayer.pModels.Clear();
            distnet.scene.UpdateModel();
        }

        private void pgAcnt_CellValueChanged(object sender, DevExpress.Xpf.PropertyGrid.CellValueChangedEventArgs args)
        {
            btnSave.IsEnabled = true;
        }

        private void btnTopoGetRelationID_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void btnTopoClearRelationID_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void btnTopoGetSuntainID_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void btnTopoClearSuntainID_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void btnTopoAddExRelationID_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void btnTopoDelExRelationID_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }



        #region ===== 拓扑查看模式 =====

        bool isTopoBrowser = false;  //是否拓扑查看模式
        System.Collections.ObjectModel.ObservableCollection<PowerBasicObject> topoTranceList = new System.Collections.ObjectModel.ObservableCollection<PowerBasicObject>();
        PowerBasicObject topoBrowseSelectedObject;
        Color traceColor = Colors.Blue;
        Color curColor = Colors.Red;
        Color maybeColor = Colors.LightBlue;
        Color normalColor = Colors.White;

        private void chkTopoBrowseMode_Checked(object sender, RoutedEventArgs e)
        {
            isTopoBrowser = true;
            btnClearTopoBrowseTrace.IsEnabled = isTopoBrowser;
            lstTopoBrowseList.ItemsSource = topoTranceList;

            distnet.scene.objManager.saveVisionProperty();
            foreach (var item in distnet.getAllObjList())
            {
                item.color = normalColor;
            }

        }

        private void chkTopoBrowseMode_Unchecked(object sender, RoutedEventArgs e)
        {
            isTopoBrowser = false;
            btnClearTopoBrowseTrace.IsEnabled = isTopoBrowser;
            clearTopoBrowseView();
            lstTopoBrowseList.ItemsSource = null;
            topoBrowseSelectedObject = null;
            topoTranceList.Clear();

            distnet.scene.objManager.restoreVisionProperty();
        }
        private void btnClearTopoBrowseTrace_Click(object sender, RoutedEventArgs e)
        {
            clearTopoBrowseView();
            topoTranceList.Clear();
        }
        ///<summary>处理在拓扑浏览模式下，拾取了对象</summary>
        void handleTopoBrowsePickedObject(PowerBasicObject pickedObject)
        {
            if (pickedObject == null || pickedObject is EDDot) return;
            if (topoBrowseSelectedObject == null)
            {
                topoTranceList.Add(pickedObject);
                topoBrowseSelectedObject = pickedObject;
                setTopoBrowseSelecedView();
            }
            else
            {
                if (topoTranceList.Contains(pickedObject)) //若为路径中对象，重构路径
                {
                    resetTopoBrowseSelecedView();
                    //清除选中对象之后的对象
                    int idx = 0;
                    bool isFind=false;
                    while (idx<topoTranceList.Count)
                    {
                        if (!isFind)
                        {
                            isFind = (topoTranceList[idx] == pickedObject);
                            idx++;
                        }
                        else
                        {
                            topoTranceList.RemoveAt(idx);
                        }
                    }
                    topoBrowseSelectedObject = pickedObject;
                    setTopoBrowseSelecedView(); //设置
                }
                else
                {
                    //比较拾取对象是否为可拾取对象
                    bool isAllowPick = false;
                    foreach (var item in (topoBrowseSelectedObject.busiTopo as DistNetLibrary.TopoData).relationObjs)
                    {
                        if (item.id == pickedObject.id)
                        {
                            isAllowPick = true;
                            break;
                        }
                    }
                    if (isAllowPick)
                    {
                        if (!topoTranceList.Contains(pickedObject))
                            topoTranceList.Add(pickedObject);
                        resetTopoBrowseSelecedView();  //恢复
                        topoBrowseSelectedObject = pickedObject;
                        setTopoBrowseSelecedView(); //设置

                    }
                }
            }


        }

        ///<summary>恢复之前的当前对象相关显示</summary>
        void resetTopoBrowseSelecedView()
        {
            if (topoBrowseSelectedObject == null) return;
            topoBrowseSelectedObject.color = traceColor;//设置前一个当前对象色为跟踪色
            foreach (var item in (topoBrowseSelectedObject.busiTopo as TopoData).relationObjs) //恢复可选对象为普通色
            {
                PowerBasicObject obj = distnet.findObj(item.id);
                if (!topoTranceList.Contains(obj))
                    obj.color = normalColor;
            }

        }
        ///<summary>设置当前对象相关显示</summary>
        void setTopoBrowseSelecedView()
        {
            if (topoBrowseSelectedObject == null) return;
            topoBrowseSelectedObject.color = curColor; //设置当前对象色
            foreach (var item in (topoBrowseSelectedObject.busiTopo as TopoData).relationObjs) //设置可选对象为可选色
            {
                PowerBasicObject obj = distnet.findObj(item.id);
                if (!topoTranceList.Contains(obj))
                    obj.color = maybeColor;
            }
        }

        ///<summary>清理所有相关显示</summary>
        void clearTopoBrowseView()
        {
            resetTopoBrowseSelecedView();
            foreach (var item in topoTranceList)
            {
                item.color = normalColor;
            }
        }

        #endregion





    }
}
