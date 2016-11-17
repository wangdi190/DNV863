using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace WpfEarthLibrary
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class EarthManager 
    {
        public EarthManager(Earth pearth)
        {
            datas = new EarthData(this,null, 0, 0);
            earth = pearth;
            bworker.DoWork += new DoWorkEventHandler(bworker_DoWork);
            bworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bworker_RunWorkerCompleted);


            //==== 初始化c#相机
            float tmp = 1.001f * Para.scalepara;
            earth.camera = new Camera(new Vector3(2175.563f, 4090f, 4384f) * tmp, new Vector3(0, 0, 0), Vector3.Up, earth);

            //==== 初始化d3d相机

            initEarth();
        }

        internal Earth earth;

        internal enum ECalStatus { 空闲, 计算, 完成 }

        #region ===== 公开属性 =====

        #region =============================== 平面模式相关 =================================
        private System.Windows.Rect _planeViewBox=new Rect(0,0,800,600);
        ///<summary>平面模式下场景的范围</summary>
        public System.Windows.Rect planeViewBox
        {
            get { return _planeViewBox; }
            set { _planeViewBox = value; planeAdjustCamera(); }
        }
        
        private float _planeCameraHeight=1.5f;
        ///<summary>平面模式下相机离地面高度</summary>
        public float planeCameraHeight
        {
            get { return _planeCameraHeight; }
            set { _planeCameraHeight = value; }
        }

        ///<summary>根据planeviewbox和planecameraheight自动调整相机位置</summary>
        void planeAdjustCamera()
        {
            //计算相机位置
            System.Windows.Point pnt = new System.Windows.Point(planeViewBox.Width / 2, planeViewBox.Height / 2);
            System.Windows.Point geopnt = geohelper.planeToGeo(pnt.ToString());
            VECTOR3D vc = MapHelper.JWHToPoint(geopnt.Y, geopnt.X, Para.LineHeight, earthpara);

            float tmp = 1.0f + planeCameraHeight / Para.Radius;
            earth.camera = new Camera(new Vector3(vc.x,vc.y,vc.z) * tmp, new Vector3(0, 0, 0), Vector3.Up, earth);

        }
        #endregion

        ///<summary>是否允许显示3D地形</summary>
        public bool TerrainAllow {get;set;}
        ///<summary>地形显示离地高度限制，显示3D地形的最大离地高度，只有小于该值才会显示3D地形，缺省0</summary>
        public double TerrainMaxDistance { get; set; }
        ///<summary>地形显示地心线角高度限制，显示3D地形的最大地面夹角(角度值)，只有大于该值才显示3D地形，缺省0</summary>
        public double TerrainMaxAngle { get; set; }
        ///<summary>高程图黑色代表的最小高度（系统内部坐标系）</summary>
        public float TerrainMinHeight = 0;
        ///<summary>高程图最大高度落差，即minHeight+dropHeight为高程图白色表示的最大高度（系统内部坐标系）</summary>
        public float TerrainDropHeight = 2;
        ///<summary>地形显示层号限制，允许显示地形的最大层号，只有大于等于最大层号且小于等于最小层号的瓦片才能显示地形</summary>
        public int TerrainMaxLayer = 12;
        ///<summary>地形显示层号限制，允许显示地形的最小层号</summary>
        public int TerrainMinLayer = 10;
        ///<summary>地形显示时，TerrainMaxLayer属性（允许显示地形的最大层号）的瓦片分切分数(缺省16)，瓦片层每小1，切分数*2</summary>
        public int TerrainMaxLayerSliceCount = 16;

        //地球控制参数初始化
        public STRUCT_EarthPara earthpara = new STRUCT_EarthPara()
        {
            mapType = (int)EMapType.卫星, 
            isShowOverlay = true, 
            background= Helpler.ColorToUInt(System.Windows.Media.Colors.Black),
            SceneMode= ESceneMode.地球,
            InputCoordinate= EInputCoordinate.WGS84球面坐标,
            AdjustAspect=1.3,
            ArrowSpan=0.1
        };


        ///<summary>地图类型</summary>
        public EMapType mapType
        {
            get { return earth.earthManager.earthpara.mapType; }
            set
            {
                if (earth.earthManager.earthpara.mapType != value)
                {
                    earth.earthManager.earthpara.mapType = value;
                    if (value == EMapType.卫星)
                        earth.earthManager.earthpara.isShowOverlay = true;
                    else
                        earth.earthManager.earthpara.isShowOverlay = false;
                    earth.earthManager.updateEarthPara();
                    earth.refreshColor();
                    earth.global.isUpdate = true;
                }
            }
        }

        #endregion

        public void updateEarthPara()
        {
            //传实例参数
            IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(earthpara));
            Marshal.StructureToPtr(earthpara, ipPara, false);
            D3DManager.ChangeProperty(earth.earthkey,(int)EModelType.地图,(int)EPropertyType.参数 , 0, 0, ipPara, 0,IntPtr.Zero,0);
            Marshal.FreeCoTaskMem(ipPara);
            //传map ip地址
            IntPtr ipLabel = Marshal.StringToCoTaskMemUni(Config.MapIP); //根据瓦片读取模式传IP或path
            D3DManager.ChangeProperty(earth.earthkey, (int)EModelType.地图, (int)EPropertyType.地址, 0, 0, ipLabel, 0, IntPtr.Zero, 0);
            Marshal.FreeCoTaskMem(ipLabel);

            //传路径2 overlay
            if (earthpara.tileReadMode == ETileReadMode.自定义文件瓦片 || earthpara.tileReadMode == ETileReadMode.自定义Web瓦片)
            {
                ipLabel = Marshal.StringToCoTaskMemUni(Config.MapPath); //路径
                D3DManager.ChangeProperty(earth.earthkey, (int)EModelType.地图, (int)EPropertyType.路径, 0, 0, ipLabel, 0, IntPtr.Zero, 0);
                Marshal.FreeCoTaskMem(ipLabel);
                ipLabel = Marshal.StringToCoTaskMemUni(Config.MapPath2); //路径2, overlay
                D3DManager.ChangeProperty(earth.earthkey, (int)EModelType.地图, (int)EPropertyType.路径2, 0, 0, ipLabel, 0, IntPtr.Zero, 0);
                Marshal.FreeCoTaskMem(ipLabel);
            }
        }


        BackgroundWorker bworker = new BackgroundWorker();

        internal bool isShowEarth = true;


        ///<summary>后台扫描和预计算</summary>
        void bworker_DoWork(object sender, DoWorkEventArgs e)
        {
            //earth.camera.cameraFrustum = new BoundingFrustum(earth.camera.view * Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4 * 1.2f, (float)earth.global.ScreenWidth / earth.global.ScreenHeight, earth.camera.Near, earth.camera.Far));
            earth.camera.cameraFrustum = new BoundingFrustum(earth.camera.view * earth.camera.projection);
            earth.global.maxlayer = 0;
            earth.global.maxlayertileinfo = "";

            scan(datas);

            int result = D3DManager.BeginTransfer(earth.earthkey);
            if (result != 0)
            {
                update(datas);
                D3DManager.EndTransfer(earth.earthkey);
            }

        }

        void bworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            calStatus = ECalStatus.完成;
            
            if (isMore)
            {
                earth.global.isUpdate = true;
                isMore = false;
            }
            else
                earth.global.isUpdate = false;
        }

        bool isMore = false;
        ///<summary>刷新地图</summary>
        public void refreshEarth()
        {
            earth.global.isUpdate = true;
            if (calStatus != ECalStatus.空闲)  //本次刷新后，再多一次刷新
                isMore = true;
        }

        internal ECalStatus calStatus = ECalStatus.空闲;
        ///<summary>计算并更新数据</summary>
        internal void updateEarth()
        {
            //地球
            if (isShowEarth)
            {
                if (earth.global.isUpdate && calStatus == ECalStatus.空闲)
                {
                    calStatus = ECalStatus.计算;
                    bworker.RunWorkerAsync();
                }
                if (calStatus == ECalStatus.完成)
                {
                    //update(datas);
                    calStatus = ECalStatus.空闲;
                }
            }
        }


        #region 地球瓦片相关

        internal EarthData datas;  //地球弧面可视化节点树
        bool isCanUpdate = true;


        public delegate void UpdateModelDelegate();
        public UpdateModelDelegate updatemodel { get; set; }


        public void initEarth()
        {

            if (isCanUpdate)
            {

                //MapHelper.maxTerrainLayer = 0;
                isCanUpdate = false;

                //global.cameraFrustum = new BoundingFrustum(global.camera.view * Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4 * 1.2f, (float)global.ScreenWidth / global.ScreenHeight, 0.1f, 100f));
                //scan(datas);
                //update(datas);


                //if (updatemodel != null)
                    //updatemodel();

                isCanUpdate = true;

            }
        }



        ///<summary>迭代遍历刷新，设置操作指令和状态</summary>
        private void scan(EarthData node)
        {


            if (earth.global.maxlayer < node.layer) { earth.global.maxlayer = node.layer; earth.global.maxlayertileinfo = string.Format("{0}-{1}-{2}",node.layer,node.idxx,node.idxy); }

            node.handled = false;
            node.prehandled = false;
            node.tileStatus = ETileStatus.不可见;
            node.isShowTerrain = false;
            node.info = "0";
            //Rect rectViewport = new Rect(-1000, -1000, earth.global.ScreenWidth + 2000, earth.global.ScreenHeight + 2000);
            Rect rectViewport = new Rect(0, 0, earth.global.ScreenWidth, earth.global.ScreenHeight);
            double LimitWH = 400;

            if ((earth.earthManager.mapType == EMapType.卫星 && node.layer + 1 <= Config.satmaxLayer) || (earth.earthManager.mapType != EMapType.卫星 && node.layer + 1 <= Config.roadmaxLayer))//下一级不大于允许最大级
            {
                node.info = "1";
                //===判断可见性
                //int ycount = (int)Math.Pow(2, node.layer);
                //double angle = 360.0 / ycount;



                if ((node.layer < (earthpara.SceneMode== ESceneMode.地球? 3:earthpara.StartLayer) || isFaceAndIntersect(node)))   //检测是否面向相机或层级小于2且与视锥体相交
                {
                    //首先考虑换一种验证大小面积的方法，用屏幕转换为地面，与块对比，这种方法与现用的块转换屏幕相反，（块转换屏幕在斜视下不可得，因为块平面与相机近平面相交了）

                    //新判断方式，以瓦片的实际经纬与屏幕映射相比较来判定
                    if (node.range.Contains(earth.camera.curCamearaViewRange.rect) )  //块包括屏幕
                    {
                        node.setExpand();
                        foreach (EarthData item in node.subCurve)
                            scan(item);
                    }
                    else if (node.range.IntersectsWith(earth.camera.curCamearaViewRange.rect)) //块与屏幕相交
                    {
                        bool handle=false;
                        foreach (var rec in earth.camera.curCamearaViewRange.rects)
                        {
                            if (node.range.IntersectsWith(rec))
                            {
                                if (node.range.Width>0.3*rec.Width)  //zh注：以后可以优化，通过预先计算纬度带的屏高，以计算面积来判定是否分拆
                                {
                                    node.setExpand();
                                    foreach (EarthData item in node.subCurve)
                                        scan(item);
                                }
                                else
                                {
                                    node.setFold();
                                    node.isShowTerrain = true;  //地形注：只有可见才可以显示地形
                                }
                                handle = true;
                                break;
                            }
                        }
                        if (!handle) //指在矩形中的梯形的其余部分，未真正与屏幕相交
                            node.setFold();

                    }
                    else //不相交, 应不出现
                    { 
                        node.setFold();
                    }

                    #region ===== 旧的判断方式 =====
                    //node.info = "2";
                    //Rect rectsub = MapHelper.GetTileRect(node,earth.global);
                    //node.rect = rectsub;
                    //if (rectsub.Contains(rectViewport) || node.layer < (earthpara.SceneMode == ESceneMode.地球 ? 3 : earthpara.StartLayer)) //弧面包含屏幕, 转下一层级
                    //{
                    //    node.info = "3";
                    //    node.setExpand();
                    //    foreach (EarthData item in node.subCurve)
                    //        scan(item);
                    //}
                    //else
                    //{
                    //    node.info = "4";
                    //    if (rectViewport.IntersectsWith(rectsub)) //相交
                    //    {
                    //        node.info = "5";

                    //        if (rectsub.Width * rectsub.Height > LimitWH * LimitWH || rectsub.Width>1000 )//用面积判断是否切分
                    //        {
                    //            node.info = "6";
                    //            node.setExpand();
                    //            foreach (EarthData item in node.subCurve)
                    //                scan(item);
                    //        }
                    //        else
                    //        {
                    //            node.info = "7";
                    //            node.tileStatus = ETileStatus.部分可见;
                    //            node.isShowTerrain = true;
                    //            node.setFold();
                    //            node.texture = "box3";
                    //        }
                    //    }
                    //    else //不相交, 此，原本不应出现，三3D相交，2D不相交，计算上有问题，由于3D包围盒 
                    //    {
                    //        if (rectsub.Width > 5000)
                    //        {
                    //            node.setExpand();
                    //            foreach (EarthData item in node.subCurve)
                    //                scan(item);
                    //        }
                    //        else
                    //        {
                    //            node.info = "8";
                    //            node.tileStatus = ETileStatus.部分可见;
                    //            node.isShowTerrain = true;
                    //            //node.setDelete();
                    //            node.setFold();
                    //            node.texture = "box2";
                    //        }
                    //    }
                    //}

                    #endregion

                }
                else //不可见
                {
                    //node.setDelete();
                    node.setFold();


                    node.texture = "box";
                }
            }
            else //最小级
            {
                node.info = "a";
                //node.setDelete();
                node.setFold();
                node.texture = "box4";
            }



        }

  

        /// <summary>
        /// 遍历进行操作
        /// </summary>
        /// <param name="node"></param>
        private void update(EarthData node)  //遍历进行更新操作
        {
            while (node.subCurve.Count(p => !p.handled) > 0)
            {
                update(node.subCurve.First(p => !p.handled));
            }


            switch (node.operate)
            {
                case EOperate.节点模型删除:
                    //mapTiles.Remove(node.id);
                    //Earth.DelMapTile(node.layer,node.idxx,node.idxy);
                    node.parent.subCurve.Remove(node);
                    node.parent = null;
                    break;
                case EOperate.节点删除:
                    node.parent.subCurve.Remove(node);
                    node.parent = null;
                    break;
                case EOperate.模型删除:
                    //mapTiles.Remove(node.id);
                    //Earth.DelMapTile(node.layer,node.idxx,node.idxy);
                    node.curStatus = node.oprStatus;
                    break;
                case EOperate.模型加入:
                    //mapTiles.Add(node.id, updateTiles[node.id]);
                    {
                        if (node.mustModelStatus == EMeshStatus.三维)
                        {
                            int count = node.terrainHeigList.Count;
                            IntPtr ipHigh = Marshal.AllocCoTaskMem(Marshal.SizeOf(node.terrainHeigList[0]) * count);  //传递点序列结构数组指针
                            for (int i = 0; i < count; i++)
                                Marshal.StructureToPtr(node.terrainHeigList[i], (IntPtr)(ipHigh.ToInt32() + i * Marshal.SizeOf(node.terrainHeigList[i])), false);
                            D3DManager.AddMapTile(earth.earthkey, node.hashcode, node.layer, node.idxx, node.idxy, node.mustModelStatus == EMeshStatus.三维, node.terrainSliceCount, ipHigh); // 当isshowterrain为true且terrainspan与原值不同，将会重建地形mesh
                            Marshal.FreeCoTaskMem(ipHigh);
                        }
                        else
                            D3DManager.AddMapTile(earth.earthkey, node.hashcode, node.layer, node.idxx, node.idxy, false, 0, IntPtr.Zero);  //isShowTerrain为false时，不会更改地形相关数据，仅为开关控制
                    }
                    node.curStatus = node.oprStatus;
                    break;
                case EOperate.模型更新:
                    {
                        if (node.mustModelStatus == EMeshStatus.三维)
                        {
                            int count = node.terrainHeigList.Count;
                            IntPtr ipHigh = Marshal.AllocCoTaskMem(Marshal.SizeOf(node.terrainHeigList[0]) * count);  //传递点序列结构数组指针
                            for (int i = 0; i < count; i++)
                                Marshal.StructureToPtr(node.terrainHeigList[i], (IntPtr)(ipHigh.ToInt32() + i * Marshal.SizeOf(node.terrainHeigList[i])), false);
                            D3DManager.AddMapTile(earth.earthkey, node.hashcode, node.layer, node.idxx, node.idxy, node.mustModelStatus == EMeshStatus.三维, node.terrainSliceCount, ipHigh); // 当isshowterrain为true且terrainspan与原值不同，将会重建地形mesh
                            Marshal.FreeCoTaskMem(ipHigh);
                        }
                        else
                            D3DManager.AddMapTile(earth.earthkey, node.hashcode, node.layer, node.idxx, node.idxy, false, 0, IntPtr.Zero);  //isShowTerrain为false时，不会更改地形相关数据，仅为开关控制
                    }

                    //D3DManager.AddMapTile(earth.earthkey, node.hashcode, node.layer, node.idxx, node.idxy);  


                    //models.Remove(node.model);
                    //node.updateModel();
                    //models.Add(node.model);
                    //node.curStatus = node.oprStatus;
                    break;
                case EOperate.none:
                    node.curStatus = node.oprStatus;
                    break;
            }
            node.operate = EOperate.none;
            node.handled = true;




        }


        /// <summary>判断弧面的四角点是否有面向相机，并且与视锥体相交</summary>
        private bool isFaceAndIntersect(EarthData node)
        {

            Vector3 vecDir = earth.camera.cameraDirection - earth.camera.cameraPosition;
            Vector3[] normals = node.normals;
            bool isface = false;
            for (int i = 0; i < normals.Length; i++)
            {
                if (Vector3.Dot(normals[i], vecDir) < 0)
                {
                    isface = true;
                    break;
                }
            }

            if (isface)
            {
                //BoundingFrustum cameraFrustum = new BoundingFrustum(global.camera.view * global.camera.projection);
                //BoundingFrustum cameraFrustum = new BoundingFrustum(global.camera.view * Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4*1.2f, (float)global.ScreenWidth / global.ScreenHeight, 0.1f, 100f));
                //if (cameraFrustum.Intersects(node.boundingBox))
                if (earth.camera.cameraFrustum.Intersects(node.boundingBox))
                    return true;
            }




            return false;
        }

        #endregion


        ///<summary>返回场景中3D点在场景平面的坐标</summary>
        public  System.Windows.Point transformD3DToScreen(VECTOR3D point3d)
        {
            POINT p = D3DManager.TransformD3DToScreen(earth.earthkey, point3d);
            return new System.Windows.Point(p.x, p.y);
        }

        public VECTOR3D? transformScreenToD3D(System.Windows.Point point)
        {
            Vector3? result = Helpler.GetProjectPoint3D(new Vector2((float)point.X, (float)point.Y), earth.camera, earth.global.ScreenWidth, earth.global.ScreenHeight,0, earth.earthManager.earthpara);
            if (result == null)
                return null;
            else
            {
                Vector3 v =(Vector3)result;
                return new VECTOR3D(v.X, v.Y, v.Z);
            }
        }

    }
}
