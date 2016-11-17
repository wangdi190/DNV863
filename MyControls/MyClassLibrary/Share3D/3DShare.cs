using System;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyClassLibrary.Share3D
{
    /// <summary>
    /// 3D场景参数结构
    /// </summary>
    public struct ScenePara3D
    {
        public Point3D Center;  //场景中心点
        public Point3D CamOrgPosition;//相机初始
        public Vector3D CamOrgLookAt;
 
        
        public Vector3D Limit;  //参数的xyz分量，控制数据在3D中xyz方向的量
    }



}
