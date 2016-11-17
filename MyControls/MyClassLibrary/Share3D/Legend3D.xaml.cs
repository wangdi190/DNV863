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
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyClassLibrary.Share3D
{
    /// <summary>
    /// Legend3D.xaml 的交互逻辑
    /// </summary>
    public partial class Legend3D : UserControl
    {
        public Legend3D(Legend3DData LegendData)
        {
            data = LegendData;
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            data.grdMain = grdmain;
            data.grdFlag = grdFlag;
            data.colorFlag = colorFlag;
            data.modelMain = modelMain;
            data.genAllLegend();
            mgAll.Children.Add(data.mg);
            daClose.Completed += new EventHandler(daClose_Completed);
        }


        Legend3DData data;

        public bool isShow { get; set; }


        void daClose_Completed(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            this.Visibility = Visibility.Hidden;
        }

        DoubleAnimation daOpen = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
        DoubleAnimation daClose = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
        Vector vectranslate;
        public void showLegend(Vector vec_translate)
        {
            vectranslate = vec_translate;
            //动画放大
            this.Visibility = Visibility.Visible;
            thisScale.BeginAnimation(ScaleTransform.ScaleXProperty, daOpen);
            thisScale.BeginAnimation(ScaleTransform.ScaleYProperty, daOpen);

            DoubleAnimation tax = new DoubleAnimation(vectranslate.X, 0, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
            DoubleAnimation tay = new DoubleAnimation(vectranslate.Y, 0, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
            thisTranslate.BeginAnimation(TranslateTransform.XProperty, tax);
            thisTranslate.BeginAnimation(TranslateTransform.YProperty, tay);
            isShow = true;
        }
        public void closeLegend()
        {
            thisScale.BeginAnimation(ScaleTransform.ScaleXProperty, daClose);
            thisScale.BeginAnimation(ScaleTransform.ScaleYProperty, daClose);

            DoubleAnimation tax = new DoubleAnimation(0, vectranslate.X, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
            DoubleAnimation tay = new DoubleAnimation(0, vectranslate.Y, TimeSpan.FromSeconds(0.3), FillBehavior.HoldEnd);
            thisTranslate.BeginAnimation(TranslateTransform.XProperty, tax);
            thisTranslate.BeginAnimation(TranslateTransform.YProperty, tay);
            isShow = false;
        }
        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            closeLegend();
        }

    
    





    }





}
