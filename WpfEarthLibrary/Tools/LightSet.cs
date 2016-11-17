using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace WpfEarthLibrary.Tools
{
    public class LightSet : MyClassLibrary.MVVM.NotificationObject
    {
        public LightSet()
        {
        }
        public LightSet(bool isCreateDefaultLight)
        {
            if (isCreateDefaultLight)
            {
                lights = new List<LightPara>();
                for (int i = 0; i < 6; i++)
                {
                    lights.Add(new LightPara() { num = i, lightname = string.Format("光源{0}", i), lightset = this });
                }
                AmbientLight = Colors.Silver;
            }
        }

        public enum ELightType
        {
            点光源 = 1,
            锥光源 = 2,
            方向光 = 3,
        }
        public List<string> LightTypes { get { return Enum.GetNames(typeof(ELightType)).ToList(); } }


        public List<LightPara> lights { get; set; }


        private LightPara _curLight;
        public LightPara curLight
        {
            get { return _curLight; }
            set { _curLight = value; RaisePropertyChanged(() => curLight); }
        }


        private Color _AmbientLight;
        public Color AmbientLight
        {
            get { return _AmbientLight; }
            set
            {
                _AmbientLight = value;
                RaisePropertyChanged(() => AmbientLight);
                AmbientSlider = value.R;
                RaisePropertyChanged(() => AmbientSlider);
            }
        }



        private bool _isEnableSpecular;
        public bool isEnableSpecular
        {
            get { return _isEnableSpecular; }
            set { _isEnableSpecular = value; RaisePropertyChanged(() => isEnableSpecular); }
        }

        public string xmlfile { get; set; }


        //---viewmodel
        private byte _AmbientSlider;
        [XmlIgnore]
        public byte AmbientSlider
        {
            get { return _AmbientSlider; }
            set
            {
                _AmbientSlider = value;
                _AmbientLight = Color.FromRgb(value, value, value);
                RaisePropertyChanged(() => AmbientLight);
                RaiseLightParaChangedEvent();
            }
        }
        public Visibility curVisibiliy { get { return curLight == null ? Visibility.Hidden : Visibility.Visible; } }


        ///<summary>静态方法：从xml文件读取并返回LightSet对象</summary>
        public static LightSet ReadFromXml(string filename)
        {
            LightSet set = (LightSet)MyClassLibrary.XmlHelper.readFromXml(filename, typeof(LightSet));
            if (set != null)
            {
                set.xmlfile = filename;
                foreach (var item in set.lights)
                    item.lightset = set;
                set.curLight = set.lights[0];
            }
            return set;
        }

        ///<summary>保存到xmlfile指定的xml文件</summary>
        public void SaveToXml()
        {
            if (string.IsNullOrWhiteSpace(xmlfile)) return;
            MyClassLibrary.XmlHelper.saveToXml(xmlfile, this);
        }

        ///<summary>光源参数改变事件</summary>
        public event EventHandler LightParaChanged;
        internal virtual void RaiseLightParaChangedEvent()
        {
            if (LightParaChanged != null)
                LightParaChanged(this, null);
        }


        ///<summary>光照方向改变事件</summary>
        public event EventHandler DirectionChanged;
        internal virtual void RaiseDirectionChangedEvent()
        {
            if (DirectionChanged != null)
                DirectionChanged(this, null);
        }




    }


    public class LightPara : MyClassLibrary.MVVM.NotificationObject
    {
        public LightPara()
        {

        }

        [XmlIgnore]
        internal LightSet lightset;

        public int num { get; set; }
        public string lightname { get; set; }

        private bool _isEnable;
        public bool isEnable
        {
            get { return _isEnable; }
            set
            {
                _isEnable = value;
                if (lightset!=null)
                    lightset.RaiseLightParaChangedEvent();
            }
        }


        private LightSet.ELightType _LightType = LightSet.ELightType.方向光;
        public LightSet.ELightType LightType
        {
            get { return _LightType; }
            set
            {
                _LightType = value;
                RaisePropertyChanged(() => LightType);
                RaisePropertyChanged(() => OnlyConeVisibility);
                RaisePropertyChanged(() => DirectionVisibility);
                RaisePropertyChanged(() => ConeDotVisibility);
            }
        }


        [XmlIgnore]
        public int lighttypenum
        {
            get { return ((int)LightType) - 1; }
            set { LightType = (LightSet.ELightType)(value + 1); }
        }

        private Color _Ambient;
        public Color Ambient
        {
            get { return _Ambient; }
            set
            {
                _Ambient = value;
                RaisePropertyChanged(() => Ambient);
                AmbientSlider = value.R;
                RaisePropertyChanged(() => AmbientSlider);
            }
        }
        private Color _Diffuse;
        public Color Diffuse
        {
            get { return _Diffuse; }
            set
            {
                _Diffuse = value;
                RaisePropertyChanged(() => Diffuse);
                DiffuseSlider = value.R;
                RaisePropertyChanged(() => DiffuseSlider);
            }
        }
        private Color _Specular;
        public Color Specular
        {
            get { return _Specular; }
            set
            {
                _Specular = value;
                RaisePropertyChanged(() => Specular);
                SpecularSlider = value.R;
                RaisePropertyChanged(() => SpecularSlider);
            }
        }

        public System.Windows.Media.Media3D.Vector3D Position { get; set; }

        private System.Windows.Media.Media3D.Vector3D _Direction;
        public System.Windows.Media.Media3D.Vector3D Direction
        {
            get { return _Direction; }
            set
            {
                _Direction = value;
                RaisePropertyChanged(() => Direction);
                RaisePropertyChanged(() => dirX);
                RaisePropertyChanged(() => dirY);
                RaisePropertyChanged(() => dirZ);
                if (lightset != null)
                {
                    lightset.RaiseLightParaChangedEvent();
                    lightset.RaiseDirectionChangedEvent();
                }


            }
        }
        [XmlIgnore]
        public float dirX
        {
            get { return (float)Direction.X; }
            set { Direction = new System.Windows.Media.Media3D.Vector3D(value, Direction.Y, Direction.Z); }
        }
        [XmlIgnore]
        public float dirY
        {
            get { return (float)Direction.Y; }
            set { Direction = new System.Windows.Media.Media3D.Vector3D(Direction.X, value, Direction.Z); }
        }
        [XmlIgnore]
        public float dirZ
        {
            get { return (float)Direction.Z; }
            set { Direction = new System.Windows.Media.Media3D.Vector3D(Direction.X, Direction.Y, value); }
        }




        public float Range { get; set; }
        public float Falloff { get; set; }
        public float Attenuation0 { get; set; }
        public float Attenuation1 { get; set; }
        public float Attenuation2 { get; set; }
        public float Theta { get; set; }
        public float Phi { get; set; }


        //viewmodel
        public Visibility ConeDotVisibility { get { return LightType != LightSet.ELightType.方向光 ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility DirectionVisibility { get { return LightType != LightSet.ELightType.点光源 ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility OnlyConeVisibility { get { return LightType == LightSet.ELightType.锥光源 ? Visibility.Visible : Visibility.Collapsed; } }


        private byte _AmbientSlider;
        [XmlIgnore]
        public byte AmbientSlider
        {
            get { return _AmbientSlider; }
            set
            {
                _AmbientSlider = value;
                _Ambient = Color.FromRgb(value, value, value);
                RaisePropertyChanged(() => Ambient);
                if (lightset != null)
                    lightset.RaiseLightParaChangedEvent();
            }
        }
        private byte _DiffuseSlider;
        [XmlIgnore]
        public byte DiffuseSlider
        {
            get { return _DiffuseSlider; }
            set
            {
                _DiffuseSlider = value;
                _Diffuse = Color.FromRgb(value, value, value);
                RaisePropertyChanged(() => Diffuse);
                if (lightset != null)
                    lightset.RaiseLightParaChangedEvent();
            }
        }
        private byte _SpecularSlider;
        [XmlIgnore]
        public byte SpecularSlider
        {
            get { return _SpecularSlider; }
            set
            {
                _SpecularSlider = value;
                _Specular = Color.FromRgb(value, value, value);
                RaisePropertyChanged(() => Specular);
                if (lightset != null)
                    lightset.RaiseLightParaChangedEvent();
            }
        }



        internal STRUCT_Light lightSturPara
        {
            get
            {
                System.Windows.Media.Media3D.Vector3D dir = new System.Windows.Media.Media3D.Vector3D(Direction.X, Direction.Y, Direction.Z);
                dir.Normalize();
                STRUCT_Light l = new STRUCT_Light();
                l.isEnable = isEnable;
                l.light.Ambient = Helpler.getD3DColor(Ambient);
                l.light.Attenuation0 = Attenuation0;
                l.light.Attenuation1 = Attenuation1;
                l.light.Attenuation2 = Attenuation2;
                l.light.Diffuse = Helpler.getD3DColor(Diffuse);
                l.light.Direction = new VECTOR3D(dir.X, dir.Y, dir.Z);
                l.light.Falloff = Falloff;
                l.light.Phi = Phi;
                l.light.Position = new VECTOR3D(Position.X, Position.Y, Position.Z);
                l.light.Range = Range;
                l.light.Specular = Helpler.getD3DColor(Specular);
                l.light.Theta = Theta;
                l.light.Type = (int)LightType;
                return l;
            }
        }
    }

}
