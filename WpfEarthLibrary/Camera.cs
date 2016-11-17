using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Runtime.InteropServices;


namespace WpfEarthLibrary
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Camera
    {
        public Camera(Vector3 pos, Vector3 target, Vector3 up, Earth pearth)
        {
            earth = pearth;
            cameraPosition = pos;
            cameraLookat = target;
            cameraDirection = target - pos;
            cameraDirection.Normalize();
            cameraUp = up;
            calCameraByDirection();
            tmr.Interval = TimeSpan.FromMilliseconds(1000);
            tmr.Tick += new EventHandler(tmr_Tick);
        }


        Earth earth;

        internal Matrix view { get; set; }
        internal Matrix projection { get; set; }


        private EOperateMode _operateMode;
        ///<summary>用户操作模式,决定场景如何响应用户操作。当用户操作为轨迹球模式时，需指定轨迹球中心（traceBallCenter属性）</summary>
        public EOperateMode operateMode
        {
            get { return _operateMode; }
            set
            {
                _operateMode = value;
                if (value == EOperateMode.自由视角)
                    earth.Cursor = System.Windows.Input.Cursors.None;
                else
                    earth.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        ///<summary>当用户操作为轨迹球模式或平面模式时，需指定轨迹球中心（三维场景中的坐标）</summary>
        public System.Windows.Media.Media3D.Point3D traceBallCenter { get; set; }


        private float _moveDistance = 0.00005f;
        ///<summary>当用户操作为自由视角模式时，前后左右位移距离，缺省0.00005</summary>
        public float moveDistance
        {
            get { return _moveDistance; }
            set { _moveDistance = value; }
        }

        ///<summary>在屏幕右方的mousr移动操作超过这个比例后，转为X轴旋转操作，缺省0.9</summary>
        public double XRotationScale = 0.9;

        ///<summary>允许的相机离地最小距离，注意应与相机近平面距离相适应</summary>
        public float MinGroundDistance = 1;
        ///<summary>允许的相机离地最大距离，注意应与相机远平面距离相适应</summary>
        public float MaxGroundDistance = 10;


        Vector3 _position;
        public Vector3 cameraPosition
        {
            get { return _position; }
            set
            {
                _position = value;
                //_direction = _lookat - _position;
                //_direction.Normalize();
            }
        }
        Vector3 _lookat;
        public Vector3 cameraLookat
        {
            get { return _lookat; }
            set
            {
                _lookat = value;
                //_direction = _lookat - _position;
                //_direction.Normalize();
            }
        }
        //Vector3 _direction;
        public Vector3 cameraDirection;
        //{
        //    get { return _direction; }
        //    set
        //    {
        //        _direction = value;
        //        //_lookat = _position + _direction;
        //    }
        //}

        public Vector3 cameraUp { get; set; }
        public float FieldOfView = MathHelper.PiOver4;
        public float Near = 0.1f;
        public float Far = 10f;

        public BoundingFrustum cameraFrustum {get;set;} //计算用视锥体，一次遍历过程中，固定不变

        

        public void calCameraByDirection()
        {
            cameraDirection.Normalize();
            cameraLookat = cameraPosition + cameraDirection;

            view = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraDirection, cameraUp);
            projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, 1.0f * earth.global.ScreenWidth / earth.global.ScreenHeight, Near, Far);

        }
        public void calCameraByLookAt()
        {
            cameraDirection = _lookat - _position;
            cameraDirection.Normalize();

            view = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraDirection, cameraUp);
            projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, 1.0f * earth.global.ScreenWidth / earth.global.ScreenHeight, Near, Far);

        }



        BoundingSphere boundsphere = new BoundingSphere(new Vector3(0, 0, 0), Para.Radius);
        Plane plane = new Plane(new Vector3(0, 0, 1), 0);
        ///<summary>获取相机中心与地面的交点, crosspoint为交点, distance为相机与交点距离</summary>
        internal void getCrossGround(out Vector3? crosspoint, out float? distance)
        {
            Ray ray = new Ray(cameraPosition, cameraDirection);
            if (earth.earthManager.earthpara.SceneMode == ESceneMode.地球)
                distance = ray.Intersects(boundsphere);
            else
                distance = ray.Intersects(plane);
            cameraDirection.Normalize();
            if (distance == null)
                crosspoint = null;
            else
                crosspoint = cameraPosition + cameraDirection * distance;
        }
        ///<summary>获取相机中心与地面的交点, crosspoint为交点, distance为相机与交点距离</summary>
        public void getCrossGround(out VECTOR3D? crosspoint, out float? distance)
        {
            Vector3? vec3;
            getCrossGround(out vec3,out distance);
            if (vec3 == null)
                crosspoint = null;
            else
            {
                crosspoint = new VECTOR3D(((Vector3)vec3).X, ((Vector3)vec3).Y, ((Vector3)vec3).Z);
            }
        }


        /// <summary>
        /// 调整相机观察角
        /// </summary>
        /// <param name="angle">与地面垂直线的夹角, 有效范围0-60</param>
        public void adjustCameraAngle(float angle)
        {
            if (angle < 0 || angle > 60) return;
            //计算与地面交点
            Vector3? crosspnt;
            float? distance;
            getCrossGround(out crosspnt, out distance);
            if (crosspnt != null)
            {
                Vector3 cp = (Vector3)crosspnt;
                float dis = (float)distance;

                if (angle == 0) //调整为俯视
                {
                    Vector3 dir;
                    if (earth.earthManager.earthpara.SceneMode == ESceneMode.地球)
                    {
                        cameraLookat = new Vector3(0, 0, 0);
                        dir = new Vector3(cp.X, cp.Y, cp.Z);
                    }
                    else
                    {
                        cameraLookat = new Vector3(cp.X, cp.Y, 0);
                        dir = new Vector3(0, 0, 1);
                    }
                    dir.Normalize();
                    cameraPosition = cp + dir * dis;

                    cameraDirection = cameraLookat - cameraPosition;
                    cameraDirection.Normalize();
                }
                else//调整为斜视
                {
                    Vector3 dir;
                    Vector3 axis;
                    if (earth.earthManager.earthpara.SceneMode == ESceneMode.地球)
                    {
                        dir = new Vector3(cp.X, cp.Y, cp.Z);
                        dir.Normalize();
                        axis = Vector3.Cross(dir, new Vector3(0, 1, 0));
                        axis.Normalize();
                    }
                    else
                    {
                        dir = new Vector3(0, 0, 1);
                        axis =new Vector3(-1,0,0);
                        axis.Normalize();

                    }
                    Matrix matrix = Matrix.CreateFromAxisAngle(axis, -MathHelper.Pi * angle / 180);
                    dir = Vector3.Transform(dir, matrix);
                    dir.Normalize();
                    cameraLookat = new Vector3(cp.X, cp.Y, cp.Z);
                    cameraPosition = cp + dir * dis;

                    cameraDirection = cameraLookat - cameraPosition;
                    cameraDirection.Normalize();
                }
                calCameraByDirection();

                if (earth.IsLoaded)
                {
                    updateD3DCamera(true, 500);
                    earth.global.isUpdate = true;
                }
            }

        }

        /// <summary>
        /// 调整相机距离
        /// </summary>
        /// <param name="Distance">与地面目标的距离</param>
        public void adjustCameraDistance(float Distance)
        {
            //if (Distance < 1 || Distance > 100) return;
            //当前观察点
            Vector3? crosspnt;
            float? distance;
            getCrossGround(out crosspnt, out distance);
            if (crosspnt != null)
            {
                float dis = (float)distance;
                float div = dis - Distance;
                // 原写法 cameraPosition = cameraLookat + cameraDirection * div;
                cameraPosition = cameraPosition + cameraDirection * div;
                calCameraByDirection();
                updateD3DCamera(true, 500);
                earth.global.isUpdate = true;
            }


        }

        /// <summary>
        /// 自动调整相机距离以显示指定范围
        /// </summary>
        public void adjustCameraRange(System.Windows.Rect rect)
        {
            double minjd, maxjd, minwd, maxwd;
            minwd = rect.Left; maxwd = rect.Right; minjd = rect.Top; maxjd = rect.Bottom;
            List<VECTOR3D> corners = new List<VECTOR3D>();
            VECTOR3D lefttop, righttop, leftbottom, rightbottom, center;
            System.Windows.Media.Media3D.Vector3D lt, rt, lb, rb;
            lefttop = MapHelper.JWHToPoint(minjd, maxwd, 0,earth.earthManager.earthpara); corners.Add(lefttop); lt = Helpler.vecD3DToWpf(lefttop);
            righttop = MapHelper.JWHToPoint(maxjd, maxwd, 0, earth.earthManager.earthpara); corners.Add(righttop); rt = Helpler.vecD3DToWpf(righttop);
            leftbottom = MapHelper.JWHToPoint(minjd, minwd, 0, earth.earthManager.earthpara); corners.Add(leftbottom); lb = Helpler.vecD3DToWpf(leftbottom);
            rightbottom = MapHelper.JWHToPoint(maxjd, minwd, 0, earth.earthManager.earthpara); corners.Add(rightbottom); rb = Helpler.vecD3DToWpf(rightbottom);
            System.Windows.Media.Media3D.Vector3D cent = new System.Windows.Media.Media3D.Vector3D((corners.Max(p => p.x) + corners.Min(p => p.x)) / 2, (corners.Max(p => p.y) + corners.Min(p => p.y)) / 2, (corners.Max(p => p.z) + corners.Min(p => p.z)) / 2);
            if (earth.earthManager.earthpara.SceneMode == ESceneMode.地球)
            {
                cent = cent * Para.Radius / cent.Length;
                center = new VECTOR3D(cent.X, cent.Y, cent.Z);
            }
            else  //zh注：还未验证
            {
                center = new VECTOR3D(cent.X, cent.Y, 0);
            }
            double width, height;
            width = Math.Max((rt - lt).Length, (rb - lb).Length) * 1.5;
            height = Math.Max((lb - lt).Length, (rb - rt).Length) * 1.5;
            double distance;
            if (width / height > earth.global.ScreenWidth / earth.global.ScreenHeight) //以宽度来计算
            {
                distance = width * earth.global.ScreenHeight / earth.global.ScreenWidth / 2 / Math.Atan(FieldOfView / 2);
            }
            else //以高度来计算
            {
                distance = height / 2 / Math.Atan(FieldOfView / 2);
            }

            aniLook(center);
            adjustCameraDistance((float)distance);

        }


        private bool _isAutoUpdateMapModel = true;
        ///<summary>相机动画后，是否自动刷新地图和模型, 缺省true</summary>
        public bool isAutoUpdateMapModel
        {
            get { return _isAutoUpdateMapModel; }
            set { _isAutoUpdateMapModel = value; }
        }


        ///<summary>更新D3D相机, isAni是否以动画方式更新相机，duraion动画时长毫秒, isImmediateRefreshEarth是否立即刷目标位置地图</summary>
        public void updateD3DCamera(bool isAni = false, int duration = 500, bool isImmediateRefreshEarth=true)
        {
            STRUCT_Camera para = new STRUCT_Camera() { far = Far, near = Near, fieldofview = FieldOfView };
            para.lookat = new VECTOR3D(cameraLookat.X, cameraLookat.Y, cameraLookat.Z);
            para.pos = new VECTOR3D(cameraPosition.X, cameraPosition.Y, cameraPosition.Z);
            para.up = new VECTOR3D(cameraUp.X, cameraUp.Y, cameraUp.Z);
            para.direction = new VECTOR3D(cameraDirection.X, cameraDirection.Y, cameraDirection.Z);

            IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(para));
            Marshal.StructureToPtr(para, ipPara, false);

            D3DManager.ChangeCameraPara(earth.earthkey, ipPara, isAni, duration);
            Marshal.FreeCoTaskMem(ipPara);


            

            if (duration > 0 && _isAutoUpdateMapModel && operateMode== EOperateMode.地图模式)
            {
                if (isImmediateRefreshEarth)
                    earth.earthManager.refreshEarth();
                tmr.Interval = TimeSpan.FromMilliseconds(duration);
                tmr.Start();
            }
        }


        ///<summary>动画移动相机查看指定位置,按内部三维坐标, timerFactor: 速度系数, 为0则无动画直接刷新</summary>
        public void aniLook(VECTOR3D vecLocation, double speedFactor = 1)
        {

            //计算当前与地面夹角
            System.Windows.Media.Media3D.Vector3D v1, v2, v3, v4, v5;

            Vector3 vec = new Vector3(vecLocation.x, vecLocation.y, vecLocation.z);
            if (earth.earthManager.earthpara.SceneMode == ESceneMode.地球)
                vec = vec / vec.Length() * Para.Radius; //换算到地表高度

            //当前观察点
            Vector3? crosspnt;
            float? distance;
            float height;
            getCrossGround(out crosspnt, out distance);
            if (crosspnt != null)
            {
                float dis = (float)distance;
                Vector3 cp = (Vector3)crosspnt;

                if (earth.earthManager.earthpara.SceneMode == ESceneMode.地球)
                {
                    v1 = new System.Windows.Media.Media3D.Vector3D(cp.X, cp.Y, cp.Z);
                    v2 = new System.Windows.Media.Media3D.Vector3D(cameraUp.X, cameraUp.Y, cameraUp.Z);
                    v3 = System.Windows.Media.Media3D.Vector3D.CrossProduct(v1, v2);
                    v4 = System.Windows.Media.Media3D.Vector3D.CrossProduct(v3, v1);
                    v5 = new System.Windows.Media.Media3D.Vector3D(cameraDirection.X, cameraDirection.Y, cameraDirection.Z);
                    float angle = (float)System.Windows.Media.Media3D.Vector3D.AngleBetween(v5, v4);
                    height = (new Vector3(cameraPosition.X, cameraPosition.Y, cameraPosition.Z)).Length() - Para.Radius;

                    Vector3 dir = vec;
                    dir.Normalize();
                    Vector3 axis = Vector3.Cross(dir, cameraUp);
                    axis.Normalize();
                    Matrix matrix = Matrix.CreateFromAxisAngle(axis, -MathHelper.Pi * (90.0f - angle) / 180.0f);
                    dir = Vector3.Transform(dir, matrix);
                    dir.Normalize();
                    cameraLookat = vec;
                    cameraPosition = cameraLookat + dir * dis;

                    cameraDirection = cameraLookat - cameraPosition;
                    cameraDirection.Normalize();
                }
                else
                {
                    Matrix matrix = Matrix.CreateTranslation(vecLocation.x - cp.X, vecLocation.y - cp.Y, 0);
                    cameraPosition = Vector3.Transform(cameraPosition, matrix);
                    height = cameraPosition.Z;
                }


                if (speedFactor == 0)
                {
                    calCameraByDirection();
                    updateD3DCamera();
                    earth.global.isUpdate = true;
                }
                else
                {
                    int time = (int)((cp - cameraLookat).Length() / height / speedFactor * 100); //时长，与高度反比，和速度系数反比 

                    calCameraByDirection();

                    updateD3DCamera(true, time);

                }
            }
        }


        ///<summary>动画移动相机查看指定位置, 按原始坐标, yjd为原始坐标的Y值, timerFactor: 速度系数, 为0则无动画直接刷新</summary>
        public void aniLook(double yjd, double xwd, double gd, double speedFactor = 1)
        {
            System.Windows.Point pnt = new System.Windows.Point(xwd, yjd);
            if (earth.coordinateManager.Enable)
            {
                pnt = earth.coordinateManager.transToInner(pnt);  //若启用了坐标转换
                yjd = pnt.Y;
                xwd = pnt.X;
            }
            Vector3 tmp = Helpler.JWHToPoint(yjd, xwd, gd, earth.earthManager.earthpara);
            aniLook(new VECTOR3D(tmp.X, tmp.Y, tmp.Z), speedFactor);
        }
        ///<summary>动画移动相机查看指定位置, 按经纬坐标, timerFactor: 速度系数, 为0则无动画直接刷新</summary>
        public void aniLookGeo(double yjd, double xwd, double gd, double speedFactor = 1)
        {
            Vector3 tmp = Helpler.JWHToPoint(yjd, xwd, gd, earth.earthManager.earthpara);
            aniLook(new VECTOR3D(tmp.X, tmp.Y, tmp.Z), speedFactor);
        }

        ///<summary>初始化相机到指定位置</summary>
        public void initCamera(double jd, double wd, double gd)
        {
            if (earth.coordinateManager.Enable)
            {
                //System.Windows.Point pnt = earth.coordinateManager.transToInner(new System.Windows.Point(wd, jd)); //若激活了坐标转换，转换为内部坐标
                System.Windows.Point pnt = earth.coordinateManager.transToInner(new System.Windows.Point(jd, wd)); //若激活了坐标转换，转换为内部坐标
                jd = pnt.Y; wd = pnt.X;
            }

            cameraPosition = Helpler.JWHToPoint(jd, wd, gd,earth.earthManager.earthpara);
            if (earth.earthManager.earthpara.SceneMode== ESceneMode.地球)
                cameraLookat = new Vector3(0, 0, 0);
            else
                cameraLookat = Helpler.JWHToPoint(jd, wd, 0, earth.earthManager.earthpara);

            cameraDirection = cameraLookat - cameraPosition;
            cameraDirection.Normalize();
            calCameraByDirection();
        }

        void tmr_Tick(object sender, EventArgs e)
        {
            earth.earthManager.refreshEarth();
            earth.UpdateModel();
                tmr.Stop();
        }


        System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer();


        ///<summary>获取屏幕中心外部坐标</summary>
        public System.Windows.Point? getScreenCenter()
        {
            return getOuterCoordinate(1.0f * earth.global.ScreenWidth / 2, 1.0f * earth.global.ScreenHeight / 2);
        }
        ///<summary>获取屏幕中心经纬坐标，返回，x纬度, y经度</summary>
        public System.Windows.Point? getScreenCenterGeo()
        {
            return getGeoCoordinate(1.0f * earth.global.ScreenWidth / 2, 1.0f * earth.global.ScreenHeight / 2);
        }

        ///<summary>获取指定点外部坐标</summary>
        public System.Windows.Point? getOuterCoordinate(float x, float y)
        {
            System.Windows.Point pnt;
            System.Windows.Point? tmp = getGeoCoordinate(x, y);
            if (tmp == null) return null;
            pnt = (System.Windows.Point)tmp;
            if (earth.coordinateManager.Enable) pnt = earth.coordinateManager.transToOuter(pnt);  //若启用了坐标转换
            return pnt;
        }
        ///<summary>获取指定屏幕点经纬坐标</summary>
        public System.Windows.Point? getGeoCoordinate(float x, float y)
        {
            Vector3? vectmp = Helpler.GetProjectPoint3D(new Vector2(x, y), this, earth.global.ScreenWidth, earth.global.ScreenHeight,0, earth.earthManager.earthpara);
            if (vectmp == null)
                return null;
            else
            {
                Vector3 vec = (Vector3)vectmp;
                System.Windows.Media.Media3D.Point3D jwh = Helpler.PointToJWH(new System.Windows.Media.Media3D.Point3D(vec.X, vec.Y, vec.Z), earth.earthManager.earthpara);
                System.Windows.Point pnt = new System.Windows.Point(jwh.Y, jwh.X);
                return pnt;
            }
        }


        #region 经纬范围相关
        ///<summary>当前相机可视范围经纬度范围</summary>
        public ViewRange curCamearaViewRange
        {
            get
            {

                ViewRange viewbox = new ViewRange();
                //zh注：暂假定全可相交球面，仅北半球有效
                Vector3 vec;
                System.Windows.Media.Media3D.Point3D jwh;
                //底部中央, 获取纬度起始
                vec = (Vector3)Helpler.GetProjectPoint3D(new Vector2(1.0f * earth.global.ScreenWidth / 2, earth.global.ScreenHeight), this, earth.global.ScreenWidth, earth.global.ScreenHeight,0, earth.earthManager.earthpara);
                jwh = Helpler.PointToJWH(new System.Windows.Media.Media3D.Point3D(vec.X, vec.Y, vec.Z), earth.earthManager.earthpara);
                viewbox.latitudeStart = (float)jwh.Y;
                if (earth.coordinateManager.Enable) jwh = earth.coordinateManager.transToOuter(jwh);  //若启用了坐标转换
                viewbox.yStart = (float)jwh.Y;
                //顶部左端，获取纬度终止, 获取远端经度起始
                vec = (Vector3)Helpler.GetProjectPoint3D(new Vector2(0, 0), this, earth.global.ScreenWidth, earth.global.ScreenHeight,0, earth.earthManager.earthpara);
                jwh = Helpler.PointToJWH(new System.Windows.Media.Media3D.Point3D(vec.X, vec.Y, vec.Z), earth.earthManager.earthpara);
                viewbox.latitudeEnd = (float)jwh.Y;

                if (earth.earthManager.TerrainAllow)//下端纬度扩展1/10，适应地形情况
                    viewbox.latitudeStart = viewbox.latitudeStart - (viewbox.latitudeEnd - viewbox.latitudeStart) / 10;  

                viewbox.farLongitudeStart = (float)jwh.X;
                if (earth.coordinateManager.Enable) jwh = earth.coordinateManager.transToOuter(jwh);  //若启用了坐标转换
                viewbox.yEnd = (float)jwh.Y;
                viewbox.farXStart = (float)jwh.X;
                //顶部右端，获取远端经度终止
                vec = (Vector3)Helpler.GetProjectPoint3D(new Vector2(earth.global.ScreenWidth, 0), this, earth.global.ScreenWidth, earth.global.ScreenHeight,0, earth.earthManager.earthpara);
                jwh = Helpler.PointToJWH(new System.Windows.Media.Media3D.Point3D(vec.X, vec.Y, vec.Z), earth.earthManager.earthpara);
                viewbox.farLongitudeEnd = (float)jwh.X;
                if (earth.coordinateManager.Enable) jwh = earth.coordinateManager.transToOuter(jwh);  //若启用了坐标转换
                viewbox.farXEnd = (float)jwh.X;
                //底部左端，获取近端经度起始
                vec = (Vector3)Helpler.GetProjectPoint3D(new Vector2(0, earth.global.ScreenHeight), this, earth.global.ScreenWidth, earth.global.ScreenHeight,0, earth.earthManager.earthpara);
                jwh = Helpler.PointToJWH(new System.Windows.Media.Media3D.Point3D(vec.X, vec.Y, vec.Z), earth.earthManager.earthpara);
                viewbox.nearLongitudeStart = (float)jwh.X;
                if (earth.coordinateManager.Enable) jwh = earth.coordinateManager.transToOuter(jwh);  //若启用了坐标转换
                viewbox.nearXStart = (float)jwh.X;
                //底部右端，获取近端经度终止
                vec = (Vector3)Helpler.GetProjectPoint3D(new Vector2(earth.global.ScreenWidth, earth.global.ScreenHeight), this, earth.global.ScreenWidth, earth.global.ScreenHeight,0, earth.earthManager.earthpara);
                jwh = Helpler.PointToJWH(new System.Windows.Media.Media3D.Point3D(vec.X, vec.Y, vec.Z), earth.earthManager.earthpara);
                viewbox.nearLongitudeEnd = (float)jwh.X;
                if (earth.coordinateManager.Enable) jwh = earth.coordinateManager.transToOuter(jwh);  //若启用了坐标转换
                viewbox.nearXEnd = (float)jwh.X;
                
                viewbox.calRanges(earth.coordinateManager.isXAsLong);
                viewbox.calRects(true);

                if (earth.coordinateManager.Enable)
                {
                    viewbox.calRangesOuter(earth.coordinateManager.isXAsLong);
                    viewbox.calRectsOuter(true);
                }
                else
                {
                    viewbox.rangesOuter = viewbox.ranges;
                    viewbox.rectsOuter = viewbox.rects;
                }

               //若x对应经度，交换xywh, range计算中，按x对应纬度进行的


                return viewbox;
            }
        }


        ///<summary>当前相机离地面距离</summary>
        public float curCameraDistanceToGround
        {
            get
            {
                Vector3? vec3;
                float? distance;
                //getCrossGround(out vec3, out distance);// 相机与地面相交距离
                distance = (earth.earthManager.earthpara.SceneMode == ESceneMode.地球) ? cameraPosition.Length() - Para.Radius : cameraPosition.Z;

                return (float)distance;  //zh注：假定一定与球面相交
            }
        }
        ///<summary>当前相机离屏幕中心点地面的距离</summary>
        public float curCameraDistanceToScreenCenterGround
        {
            get
            {
                Vector3? vec3;
                float? distance;
                getCrossGround(out vec3, out distance);// 相机与地面相交距离
                return (float)distance;  //zh注：假定一定与球面相交
            }
        }

        ///<summary>当前相机与地心线夹角(角度值)</summary>
        public float curCameraAngle
        {
            get
            {
                if (earth.earthManager.earthpara.SceneMode == ESceneMode.地球)
                {
                    System.Windows.Media.Media3D.Vector3D v1, v2;
                    v1 = new System.Windows.Media.Media3D.Vector3D(-cameraPosition.X, -cameraPosition.Y, -cameraPosition.Z);
                    v1.Normalize();
                    v2 = new System.Windows.Media.Media3D.Vector3D(cameraDirection.X, cameraDirection.Y, cameraDirection.Z);
                    return (float)System.Windows.Media.Media3D.Vector3D.AngleBetween(v1, v2);
                }
                else
                {
                    System.Windows.Media.Media3D.Vector3D v1, v2;
                    v1 = new System.Windows.Media.Media3D.Vector3D(0, 0, -1);
                    v2 = new System.Windows.Media.Media3D.Vector3D(cameraDirection.X, cameraDirection.Y, cameraDirection.Z);
                    return (float)System.Windows.Media.Media3D.Vector3D.AngleBetween(v1, v2);
                }
            }
        }


        #endregion

        public struct ViewRange
        {
            //  内部使用经纬坐标
            ///<summary>近端经度起始</summary>
            public float nearLongitudeStart { get; set; }
            ///<summary>近端经度终止</summary>
            public float nearLongitudeEnd { get; set; }
            ///<summary>远端经度起始</summary>
            public float farLongitudeStart { get; set; }
            ///<summary>远端经度终止</summary>
            public float farLongitudeEnd { get; set; }
            ///<summary>纬度开始</summary>
            public float latitudeStart { get; set; }
            ///<summary>纬度终止</summary>
            public float latitudeEnd { get; set; }

            //  外部坐标
            ///<summary>近端横坐标起始</summary>
            public float nearXStart { get; set; }
            ///<summary>近端横坐标终止</summary>
            public float nearXEnd { get; set; }
            ///<summary>远端横坐标起始</summary>
            public float farXStart { get; set; }
            ///<summary>远端横坐标终止</summary>
            public float farXEnd { get; set; }
            ///<summary>纵坐标开始</summary>
            public float yStart { get; set; }
            ///<summary>纵坐标终止</summary>
            public float yEnd { get; set; }



            ///<summary>扩展expandscale后经纬范围列表，y经，x纬</summary>
            public List<System.Windows.Rect> ranges;

            ///<summary>扩展expandscale后经纬范围列表，外部坐标</summary>
            public List<System.Windows.Rect> rangesOuter;

            ///<summary>计算扩展expandscale后的范围</summary>
            internal void calRanges(bool isXAsLong)
            {
                if (nearLongitudeStart > nearLongitudeEnd) { float tmp = nearLongitudeEnd; nearLongitudeEnd = nearLongitudeStart; nearLongitudeStart = tmp; }
                if (farLongitudeStart > farLongitudeEnd) { float tmp = farLongitudeEnd; farLongitudeEnd = farLongitudeStart; farLongitudeStart = tmp; }
                if (latitudeStart > latitudeEnd) { float tmp = latitudeEnd; latitudeEnd = latitudeStart; latitudeStart = tmp; }


                float farLongLen = farLongitudeEnd - farLongitudeStart;
                float nearLongLen = nearLongitudeEnd - nearLongitudeStart;
                float latiLen = latitudeEnd - latitudeStart;
                float expandscale =  0.25f;

                float nearLongitudeStart2 = nearLongitudeStart - nearLongLen * expandscale;
                float nearLongitudeEnd2 = nearLongitudeEnd + nearLongLen * expandscale;
                float farLongitudeStart2 = farLongitudeStart - farLongLen * expandscale;
                float farLongitudeEnd2 = farLongitudeEnd + farLongLen * expandscale;
                float latitudeStart2 = latitudeStart - latiLen * expandscale;
                float latitudeEnd2 = latitudeEnd + latiLen * expandscale;


                ranges = new List<System.Windows.Rect>();
                int power = (int)((farLongitudeEnd - farLongitudeStart) / (nearLongitudeEnd - nearLongitudeStart));
                power = power < 1 ? 1 : power;
                power = power > 10 ? 10 : power;

                for (int i = 0; i < power; i++)
                {
                    System.Windows.Rect rect = new System.Windows.Rect();

                    if (isXAsLong)               //若x对应经度，交换xywh, range计算中，按x对应纬度进行的
                    {
                        rect.X = farLongitudeStart2 + (power - i - 1) * (nearLongitudeStart2 - farLongitudeStart2) / power;
                        rect.Y = latitudeStart2 + i * (latitudeEnd2 - latitudeStart2) / power;
                        rect.Width = (nearLongitudeEnd2 - nearLongitudeStart2) + (i + 1) * ((farLongitudeEnd2 - farLongitudeStart2) - (nearLongitudeEnd2 - nearLongitudeStart2)) / power;
                        rect.Height = (latitudeEnd2 - latitudeStart2) / power;
                        ranges.Add(rect);
                    }
                    else
                    {
                        rect.Y = farLongitudeStart2 + (power - i - 1) * (nearLongitudeStart2 - farLongitudeStart2) / power;
                        rect.X = latitudeStart2 + i * (latitudeEnd2 - latitudeStart2) / power;
                        rect.Height = (nearLongitudeEnd2 - nearLongitudeStart2) + (i + 1) * ((farLongitudeEnd2 - farLongitudeStart2) - (nearLongitudeEnd2 - nearLongitudeStart2)) / power;
                        rect.Width = (latitudeEnd2 - latitudeStart2) / power;
                        ranges.Add(rect);
                    }
                }

            }
            ///<summary>计算扩展expandscale后的范围，外部坐标</summary>
            internal void calRangesOuter(bool isXAsLong)
            {
                if (nearXStart > nearXEnd) { float tmp = nearXEnd; nearXEnd = nearXStart; nearXStart = tmp; }
                if (farXStart > farXEnd) { float tmp = farXEnd; farXEnd = farXStart; farXStart = tmp; }
                if (yStart > yEnd) { float tmp = yEnd; yEnd = yStart; yStart = tmp; }


                float farLongLen = farXEnd - farXStart;
                float nearLongLen = nearXEnd - nearXStart;
                float latiLen = yEnd - yStart;
                float expandscale = 0.25f;

                float nearXStart2 = nearXStart - nearLongLen * expandscale;
                float nearXEnd2 = nearXEnd + nearLongLen * expandscale;
                float farXStart2 = farXStart - farLongLen * expandscale;
                float farXEnd2 = farXEnd + farLongLen * expandscale;
                float yStart2 = yStart - latiLen * expandscale;
                float yEnd2 = yEnd + latiLen * expandscale;


                rangesOuter = new List<System.Windows.Rect>();
                int power = (int)((farXEnd - farXStart) / (nearXEnd - nearXStart));
                power = power < 1 ? 1 : power;
                power = power > 10 ? 10 : power;

                for (int i = 0; i < power; i++)
                {
                    System.Windows.Rect rect = new System.Windows.Rect();

                    if (isXAsLong)               //若x对应经度，交换xywh, range计算中，按x对应纬度进行的
                    {
                        rect.X = farXStart2 + (power - i - 1) * (nearXStart2 - farXStart2) / power;
                        rect.Y = yStart2 + i * (yEnd2 - yStart2) / power;
                        rect.Width = (nearXEnd2 - nearXStart2) + (i + 1) * ((farXEnd2 - farXStart2) - (nearXEnd2 - nearXStart2)) / power;
                        rect.Height = (yEnd2 - yStart2) / power;
                        rangesOuter.Add(rect);
                    }
                    else
                    {
                        rect.Y = farXStart2 + (power - i - 1) * (nearXStart2 - farXStart2) / power;
                        rect.X = yStart2 + i * (yEnd2 - yStart2) / power;
                        rect.Height = (nearXEnd2 - nearXStart2) + (i + 1) * ((farXEnd2 - farXStart2) - (nearXEnd2 - nearXStart2)) / power;
                        rect.Width = (yEnd2 - yStart2) / power;
                        rangesOuter.Add(rect);
                    }
                }

                 //if (isXAsLong)               //若x对应经度，交换xywh, range计算中，按x对应纬度进行的
                 //    rectOuter= new System.Windows.Rect(farXStart, yStart, farXEnd - farXStart, yEnd - yStart);
                 //else
                 //    rectOuter = new System.Windows.Rect(yStart, farXStart, yEnd - yStart, farXEnd - farXStart);

            }

            ///<summary>屏幕范围最大经纬形成的矩形，x经y纬, 仅用于地图瓦片</summary>
            public System.Windows.Rect rect 
            {
                get
                {
                    return new System.Windows.Rect(farLongitudeStart,latitudeStart,farLongitudeEnd-farLongitudeStart,latitudeEnd-latitudeStart);
                }
            }

            /////<summary>屏幕范围最大横纵坐标形成的矩形，外面坐标</summary>
            //public System.Windows.Rect rectOuter { get; set; }
            ////{
            ////    get
            ////    {
            ////        return new System.Windows.Rect(farXStart, yStart, farXEnd - farXStart, yEnd - yStart);
            ////    }
            ////}


            ///<summary>屏幕范围经纬范围列表，用于瓦片，x经，y纬</summary>
            public List<System.Windows.Rect> rects;
            ///<summary>屏幕范围经纬范围列表，外部坐标</summary>
            public List<System.Windows.Rect> rectsOuter;

            ///<summary>计算屏幕范围经纬范围列表</summary>
            internal void calRects(bool isXAsLong)
            {
                if (nearLongitudeStart > nearLongitudeEnd) { float tmp = nearLongitudeEnd; nearLongitudeEnd = nearLongitudeStart; nearLongitudeStart = tmp; }
                if (farLongitudeStart > farLongitudeEnd) { float tmp = farLongitudeEnd; farLongitudeEnd = farLongitudeStart; farLongitudeStart = tmp; }
                if (latitudeStart > latitudeEnd) { float tmp = latitudeEnd; latitudeEnd = latitudeStart; latitudeStart = tmp; }


                float farLongLen = farLongitudeEnd - farLongitudeStart;
                float nearLongLen = nearLongitudeEnd - nearLongitudeStart;
                float latiLen = latitudeEnd - latitudeStart;
                float expandscale = 0f;

                float nearLongitudeStart2 = nearLongitudeStart - nearLongLen * expandscale;
                float nearLongitudeEnd2 = nearLongitudeEnd + nearLongLen * expandscale;
                float farLongitudeStart2 = farLongitudeStart - farLongLen * expandscale;
                float farLongitudeEnd2 = farLongitudeEnd + farLongLen * expandscale;
                float latitudeStart2 = latitudeStart - latiLen * expandscale;
                float latitudeEnd2 = latitudeEnd + latiLen * expandscale;


                rects = new List<System.Windows.Rect>();
                int power = (int)((farLongitudeEnd - farLongitudeStart) / (nearLongitudeEnd - nearLongitudeStart));
                power = power < 1 ? 1 : power;
                power = power > 10 ? 10 : power;

                for (int i = 0; i < power; i++)
                {
                    System.Windows.Rect rect = new System.Windows.Rect();

                    if (isXAsLong)               //若x对应经度，交换xywh, range计算中，按x对应纬度进行的
                    {
                        rect.X = farLongitudeStart2 + (power - i - 1) * (nearLongitudeStart2 - farLongitudeStart2) / power;
                        rect.Y = latitudeStart2 + i * (latitudeEnd2 - latitudeStart2) / power;
                        rect.Width = (nearLongitudeEnd2 - nearLongitudeStart2) + (i + 1) * ((farLongitudeEnd2 - farLongitudeStart2) - (nearLongitudeEnd2 - nearLongitudeStart2)) / power;
                        rect.Height = (latitudeEnd2 - latitudeStart2) / power;
                        rects.Add(rect);
                    }
                    else
                    {
                        rect.Y = farLongitudeStart2 + (power - i - 1) * (nearLongitudeStart2 - farLongitudeStart2) / power;
                        rect.X = latitudeStart2 + i * (latitudeEnd2 - latitudeStart2) / power;
                        rect.Height = (nearLongitudeEnd2 - nearLongitudeStart2) + (i + 1) * ((farLongitudeEnd2 - farLongitudeStart2) - (nearLongitudeEnd2 - nearLongitudeStart2)) / power;
                        rect.Width = (latitudeEnd2 - latitudeStart2) / power;
                        rects.Add(rect);
                    }
                }

            }

            ///<summary>计算屏幕范围经纬范围列表，外部坐标</summary>
            internal void calRectsOuter(bool isXAsLong)
            {
                if (nearXStart > nearXEnd) { float tmp = nearXEnd; nearXEnd = nearXStart; nearXStart = tmp; }
                if (farXStart > farXEnd) { float tmp = farXEnd; farXEnd = farXStart; farXStart = tmp; }
                if (yStart > yEnd) { float tmp = yEnd; yEnd = yStart; yStart = tmp; }


                float farLongLen = farXEnd - farXStart;
                float nearLongLen = nearXEnd - nearXStart;
                float latiLen = yEnd - yStart;
                float expandscale = 0f;

                float nearXStart2 = nearXStart - nearLongLen * expandscale;
                float nearXEnd2 = nearXEnd + nearLongLen * expandscale;
                float farXStart2 = farXStart - farLongLen * expandscale;
                float farXEnd2 = farXEnd + farLongLen * expandscale;
                float yStart2 = yStart - latiLen * expandscale;
                float yEnd2 = yEnd + latiLen * expandscale;


                rectsOuter = new List<System.Windows.Rect>();
                int power = (int)((farXEnd - farXStart) / (nearXEnd - nearXStart));
                power = power < 1 ? 1 : power;
                power = power > 10 ? 10 : power;

                for (int i = 0; i < power; i++)
                {
                    System.Windows.Rect rect = new System.Windows.Rect();

                    if (isXAsLong)               //若x对应经度，交换xywh, range计算中，按x对应纬度进行的
                    {
                        rect.X = farXStart2 + (power - i - 1) * (nearXStart2 - farXStart2) / power;
                        rect.Y = yStart2 + i * (yEnd2 - yStart2) / power;
                        rect.Width = (nearXEnd2 - nearXStart2) + (i + 1) * ((farXEnd2 - farXStart2) - (nearXEnd2 - nearXStart2)) / power;
                        rect.Height = (yEnd2 - yStart2) / power;
                        rectsOuter.Add(rect);
                    }
                    else
                    {
                        rect.Y = farXStart2 + (power - i - 1) * (nearXStart2 - farXStart2) / power;
                        rect.X = yStart2 + i * (yEnd2 - yStart2) / power;
                        rect.Height = (nearXEnd2 - nearXStart2) + (i + 1) * ((farXEnd2 - farXStart2) - (nearXEnd2 - nearXStart2)) / power;
                        rect.Width = (yEnd2 - yStart2) / power;
                        rectsOuter.Add(rect);
                    }
                }

            }
        }



    }
}
