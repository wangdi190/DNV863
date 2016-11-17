using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;

namespace WpfEarthLibrary.Tools
{
    /// <summary>
    /// LightSetTool.xaml 的交互逻辑
    /// </summary>
    public partial class LightSetTool : UserControl
    {
        public LightSetTool()
        {
            InitializeComponent();
        }


        
        private LightSet _lightset;
        ///<summary>光源设置类</summary>
        public LightSet lightset
        {
            get { return _lightset; }
            set 
            {
                if (_lightset != null)
                    _lightset.DirectionChanged -= new EventHandler(_lightset_DirectionChanged);
                _lightset = value;
                grdMain.DataContext = value;
                if (value != null)
                    _lightset.DirectionChanged += new EventHandler(_lightset_DirectionChanged);

            }
        }



        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            lightset.SaveToXml();
        }

        private void btnAbmColor_Click(object sender, RoutedEventArgs e)
        {

        }

        _3DTools.Trackball trackball;
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            geoGround.Geometry = MyClassLibrary.Share3D.Model3DHelper.genTextVPlaneMesh();
            geoCone.Geometry = MyClassLibrary.Share3D.Model3DHelper.genCone3DMesh(new System.Windows.Media.Media3D.Point3D(0, 0.9, 0), new System.Windows.Media.Media3D.Point3D(0, 0.82, 0), 0, 0.08);
            geoCyl.Geometry = MyClassLibrary.Share3D.Model3DHelper.genCylinder3DMesh();
            trackball = new _3DTools.Trackball() { EventSource = myElement };
            mgCone.Transform = trackball.Transform;

            myElement.MouseMove += new MouseEventHandler(myElement_MouseMove);
        }

        void myElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (lightset.curLight == null) return;
            if (e.LeftButton== MouseButtonState.Pressed)
            {
                Vector3D orgdir = new Vector3D(0, 0, -1);
                lightset.curLight.Direction = mgCone.Transform.Transform(orgdir);
            }
        }

        void _lightset_DirectionChanged(object sender, EventArgs e)
        {
            refreshDirectionShow();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lightset.curLight == null) return;

            refreshDirectionShow();
        }

        ///<summary>刷新界面光照方向的显示</summary>
        void refreshDirectionShow()
        {
            if (lightset.curLight == null || lightset.curLight.LightType == LightSet.ELightType.点光源) return;
            Vector3D orgdir = new Vector3D(0, 0, -1);
            Vector3D newdir = lightset.curLight.Direction;
            newdir.Normalize();
            Vector3D axis = Vector3D.CrossProduct(orgdir, newdir);
            double angle = Vector3D.AngleBetween(orgdir, newdir);
            trackball._rotation.Axis = axis;
            trackball._rotation.Angle = angle;
        }


        #region ===== 工具自身移动 =====
        Point mouseprev;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseprev = e.GetPosition(grdMain);
        }
        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousenew = e.GetPosition(grdMain);
                Vector vec = mousenew - mouseprev;
                //mouseprev = mousenew;

                translate.X += vec.X;
                translate.Y += vec.Y;
            }
        }
        #endregion

        private void btnHide_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }


    }
}
