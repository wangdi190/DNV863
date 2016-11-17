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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MyClassLibrary.Share3D
{
    /// <summary>
    /// ControlPanel3D.xaml 的交互逻辑
    /// zoom 提供两种使用方法，zoomvalue绝对值, zoomscale, 相对变化
    /// </summary>
    public partial class ControlPanel3D : UserControl
    {
        public ControlPanel3D()
        {
            InitializeComponent();
            this.DataContext = this;

        }

        public enum EPanelStatus { none, rotation, move }

        public delegate void zoomEventHandle(Object sender, EventArgs e);
        public event zoomEventHandle zoomEvent;

        public delegate void moveratationEventHandle(Object sender, EventArgs e);
        public event moveratationEventHandle moveratationEvent;

     
        public EPanelStatus panelStatus = EPanelStatus.none;
        public Vector rotationVec = new Vector(0, 0);
        public Vector moveVec = new Vector(0, 0);
        public double zoomScale = 1;

        public static DependencyProperty zoomValueProperty = DependencyProperty.Register("zoomValue", typeof(double), typeof(ControlPanel3D));
        public double zoomValue
        {
            get
            {
                return (double)GetValue(zoomValueProperty);
            }
            set
            {
                if (value >= slidezoom.Minimum && value <= slidezoom.Maximum)
                {
                    SetValue(zoomValueProperty, value);
                }
            }
        }
        public double zoomMin
        {
            get
            {
                return slidezoom.Minimum;
            }
            set
            {
                slidezoom.Minimum = value;
                slidezoom.LargeStep = (slidezoom.Maximum - slidezoom.Minimum) / 10;
                slidezoom.SmallStep = (slidezoom.Maximum - slidezoom.Minimum) / 100;
            }
        }
        public double zoomMax
        {
            get
            {
                return slidezoom.Maximum;
            }
            set
            {
                slidezoom.Maximum = value;
                slidezoom.LargeStep = (slidezoom.Maximum - slidezoom.Minimum) / 10;
                slidezoom.SmallStep = (slidezoom.Maximum - slidezoom.Minimum) / 100;
            }
        }

        bool _isUnloadDispose = true;
        public bool isUnloadDispose { get { return _isUnloadDispose; } set { _isUnloadDispose = value; } }

        DispatcherTimer timer = new DispatcherTimer();

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            inittimer();
            zoomValue = zoomMax / 2;
        }

        private void inittimer()
        {
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = TimeSpan.FromSeconds(1.0 / 100.0);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            moveratationEvent(this, e);
        }


        private void controller1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Ellipse).Equals(controller1))
                setController1(e.GetPosition(controller1), new Point(controller1.ActualWidth / 2, controller1.ActualHeight / 2));
            else
                setController2(e.GetPosition(controller2), new Point(controller2.ActualWidth / 2, controller2.ActualHeight / 2));

            timer.Start();
        }
        private void controller1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if ((sender as Ellipse).Equals(controller1))
                    setController1(e.GetPosition(controller1), new Point(controller1.ActualWidth / 2, controller1.ActualHeight / 2));
                else
                    setController2(e.GetPosition(controller2), new Point(controller2.ActualWidth / 2, controller2.ActualHeight / 2));
                if (!timer.IsEnabled)
                    timer.Start();
            }
        }
        private void setController1(Point mp, Point cp)
        {
            Vector vec = mp - cp;
            vec = new Vector(vec.X / controller1.ActualWidth * 2, -vec.Y / controller1.ActualHeight * 2);
            double angle = Vector.AngleBetween(vec, new Vector(1, 0));
            controller1Rotate.Angle = angle - 135;

            rotationVec = vec;
            panelStatus = EPanelStatus.rotation;
            controller1Brush.Opacity = 1;
        }
        private void setController2(Point mp, Point cp)
        {
            Vector vec = mp - cp;
            vec = new Vector(vec.X / controller2.ActualWidth * 2, -vec.Y / controller2.ActualHeight * 2);
            double angle = Vector.AngleBetween(vec, new Vector(1, 0));
            controller2Rotate.Angle = angle - 135;

            moveVec = vec;
            panelStatus = EPanelStatus.move;
            controller2Brush.Opacity = 1;
        }
        private void controller1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            panelStatus = EPanelStatus.none;
            controller1Brush.Opacity = 0;
            controller2Brush.Opacity = 0;
            timer.Stop();
        }
        private void controller1_MouseLeave(object sender, MouseEventArgs e)
        {
            panelStatus = EPanelStatus.none;
            controller1Brush.Opacity = 0;
            controller2Brush.Opacity = 0;
            timer.Stop();
        }


        private void slidezoom_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {

            zoomScale = 1.0 - ((double)e.NewValue - (double)e.OldValue) / 100;
            if (this.IsInitialized && zoomEvent!=null)
                zoomEvent(this, e);

        }

   
        private void userControlSelf_Unloaded(object sender, RoutedEventArgs e)
        {
            //if (isUnloadDispose)
            //{
            //    this.DataContext = null;
            //    BindingOperations.ClearBinding(slidezoom, DevExpress.Xpf.Editors.TrackBarEdit.ValueProperty);
            //}
        }


    }
}
