using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace WpfEarthLibrary
{
    public class pObjectManager
    {
        public pObjectManager(Earth Parent)
        {
            earth = Parent;
            bworker.DoWork += new DoWorkEventHandler(bworker_DoWork);
            bworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bworker_RunWorkerCompleted);
        }

        void bworker_DoWork(object sender, DoWorkEventArgs e)
        {
            updateModel();
        }
        void bworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }


        private ECheckMode _ObjectCheckMode = ECheckMode.视锥体检查;
        ///<summary>对象可视性检查模式,缺省为按经纬检查</summary>
        public ECheckMode ObjectCheckMode
        {
            get { return _ObjectCheckMode; }
            set { _ObjectCheckMode = value; }
        }



        #region ----- 动态线宽参数 -----
        ///<summary>是否使用动态线宽，若为True则必须同时设置dynLineWidthDefaultDistance,dynLineWidthMin,dynLineWidthMax三个参数</summary>
        public bool dynLineWidthEnable { get; set; }

        ///<summary>动态线宽计算用，显示缺省线宽时的相机屏幕中心距离，可设置为初始化时的距离</summary>
        public float dynLineWidthDefaultDistance { get; set; }
        ///<summary>使用动态线宽时，最小线宽</summary>
        public float dynLineWidthMin { get; set; }
        ///<summary>使用动态线宽时的最大线宽</summary>
        public float dynLineWidthMax { get; set; }

        #endregion

        private bool _isUseXModel;
        ///<summary>可使用XModel的对象是否使用XModel</summary>
        public bool isUseXModel
        {
            get { return _isUseXModel; }
            set
            {
                _isUseXModel = value;
                foreach (pLayer layer in earth.objManager.zLayers.Values)
                {
                    foreach (var obj in layer.pModels.Values)
                    {
                        if (obj is pSymbolObject)
                        {
                            (obj as pSymbolObject).isUseXModel = value && !string.IsNullOrWhiteSpace((obj as pSymbolObject).XModelKey);
                        }
                    }
                }
            }
        }


        private bool _isCheckPoints;
        ///<summary>是否启用内置的折线区域点集检查，剔除不合格的点（重复点或过近的点）和多余的点（直线点), 请先设置checkPointsMinLength属性</summary>
        public bool isCheckPoints
        {
            get { return _isCheckPoints; }
            set { _isCheckPoints = value; }
        }

        private double _checkPointsMinLength = 0.1;
        ///<summary>检查点集时，被剔除的过近点最小点距，（原始录入坐标系）,缺省0.1</summary>
        public double checkPointsMinLength
        {
            get { return _checkPointsMinLength; }
            set { _checkPointsMinLength = value; }
        }




        public Earth earth;
        BackgroundWorker bworker = new BackgroundWorker();
        internal bool isUpdating;

        #region 公开对象
        public Dictionary<string, pLayer> zLayers = new Dictionary<string, pLayer>();  //层集合
        public Dictionary<string, pSymbol> zSymbols = new Dictionary<string, pSymbol>();  //图元字典
        public Dictionary<string, pStyle> zStyles = new Dictionary<string, pStyle>();  //风格字典
        public Dictionary<string, pGeometry> zGeometries = new Dictionary<string, pGeometry>();  //公用几何体字典
        public Dictionary<string, pXModel> zXModels = new Dictionary<string, pXModel>();  //公用X模型字典


        #endregion

        #region 拾取检测
        ///<summary>拾取对象</summary>
        public PowerBasicObject pick(System.Windows.Point screenpoint)
        {

            int getid = D3DManager.PickModel(earth.earthkey, new POINT(screenpoint));
            if (getid != 0)
                foreach (pLayer e0 in zLayers.Values)
                {
                    foreach (PowerBasicObject e1 in e0.pModels.Values)
                    {
                        if (e1 is pData)
                            foreach (Data e2 in (e1 as pData).datas)  //数据对象需再进一层查询数据项
                            {
                                if (e2.hashcode == getid)
                                    return e1;
                            }
                        else
                        {
                            if (e1.hashcode == getid)
                                return e1;
                            else
                            {
                                foreach (PowerBasicObject ed in e1.submodels.Values)
                                {
                                    if (ed is pData)
                                    {
                                        foreach (Data ed2 in (ed as pData).datas)  //数据对象需再进一层查询数据项
                                        {
                                            if (ed2.hashcode == getid)
                                                return ed;
                                        }
                                    }
                                    else
                                        if (ed.hashcode == getid)
                                            return ed;
                                }
                            }

                        }

                    }
                }

            return null;
        }

        ///<summary>拾取对象，限定flag标志</summary>
        public PowerBasicObject pickByFlag(System.Windows.Point screenpoint, string flag)
        {

            int getid = D3DManager.PickFlagModel(earth.earthkey, new POINT(screenpoint), flag.GetHashCode());
            if (getid != 0)
                foreach (pLayer e0 in zLayers.Values)
                {
                    foreach (PowerBasicObject e1 in e0.pModels.Values)
                    {
                        if (e1 is pData)
                            foreach (Data e2 in (e1 as pData).datas)  //数据对象需再进一层查询数据项
                            {
                                if (e2.hashcode == getid)
                                    return e1;
                            }
                        else
                        {
                            if (e1.hashcode == getid)
                                return e1;
                            else
                            {
                                foreach (PowerBasicObject ed in e1.submodels.Values)
                                {
                                    if (ed is pData)
                                    {
                                        foreach (Data ed2 in (ed as pData).datas)  //数据对象需再进一层查询数据项
                                        {
                                            if (ed2.hashcode == getid)
                                                return ed;
                                        }
                                    }
                                    else
                                        if (ed.hashcode == getid)
                                            return ed;
                                }
                            }

                        }

                    }
                }

            return null;
        }
        #endregion


        #region 公开方法

        ///<summary>查找对象</summary>
        public PowerBasicObject find(string id)
        {
            PowerBasicObject obj = null;
            foreach (pLayer layer in zLayers.Values)
            {
                if (layer.pModels.TryGetValue(id, out obj))
                    return obj;
            }
            return null;
        }

        ///<summary>获取所有可见对象</summary>
        public IEnumerable<PowerBasicObject> getObjList()
        {
            return from e0 in zLayers.Values.Where(p=>p.isShow)
                   from e1 in e0.pModels.Values.Where(p2 => p2.isShowObject)
                   select e1;
        }
        ///<summary>获取业务扩展数据中指定busisort的所有可见对象</summary>
        public IEnumerable<PowerBasicObject> getObjList(string busisort)
        {
            return from e0 in zLayers.Values.Where(p => p.isShow)
                   from e1 in e0.pModels.Values.Where(p => p.busiData != null && p.busiData.busiSort == busisort && p.isShowObject)
                   select e1;
        }
        ///<summary>获取业务扩展数据中指定busisort的所有可见对象</summary>
        public Dictionary<string, PowerBasicObject> getObjDict(string busiCategory)
        {
            return getObjList(busiCategory).ToDictionary(p => p.id);
        }
        ///<summary>获取业务扩展数据中指定busiCategory的所有可见对象</summary>
        public IEnumerable<PowerBasicObject> getObjListBelongtoCategory(string busiCategory)
        {
            return from e0 in zLayers.Values.Where(p => p.isShow)
                   from e1 in e0.pModels.Values.Where(p => p.busiData.busiCategory == busiCategory && p.isShowObject)
                   select e1;
        }
        ///<summary>获取业务扩展数据中指定busiCategory的所有可见对象</summary>
        public Dictionary<string, PowerBasicObject> getObjDictBelongtoCategory(string busiCategory)
        {
            return getObjListBelongtoCategory(busiCategory).ToDictionary(p => p.id);
        }

        ///<summary>获取所有对象</summary>
        public IEnumerable<PowerBasicObject> getAllObjList()
        {
            return from e0 in zLayers.Values
                   from e1 in e0.pModels.Values
                   select e1;
        }
        ///<summary>获取所有对象</summary>
        public IEnumerable<PowerBasicObject> getAllObjList(string busisort)
        {
            return from e0 in zLayers.Values
                   from e1 in e0.pModels.Values.Where(p => p.busiData.busiSort == busisort)
                   select e1;
        }
        ///<summary>获取所有对象</summary>
        public Dictionary<string, PowerBasicObject> getAllObjDict(string busisort)
        {
            return getAllObjList(busisort).ToDictionary(p => p.id);
        }
        ///<summary>获取所有对象</summary>
        public IEnumerable<PowerBasicObject> getAllObjListBelongtoCategory(string busiCategory)
        {
            return from e0 in zLayers.Values
                   from e1 in e0.pModels.Values.Where(p => p.busiData.busiCategory == busiCategory)
                   select e1;
        }
        ///<summary>获取所有对象</summary>
        public Dictionary<string, PowerBasicObject> getAllObjDictBelongtoCategory(string busiCategory)
        {
            return getAllObjListBelongtoCategory(busiCategory).ToDictionary(p => p.id);
        }


        ///<summary>后台刷新模型，注：还要修改，对于某些绑定了值的对象，将会出现不在同一线程, 若要实现后台刷新，还需分别处理</summary>
        //internal void updateModelBackgound()
        //{
        //    if (!isUpdating && !bworker.IsBusy)
        //    {
        //        bworker.RunWorkerAsync();
        //    }
        //}


        
        private bool _isCheckModelVisualization=true;
        ///<summary>是否检查对象可视性，缺省为true, 指在地图模式下，检查对象是否与视锥体相交以及满足离地高度限制</summary>
        public bool isCheckModelVisualization
        {
            get { return _isCheckModelVisualization; }
            set { _isCheckModelVisualization = value; }
        }
      

        bool isAgainUpdate;
        ///<summary>直接刷新模型</summary>
        internal void updateModel()
        {
            if (!isUpdating)
            {
                isUpdating = true;
                isAgainUpdate = false;
                if (earth.config.isDynShow && earth.camera.operateMode == EOperateMode.地图模式 && isCheckModelVisualization)
                {
                    //earth.camera.cameraFrustum = new BoundingFrustum(earth.camera.view * Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4 * 0.5f, (float)earth.global.ScreenWidth / earth.global.ScreenHeight, earth.camera.Near, earth.camera.Far));
                    earth.camera.cameraFrustum = new BoundingFrustum(earth.camera.view * earth.camera.projection);

                    checkModelVisualization();
                }


                //遍历所有一级可见对象
                List<PowerBasicObject> Models = (from e0 in zLayers.Values.Where(p => p.isShow).OrderBy(p => p.deepOrder)
                                                 from e1 in e0.pModels.Values.Where(p2 => p2.isShowObject)
                                                 select e1).ToList();


                D3DManager.BeginTransferModel(earth.earthkey);
                updateModel(Models);
                D3DManager.EndTransferModel(earth.earthkey);
                isUpdating = false;
                if (isAgainUpdate)
                    updateModel();

            }
            else
                isAgainUpdate = true;
        }



        void updateModel(List<PowerBasicObject> Models)
        {
            foreach (PowerBasicObject item in Models)
            {
                if (item is pPowerLine)  //线路模型
                {
                    #region 折线对象操作
                    pPowerLine obj = item as pPowerLine;
                    //传递参数结构
                    STRUCT_Line strupara = new STRUCT_Line()
                    {
                        isReceivePick = obj.isReceivePick,
                        pickFlagId = obj.pickFlagId,
                        deepOrd = obj.deepOrd,
                        thickness = obj.thickness,
                        material=obj.material.materialSturPara,
                        arrowColor = Helpler.ColorToUInt(obj.arrowColor),
                        arrowSize = obj.arrowSize,
                        isInverse = obj.isInverse,
                        aniDraw = obj.aniDraw,
                        aniFlow = obj.aniFlow,
                        aniTwinkle = obj.aniTwinkle,
                        axis = obj.axis,
                        angle = obj.angle,
                        radCount=obj.radCount,
                    };
                    IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(strupara));
                    Marshal.StructureToPtr(strupara, ipPara, false);
                    //mesh数据                   
                    int count = obj.VecPoints.Count;
                    IntPtr ipData = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj.VecPoints[0]) * count);  //传递点序列结构数组指针
                    for (int i = 0; i < count; i++)
                    {
                        Marshal.StructureToPtr(obj.VecPoints[i], (IntPtr)(ipData.ToInt32() + i * Marshal.SizeOf(obj.VecPoints[i])), false);
                    }

                    D3DManager.AddModel(earth.earthkey, (int)EModelType.折线, obj.id.GetHashCode(), ipPara, ipData, count, IntPtr.Zero, 0);
                    Marshal.FreeCoTaskMem(ipPara);
                    Marshal.FreeCoTaskMem(ipData);

                    #endregion
                }
                else if (item is pSymbolObject)  //符号对象，厂站
                {
                    #region 符号对象操作
                    pSymbolObject obj = item as pSymbolObject;
                    //传递参数结构
                    STRUCT_Symbol strupara = new STRUCT_Symbol
                    {
                        isReceivePick = obj.isReceivePick,
                        pickFlagId = obj.pickFlagId,
                        deepOrd = obj.deepOrd,
                        isH = obj.isH,
                        ScaleX = (float)obj.scaleX,
                        ScaleY = (float)obj.scaleY,
                        ScaleZ = (float)obj.scaleZ,
                        texturekey = string.IsNullOrWhiteSpace(obj.symbolid) ? 0 : zSymbols[obj.symbolid].hashcode,
                        aniTwinkle = obj.aniTwinkle,
                        aniShow = obj.aniShow,
                        aniScale = obj.aniScale,
                        material=obj.material.materialSturPara,
                        isUseColor = obj.isUseColor,
                        isUseXModel = obj.isUseXModel,
                        //XMKey = string.IsNullOrWhiteSpace(obj.XModelKey) ? 0 : zXModels[obj.XModelKey].hashcode,
                        XMKey = string.IsNullOrWhiteSpace(obj.XModelKey) ? 0 : obj.XModelKey.GetHashCode(),  //注：特别的为适应以几何体为键
                        XMScaleAddition = (float)obj.XMScaleAddition,
                        axis = obj.axis,
                        angle = obj.angle,
                    };
                    IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(strupara));
                    Marshal.StructureToPtr(strupara, ipPara, false);
                    //mesh数据
                    int count = 1;
                    IntPtr ipData = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj.VecLocation) * count);  //传递点序列结构数组指针
                    Marshal.StructureToPtr(obj.VecLocation, (IntPtr)ipData.ToInt32(), false);


                    if (strupara.texturekey == 0)  //生成并传输纹理
                    {
                        byte[] bitmaps;
                        int tcount;
                        IntPtr ipTexture;
                        //生成标签纹理
                        int size = 256;
                        System.Windows.Shapes.Rectangle rec = new System.Windows.Shapes.Rectangle();
                        rec.Fill = obj.brush;
                        rec.Width = rec.Height = size;
                        rec.Measure(new System.Windows.Size(size, size));
                        rec.Arrange(new System.Windows.Rect(0, 0, size, size));

                        RenderTargetBitmap renderTarget = new RenderTargetBitmap(size, size, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                        renderTarget.Render(rec);
                        renderTarget.Freeze();
                        using (MemoryStream ms = new MemoryStream())
                        {
                            PngBitmapEncoder encode = new PngBitmapEncoder();
                            encode.Frames.Add(BitmapFrame.Create(renderTarget));
                            encode.Save(ms);
                            bitmaps = ms.GetBuffer();
                        }

                        //标签纹理
                        tcount = bitmaps.Length;
                        ipTexture = Marshal.AllocCoTaskMem(Marshal.SizeOf(bitmaps[0]) * tcount);  //传递材质数据数组指针
                        for (int i = 0; i < tcount; i++)
                        {
                            Marshal.StructureToPtr(bitmaps[i], (IntPtr)(ipTexture.ToInt32() + i * Marshal.SizeOf(bitmaps[i])), false);
                        }

                        D3DManager.AddModel(earth.earthkey, (int)EModelType.图元, obj.hashcode, ipPara, ipData, count, ipTexture, tcount);  //2 符号类型
                        Marshal.FreeCoTaskMem(ipTexture);

                    }
                    else  //不传送纹理
                    {
                        D3DManager.AddModel(earth.earthkey, (int)EModelType.图元, obj.hashcode, ipPara, ipData, count, IntPtr.Zero, 0);  //2 符号类型
                    }



                    Marshal.FreeCoTaskMem(ipPara);
                    Marshal.FreeCoTaskMem(ipData);
                    #endregion
                }
                else if (item is pText)  //文字对象
                {
                    #region 文字对象操作
                    pText obj = item as pText;
                    //传递参数结构
                    STRUCT_Symbol strupara = new STRUCT_Symbol
                    {
                        isReceivePick = obj.isReceivePick,
                        pickFlagId = obj.pickFlagId,
                        deepOrd = obj.deepOrd,
                        rootid = obj.hashcode,
                        parentid = 0,
                        isH = obj.isH,
                        texturekey = string.IsNullOrWhiteSpace(obj.textureid) ? 0 : obj.textureid.GetHashCode(),
                        ScaleX = obj.scaleX,
                        ScaleY = obj.scaleY,
                        ScaleZ = obj.scaleY,
                        material=obj.material.materialSturPara,
                        aniTwinkle = obj.aniTwinkle,
                        axis = obj.axis,
                        angle = obj.angle,
                    };
                    IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(strupara));
                    Marshal.StructureToPtr(strupara, ipPara, false);
                    //mesh数据
                    int count = 1;
                    IntPtr ipData = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj.VecLocation) * count);  //传递点序列结构数组指针
                    Marshal.StructureToPtr(obj.VecLocation, (IntPtr)ipData.ToInt32(), false);
                    //标签文字
                    IntPtr ipLabel = Marshal.StringToCoTaskMemUni(obj.text);

                    D3DManager.AddModel(earth.earthkey, (int)EModelType.文字, obj.hashcode, ipPara, ipData, count, ipLabel, 0);  //2 标签类型
                    Marshal.FreeCoTaskMem(ipPara);
                    Marshal.FreeCoTaskMem(ipLabel);
                    #endregion
                }
                else if (item is pData)  //数据对象
                {
                    #region 数据对象操作
                    pData obj = item as pData;

                    int parentid = 0;
                    int id;
                    int rootid = 0;
                    if (obj.showType == pData.EShowType.几何体)
                    {
                        foreach (Data zd in obj.datas)
                        {
                            if (rootid == 0)
                                rootid = zd.id.GetHashCode();
                            //传递参数结构
                            STRUCT_PolyCol strupara = new STRUCT_PolyCol
                            {
                                isReceivePick = obj.isReceivePick,
                                pickFlagId = obj.pickFlagId,
                                deepOrd = obj.deepOrd,
                                rootid = rootid,
                                parentid = parentid,
                                material=zd.material.materialSturPara,
                                sizex = (float)obj.radScale,
                                sizey = (float)obj.radScale,
                                height = (float)(zd.CurValue * obj.valueScale),
                                geokey = zd.geokeyhashcode,
                                aniScale = obj.aniScale,
                                aniRotation = obj.aniRotation,
                                axis = obj.axis,
                                angle = obj.angle,
                            };
                            IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(strupara));
                            Marshal.StructureToPtr(strupara, ipPara, false);
                            //mesh数据
                            int count = 1;
                            IntPtr ipData = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj.VecLocation) * count);  //传递点序列结构数组指针
                            Marshal.StructureToPtr(obj.VecLocation, (IntPtr)ipData.ToInt32(), false);
                            id = zd.hashcode;
                            D3DManager.AddModel(earth.earthkey, (int)EModelType.几何体, id, ipPara, ipData, count, IntPtr.Zero, 0);  //2 符号类型
                            Marshal.FreeCoTaskMem(ipPara);
                            Marshal.FreeCoTaskMem(ipData);

                            parentid = id;
                        }
                        if (obj.isShowLabel) //显示标签
                        {
                            //传递参数结构
                            STRUCT_Symbol strupara = new STRUCT_Symbol
                            {
                                deepOrd = obj.deepOrd,
                                rootid = rootid,
                                parentid = parentid,
                                isH = obj.isH,
                                texturekey = 0,
                                ScaleX = obj.labelScaleX,  //文字缩放修正系数
                                ScaleY = obj.labelScaleY,
                                ScaleZ = obj.labelScaleY,
                                material=obj.material.materialSturPara,
                                axis = obj.axis,
                                angle = obj.angle,
                            };
                            IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(strupara));
                            Marshal.StructureToPtr(strupara, ipPara, false);
                            //mesh数据
                            int count = 1;
                            IntPtr ipData = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj.VecLocation) * count);  //传递点序列结构数组指针
                            Marshal.StructureToPtr(obj.VecLocation, (IntPtr)ipData.ToInt32(), false);
                            //标签文字
                            IntPtr ipLabel = Marshal.StringToCoTaskMemUni(obj.dataLabel);
                            D3DManager.AddModel(earth.earthkey, (int)EModelType.文字, obj.labelhashcode, ipPara, ipData, count, ipLabel, 0);  //2 标签类型
                            Marshal.FreeCoTaskMem(ipPara);
                            Marshal.FreeCoTaskMem(ipLabel);

                        }
                    }
                    else
                    {
                        //传递参数结构
                        STRUCT_Symbol strupara = new STRUCT_Symbol
                        {
                            deepOrd = obj.deepOrd,
                            rootid = obj.labelhashcode,
                            parentid = 0,
                            isH = obj.isH,
                            texturekey = 0,
                            ScaleX = obj.labelScaleX,  //文字缩放修正系数
                            ScaleY = obj.labelScaleY,
                            ScaleZ = obj.labelScaleY,
                            material=obj.material.materialSturPara,
                            axis = obj.axis,
                            angle = obj.angle,
                        };
                        IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(strupara));
                        Marshal.StructureToPtr(strupara, ipPara, false);
                        //mesh数据
                        int count = 1;
                        IntPtr ipData = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj.VecLocation) * count);  //传递点序列结构数组指针
                        Marshal.StructureToPtr(obj.VecLocation, (IntPtr)ipData.ToInt32(), false);
                        //标签文字
                        IntPtr ipLabel = Marshal.StringToCoTaskMemUni(obj.dataLabel);
                        D3DManager.AddModel(earth.earthkey, (int)EModelType.文字, obj.labelhashcode, ipPara, ipData, count, ipLabel, 0);  //2 标签类型
                        Marshal.FreeCoTaskMem(ipPara);
                        Marshal.FreeCoTaskMem(ipLabel);
                    }
                    #endregion
                }
                else if (item is pArea)  //区域对象
                {
                    #region 区域对象操作
                    pArea obj = item as pArea;
                    if (obj.VecPoints.Count() > 0 && obj.indexes.Count() > 0)
                    {
                        //传递参数结构
                        STRUCT_Area strupara = new STRUCT_Area()
                        {
                            isReceivePick = obj.isReceivePick,
                            pickFlagId = obj.pickFlagId,
                            deepOrd = obj.deepOrd,
                            material=obj.material.materialSturPara,
                            aniShow = obj.aniShow,
                            axis = obj.axis,
                            angle = obj.angle,
                        };
                        IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(strupara));
                        Marshal.StructureToPtr(strupara, ipPara, false);
                        //mesh数据                   
                        int count = obj.VecPoints.Count;
                        IntPtr ipData = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj.VecPoints[0]) * count);  //传递点序列结构数组指针
                        for (int i = 0; i < count; i++)
                        {
                            Marshal.StructureToPtr(obj.VecPoints[i], (IntPtr)(ipData.ToInt32() + i * Marshal.SizeOf(obj.VecPoints[i])), false);
                        }
                        //索引数据，注：借用材质指针
                        int idxcount = obj.indexes.Length;
                        IntPtr ipIndex = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj.indexes[0]) * idxcount);  //传递点序列结构数组指针
                        for (int i = 0; i < idxcount; i++)
                        {
                            Marshal.StructureToPtr(obj.indexes[i], (IntPtr)(ipIndex.ToInt32() + i * Marshal.SizeOf(obj.indexes[i])), false);
                        }

                        D3DManager.AddModel(earth.earthkey, (int)EModelType.区域, obj.hashcode, ipPara, ipData, count, ipIndex, idxcount);
                        Marshal.FreeCoTaskMem(ipPara);
                        Marshal.FreeCoTaskMem(ipData);
                        Marshal.FreeCoTaskMem(ipIndex);
                    }
                    #endregion
                }
                else if (item is pContour)  //等值图对象
                {
                    #region 等值图对象操作
                    pContour obj = item as pContour;
                    //传递参数结构
                    STRUCT_Area strupara = new STRUCT_Area()
                    {
                        isReceivePick = obj.isReceivePick,
                        pickFlagId = obj.pickFlagId,
                        deepOrd = obj.deepOrd,
                        aniShow = obj.aniShow,
                        axis = obj.axis,
                        angle = obj.angle,
                    };
                    IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(strupara));
                    Marshal.StructureToPtr(strupara, ipPara, false);

                    //mesh数据                   
                    int count = obj.points.Length;
                    IntPtr ipData = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj.points[0]) * count);  //传递点序列结构数组指针
                    for (int i = 0; i < count; i++)
                    {
                        Marshal.StructureToPtr(obj.points[i], (IntPtr)(ipData.ToInt32() + i * Marshal.SizeOf(obj.points[i])), false);
                    }
                    //材质数据
                    int size = 1024;
                    System.Windows.Shapes.Rectangle rec = new System.Windows.Shapes.Rectangle();
                    rec.Fill = obj.brush;
                    rec.Width = rec.Height = size;
                    rec.Measure(new System.Windows.Size(size, size));
                    rec.Arrange(new System.Windows.Rect(0, 0, size, size));


                    RenderTargetBitmap renderTarget = new RenderTargetBitmap(size, size, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                    renderTarget.Render(rec);
                    renderTarget.Freeze();

                    byte[] bitmaps;
                    int tcount;
                    IntPtr ipTexture;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        PngBitmapEncoder encode = new PngBitmapEncoder();
                        encode.Frames.Add(BitmapFrame.Create(renderTarget));
                        encode.Save(ms);
                        bitmaps = ms.GetBuffer();

                    }

                    tcount = bitmaps.Length;
                    ipTexture = Marshal.AllocCoTaskMem(Marshal.SizeOf(bitmaps[0]) * tcount);  //传递材质数据数组指针
                    for (int i = 0; i < tcount; i++)
                    {
                        Marshal.StructureToPtr(bitmaps[i], (IntPtr)(ipTexture.ToInt32() + i * Marshal.SizeOf(bitmaps[i])), false);
                    }


                    D3DManager.AddModel(earth.earthkey, (int)EModelType.等值图, obj.hashcode, ipPara, ipData, count, ipTexture, tcount);
                    Marshal.FreeCoTaskMem(ipData);
                    Marshal.FreeCoTaskMem(ipTexture);
                    #endregion
                }
                else if (item is pCustomObject)  //自定义模型对象
                {
                    #region 自定义模型对象操作
                    pCustomObject obj = item as pCustomObject;
                    if (obj.VecVertices.Count() > 0 && obj.VecIndexes.Count() > 0)
                    {
                        //传递参数结构
                        STRUCT_Custom strupara = new STRUCT_Custom()
                        {
                            isReceivePick = obj.isReceivePick,
                            pickFlagId = obj.pickFlagId,
                            deepOrd = obj.deepOrd,
                            material=obj.material.materialSturPara,
                            ScaleX = (float)obj.scaleX,
                            ScaleY = (float)obj.scaleY,
                            ScaleZ = (float)obj.scaleZ,
                            axis = obj.axis,
                            angle = obj.angle,
                            texturekey = string.IsNullOrWhiteSpace(obj.symbolid) ? 0 : zSymbols[obj.symbolid].hashcode,
                            drawMode = (int)obj.drawMode,
                        };
                        IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(strupara));
                        Marshal.StructureToPtr(strupara, ipPara, false);
                        //lcation数据
                        IntPtr ipLocation = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj.VecLocation));  //传递点序列结构数组指针
                        Marshal.StructureToPtr(obj.VecLocation, (IntPtr)ipLocation.ToInt32(), false);

                        //vertices数据                   
                        int count = obj.VecVertices.Count;
                        IntPtr ipVertices = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj.VecVertices[0]) * count);  //传递点序列结构数组指针
                        for (int i = 0; i < count; i++)
                        {
                            Marshal.StructureToPtr(obj.VecVertices[i], (IntPtr)(ipVertices.ToInt32() + i * Marshal.SizeOf(obj.VecVertices[i])), false);
                        }
                        //normals数据
                        IntPtr ipNormal =obj.VecNormals==null? IntPtr.Zero: Marshal.AllocCoTaskMem(Marshal.SizeOf(obj.VecNormals[0]) * count);  //传递点序列结构数组指针
                        if (obj.VecNormals!=null)
                        for (int i = 0; i < count; i++)
                        {
                            Marshal.StructureToPtr(obj.VecNormals[i], (IntPtr)(ipNormal.ToInt32() + i * Marshal.SizeOf(obj.VecNormals[i])), false);
                        }
                        //索引数据
                        int idxcount = obj.VecIndexes.Count;
                        IntPtr ipIndex = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj.VecIndexes[0]) * idxcount);  //传递点序列结构数组指针
                        for (int i = 0; i < idxcount; i++)
                        {
                            Marshal.StructureToPtr(obj.VecIndexes[i], (IntPtr)(ipIndex.ToInt32() + i * Marshal.SizeOf(obj.VecIndexes[i])), false);
                        }
                        //uv数据
                        int uvcount = obj.uvs == null ? 0 : obj.uvs.Count;
                        IntPtr ipUV = obj.uvs == null ? IntPtr.Zero : Marshal.AllocCoTaskMem(Marshal.SizeOf(obj.uvs[0]) * uvcount);  //传递点序列结构数组指针
                        for (int i = 0; i < uvcount; i++)
                        {
                            Marshal.StructureToPtr(obj.uvs[i], (IntPtr)(ipUV.ToInt32() + i * Marshal.SizeOf(obj.uvs[i])), false);
                        }

                        //纹理路径
                        IntPtr iptexture = string.IsNullOrWhiteSpace(obj.texture) ? IntPtr.Zero : Marshal.StringToCoTaskMemUni(obj.texture);


                        D3DManager.AddCustomModel(earth.earthkey, obj.hashcode, ipPara, ipLocation, ipVertices, ipNormal, count, ipIndex, idxcount, ipUV, uvcount, iptexture);
                        Marshal.FreeCoTaskMem(ipPara);
                        Marshal.FreeCoTaskMem(ipLocation);
                        Marshal.FreeCoTaskMem(ipVertices);
                        Marshal.FreeCoTaskMem(ipNormal);
                        Marshal.FreeCoTaskMem(ipIndex);
                        Marshal.FreeCoTaskMem(ipUV);
                        Marshal.FreeCoTaskMem(iptexture);
                    }
                    #endregion
                }


                //遍历所有依附的数据对象
                if (item.isShowSubObject && item.submodels.Count > 0)
                {
                    List<PowerBasicObject> submodels = (from e0 in item.submodels.Values
                                                        select e0 as PowerBasicObject).ToList();
                    updateModel(submodels);
                }
            }


        }


        ///<summary>
        ///检查并设置所有层的范围可视性。
        ///检测逻辑：
        ///条件：相机离地距离是否在对象的可视离地范围内；
        ///</summary>
        internal void checkLayerVisualization()
        {
            foreach (pLayer lay in zLayers.Values)
                lay.checkVisualization();
        }
        ///<summary>
        ///检查并设置所有对象的范围可视性。
        ///检测逻辑：
        ///条件一：相机离地距离是否在对象的可视离地范围内；
        ///条件二：点对象中心点是否在相机可视经纬范围内，区域对象经纬范围是否与相机可视经纬范围相交。
        ///</summary>
        internal void checkModelVisualization()
        {


            //遍历所有一级可见对象
            List<PowerBasicObject> Models = (from e0 in zLayers.Values.Where(p => p.isShow).OrderBy(p => p.deepOrder)
                                             from e1 in e0.pModels.Values
                                             select e1).ToList();

            foreach (PowerBasicObject item in Models)
            {
                item.checkVisiualization();

                ////遍历所有依附的数据对象
                //if (item.isShowSubObject && item.submodels.Count > 0)
                //{
                //    List<PowerBasicObject> submodels = (from e0 in item.submodels.Values
                //                                        select e0 as PowerBasicObject).ToList();
                //    updateModel(submodels);
                //}
            }

        }


        //=====测试后台刷新动态线宽



        ///<summary>刷新动态线宽</summary>
        internal void refreshDynLineWidth()
        {
            List<pPowerLine> Models = (from e0 in zLayers.Values.Where(p => p.isShow)
                                       from e1 in e0.pModels.Values.Where(p => p is pPowerLine && (p as pPowerLine).dynLineWidthEnable)
                                       select e1 as pPowerLine).ToList();

            float linescale = earth.camera.curCameraDistanceToGround / earth.objManager.dynLineWidthDefaultDistance;
            float lw;
            foreach (pPowerLine item in Models)
            {
                lw = item.defaultThickness * linescale;
                if (lw < earth.objManager.dynLineWidthMin) lw = dynLineWidthMin;
                else if (lw > earth.objManager.dynLineWidthMax) lw = dynLineWidthMax;

                if (item.thickness != lw)
                {
                    item.thickness = lw;
                    item.arrowSize = item.defaultArrowSize * lw / item.defaultThickness; //同比例缩放箭头
                }
            }
        }


        ///<summary>所有对象所占的场景范围，根据所有可视对象计算出，每次调用均实时计算。原始坐标</summary>
        public System.Windows.Rect getBounds()
        {
            System.Windows.Rect bounds;
            bounds = System.Windows.Rect.Empty;
            foreach (PowerBasicObject obj in getObjList())
            {
                if (bounds == System.Windows.Rect.Empty)
                {
                    if (obj is pPowerLine)
                        bounds = (obj as pPowerLine).bounds;
                    else if (obj is pSymbolObject)
                        bounds = (obj as pSymbolObject).bounds;
                    else if (obj is pArea)
                        bounds = (obj as pArea).bounds;
                }
                else
                {
                    if (obj is pPowerLine)
                        bounds.Union((obj as pPowerLine).bounds);
                    else if (obj is pSymbolObject)
                        bounds.Union((obj as pSymbolObject).bounds);
                    else if (obj is pArea)
                        bounds.Union((obj as pArea).bounds);
                }

            }
            return bounds;
        }

        ///<summary>更新公用XModel资源</summary>
        public void updateXModel()
        {
            foreach (pXModel item in zXModels.Values)
            {
                if (item.eModelType == pXModel.EModelType.X模型)
                {
                    IntPtr ipFile = Marshal.StringToCoTaskMemUni(item.filepath);

                    IntPtr ipAxis = Marshal.AllocCoTaskMem(Marshal.SizeOf(item.rotationAxis));
                    Marshal.StructureToPtr(item.rotationAxis, ipAxis, false);

                    D3DManager.AddXModel(earth.earthkey, item.hashcode, ipFile, ipAxis, item.rotationAngle);
                    Marshal.FreeCoTaskMem(ipFile);
                    Marshal.FreeCoTaskMem(ipAxis);
                }
                else if (item.eModelType== pXModel.EModelType.Custom模型)
                {
                    DirectAddModelToD3D(item.id, item.VecVertices, item.VecNormals, item.VecIndexes, item.uvs, item.texture, item.rotationAxis, item.rotationAngle);
                }
            }
        }

        ///<summary>更新公用图元纹理资源</summary>
        public void updateSymbolTexture()
        {
            foreach (pSymbol item in zSymbols.Values)
            {
                if (!string.IsNullOrWhiteSpace(item.texturefile))  //采用文件材质
                {
                    if (System.IO.File.Exists(item.texturefile))
                    {
                        IntPtr ipFile = Marshal.StringToCoTaskMemUni(item.texturefile);
                        D3DManager.AddTextureFromFile(earth.earthkey, item.id.GetHashCode(), ipFile);
                        Marshal.FreeCoTaskMem(ipFile);
                    }
                    else
                    {
                        MyBaseControls.StatusBarTool.StatusBarTool.reportInfo.showInfo(string.Format("未找到纹理文件{0}",item.texturefile), 30);
                    }
                }
                else //根据bursh生成材质
                {
                    int size = 256;
                    System.Windows.Shapes.Rectangle rec = new System.Windows.Shapes.Rectangle();
                    rec.Fill = item.brush;
                    rec.Width = rec.Height = size;
                    rec.Measure(new System.Windows.Size(size, size));
                    rec.Arrange(new System.Windows.Rect(0, 0, size, size));


                    RenderTargetBitmap renderTarget = new RenderTargetBitmap(size, size, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                    renderTarget.Render(rec);
                    renderTarget.Freeze();

                    byte[] bitmaps;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        PngBitmapEncoder encode = new PngBitmapEncoder();
                        encode.Frames.Add(BitmapFrame.Create(renderTarget));
                        encode.Save(ms);
                        bitmaps = ms.GetBuffer();

                        int count = bitmaps.Length;
                        IntPtr ipData = Marshal.AllocCoTaskMem(Marshal.SizeOf(bitmaps[0]) * count);  //传递点序列结构数组指针
                        for (int i = 0; i < count; i++)
                        {
                            Marshal.StructureToPtr(bitmaps[i], (IntPtr)(ipData.ToInt32() + i * Marshal.SizeOf(bitmaps[i])), false);
                        }

                        D3DManager.AddTexture(earth.earthkey, item.id.GetHashCode(), ipData, count);

                        //Marshal.FreeCoTaskMem(ipID);
                        Marshal.FreeCoTaskMem(ipData);

                    }
                }
            }

        }

        ///<summary>更新公用几何体资源</summary>
        public void updateGeomeries()
        {
            STRUCT_Geometry strupara = new STRUCT_Geometry();
            foreach (pGeometry item in zGeometries.Values)
            {
                switch (item.goetype)
                {
                    case EGeometryType.立方体:
                        strupara = new STRUCT_Geometry()
                        {
                            geoType = (int)item.goetype,
                            pf1 = item.pf1,
                            pf2 = item.pf2,
                            pf3 = item.pf3
                        };
                        break;
                    case EGeometryType.柱体:
                        strupara = new STRUCT_Geometry()
                        {
                            geoType = (int)item.goetype,
                            pf1 = item.pf1,
                            pf2 = item.pf2,
                            pf3 = item.pf3,
                            pi1 = item.pi1,
                            pi2 = item.pi2
                        };

                        break;
                    case EGeometryType.球体:
                        strupara = new STRUCT_Geometry()
                        {
                            geoType = (int)item.goetype,
                            pf1 = item.pf1,
                            pi1 = item.pi1,
                            pi2 = item.pi2
                        };

                        break;
                }
                //传递参数结构

                IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(strupara));
                Marshal.StructureToPtr(strupara, ipPara, false);
                D3DManager.AddGeometry(earth.earthkey, item.hashcode, ipPara);
                Marshal.FreeCoTaskMem(ipPara);



            }

        }

        /// <summary>
        /// 向几何体资源字典添加立方体资源
        /// </summary>
        /// <param name="key">唯一键</param>
        /// <param name="width">宽度</param>
        /// <param name="thickness">厚度</param>
        /// <param name="height">高度，在资源字典中使用时，请填写1，为以后的扩展保留此参数</param>
        public void AddBoxResource(string key, float width, float thickness, float height)
        {
            zGeometries.Add(key, new pGeometry() { goetype = EGeometryType.立方体, id = key, pf1 = width, pf2 = thickness, pf3 = height });
        }
        /// <summary>
        /// 向几何体资源字典添加柱体或锥体
        /// </summary>
        /// <param name="key">唯一键</param>
        /// <param name="topRad">顶部半径</param>
        /// <param name="bottomRad">底部半径</param>
        /// <param name="height">高度，在资源字典中使用时，请填写1，为以后的扩展保留此参数</param>
        /// <param name="slice">圆周切分，例如4则为立方体</param>
        /// <param name="stack">填1</param>
        public void AddCylinderResource(string key, float topRad, float bottomRad, float height, int slice, int stack)
        {
            zGeometries.Add(key, new pGeometry() { goetype = EGeometryType.柱体, id = key, pf1 = topRad, pf2 = bottomRad, pf3 = height, pi1 = slice, pi2 = stack });
        }
        /// <summary>
        /// 向几何体资源字典添加球体
        /// </summary>
        /// <param name="key">唯一键</param>
        /// <param name="Radius">半径，在资源字典中使用时，请填写1，为以后的扩展保留此参数</param>
        /// <param name="slice">横向切分数</param>
        /// <param name="stack">纵向切分数</param>
        public void AddSphereResource(string key, float Radius, int slice, int stack)
        {
            zGeometries.Add(key, new pGeometry() { goetype = EGeometryType.球体, id = key, pf1 = Radius, pi1 = slice, pi2 = stack });
        }

        /// <summary>
        /// 添加图元纹理资源
        /// </summary>
        /// <param name="key">唯一键</param>
        /// <param name="imagefile">图片文件名，同时做为纹理文件</param>
        public bool AddSymbol(string sort, string key, string imagefile)
        {
            if (zSymbols.Keys.Contains(key))
                return false;
            else
                zSymbols.Add(key, new pSymbol() { sort = sort, id = key, imagefile = imagefile, texturefile = imagefile });
            return true;
        }
        /// <summary>
        /// 添加图元纹理资源
        /// </summary>
        /// <param name="key">唯一键</param>
        /// <param name="imagefile">图片文件名</param>
        /// <param name="texturefile">纹理文件名</param>
        public bool AddSymbol(string sort, string key, string imagefile, string texturefile)
        {
            if (zSymbols.Keys.Contains(key))
                return false;
            else
                zSymbols.Add(key, new pSymbol() { sort = sort, id = key, imagefile = imagefile, texturefile = texturefile });
            return true;
        }
        /// <summary>
        /// 添加图元纹理资源
        /// </summary>
        /// <param name="key">唯一键</param>
        /// <param name="file">文件名</param>
        public bool AddSymbol(string sort, string key, System.Windows.Media.Brush brush, int width, int height)
        {
            if (zSymbols.Keys.Contains(key))
                return false;
            else
                zSymbols.Add(key, new pSymbol() { id = key, brush = brush, sizeX = width, sizeY = height, sort = sort });
            return true;
        }


        ///<summary>添加XModel模型，不直接生效，用于初始化时</summary>
        public bool AddXModel(string key, string file)
        {
            if (zXModels.Keys.Contains(key))
                return false;
            else
                zXModels.Add(key, new pXModel() { id = key, filepath = file , eModelType= pXModel.EModelType.X模型});
            return true;
        }
        ///<summary>添加Custom模型，不直接生效，用于初始化时</summary>
        public bool AddCustomModel(string key, List<VECTOR3D> vecs, List<VECTOR3D> normals, List<ushort> idxes, List<VECTOR2D> uvs, string texturefile, VECTOR3D rotationAxis, float rotationAngle)
        {
            if (zXModels.Keys.Contains(key))
                return false;
            else
                zXModels.Add(key, new pXModel() { id = key, eModelType = pXModel.EModelType.Custom模型, texture=texturefile, VecVertices=vecs, VecNormals=normals, VecIndexes=idxes, uvs=uvs, rotationAxis= rotationAxis, rotationAngle=rotationAngle  });
            return true;
        }


        /// <summary>
        /// 添加层并返回层，若同键值层已存在，返回已有的层
        /// </summary>
        /// <param name="Key">层的字典键值</param>
        /// <param name="ID">层ID</param>
        /// <param name="LayerName"></param>
        public pLayer AddLayer(string Key, string ID, string LayerName)
        {
            pLayer player;
            if (!zLayers.TryGetValue(Key, out player))
            {
                player = new pLayer(this) { id = ID, name = LayerName };
                zLayers.Add(Key, player);
            }
            return player;
        }

        ///<summary>直接添加自定义模型到D3D模型库</summary>
        public void DirectAddModelToD3D(string modelkey, List<VECTOR3D> VecVertices, List<VECTOR3D> VecNormals, List<ushort> VecIndexes, List<VECTOR2D> uvs, string texture, VECTOR3D rotationAxis, float rotationAngle)
        {
            #region 自定义模型数据传入d3d

            //vertices数据                   
            int count = VecVertices.Count;
            IntPtr ipVertices = Marshal.AllocCoTaskMem(Marshal.SizeOf(VecVertices[0]) * count);  //传递点序列结构数组指针
            for (int i = 0; i < count; i++)
            {
                Marshal.StructureToPtr(VecVertices[i], (IntPtr)(ipVertices.ToInt32() + i * Marshal.SizeOf(VecVertices[i])), false);
            }
            //normals数据
            IntPtr ipNormal =VecNormals==null?IntPtr.Zero: Marshal.AllocCoTaskMem(Marshal.SizeOf(VecNormals[0]) * count);  //传递点序列结构数组指针
            if (VecNormals!=null)
            for (int i = 0; i < count; i++)
            {
                Marshal.StructureToPtr(VecNormals[i], (IntPtr)(ipNormal.ToInt32() + i * Marshal.SizeOf(VecNormals[i])), false);
            }
            //索引数据
            int idxcount = VecIndexes.Count;
            IntPtr ipIndex = Marshal.AllocCoTaskMem(Marshal.SizeOf(VecIndexes[0]) * idxcount);  //传递点序列结构数组指针
            for (int i = 0; i < idxcount; i++)
            {
                Marshal.StructureToPtr(VecIndexes[i], (IntPtr)(ipIndex.ToInt32() + i * Marshal.SizeOf(VecIndexes[i])), false);
            }
            //uv数据
            int uvcount = uvs == null ? 0 : uvs.Count;
            IntPtr ipUV = uvs == null ? IntPtr.Zero : Marshal.AllocCoTaskMem(Marshal.SizeOf(uvs[0]) * uvcount);  //传递点序列结构数组指针
            for (int i = 0; i < uvcount; i++)
            {
                Marshal.StructureToPtr(uvs[i], (IntPtr)(ipUV.ToInt32() + i * Marshal.SizeOf(uvs[i])), false);
            }

            //纹理路径
            IntPtr iptexture = string.IsNullOrWhiteSpace(texture) ? IntPtr.Zero : Marshal.StringToCoTaskMemUni(texture);

            IntPtr ipAxis = Marshal.AllocCoTaskMem(Marshal.SizeOf(rotationAxis));
            Marshal.StructureToPtr(rotationAxis, ipAxis, false);

            D3DManager.AddCustomAsXModel(earth.earthkey, modelkey.GetHashCode(), ipVertices, ipNormal, count, ipIndex, idxcount, ipUV, uvcount, iptexture, ipAxis, rotationAngle);
            Marshal.FreeCoTaskMem(ipVertices);
            Marshal.FreeCoTaskMem(ipNormal);
            Marshal.FreeCoTaskMem(ipIndex);
            Marshal.FreeCoTaskMem(ipUV);
            Marshal.FreeCoTaskMem(iptexture);
            Marshal.FreeCoTaskMem(ipAxis);
            #endregion

        }



        #endregion


        #region ----- 保存视觉属性机制 -----
        Dictionary<string, ObjSaveStatus> saveObjsStatus = new Dictionary<string, ObjSaveStatus>();

        ///<summary>保存指定对象的视觉属性（已实现颜色，大小，逻辑可见性），若该对象已在字典中，忽略</summary>
        public void saveVisionProperty(PowerBasicObject obj)
        {
            if (!saveObjsStatus.ContainsKey(obj.id))
                saveObjsStatus.Add(obj.id, new ObjSaveStatus(obj));
        }
        ///<summary>保存所有对象的视觉属性（已实现颜色，大小，逻辑可见性），若该对象已在字典中，忽略</summary>
        public void saveVisionProperty()
        {
            foreach (pLayer lay in zLayers.Values)
            {
                foreach (PowerBasicObject obj in lay.pModels.Values)
                {
                    if (!saveObjsStatus.ContainsKey(obj.id))
                        saveObjsStatus.Add(obj.id, new ObjSaveStatus(obj));
                }
            }

        }


        ///<summary>恢复所有曾保存对象视觉属性，强制清除闪烁动画，并清空视觉属性存储列表</summary>
        public void restoreVisionProperty()
        {
            foreach (ObjSaveStatus item in saveObjsStatus.Values)
            {
                item.pop();
            }
            saveObjsStatus.Clear();
        }

        ///<summary>清除保存的视觉属性列表</summary>
        public void clearVisionProperty()
        {
            saveObjsStatus.Clear();
        }

        #endregion




    }



    ///<summary>对象状态存储类</summary>
    class ObjSaveStatus
    {
        ///<summary>创建对象状态存储类，并保存当前状态</summary>
        public ObjSaveStatus(PowerBasicObject Obj)
        {
            obj = Obj;
            push();
        }

        PowerBasicObject obj;

        ObjPropBase save;

        //public bool isTwinkle;

        internal void push()
        {
            if (obj is pSymbolObject)
            {
                pSymbolObject to = (pSymbolObject)obj;
                symbolProp tp = new symbolProp();
                save = tp;
                tp.color = to.color;
                tp.scalex = to.scaleX;
                tp.scaley = to.scaleY;
                tp.scalez = to.scaleZ;
                tp.logicVisibility = to.logicVisibility;
            }
            else if (obj is pPowerLine)
            {
                pPowerLine to = (pPowerLine)obj;
                polylineProp tp = new polylineProp();
                save = tp;
                tp.color = to.color;
                tp.defaultThickness = to.defaultThickness;
                tp.thickness = to.thickness;
                tp.arrowColor = to.arrowColor;
                tp.arrowSize = to.arrowSize;
                tp.defaultArrowSize = to.defaultArrowSize;
                tp.logicVisibility = to.logicVisibility;
            }
        }

        ///<summary>恢复对象状态</summary>
        internal void pop()
        {
            if (obj is pSymbolObject)
            {
                pSymbolObject to = (pSymbolObject)obj;
                symbolProp tp = (symbolProp)save;
                to.color = tp.color;
                to.scaleX = tp.scalex;
                to.scaleY = tp.scaley;
                to.scaleZ = tp.scalez;
                to.logicVisibility = tp.logicVisibility;
                //if (isTwinkle)
                (obj as pSymbolObject).AnimationStop(pSymbolObject.EAnimationType.闪烁);
            }
            else if (obj is pPowerLine)
            {
                pPowerLine to = (pPowerLine)obj;
                polylineProp tp = (polylineProp)save;
                to.color = tp.color;
                to.thickness = tp.thickness;
                to.defaultThickness = tp.defaultThickness;
                to.arrowColor = tp.arrowColor;
                to.arrowSize = tp.arrowSize;
                to.defaultArrowSize = tp.defaultArrowSize;
                to.logicVisibility = tp.logicVisibility;
                //if (isTwinkle)
                (obj as pPowerLine).AnimationStop(pPowerLine.EAnimationType.闪烁);
            }

        }

    }

    ///<summary>存储属性基类</summary>
    class ObjPropBase
    {
        public System.Windows.Media.Color color { get; set; }
        public bool logicVisibility { get; set; }
    }

    ///<summary>图元类属性</summary>
    class symbolProp : ObjPropBase
    {
        public float scalex { get; set; }
        public float scaley { get; set; }
        public float scalez { get; set; }
    }

    ///<summary>折线类属性</summary>
    class polylineProp : ObjPropBase
    {
        public System.Windows.Media.Color arrowColor { get; set; }
        public float defaultArrowSize { get; set; }
        public float arrowSize { get; set; }
        public float defaultThickness { get; set; }
        public float thickness { get; set; }


    }

}
