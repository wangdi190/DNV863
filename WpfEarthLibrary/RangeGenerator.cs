using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfEarthLibrary
{
    /// <summary>
    /// 范围生成，可借用pContour配合使用
    /// 相当于把范围图当作是等值图
    /// </summary>
    public class RangeGenerator
    {
        public RangeGenerator()
        {
            canv = new System.Windows.Controls.Canvas();
        }
        System.Windows.Controls.Canvas canv;

        public class StruDrawObjDesc
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="stringpoints">表示点集的字串</param>
            /// <param name="radius">在画布上的扩散半径</param>
            public StruDrawObjDesc(string stringpoints, double radius)
            {
                strPoints = stringpoints;
                rad = radius;
            }

            ///<summary>对象的点集</summary>
            public string strPoints { get; set; }
            ///<summary>在画布上的扩散半径(单独扩散暂无效，请使用整体扩散半径)</summary>
            public double rad { get; set; }


                 

            internal List<Point> listpnts { get; set; }

            internal PointCollection pc { get; set; }
        }



        private List<StruDrawObjDesc> _drawObjects = new List<StruDrawObjDesc>();
        ///<summary>将绘制范围的对象的描述集合</summary>
        public List<StruDrawObjDesc> drawObjects
        {
            get { return _drawObjects; }
            set { _drawObjects = value; }
        }


        ///<summary>生成的范围色彩画刷</summary>
        public VisualBrush RangeBrush { get; set; }

        private double _rad = 20;
        ///<summary>整体扩散半径系数</summary>
        public double rad
        {
            get { return _rad; }
            set { _rad = value; }
        }


        private Brush _brush = new SolidColorBrush(Color.FromArgb(0x80, 0xE8, 0xFF, 0x00));
        ///<summary>用于范围着色的画刷</summary>
        public Brush brush
        {
            get { return _brush; }
            set { _brush = value; }
        }

        private int _size = 1024;
        ///<summary>画布大小，缺省1024</summary>
        public int size
        {
            get { return _size; }
            set { _size = value; }
        }

        private double _blueRadius = 0;
        ///<summary>模糊半径</summary>
        public double blueRadius
        {
            get { return _blueRadius; }
            set { _blueRadius = value; }
        }

        private int _layerCount = 1;
        ///<summary>范围分层数</summary>
        public int layerCount
        {
            get { return _layerCount; }
            set { _layerCount = value; }
        }

        
        private int _blurFactor=200;
        ///<summary>模糊系数，缺省200，越小越模糊，模糊半径为 size/模糊系数</summary>
        public int blurFactor
        {
            get { return _blurFactor; }
            set { _blurFactor = value; }
        }
      
        


        ///<summary>画布对应的最小经度</summary>
        public double minx;
        ///<summary>画布对应的最小纬度</summary>
        public double miny;
        ///<summary>画布对应的最大经度</summary>
        public double maxx;
        ///<summary>画布对应的最大纬度</summary>
        public double maxy;



        public void GenRangeBrush()
        {
            //处理点集
            for (int i = 0; i < drawObjects.Count; i++)
            {
                StruDrawObjDesc objdesc = drawObjects[i];
                string[] ss = objdesc.strPoints.Split(' ');
                int pointcount = ss.Length;
                List<Point> points = new List<Point>();
                Point pnt = new Point(0, 0);
                for (int j = 0; j < pointcount; j++)
                {
                    pnt = Point.Parse(ss[j]);
                    points.Add(pnt);
                }
                objdesc.listpnts = points;
            }


            //miny = dots.Min(p => p.location.X); maxy = dots.Max(p => p.location.X);  //将经度换为X坐标, 纬度换为Y坐标
            //minx = dots.Min(p => p.location.Y); maxx = dots.Max(p => p.location.Y);
            miny = drawObjects.Min(p1 => p1.listpnts.Min(p2 => p2.X)); maxy = drawObjects.Max(p1 => p1.listpnts.Max(p2 => p2.X));
            minx = drawObjects.Min(p1 => p1.listpnts.Min(p2 => p2.Y)); maxx = drawObjects.Max(p1 => p1.listpnts.Max(p2 => p2.Y));
            double w = maxx - minx; double h = maxy - miny;
            //minx = minx - w * 0.2; maxx = maxx + w * 0.2;
            //miny = miny - h * 0.2; maxy = maxy + h * 0.2;

            minx = minx - 0.01; maxx = maxx + 0.01; //改为扩展固定大小
            miny = miny - 0.01; maxy = maxy + 0.01;

            w = maxx - minx; h = maxy - miny;
            //经纬换为屏幕坐标
            int size = 1024;
            double ww = w > h ? size : size * w / h;
            double hh = w > h ? size * h / w : size;
            double scalerad = size / Math.Max(w, h) / 10000;
            Size canvSize = new Size(ww, hh);

            for (int i = 0; i < drawObjects.Count; i++)
            {
                StruDrawObjDesc objdesc = drawObjects[i];
                List<Point> lp = objdesc.listpnts;
                PointCollection pc = new PointCollection();
                int pointcount = lp.Count;
                foreach (Point pp in lp)
                {
                    Point tmp = new Point((pp.Y - minx) / w * canvSize.Width, (maxy - pp.X) / h * canvSize.Height);  //重新赋与新的平面点位置, 注，纬度取反，仅适用北半球
                    pc.Add(tmp);
                }

                objdesc.pc = pc;
                objdesc.listpnts = null;
            }


            //绘制
            canv.Children.Clear();
            canv.Children.Add(new Rectangle() { Width = canvSize.Width, Height = canvSize.Height, StrokeThickness = 10, Stroke = Brushes.Transparent });


            //生成geo
            PathGeometry pg = new PathGeometry();
            foreach (StruDrawObjDesc objdesc in drawObjects)
            {
                PathFigure pf = new PathFigure();
                pg.Figures.Add(pf);
                pf.Segments.Add(new PolyLineSegment());
                pf.StartPoint = objdesc.pc[0];
                (pf.Segments[0] as PolyLineSegment).Points = objdesc.pc;
            }


            for (int i = layerCount-1; i >=0; i--)
            {
                Path path = new Path();
                path.Data = pg;
                path.Stroke = brush;
                path.StrokeStartLineCap = path.StrokeEndLineCap = PenLineCap.Round;
                path.StrokeLineJoin = PenLineJoin.Round;
                path.StrokeThickness = rad * scalerad *(i + 1) / layerCount;
                //path.Opacity = ((1.0f*layerCount - i ) / layerCount);
                path.Fill = Brushes.Transparent;

                path.Effect = new BlurEffect() { Radius = blueRadius * scalerad };
                
                canv.Children.Add(path);
            }
            canv.Effect = new BlurEffect() { Radius=Math.Max(canvSize.Width, canvSize.Height)/blurFactor };

            RangeBrush = new VisualBrush();
            RangeBrush.Visual = canv;

            canv.Measure(canvSize);
            canv.Arrange(new System.Windows.Rect(0, 0, canvSize.Width, canvSize.Height));

            canv.Measure(canvSize);
            canv.Arrange(new System.Windows.Rect(0, 0, canvSize.Width, canvSize.Height));




        }

    }
}
