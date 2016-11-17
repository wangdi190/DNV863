using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyClassLibrary.Share2D
{
    public enum EMaterailColor { 红, 黄, 蓝, 绿, 紫, 褐, 灰, 黑, 白 }
    public enum EMaterialColorDeep { 浅色, 普通, 深色, 半透 }
    public enum EMaterialValueToColorMode { 纯色, 浅到深, 深到浅, 透明到不透明, 蓝到红, 绿到红 }

    
    public static class MediaHelper
    {

        /// <summary>
        /// 获取固定画刷，来源于资源，可与材质配套
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="deep">深度</param>
        /// <returns>材质</returns>
        public static SolidColorBrush getSolidBrush(EMaterailColor color, EMaterialColorDeep deep)
        {
            string resname = "color";
            switch (deep)
            {
                case EMaterialColorDeep.半透:
                    resname += "Trans";
                    break;
                case EMaterialColorDeep.浅色:
                    resname += "Light";
                    break;
                case EMaterialColorDeep.深色:
                    resname += "Dark";
                    break;
            }

            switch (color)
            {
                case EMaterailColor.白:
                    resname += "White";
                    break;
                case EMaterailColor.黑:
                    resname += "Black";
                    break;
                case EMaterailColor.红:
                    resname += "Red";
                    break;
                case EMaterailColor.黄:
                    resname += "Yellow";
                    break;
                case EMaterailColor.灰:
                    resname += "Gray";
                    break;
                case EMaterailColor.蓝:
                    resname += "Blue";
                    break;
                case EMaterailColor.绿:
                    resname += "Green";
                    break;
                case EMaterailColor.紫:
                    resname += "Purple";
                    break;
                case EMaterailColor.褐:
                    resname += "Brown";
                    break;
            }

            return (SolidColorBrush)Application.Current.FindResource(resname);
        }

        /// <summary>
        /// 获取材质资源
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="deep">深度</param>
        /// <returns>材质</returns>
        public static Material getSolidMaterial(EMaterailColor color,EMaterialColorDeep deep)
        {
            Material mat;
            string resname = "mat";
            switch (deep)
            {
                case EMaterialColorDeep.半透:
                    resname += "Trans";
                    break;
                case EMaterialColorDeep.浅色:
                    resname += "Light";
                    break;
                case EMaterialColorDeep.深色:
                    resname += "Dark";
                    break;
            }
            
            switch (color)
            {
                case EMaterailColor.白:
                    resname += "White";
                    break;
                case EMaterailColor.黑:
                    resname += "Black";
                    break;
                case EMaterailColor.红:
                    resname += "Red";
                    break;
                case EMaterailColor.黄:
                    resname += "Yellow";
                    break;
                case EMaterailColor.灰:
                    resname += "Gray";
                    break;
                case EMaterailColor.蓝:
                    resname += "Blue";
                    break;
                case EMaterailColor.绿:
                    resname += "Green";
                    break;
                case EMaterailColor.紫:
                    resname += "Purple";
                    break;
                case EMaterailColor.褐:
                    resname += "Brown";
                    break;
            }

            mat = (Material)Application.Current.FindResource(resname);
            return mat;
        }

        public static Material getValueColorMaterial(double min, double max, double value, EMaterialValueToColorMode mode, Color solidcolor)
        {
            Color color=Colors.Pink ;
            double scale = value / (max - min);
            switch (mode)
            { 
                case EMaterialValueToColorMode.蓝到红:
                    color = getColorBetween(scale, Colors.Blue, Color.FromRgb(0xFF, 0x00, 0x01));
                    break;
                case EMaterialValueToColorMode.绿到红:
                    color = getColorBetween(scale, Colors.Green, Colors.Red);
                    break;
                case EMaterialValueToColorMode.浅到深:
                    color = getColorSaturation(scale, solidcolor);
                    break;
                case EMaterialValueToColorMode.透明到不透明:
                    color = Color.FromArgb((byte)(255 * scale), solidcolor.R, solidcolor.G, solidcolor.B);
                    break;
            }

            MaterialGroup mat = new MaterialGroup();
            mat.Children.Add(new DiffuseMaterial(new SolidColorBrush(color)));
            mat.Children.Add(new SpecularMaterial(Brushes.White, 80));


            return mat;
        }

        /// <summary>
        /// 按比例，获取两个色之间的色
        /// </summary>
        /// <param name="scale">比例, 0-1之间的数据</param>
        /// <param name="colorStart">开始颜色</param>
        /// <param name="colorEnd">结束颜色</param>
        /// <returns></returns>
        public static Color getColorBetween(double scale, Color colorStart, Color colorEnd)
        {
            Color color;
            System.Drawing.Color cStart = System.Drawing.Color.FromArgb(colorStart.A, colorStart.R, colorStart.G, colorStart.B);
            System.Drawing.Color cEnd = System.Drawing.Color.FromArgb(colorEnd.A, colorEnd.R, colorEnd.G, colorEnd.B);
            float hstart = cStart.GetHue();
            float hend = cEnd.GetHue();
            float hue = hstart + (hend - hstart) * (float)scale;
            System.Drawing.Color dc = HSBColor.FromHSB(new HSBColor(255, hue*255/360, 255f, 255f));
            color = Color.FromArgb(dc.A, dc.R, dc.G, dc.B);
            return color;
        }

        /// <summary>
        /// 按比例，获取红蓝两色之间的渐变色，中间使用白色过渡，适用于表现气温、负载等
        /// </summary>
        /// <param name="scale">比例, 0-1之间的数据</param>
        /// <returns></returns>
        public static Color getColorBetweenRedBlue(double scale)
        {
            float h,s;
            //Color c = System.Drawing.Color.FromArgb(0x10, 0x00, 0xDC);
            if (scale>0.5)
            {
                float ss = (float)(scale - 0.5) * 2;
                h = 60f - 60f * ss;
                s = 255f * ss;
            }
            else
            {
                float ss = (float)scale * 2;
                h = 240 - 60 * ss;
                s = 255f * (1 - ss);
            }

            System.Drawing.Color dc = HSBColor.FromHSB(new HSBColor(255, h * 255 / 360, s, 255));
            Color color = Color.FromArgb(dc.A, dc.R, dc.G, dc.B);
            return color;
        }


        /// <summary>
        /// 按比例，获取指定色以比例为饱和度的色
        /// </summary>
        /// <param name="scale">比例, 0-1</param>
        /// <param name="orgcolor">原始色</param>
        /// <returns></returns>
        public static Color getColorSaturation(double scale, Color orgcolor)
        {
            Color color;
            System.Drawing.Color cOrg = System.Drawing.Color.FromArgb(orgcolor.A, orgcolor.R, orgcolor.G, orgcolor.B);
            //color = FromAhsb(255, cOrg.GetHue(), (float)scale, 0.5f);
            System.Drawing.Color dc= HSBColor.FromHSB(new HSBColor(255, cOrg.GetHue()/360*255,(float)scale*255,255f));
            color = Color.FromArgb(dc.A, dc.R, dc.G, dc.B);
            return color;
        }



        public static Material genArrowMat(Color color)
        {
            Grid grd = new Grid();
            Path path = new Path();
            //path.Data = Geometry.Parse("M30,77.573 L34,62.906333 0.5,79.375213 27.333333,125.573 27.333333,106.23967 C27.333333,106.23967 282.33276,232.66934 420,83.573042 421.51689,81.930216 375,76.572719 375,76.572719 375,76.572719 324.56926,-2.2201693 322,0.57252077 249,131.57221 31.206468,77.549271 30,77.573 z");
            path.Data = Geometry.Parse("M30,77.573 L34,53 0.5,79.375213 27.333333,125.573 27.333333,106.23967 C27.333333,106.23967 282.33276,232 420,160 421.51689,81.930216 420,76.572719 420,76.572719 420,76.572719 420,0 420,0.57252077 249,131.57221 31.206468,77.549271 30,77.573 z");
            
            Color chead = color;
            Color ctail = Color.FromArgb(0, color.R, color.G, color.B);
            LinearGradientBrush fillbrush = new LinearGradientBrush(chead, ctail, new Point(0, 1), new Point(1, 1));
            fillbrush.GradientStops.Add(new GradientStop(ctail, 0.9));
            LinearGradientBrush strokebrush = new LinearGradientBrush(Colors.Black, Color.FromArgb(0, 196, 196, 196), new Point(0, 1), new Point(1, 1));
            strokebrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 196, 196, 255), 0.9));
            path.Fill = fillbrush;
            path.Stroke = strokebrush;
            path.StrokeThickness = 1;

            int w = (int)path.Data.Bounds.Width;
            int h = (int)path.Data.Bounds.Height;
            grd.Children.Add(path);

            grd.Measure(new System.Windows.Size(w, h));
            grd.Arrange(new Rect(0, 0, w, h));
            System.Windows.Media.Brush brush = null;
            RenderTargetBitmap renderTarget = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(grd);
            renderTarget.Freeze();
            brush = new ImageBrush(renderTarget);

            MaterialGroup mat = new MaterialGroup();
            mat.Children.Add(new DiffuseMaterial(brush));
            return mat;
        }




    }
}
