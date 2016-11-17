using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfEarthLibrary
{
    public class Data: DependencyObject
    {
        public Data()
        {
            initAni();
        }

        string _id;
        public string id
        {
            get { return _id; }
            set
            {
                _id = value;
                hashcode = value.GetHashCode();
            }
        }
        internal int hashcode;


        ///<summary>类别</summary>
        public string sort { get; set; }
        ///<summary>标名</summary>
        public string argu { get; set; }

        string _geokey;
        ///<summary>呈现的几何体键值</summary>
        public string geokey 
        { 
            get {return _geokey;}
            set { _geokey = value; geokeyhashcode = value.GetHashCode(); }
        }
        internal int geokeyhashcode;

        ///<summary>标签格式串：{0}类别，{1}标名, {2}值, {3}单位，缺省为：{1}:{2:f0}{3} 呈现 A线有功：321WM</summary>
        public string format = "{1}:{2:f1}{3}";
        ///<summary>数据的单位</summary>
        public string unit = "";
        ///<summary>只读标签</summary>
        public string label { get { return string.Format(format, sort, argu, CurValue, unit); } }
        
        private double _value;
        public double value
        {
            get { return _value; }
            set
            {

                if (isAni)
                {
                    doAni(_value, value);
                    _value = value;
                }
                else
                {
                    _value = value;
                    CurValue = value;
                }   
            }
        }

        private Color _color = Colors.White;
        public Color color
        {
            get { return _color; }
            set { _color = value; material.Ambient = material.Diffuse = value; }
        }
        private CMaterial _material = new CMaterial();
        ///<summary>模型材质</summary>
        public CMaterial material
        {
            get { return _material; }
            set
            {
                _material = value;
            }
        }

        ///<summary>数据做为标签是否可见</summary>
        public bool isShowLable = true;


        ///<summary>动画更新数据,</summary>
        internal bool isAni { get; set; }


        public event EventHandler ValueChange;
        protected virtual void RaiseValueChangeEvent()
        {
            if (ValueChange != null)
                ValueChange(this, null);
        }

        //internal XnaEarth.Addon.PolyCol linkXnaModel { get; set; }


        Storyboard sb = new Storyboard();
        DoubleAnimation ani = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(1000), FillBehavior.HoldEnd);

        public static DependencyProperty CurValueProperty = DependencyProperty.Register("CurValue", typeof(double), typeof(Data), new PropertyMetadata(0.0, new PropertyChangedCallback(OnValueChanged)));
        public double CurValue
        {
            get { return (double)GetValue(CurValueProperty); }
            set { SetValue(CurValueProperty, value); }
        }
        internal static void OnValueChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            Data m = sender as Data;
            m.RaiseValueChangeEvent();
        }


        void initAni()
        {
            sb.Children.Add(ani);
            Storyboard.SetTarget(ani, this);
            Storyboard.SetTargetProperty(ani, new PropertyPath(Data.CurValueProperty));
        }

        void doAni(double fromData, double toData)
        {
            ani.From = fromData;
            ani.To = toData;
            sb.Begin();

        }


        //   (_mapItem as MapShape).Stroke = new SolidColorBrush(Color.FromRgb(0xFF, 0xCC, 0x00));
        //        strokeStoryboard = new Storyboard();
        //        strokeStoryboard.RepeatBehavior = RepeatBehavior.Forever;
        //        strokeAni = new ColorAnimationUsingKeyFrames();
        //        strokeAni.KeyFrames.Add(new DiscreteColorKeyFrame(Colors.Transparent, TimeSpan.FromSeconds(1)));
        //        strokeAni.KeyFrames.Add(new DiscreteColorKeyFrame(Colors.Blue, TimeSpan.FromSeconds(2)));
        //        Storyboard.SetTarget(strokeAni, (_mapItem as MapShape).Stroke);
        //        Storyboard.SetTargetProperty(strokeAni, new PropertyPath(SolidColorBrush.ColorProperty));

        //        strokeStoryboard.Children.Add(strokeAni);
        ////参考事件，自动画，参考北京的项目
    }
}
