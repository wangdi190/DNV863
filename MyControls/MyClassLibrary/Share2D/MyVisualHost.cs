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
using System.Globalization;

namespace MyClassLibrary.Share2D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    //========== 使用drawingcontext绘制的对象
    public class MyVisualHost : FrameworkElement
    {
        // Create a collection of child visual objects.
        private VisualCollection _children;

        public void ClearChildren()
        {
            _children.Clear();
        }

        public MyVisualHost()
        {
            _children = new VisualCollection(this);
        }
        // 矩形
        public void CreateDrawingVisualRectangle(Rect rRect,Brush rBrush,Pen rPen)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawRectangle(rBrush, rPen, rRect);
            drawingContext.Close();
            _children.Add(drawingVisual);
        }
        //文字
        public void CreateDrawingVisualText(string dText,Typeface dTypeface,double dFontSize,Brush dBrush,Point dPoint)
        {
            if (dTypeface == null) {dTypeface= new Typeface("Verdana"); }
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawText(
               new FormattedText(dText,
                  CultureInfo.GetCultureInfo("en-us"),
                  FlowDirection.LeftToRight,
                  dTypeface,
                  dFontSize, dBrush),
                  dPoint);
            drawingContext.Close();
            _children.Add(drawingVisual);
        }

        // 椭圆
        public void CreateDrawingVisualEllipses(Brush eBrush,Pen ePen,Point eCenter,double radX,double radY)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawEllipse(eBrush, ePen, eCenter, radX, radY);
            drawingContext.Close();
            _children.Add(drawingVisual);
        }
        // 线
        public void CreateDrawingVisualLine(Pen lPen,Point pb,Point pe)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawLine(lPen,pb,pe);
            drawingContext.Close();
            _children.Add(drawingVisual);
        }
        // 几何图形
        public void CreateDrawingVisualGeometry(Brush gBrush,Pen gPen, Geometry geo)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawGeometry(gBrush,gPen,geo);
            drawingContext.Close();
            _children.Add(drawingVisual);
        }

        // 旗帜
        public void CreateDrawingVisualFlag(Brush gBrush, Pen gPen, Point startPoint, Size flagSize)
        {
            //旗 M0,20 C50,-20 100,40 150,0 L150,100 C100,120 50,80 0,120
            PathGeometry flaggeo = new PathGeometry();
            PathFigure ffg = new PathFigure();
            ffg.StartPoint = new Point(startPoint.X, startPoint.Y+20);
            ffg.Segments.Add(new BezierSegment(new Point(startPoint.X + flagSize.Width / 3, startPoint.Y - 20), new Point(startPoint.X + flagSize.Width * 2 / 3, startPoint.Y + 20), new Point(startPoint.X + flagSize.Width, startPoint.Y), true));
            ffg.Segments.Add(new LineSegment(new Point(startPoint.X + flagSize.Width, startPoint.Y+flagSize.Height), true));
            ffg.Segments.Add(new BezierSegment(new Point(startPoint.X + flagSize.Width * 2 / 3, startPoint.Y + flagSize.Height + 20), new Point(startPoint.X + flagSize.Width / 3, startPoint.Y + flagSize.Height - 20), new Point(startPoint.X, startPoint.Y + flagSize.Height + 20), true));
            flaggeo.Figures.Add(ffg);
            flaggeo.Freeze();

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawGeometry(gBrush, gPen, flaggeo);
            drawingContext.Close();
            _children.Add(drawingVisual);
        }
        // 三角形
        public void CreateDrawingTriangle(Brush rBrush, Pen rPen, Point rCenter, double rWidth, double rHeight)
        {
            PathGeometry flaggeo = new PathGeometry();
            PathFigure ffg = new PathFigure { StartPoint = new Point(rCenter.X, rCenter.Y - rHeight) };
            ffg.Segments.Add(new LineSegment(new Point(rCenter.X - rWidth / 2, rCenter.Y ), true));
            ffg.Segments.Add(new LineSegment(new Point(rCenter.X + rWidth / 2, rCenter.Y ), true));
            ffg.Segments.Add(new LineSegment(new Point(rCenter.X , rCenter.Y-rHeight), true));
            flaggeo.Figures.Add(ffg);
            flaggeo.Freeze();

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawGeometry(rBrush, rPen, flaggeo);
            drawingContext.Close();
            _children.Add(drawingVisual);
        }
        // 倒三角形
        public void CreateDrawingTriangle2(Brush rBrush, Pen rPen, Point rCenter, double rWidth, double rHeight)
        {
            PathGeometry flaggeo = new PathGeometry();
            PathFigure ffg = new PathFigure { StartPoint = new Point(rCenter.X-rWidth/2, rCenter.Y - rHeight) };
            ffg.Segments.Add(new LineSegment(new Point(rCenter.X, rCenter.Y), true));
            ffg.Segments.Add(new LineSegment(new Point(rCenter.X+ rWidth / 2, rCenter.Y-rHeight), true));
            ffg.Segments.Add(new LineSegment(new Point(rCenter.X-rWidth/2, rCenter.Y - rHeight), true));
            flaggeo.Figures.Add(ffg);
            flaggeo.Freeze();

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawGeometry(rBrush, rPen, flaggeo);
            drawingContext.Close();
            _children.Add(drawingVisual);
        }
        // 里程碑
        public void CreateDrawingMileStone(Brush rBrush, Pen rPen,Point rCenter,double rWidth,double rHeight)
        {
            PathGeometry flaggeo = new PathGeometry();
            PathFigure ffg = new PathFigure { StartPoint = new Point(rCenter.X, rCenter.Y - rHeight / 2) };
            ffg.Segments.Add(new LineSegment(new Point(rCenter.X - rWidth / 2, rCenter.Y - rHeight / 6), true));
            ffg.Segments.Add(new LineSegment(new Point(rCenter.X - rWidth / 2, rCenter.Y + rHeight / 2), true));
            ffg.Segments.Add(new LineSegment(new Point(rCenter.X + rWidth / 2, rCenter.Y + rHeight / 2), true));
            ffg.Segments.Add(new LineSegment(new Point(rCenter.X + rWidth / 2, rCenter.Y - rHeight / 6), true));
            ffg.Segments.Add(new LineSegment(new Point(rCenter.X , rCenter.Y - rHeight / 2), true));
            flaggeo.Figures.Add(ffg);
            flaggeo.Freeze();

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawGeometry(rBrush, rPen, flaggeo);
            drawingContext.Close();
            _children.Add(drawingVisual);
        }

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }
    }


}
