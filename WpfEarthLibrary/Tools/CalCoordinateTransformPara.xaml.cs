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

namespace WpfEarthLibrary.Tools
{
    /// <summary>
    /// CalCoordinateTransformPara.xaml 的交互逻辑
    /// </summary>
    public partial class CalCoordinateTransformPara : UserControl
    {
        public CalCoordinateTransformPara()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            tlayer.Text = "0";
            tidxwd.Text = "0";
            tidxjd.Text = "0";
            tminwd.Text = "22.2115579";
            tmaxwd.Text = "25.8151361";
            tminjd.Text = "111.8917712";
            tmaxjd.Text = "115.4953494";
        }

        private void btncal_Click(object sender, RoutedEventArgs e)
        {
            int layer = int.Parse(tlayer.Text);
            int idxwd = int.Parse(tidxwd.Text);
            int idxjd = int.Parse(tidxjd.Text);
            double minjd = double.Parse(tminjd.Text);
            double minwd = double.Parse(tminwd.Text);
            double maxjd = double.Parse(tmaxjd.Text);
            double maxwd = double.Parse(tmaxwd.Text);

            double spanjd = maxjd - minjd;
            double spanwd = maxwd - minwd;

            //计算最合适的对应层
            int ycount; double angle, tileLength, xStart, xEnd, yStart, yEnd;
            int si = 0; double cz = double.PositiveInfinity;
            double divjd = 0, divwd = 0, sxstart = 0, sxend = 0, systart = 0, syend = 0;
            for (int i = 0; i < 18; i++)
            {
                int idxx = 0;int idxy = 0;
                ycount = (int)Math.Pow(2, i);
                angle = MapHelper.DegToRad(360.0 / ycount);
                tileLength = 2.0 * Math.PI / Math.Pow(2, i); //2*pi/power(2,layer)  假定半径为1, 平面地图坐标系下该层划分块的长和宽度
                xStart = (idxx * angle - Math.PI) / Math.PI * 180;//  指定索引号块的起始经度
                xEnd = ((idxx + 1) * angle - Math.PI) / Math.PI * 180;//  指定索引号块的起始经度
                yStart = (Math.Atan(Math.Exp(Math.PI - (idxy + 1) * tileLength)) * 2 - Math.PI / 2) / Math.PI * 180; //指定索引号块的结束纬度
                yEnd = (Math.Atan(Math.Exp(Math.PI - idxy * tileLength)) * 2 - Math.PI / 2) / Math.PI * 180; //atan(exp(PI-idxy*2*pi/power(2,layer)))*2-pi/2  指定索引号块的起始纬度    
                double tmp = Math.Abs(xEnd - xStart - spanjd) + Math.Abs(yStart - yEnd - spanwd);
                if (tmp < cz) { cz = tmp; si = i; }
            }
            //计算合适的瓦片序号
            ycount = (int)Math.Pow(2, si);
            angle = MapHelper.DegToRad(360.0 / ycount);
            int sidxx = (int)((minjd / 180 * Math.PI + Math.PI) / angle);
            tileLength = 2.0 * Math.PI / Math.Pow(2, si); //2*pi/power(2,layer)  假定半径为1, 平面地图坐标系下该层划分块的长和宽度
            int sidxy =(int)( (Math.PI - Math.Log(Math.Tan((minwd / 180 * Math.PI + Math.PI / 2) / 2)))/tileLength );
            //计算合适的瓦片信息
            ycount = (int)Math.Pow(2, si);
            angle = MapHelper.DegToRad(360.0 / ycount);
            tileLength = 2.0 * Math.PI / Math.Pow(2, si); //2*pi/power(2,layer)  假定半径为1, 平面地图坐标系下该层划分块的长和宽度
            xStart = (sidxx * angle - Math.PI) / Math.PI * 180;//  指定索引号块的起始经度
            xEnd = ((sidxx + 1) * angle - Math.PI) / Math.PI * 180;//  指定索引号块的起始经度
            yStart = (Math.Atan(Math.Exp(Math.PI - (sidxy + 1) * tileLength)) * 2 - Math.PI / 2) / Math.PI * 180; //指定索引号块的结束纬度
            yEnd = (Math.Atan(Math.Exp(Math.PI - sidxy * tileLength)) * 2 - Math.PI / 2) / Math.PI * 180; //atan(exp(PI-idxy*2*pi/power(2,layer)))*2-pi/2  指定索引号块的起始纬度    
            divjd = xEnd - xStart; divwd = yEnd - yStart; sxstart = xStart; sxend = xEnd; systart = yStart; syend = yEnd;

            //瓦片信息
            txtTileResult.Text = string.Format("瓦片设置参数earth.earthManager.earthpara：层偏移(tileFileOffsetLI={0}),经度序号偏移(tileFileOffsetXI={1}),纬度序号偏移(tileFileOffsetYI={2})", 
                layer - si, idxjd - sidxx, idxwd - sidxy);

            //坐标转换信息
            txtCoorResult.Text = string.Format("坐标转换设置参数 earth.coordinateManager：原点经度(orgJD={0}),原点纬度(orgWD={1}),经度缩放比例(scaleJD={2}),纬度缩放比例(scaleJD={3}),经度偏移(offsetJD={4}),纬度偏移(offsetJD={5})",
                minjd, minwd, divjd / spanjd, divwd / spanwd, sxstart - minjd, systart - minwd);

            //目标瓦片信息
            txtInfo.Text = string.Format("目标瓦片：{0}-{1}-{2}, {3}-{4}, {5}-{6}", si,sidxx,sidxy, xStart,xEnd,yStart,yEnd);




     
        }

    
    }
}
