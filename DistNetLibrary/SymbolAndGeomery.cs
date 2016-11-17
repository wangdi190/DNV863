using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using WpfEarthLibrary;

namespace DistNetLibrary
{
    ///<summary>预定义的图元名称</summary>
    public enum ESymbol { 小圆圈, 节点电压_渐变圆, 断路器分, 断路器合, 负荷开关分, 负荷开关合, 隔离开关分, 隔离开关合, 环网箱式变, 配变, 熔断器, 三绕组变压器, 双绕组变压器, 箱式变, 运行杆塔耐张型, 运行杆塔直线型, 终端站, 柱上公用变, 柱上专用变, 变电站规划, 变电站运行, 充电站, 分界室, 分界箱, 风力发电规划, 风力发电运行, 光伏发电规划, 光伏发电运行, 开关站规划, 开关站运行, 垃圾发电规划, 垃圾发电运行, 配电室 };
    ///<summary>预定义的几何体名称</summary>
    public enum EGeometry { 立方体, 圆柱体, 圆锥体, 倒锥体, 正锥体 }

    class SymbolAndGeomery
    {
        ///<summary>创建图元符号</summary>
        internal static void CreateSymbol(Earth scene)
        {
            var pngs=System.IO.Directory.EnumerateFiles(".\\symbols","*.png");
            foreach (var png in pngs)
            {
                string ddsname = png.Replace(".png", ".dds");
                string name=png.Replace(@".\symbols\","").Replace(".png","");
                string sort;
                if (name.Contains('_'))
                {
                    sort=name.Substring(0,name.IndexOf('_'));
                    name = name.Substring(name.IndexOf('_')+1);
                }
                else
                    sort="未分类";
                if (System.IO.File.Exists(ddsname))
                    scene.objManager.AddSymbol(sort, name, png, ddsname);
                else
                     scene.objManager.AddSymbol(sort, name, png); 
            }
                    

            RadialGradientBrush brush = new RadialGradientBrush();
            brush.GradientStops.Add(new GradientStop(Colors.White, 0.2));
            brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));
            scene.objManager.AddSymbol("系统内部使用","节点电压_渐变圆", brush, 64, 64);
        }

        ///<summary>创建几何体资源</summary>
        internal static void CreateGeometry(Earth scene)
        {
            scene.objManager.AddBoxResource("立方体", 1, 1, 1);
            scene.objManager.AddCylinderResource("圆柱体", 1, 1, 1, 16, 1);
            scene.objManager.AddCylinderResource("圆锥体", 0, 1, 1, 16, 1);
            scene.objManager.AddCylinderResource("倒锥体", 0, 1, 1, 4, 1);
            scene.objManager.AddCylinderResource("正锥体", 1, 0, 1, 4, 1);
            scene.objManager.AddSphereResource("球体", (float)(Math.PI * 2), 16, 16);
        }

    }
}
