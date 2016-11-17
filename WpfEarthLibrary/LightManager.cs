using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Text;
using System.Runtime.InteropServices;

namespace WpfEarthLibrary
{

    public class LightManager
    {
        public LightManager(Earth Parent)
        {
            earth = Parent;

            //lights = new CLight[6];  //注：0-5号光源由wpf管理，6 7号光源保留给d3d内部使用

            //AmbientLight = Color.FromArgb(0xFF, 0x2F, 0x2F, 0x2F);


            ////右侧上光源正常效果
            //Vector3D vecDir=new Vector3D(0.5,1,-0.3);
            //vecDir.Normalize();
            //lights[0] = new CLight(this, 0)
            //{
            //    lightdesc = "右侧上光源正常效果",
            //    Type = CLight.ELightType.方向光,
            //    Diffuse = Colors.White,
            //    Specular=Colors.White,
            //    Direction=new Vector3D(0,1,0),
            //    Range=1000,
            //    isEnable = false //最后设置并提交
            //};

            ////改对象加灯光号

            ////右侧上光源通透效果
            //lights[1] = new CLight(this, 1)
            //{
            //    lightdesc = "右侧上光源通透效果",
            //    Type = CLight.ELightType.方向光,
            //    Diffuse = Colors.White,
            //    Ambient = Colors.White,
            //    Direction = vecDir,
            //    isEnable = false //最后设置并提交
            //};

            ////正前方光源
            //lights[2] = new CLight(this, 2)
            //{
            //    lightdesc = "建筑物光源",
            //    Type = CLight.ELightType.方向光,
            //    Diffuse = Color.FromRgb(0x66, 0x66, 0x66),
            //    Ambient = Color.FromRgb(0x70, 0x70, 0x70),
            //    Direction = vecDir,
            //    isEnable = false //最后设置并提交
            //};

            ////顶光源
            //lights[3] = new CLight(this, 3)
            //{
            //    lightdesc = "正上方光源",
            //    Type = CLight.ELightType.方向光,
            //    Diffuse = Color.FromRgb(0x66, 0x66, 0x66),
            //    Ambient = Color.FromRgb(0x70, 0x70, 0x70),
            //    Direction = new Vector3D(0,0,-1),
            //    isEnable = false //最后设置并提交
            //};


            //for (int i = 0; i < lights.Length; i++)
            //{
            //    if (lights[i]==null)
            //        lights[i] = new CLight(this, i);
            //}


        }

        private Tools.LightSet _lightset;
        ///<summary>光源设置参数, 可使用WpfLibrary.Tool.LightSet静态方法从xml中读取</summary>
        public Tools.LightSet lightset
        {
            get { return _lightset; }
            set
            {
                if (_lightset!=null)
                    _lightset.LightParaChanged -= new EventHandler(_lightset_LightParaChanged);
                _lightset = value;
                if (value != null)
                {
                    _lightset.LightParaChanged += new EventHandler(_lightset_LightParaChanged);
                }
            }
        }

        void _lightset_LightParaChanged(object sender, EventArgs e)
        {
            applyLights();
        }

              


        internal Earth earth;

        /////<summary>光源集合数组，序号0-5</summary>
        //public CLight[] lights;

        //public Color AmbientLight { get; set; }

        ///<summary>提交环境光</summary>
        public void applyAmbientLight()
        {
            if (lightset == null) return;
            D3DManager.ChangeAmbientLight(earth.earthkey, Helpler.ColorToUInt(lightset.AmbientLight));
        }

        ///<summary>根据相机位置重计算缺省光源方向，并提交缺省光源</summary>
        public void applyLights()
        {
            if (lightset == null) return;
            applyAmbientLight();
            foreach (var item in lightset.lights)
            {
                IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(item.lightSturPara));
                Marshal.StructureToPtr(item.lightSturPara, ipPara, false);

                D3DManager.ChangeLightPara(earth.earthkey, item.num, ipPara);
                Marshal.FreeCoTaskMem(ipPara);

            }


            //if (earth.earthManager.earthpara.SceneMode == ESceneMode.局部平面)
            //{
            //    Vector3D vecGround = new Vector3D(0,0,-1);
            //    Vector3D vecY = new Vector3D(0, 1, 0);
            //    Vector3D vecX = new Vector3D(1, 0, 0);
            //    //右侧上光源正常效果
            //    Vector3D vecDir;
            //    RotateTransform3D rotate;
            //    rotate = new RotateTransform3D(new AxisAngleRotation3D(vecY, -60));
            //    vecDir = rotate.Transform(vecGround);
            //    rotate = new RotateTransform3D(new AxisAngleRotation3D(vecX, 60));
            //    vecDir = rotate.Transform(vecDir);

            //    //vecDir.Negate();
            //    vecDir.Normalize();
            //    lights[0].Direction = vecDir;
            //    lights[0].applyLight();

            //    //右侧上光源通透效果
            //    lights[1].Direction = vecDir;
            //    lights[1].applyLight();

            //    //正前方光源

            //    lights[2].Direction =vecY;
            //    lights[2].applyLight();

            //    lights[2].applyLight();
            //}
            //else
            //{
            //    Vector3D vecGround = new Vector3D(earth.camera.cameraPosition.X, earth.camera.cameraPosition.Y, earth.camera.cameraPosition.Z);
            //    vecGround = vecGround * (vecGround.Length / Para.Radius);
            //    Vector3D vecY = new Vector3D(0, 1, 0);
            //    Vector3D vecAxis = Vector3D.CrossProduct(vecGround, vecY);
            //    vecAxis.Normalize();
            //    //右侧上光源正常效果
            //    Vector3D vecDir;
            //    RotateTransform3D rotate;
            //    rotate = new RotateTransform3D(new AxisAngleRotation3D(vecAxis, -60));
            //    vecDir = rotate.Transform(vecGround);
            //    rotate = new RotateTransform3D(new AxisAngleRotation3D(vecGround, 60));
            //    vecDir = rotate.Transform(vecDir);
            //    vecDir.Negate();
            //    vecDir.Normalize();
            //    lights[0].Direction = vecDir;
            //    lights[0].applyLight();

            //    //右侧上光源通透效果
            //    rotate = new RotateTransform3D(new AxisAngleRotation3D(vecAxis, -60));
            //    vecDir = rotate.Transform(vecGround);
            //    rotate = new RotateTransform3D(new AxisAngleRotation3D(vecGround, 60));
            //    vecDir = rotate.Transform(vecDir);
            //    vecDir.Negate();
            //    vecDir.Normalize();
            //    lights[1].Direction = vecDir;
            //    lights[1].applyLight();

            //    //正前方光源
            //    rotate = new RotateTransform3D(new AxisAngleRotation3D(vecAxis, -60));
            //    vecDir = rotate.Transform(vecGround);
            //    //rotate = new RotateTransform3D(new AxisAngleRotation3D(vecGround, 60));
            //    //vecDir = rotate.Transform(vecDir);
            //    vecDir.Negate();
            //    vecDir.Normalize();
            //    lights[2].Direction = vecDir;
            //    lights[2].applyLight();
            //}
        }

    }

    //public class CLight
    //{
    //    public CLight(LightManager Parent, int LightNum)
    //    {
    //        parent = Parent;
    //        _lightnum = LightNum;
    //    }
    //    LightManager parent;

    //    public enum ELightType
    //    {
    //        点光源 = 1,
    //        锥光源 = 2,
    //        方向光 = 3,
    //    }
    //    internal STRUCT_Light lightSturPara
    //    {
    //        get
    //        {
    //            STRUCT_Light l = new STRUCT_Light();
    //            l.isEnable = isEnable;
    //            l.light.Ambient = Helpler.getD3DColor(Ambient);
    //            l.light.Attenuation0 = Attenuation0;
    //            l.light.Attenuation1 = Attenuation1;
    //            l.light.Attenuation2 = Attenuation2;
    //            l.light.Diffuse =Helpler.getD3DColor(Diffuse);
    //            l.light.Direction = new VECTOR3D(Direction.X, Direction.Y, Direction.Z);
    //            l.light.Falloff = Falloff;
    //            l.light.Phi = Phi;
    //            l.light.Position = new VECTOR3D(Position.X, Position.Y, Position.Z);
    //            l.light.Range = Range;
    //            l.light.Specular = Helpler.getD3DColor(Specular);
    //            l.light.Theta = Theta;
    //            l.light.Type = (int)Type;
    //            return l;
    //        }
    //    }
    //    public string lightdesc = "未使用";

    //    private bool _isEnable;
    //    ///<summary>是否生效</summary>
    //    public bool isEnable
    //    {
    //        get { return _isEnable; }
    //        set
    //        {
    //            _isEnable = value;
    //            //applyLight();
    //        }
    //    }

    //    ///<summary>提交光源</summary>
    //    public void applyLight()
    //    {
    //        IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(lightSturPara));
    //        Marshal.StructureToPtr(lightSturPara, ipPara, false);

    //        D3DManager.ChangeLightPara(parent.earth.earthkey, lightnum, ipPara);
    //        Marshal.FreeCoTaskMem(ipPara);
    //    }


    //    int _lightnum = 0;
    //    int lightnum { get { return _lightnum; } }

    //    ///<summary>光源类型</summary>
    //    public ELightType Type = ELightType.方向光;
    //    public Color Diffuse;         /* Diffuse color of light */
    //    public Color Specular;        /* Specular color of light */
    //    public Color Ambient;         /* Ambient color of light */
    //    public Vector3D Position;         /* Position in world space */
    //    public Vector3D Direction;        /* Direction in world space */
    //    public float Range=100;            /* Cutoff range */
    //    public float Falloff;          /* Falloff */
    //    public float Attenuation0;     /* Constant attenuation */
    //    public float Attenuation1;     /* Linear attenuation */
    //    public float Attenuation2;     /* Quadratic attenuation */
    //    public float Theta;            /* Inner angle of spotlight cone */
    //    public float Phi;              /* Outer angle of spotlight cone */


    //}

    public class CMaterial
    {
        public CMaterial()
        {

        }

        internal D3DMATERIAL9 materialSturPara
        {
            get
            {
                D3DMATERIAL9 l = new D3DMATERIAL9();
                l.Diffuse = Helpler.getD3DColor(Diffuse);
                l.Specular = Helpler.getD3DColor(Specular);
                l.Ambient = Helpler.getD3DColor(Ambient);
                l.Emissive = Helpler.getD3DColor(Emissive);
                l.Power = pow;
                return l;
            }
        }

        public Color Diffuse;         /* Diffuse color of light */
        public Color Specular;        /* Specular color of light */
        public Color Ambient;         /* Ambient color of light */
        public Color Emissive;
        public float pow;
    }
}
