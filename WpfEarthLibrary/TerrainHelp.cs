using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using draw = System.Drawing;

namespace WpfEarthLibrary
{
    public static class TerrainHelp
    {

        internal static Dictionary<string, draw.Bitmap> dicImages = new Dictionary<string, draw.Bitmap>();

      


        #region ===== 按经纬的方式（有误差） =====
        ///<summary>获取瓦片节点的高度列表</summary>
        internal static List<float> getHeigList(EarthData node)
        {
            List<float> result = new List<float>();

            float jd, wd;
            double spanjd = node.range.Width / node.terrainSliceCount;
            for (int j = 0; j < node.terrainSliceCount + 1; j++)
            {
                for (int i = 0; i < node.terrainSliceCount + 1; i++)
                {
                    jd =(float)(node.range.X + spanjd * i);
                    double tileLength = 2.0 * Math.PI / Math.Pow(2,node.layer); //2*pi/power(2,layer)  假定半径为1, 平面地图坐标系下该层划分块的长和宽度
                    wd = (float)((Math.Atan(Math.Exp(Math.PI - (node.idxy + (float)j / node.terrainSliceCount) * tileLength)) * 2 - Math.PI / 2) / Math.PI * 180);
                    result.Add(getHigh(jd, wd, node.earthmanager.TerrainMinHeight, node.earthmanager.TerrainDropHeight));

                }
            }
            return result;
        }

        ///<summary>获取指定经纬度的高度（内部坐标系）</summary>
        internal static float getHigh(float jd, float wd, float minHeight, float dropHeight)
        {
            //查找高程图或新开高程图
            string picFile = getPicFile(jd, wd);
            draw.Bitmap bitmap;
            if (!dicImages.TryGetValue(picFile, out bitmap))
            {
                bitmap = new draw.Bitmap(picFile, false);
                dicImages.Add(picFile, bitmap);
            }


            int x, y;
            float startjd = -180 + ((int)((jd + 180) / 5)) * 5; //高程图开始经度
            float startwd = 60 - ((int)((60 - wd) / 5)) * 5;
            x = (int)((jd - startjd) / 5 * 6000);
            y = (int)((startwd - wd) / 5 * 6000);

            return minHeight + bitmap.GetPixel(x, y).GetBrightness() * dropHeight;
        }

        ///<summary>获取指定经纬所在的高程图片编号</summary>
        static string getPicFile(float jd, float wd)
        {
            int idxx = (int)((jd + 180) / 5) + 1; //经度从-180到180，每5度一格
            int idxy = (int)((60 - wd) / 5) + 1; //纬度从-60到60，每5度一格
            return String.Format("srtm_{0:00}_{1:00}.tif", idxx, idxy);
        }
        #endregion
    }
}
