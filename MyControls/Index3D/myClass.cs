using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Text;

namespace MyControlLibrary.Controls3D.Index3D
{
    /// <summary>
    /// 指标分类集合，在十二面体的一个面，特定的一种情况是无指标，只显示图形
    /// </summary>
    class indexPlane
    {
        List<indexPoint> _lstindex=new List<indexPoint>();
        ///<summary>指标分类名称</summary>
        public string title { get; set; }
        public ModelVisual3D modelgroup { get; set; }  //3D场景中的一个面，其content为modelgroup, 其中第一个子元素为面板五边形model，第二个为点组和底圆盘
        public ModelVisual3D colgroup { get; set; }  // 放置col, 以解决半透
        ///<summary>指标面序号</summary>
        public int planeidx { get; set; } //面的序号，通过序号得到angle, angle不用直接赋值, 应确保和xml中模型定义一致
        ///<summary>面材质序号</summary>
        public int matidx { get; set; }  //面的背景材质序号
        ///<summary>指标点的分布圆半径</summary>
        public List<double> radius { get; set; }
        public indexPlane()
        {
            radius = new List<double>();
        }

        public indexPoint addChildren(string indexname)
        {
            indexPoint sub = new indexPoint(this, indexname);
            _lstindex.Add(sub);
            return sub;
        }

        public List<indexPoint> getChildren()
        {
            return _lstindex;
        }

        ///<summary>计算指标分布位置</summary>
        public void calPoint() 
        {
            if (_lstindex.Count == 0) return;
            int maxlayer = _lstindex.Max<indexPoint>(p => p.indexlayer);
            double b = 0;
            for (int i = 0; i <= maxlayer; i++)
            {
                b = b + Math.Pow(0.8, i);
            }
            b = 2 * b;
            int indexcount = _lstindex.Count<indexPoint>(p => p.indexlayer == 0);
            double sina =Math.Sin(Math.PI / indexcount);
            double r0 = r0 = 1 / (1 - sina + b*sina);
            double r1 = r0 * sina;
            r0 = r0 - r1;

            double r = r0;
            double baseangle = 0;
            for (int i = 0; i <= maxlayer; i++)
            {
                var l = _lstindex.Where<indexPoint>(p => p.indexlayer == i);
                indexcount = _lstindex.Count<indexPoint>(p => p.indexlayer == i);
                double ther = r1 *Math.Pow(0.8, i);
                r = r + ther;
                radius.Add(r);
                int count = 0;
                foreach (indexPoint one in l)
                {
                    one.radius = ther;
                    one.angle = 360.0 * count / indexcount+baseangle;
                    one.pcenter = new Point(r * Math.Cos(one.angle/180* Math.PI ), r * Math.Sin(one.angle/180 * Math.PI ));
                    count++;
                }
                baseangle = 360.0 / indexcount / 2;
                r = r + ther;
            }            
        }

        ///<summary>面的初始旋转角度，以确保平铺模式时，文字向上的角度</summary>
        public double getTextAngle()   
        {
            double angle = 0;
            switch (planeidx)
            {
                case 0:
                    angle = 0;
                    break;
                case 1:
                    angle = -108;
                    break;
                case 2:
                    angle = 108;
                    break;
                case 3:
                    angle = -108;
                    break;
                case 4:
                    angle = 108;
                    break;
                case 5:
                    angle = 108;
                    break;
                case 6:
                    angle = -72;
                    break;
                case 7:
                    angle = 180;
                    break;
                case 8:
                    angle = 72;
                    break;
                case 9:
                    angle = -72;
                    break;
                case 10:
                    angle = 72;
                    break;
                case 11:
                    angle = -72;
                    break;
            }
            return angle;
        }


    }

    ///<summary>指标点类</summary>
    class indexPoint
    {
        ///<summary>引用数据模型中指标</summary>
        public zIndex indexdata;// 引用数据模型中指标

        ///<summary>指标名</summary>
        public string indexname { get; set; }  //指标名
        ///<summary>指标值</summary>
        public double value { get; set; }  //指标值，如频率差百分比
        ///<summary>指标格式化串</summary>
        public string format { get; set; } //指标格式化串
        ///<summary>参考类型</summary>
        public string referType { get; set; } //参考类型
        ///<summary>参考值1 如2%</summary>
        public Nullable<double> refer1 { get; set; } //参考值1 如2%
        ///<summary>参考值2 如3%</summary>
        public Nullable<double> refer2 { get; set; } //参考值2 如3%
        ///<summary>参考值说明</summary>
        public string referNote { get; set; }
        ///<summary>指标定义</summary>
        public string definition { get; set; }  //指标定义
        ///<summary>模型类型，百分比，比值，数值</summary>
        public string modelType { get; set; }   //模型类型，百分比，比值，数值
        ///<summary>细节视图名</summary>
        public string viewType { get; set; }   //细节视图名
        ///<summary>细节视图附加信息字符串</summary>
        public string viewInfo { get; set; } //细节视图附加信息字符串
        ///<summary>细节视图sql</summary>
        public string viewSQL { get; set; } //细节视图sql
        ///<summary>值计算的sql </summary>
        public string valueSQL { get; set; } //值计算的sql 
        ///<summary>指标以0为起始的层级号，按层级总数和指标数，自动计算以1为半径圆内的分布点和本指标分配的最大半径</summary>
        public int indexlayer { get; set; } // 指标以0为起始的层级号，按层级总数和指标数，自动计算以1为半径圆内的分布点和本指标分配的最大半径
        ///<summary>标志，是否绘提示渐变园,0：正常，1:黄 2:红</summary>
        public int flag { get; set; }  //标志，是否绘提示渐变园,0：正常，1:黄 2:红
        ///<summary>2d平面上的分布中心点</summary>
        public Point pcenter { get; set; }//2d平面上的分布中心点
        ///<summary>指标点角度</summary>
        public double angle { get; set; } //指标点角度
        ///<summary>2d平面上的本指标可呈现的最大半径</summary>
        public double radius { get; set; } //2d平面上的本指标可呈现的最大半径
        ///<summary>所属指标组或指标面板</summary>
        public indexPlane owner { get; set; }  //所属指标组或指标面板
        public Model3DGroup model3d { get; set; }   //3d模型组   
        public UserControl detail { get; set; }  //怎么实现，再考虑
        ///<summary>仪表盘信息</summary>
        public GaugeInfo gaugeInfo { get; set; } //仪表盘信息
        public indexPoint(indexPlane own,string name)
        {
            indexname = name.Trim();
            owner = own;
            gaugeInfo = new GaugeInfo();
        }
    }

    ///<summary>仪表数据类</summary>
    public class GaugeInfo
    {
        public double value;
        public double[,] range=new double[3,2];
        public Brush[] rangeBrush=new SolidColorBrush[3];
        public double startValue;
        public double endValue;
        public string labelFormat;
        public string title;
        public Brush titleBrush;
    }

    


    #region 指标数据类
    ///<summary>指标数据类</summary>
    public class zIndex
    {
        public string indexSort { get; set; }
        public string indexSort2 { get; set; }
        public string indexName { get; set; }
        public string indexDefine { get; set; }
        public string indexUnit { get; set; }
        public Nullable<double> refer1 { get; set; }
        public Nullable<double> refer2 { get; set; }
        public string refertype { get; set; }
        public string refernote { get; set; }
        public bool iskpi { get; set; }
        public double important { get; set; }
        public string format { get; set; }
        public string modelType { get; set; }
        public string viewType { get; set; }
        public string viewInfo { get; set; }
        public string viewSQL { get; set; }
        public string valueSQL { get; set; }
        public double minValue { get; set; }
        public double maxValue { get; set; }


        public List<struindex> indexValues = new List<struindex>();

        //计算指标值
        public double calIndexValue(DateTime db, DateTime de)
        {
            double result = 0;
            //double? tmp;
            //tmp = indexhelper.calIndexValueMarket(indexName, db, de);
            //if (tmp == null)
            //    tmp = indexhelper.calIndexValueOther(indexName, db, de);
            //if (tmp == null)
            //    tmp = indexhelper.calIndexValueSystem(indexName, db, de);
            //if (tmp != null)
            //    result = (double)tmp;
            return result;
        }
        //获得细节列表
        public List<TRow> getDetailData(DateTime db, DateTime de)
        {
            List<TRow> tmp = null;
            //tmp = indexhelper.getIndexDetailMarket(indexName, db, de);
            //if (tmp == null)
            //    tmp = indexhelper.getIndexDetailOther(indexName, db, de);
            //if (tmp == null)
            //    tmp = indexhelper.getIndexDetailSystem(indexName, db, de);
            return tmp;
        }

    }

    /// <summary>
    /// 指标细节视图数据结构
    /// </summary>
    public class TRow
    {
        public DateTime zdate;
        public string zname;
        public double zvalue1;
        public double zvalue2;
        public double zvalue3;
    }


    public class struindex
    {
        public string iKey { get; set; } //此处为年
        public double? indexValue { get; set; }

    }

    #endregion


}
