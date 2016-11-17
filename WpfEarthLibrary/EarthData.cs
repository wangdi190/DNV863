using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;


namespace WpfEarthLibrary
{
    internal enum EOperate { none, 节点模型删除, 节点删除, 模型删除, 模型加入, 模型更新 } //节点应进行的操作
    internal enum EStatus { none, 收缩, 扩张 } //节点状态
    internal enum ETileStatus { 不可见, 预载, 部分可见, 全部可见 }
    internal enum EMeshStatus { none, 平面, 三维 }

    /// <summary>
    /// 动态维护数据类
    /// </summary>
    internal class EarthData
    {
        internal EarthData(EarthManager Earthmanager, EarthData Parent, int idxX, int idxY)
        {
            earthmanager = Earthmanager;
            parent = Parent;
            if (Parent == null)
                layer = 0;
            else
                layer = parent.layer + 1;
            idxx = idxX;
            idxy = idxY;
            quadkey = MapHelper.TileXYToQuadKey(layer, idxx, idxy);

            id = string.Format("{0}_{1}_{2}", layer, idxx, idxy);
            hashcode = id.GetHashCode();



        }
        internal string id;

        internal EarthManager earthmanager;
        internal EarthData parent;
        internal int layer;
        internal int idxx;
        internal int idxy;

        internal string quadkey;
        internal int hashcode;

        string _texture = "box";
        public string texture
        {
            set
            {
                _texture = value;
                //if (_model != null)
                //    _model.strtexture = _texture;
            }
        }

        public Rect rect;// debug 

        public string addinfo; //debug

        ///<summary>块的范围, x经y纬</summary>
        internal Rect range
        {
            get
            {
                //采用google瓦片的，按整个地球四分下去计算idxx,idxy,layer
                if (earthmanager.earthpara.InputCoordinate == EInputCoordinate.WGS84球面坐标 && (earthmanager.earthpara.tileReadMode == ETileReadMode.内置瓦片服务 || earthmanager.earthpara.tileReadMode == ETileReadMode.自定义Web瓦片))
                {
                    int ycount = (int)Math.Pow(2, layer);
                    double angle = MapHelper.DegToRad(360.0 / ycount);
                    double tileLength = 2.0 * Math.PI / Math.Pow(2, layer); //2*pi/power(2,layer)  假定半径为1, 平面地图坐标系下该层划分块的长和宽度
                    double xStart = (idxx * angle - Math.PI) / Math.PI * 180;//  指定索引号块的起始经度
                    double xEnd = ((idxx + 1) * angle - Math.PI) / Math.PI * 180;//  指定索引号块的起始经度
                    double yStart = (Math.Atan(Math.Exp(Math.PI - (idxy + 1) * tileLength)) * 2 - Math.PI / 2) / Math.PI * 180; //指定索引号块的结束纬度
                    double yEnd = (Math.Atan(Math.Exp(Math.PI - idxy * tileLength)) * 2 - Math.PI / 2) / Math.PI * 180; //atan(exp(PI-idxy*2*pi/power(2,layer)))*2-pi/2  指定索引号块的起始纬度
                    return new Rect(xStart, yStart, xEnd - xStart, yEnd - yStart);
                }
                else //自定义瓦片 , 假定瓦片被等分，包括纬度方向, 以0层瓦片为根四分下去计算idxx,idxy,layer
                {
                    int count = (int)Math.Pow(2, layer);
                    double jdLength = (earthmanager.earthpara.EndLocation.Longitude - earthmanager.earthpara.StartLocation.Longitude) / count;
                    double wdLength = (earthmanager.earthpara.EndLocation.Latitude - earthmanager.earthpara.StartLocation.Latitude) / count;
                    double xStart = earthmanager.earthpara.StartLocation.Longitude + idxx * jdLength;
                    double yStart = earthmanager.earthpara.EndLocation.Latitude - idxy * wdLength - wdLength;  //纬度方向与瓦片序方向相反
                    return new Rect(xStart, yStart, jdLength, wdLength);
                }

            }
        }


        Vector3[] _normals; //四角点normal
        internal Vector3[] normals
        {
            get
            {
                if (_normals == null)
                {
                    if (earthmanager.earthpara.SceneMode == ESceneMode.地球)
                    {
                        _normals = MapHelper.getMeshAllNormal(layer, idxx, idxy);
                    }
                    else if (earthmanager.earthpara.SceneMode == ESceneMode.局部平面)
                    {//注，非正常normal，借用为位置供视锥使用
                        _normals = new Vector3[4];
                        Rect rec = range;
                        STRUCT_EarthPara para = earthmanager.earthpara;
                        _normals[0] = new Vector3((float)((rec.Left - para.StartLocation.Longitude) * para.UnitLongLen), (float)((rec.Top - para.StartLocation.Latitude) * para.UnitLatLen), 0.0f);
                        _normals[1] = new Vector3((float)((rec.Right - para.StartLocation.Longitude) * para.UnitLongLen), (float)((rec.Top - para.StartLocation.Latitude) * para.UnitLatLen), 0.0f);
                        _normals[2] = new Vector3((float)((rec.Left - para.StartLocation.Longitude) * para.UnitLongLen), (float)((rec.Bottom - para.StartLocation.Latitude) * para.UnitLatLen), 0.0f);
                        _normals[3] = new Vector3((float)((rec.Right - para.StartLocation.Longitude) * para.UnitLongLen), (float)((rec.Bottom - para.StartLocation.Latitude) * para.UnitLatLen), 0.0f);
                    }
                    float maxx = _normals.Max(p => p.X); float minx = _normals.Min(p => p.X);
                    float maxy = _normals.Max(p => p.Y); float miny = _normals.Min(p => p.Y);
                    float maxz = _normals.Max(p => p.Z); float minz = _normals.Min(p => p.Z);

                    boundingBox = new BoundingBox(new Vector3(minx, miny, minz), new Vector3(maxx, maxy, maxz));
                    Center = new Vector3(minx + maxx / 2 - minx / 2, miny + maxy / 2 - miny / 2, minz + maxz / 2 - minz / 2);
                }
                return _normals;
            }
        }

        internal Vector3 Center;


        internal BoundingBox boundingBox;

        internal List<EarthData> subCurve = new List<EarthData>();


        internal EMeshStatus curModelStatus = EMeshStatus.none;  //模型当前的形态
        public EMeshStatus mustModelStatus //模型应具的形态
        {
            get
            {
                if (earthmanager.TerrainAllow && earthmanager.earth.camera.curCameraDistanceToGround < earthmanager.TerrainMaxDistance && earthmanager.earth.camera.curCameraAngle > earthmanager.TerrainMaxAngle && isShowTerrain && layer >= earthmanager.TerrainMinLayer && layer <= earthmanager.TerrainMaxLayer)
                    return EMeshStatus.三维;
                else
                    return EMeshStatus.平面;
            }
        }

        internal EStatus curStatus = EStatus.none;
        internal EStatus oprStatus = EStatus.none;
        internal EOperate operate = EOperate.none;
        internal ETileStatus tileStatus;  //块可见性
        ///<summary>根据可见性，判断是否可以显示地形的变量</summary>
        internal bool isShowTerrain;
        internal double distance;  //距相机距离
        internal bool handled;
        internal bool prehandled;

        internal void Split() //进行切分
        {
            if (earthmanager.earthpara.SceneMode == ESceneMode.地球)
            {
                addNewSub(this.idxx * 2, this.idxy * 2);
                addNewSub(this.idxx * 2 + 1, this.idxy * 2);
                addNewSub(this.idxx * 2, this.idxy * 2 + 1);
                addNewSub(this.idxx * 2 + 1, this.idxy * 2 + 1);
            }
            else if (earthmanager.earthpara.SceneMode == ESceneMode.局部平面)
            {
                if (layer + 1 <= earthmanager.earthpara.StartLayer) //小于初始层设置时的判断
                {
                    Rect orgrect = new Rect(earthmanager.earthpara.StartLocation.Longitude, earthmanager.earthpara.StartLocation.Latitude, earthmanager.earthpara.EndLocation.Longitude - earthmanager.earthpara.StartLocation.Longitude, earthmanager.earthpara.EndLocation.Latitude - earthmanager.earthpara.StartLocation.Latitude);
                    Rect tmp = Helpler.GetTileJW(layer + 1, idxx * 2, idxy * 2);
                    if (tmp.IntersectsWith(orgrect))
                        addNewSub(this.idxx * 2, this.idxy * 2);
                    tmp = Helpler.GetTileJW(layer + 1, idxx * 2 + 1, idxy * 2);
                    if (tmp.IntersectsWith(orgrect))
                        addNewSub(this.idxx * 2 + 1, this.idxy * 2);
                    tmp = Helpler.GetTileJW(layer + 1, idxx * 2, idxy * 2 + 1);
                    if (tmp.IntersectsWith(orgrect))
                        addNewSub(this.idxx * 2, this.idxy * 2 + 1);
                    tmp = Helpler.GetTileJW(layer + 1, idxx * 2 + 1, idxy * 2 + 1);
                    if (tmp.IntersectsWith(orgrect))
                        addNewSub(this.idxx * 2 + 1, this.idxy * 2 + 1);

                }
                else
                {
                    addNewSub(this.idxx * 2, this.idxy * 2);
                    addNewSub(this.idxx * 2 + 1, this.idxy * 2);
                    addNewSub(this.idxx * 2, this.idxy * 2 + 1);
                    addNewSub(this.idxx * 2 + 1, this.idxy * 2 + 1);
                }

            }

        }

        void addNewSub(int idxx, int idxy)
        {
            if (subCurve.Find(p => p.idxx == idxx && p.idxy == idxy) == null)
            {
                EarthData newearthdata;

                string modelkey = string.Format("{0}.{1}.{2}", layer + 1, idxx, idxy);
                if (!MapHelper.models.TryGetValue(modelkey, out newearthdata))
                {

                    newearthdata = new EarthData(earthmanager, this, idxx, idxy);

                    if (MapHelper.models.Count > 500)
                        MapHelper.models.Remove(MapHelper.models.Keys.ElementAt(0));
                    MapHelper.models.Add(modelkey, newearthdata);
                }
                else
                {
                    newearthdata.parent = this;
                    newearthdata.handled = false;
                    newearthdata.prehandled = false;
                    newearthdata.curStatus = EStatus.none;
                    newearthdata.oprStatus = EStatus.none;
                    newearthdata.operate = EOperate.none;

                }



                subCurve.Add(newearthdata);
            }

        }


        internal void Merge() //合并
        {
            foreach (EarthData item in subCurve)
            {
                item.setClearSelf(); ;
            }
        }
        /// <summary>
        /// 迭代设置清理子节点
        /// </summary>
        private void setClearSelf()
        {
            handled = false;
            switch (curStatus)
            {
                case EStatus.none:
                    operate = EOperate.节点删除;
                    break;
                case EStatus.扩张:
                    operate = EOperate.节点删除;
                    break;
                case EStatus.收缩:
                    operate = EOperate.节点模型删除;
                    break;
            }
            foreach (EarthData item in subCurve)
            {
                item.setClearSelf(); ;
            }
        }

        /// <summary>
        /// 设置扩展，并生成下一级子节点
        /// </summary>
        internal void setExpand()
        {
            switch (curStatus)
            {
                case EStatus.none:
                    Split();
                    operate = EOperate.none;
                    oprStatus = EStatus.扩张;
                    break;
                case EStatus.扩张:
                    Split();
                    operate = EOperate.none;
                    break;
                case EStatus.收缩:
                    Split();
                    operate = EOperate.模型删除;
                    oprStatus = EStatus.扩张;
                    break;
            }
        }

        /// <summary>
        /// 设置收缩
        /// </summary>
        internal void setFold()
        {
            switch (curStatus)
            {
                case EStatus.none:
                    operate = EOperate.模型加入;
                    oprStatus = EStatus.收缩;
                    break;
                case EStatus.扩张:
                    Merge();
                    operate = EOperate.模型加入;
                    oprStatus = EStatus.收缩;
                    break;
                case EStatus.收缩:
                    operate = EOperate.模型更新;
                    //operate = EOperate.none;
                    break;
            }
        }

        /// <summary>
        /// 设置节点删除
        /// </summary>
        internal void setDelete()
        {
            switch (curStatus)
            {
                case EStatus.none:
                    operate = EOperate.节点删除;
                    break;
                case EStatus.扩张:
                    Merge();
                    operate = EOperate.节点删除;
                    break;
                case EStatus.收缩:
                    operate = EOperate.节点模型删除;
                    break;
            }
        }



        #region 辅助方法

        //public void refreshMat()
        //{
        //    _mat = null;
        //    if (_model != null)
        //        _model.Material = mat;
        //    foreach (EarthData sub in subCurve)
        //    {
        //        sub.refreshMat();
        //    }
        //}

        #endregion

        #region 调试用
        //public Rect subRect;
        public string info = "";
        //public Point vex0;
        //public Point vex1;
        //public Point vex2;
        //public Point vex3;
        //public double area
        //{
        //    get
        //    {
        //        double tmp = 0;
        //        if (curStatus == EStatus.收缩)
        //            tmp = Math.Pow(4, Helper.maxLayer - layer);
        //        else if (curStatus == EStatus.扩张)
        //            tmp = subCurve.Sum(p => p.area);
        //        else
        //            tmp = 0;

        //        return tmp;
        //    }
        //}
        public double blockcount
        {
            get
            {
                double tmp = 0;
                //if (oprStatus == EStatus.收缩)
                //    tmp = 1;
                //else if (oprStatus == EStatus.扩张)
                //    tmp = subCurve.Sum(p => p.blockcount);
                //else
                //    tmp = 0;

                tmp = subCurve.Sum(p => p.blockcount) + 1;


                return tmp;
            }
        }


        public EarthData searchNode(int zlayer, int zidxx, int zidxy)
        {
            if (layer == zlayer && idxx == zidxx && idxy == zidxy)
                return this;
            else
            {
                EarthData result = null;
                foreach (EarthData item in subCurve)
                {
                    result = item.searchNode(zlayer, zidxx, zidxy);
                    if (result != null)
                        return result;
                }
                return result;
            }
        }

        //public RotateTransform3D trans
        //{
        //    get
        //    {
        //        int ycount = (int)Math.Pow(2, layer);
        //        double angle = 360.0 / ycount;
        //        return new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), angle * idxx));
        //    }
        //}
        #endregion


        #region ===== 地形相关 =====
        int _terrainSliceCount = -1;
        ///<summary>瓦片切分数</summary>
        internal int terrainSliceCount
        {
            get
            {
                if (_terrainSliceCount < 0) 
                    _terrainSliceCount =(int)Math.Pow(2,earthmanager.TerrainMaxLayer - layer) * earthmanager.TerrainMaxLayerSliceCount;
                return _terrainSliceCount;
            }
        }

        List<float> _terrainHeigList;
        internal List<float> terrainHeigList
        {
            get
            {
                if (_terrainHeigList == null)
                    _terrainHeigList = TerrainHelp.getHeigList(this);  //TerrainHelp.getHeigList(terrainSliceCount, range.X, (range.X + range.Width), range.Y, (range.Y + range.Height), earthmanager.TerrainMinHeight, earthmanager.TerrainDropHeight);
                return _terrainHeigList;
            }
        }
        #endregion

    }




}
