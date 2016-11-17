using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MyClassLibrary.Share2D;

namespace MyClassLibrary.Share3D
{
    /// <summary>
    /// 3D图例数据类，addNewModelLegend添加模型图例，addNewColorLegend添加颜色图例
    /// </summary>
    public class Legend3DData
    {
        public Legend3DData()
        {
            Scene3D scene = new Scene3D(); 
            serial=scene.addNewSerial();
        }
        internal double length = 10;
        internal double height = 5;
        internal double width = 4;
        internal int ModelLegendCount { get { return ListModelLegend.Count; } }
        internal ModelSerial serial;
        //=====引用对象
        internal Grid grdMain;
        internal Grid grdFlag;
        internal StackPanel colorFlag;
        internal ModelVisual3D modelMain;

        //=====内部对象
        internal List<LegendUnit> ListModelLegend = new List<LegendUnit>();
        //internal List<StruLegendColorToName> ListColorLegend = new List<StruLegendColorToName>();
        internal Dictionary<string, Brush> ListColorLegend = new Dictionary<string, Brush>();
        internal Model3DGroup mg = new Model3DGroup();

        //=====公开方法
        /// <summary>
        /// 增加一个模型图例
        /// </summary>
        /// <param name="mtype">模型类型</param>
        /// <returns>返回新增的模型图例描述对象</returns>
        public LegendUnit addNewModelLegend(EModelType mtype)
        {
            LegendUnit l = new LegendUnit(mtype, this);
            ListModelLegend.Add(l);
            return l;
        }

        public void addNewColorLegend(string name, Brush brush)
        {
            //StruLegendColorToName str = new StruLegendColorToName();
            //str.Lname = name;
            //str.Lbrush = brush;
            //ListColorLegend.Add(str);
            ListColorLegend.Add(name, brush);
        }


        #region 内部方法
        /// <summary>
        /// 生成所有图例模型和注释
        /// </summary>
        internal void genAllLegend()
        {
            //=====模型图例
            Scene3D scene=new Scene3D();
            ModelSerial serial=scene.addNewSerial();
            serial.mAlign = new Vector3D(0.5, 0, 0.5);
            foreach (LegendUnit one in ListModelLegend)
            {
                one.genModel();
                mg.Children.Add(one.mg);
            }

            //=====颜色图例
            Rectangle rec; TextBlock txt;
            foreach (string dataname in ListColorLegend.Keys)
            {
                rec = new Rectangle() { Width = 10, Height = 10, Stroke = Brushes.White, StrokeThickness = 0.3, Fill = ListColorLegend[dataname], VerticalAlignment = VerticalAlignment.Center };
                colorFlag.Children.Add(rec);
                txt = new TextBlock() { Foreground = Brushes.White, Text = " " + dataname + "　　", VerticalAlignment = VerticalAlignment.Center };
                colorFlag.Children.Add(txt);
            }

        }
        #endregion

    }

    /// <summary>
    /// 模型图例单元，
    /// 1.可设置LegendName,SizeXName...ColorName等
    /// </summary>
    public class LegendUnit
    {
        public LegendUnit(EModelType mtype, Legend3DData Owner)
        {
            modelType = mtype;
            owner = Owner;
        }

        //=====属性
        internal int idx { get { return owner.ListModelLegend.IndexOf(this); } }
        internal Legend3DData owner;
        //=====图例属性
        ///<summary>模型图例名称</summary>
        public string LegendName;
        ///<summary>X方向宽度名称</summary>
        public string SizeXName = null;
        ///<summary>Y方向宽度名称</summary>
        public string SizeYName = null;
        ///<summary>Z方向宽度名称</summary>
        public string SizeZName = null;
        ///<summary>组合模型各部分名称</summary>
        public Dictionary<string, Brush> LstGroupLabel = new Dictionary<string, Brush>();
        //public List<StruLegendColorToName> LstGroupLabel = new List<StruLegendColorToName>();
        public string ColorName = null;
        public Color colorStart;
        public Color colorEnd;
        public String colorStartValue = "";
        public String colorEndValue = "";
        ///<summary>X方向位置名称</summary>
        public string XName = null;
        ///<summary>Y方向位置名称</summary>
        public string YName = null;
        ///<summary>z方向位置名称</summary>
        public string ZName = null;
        ///<summary>非堆叠的材质</summary>
        public Material mat=My3DHelper.getSolidMaterial(EMaterailColor.红,EMaterialColorDeep.浅色);
        public EModelType modelType = EModelType.常规园柱;
        ///<summary>附加宽度缩放比例</summary>
        public double addScale=1;

        //=====公开属性
        internal ModelBase model;
        internal Model3DGroup mg = new Model3DGroup();

        //=====私有方法

        //=====公开方法
        ///<summary>生成图例模型</summary>
        internal void genModel()
        {
            model = myModelBuilder.CreateModel(owner.serial, modelType,mat, LstGroupLabel);
            double maxwidth = Math.Min(owner.width / (owner.ModelLegendCount) * 0.7, owner.width / 2 * 0.7)*addScale;
            model.SizeX3D = maxwidth;
            model.SizeZ3D = maxwidth;
            model.SizeY3D = owner.height*0.8;// / (owner.ModelLegendCount + 0.5) * (owner.ModelLegendCount - idx);
            model.X3D = owner.length / (owner.ModelLegendCount) * (0.5 + idx - owner.ModelLegendCount / 2.0);
            //model.Rotate(new Vector3D(0,1,0),-15);
            mg.Children.Add(model.model);
            //图例名称
            if (LegendName != null)
            {
                MLabel namemodel = new MLabel(owner.serial);
                namemodel.mAlign = new Vector3D(0.5, 0.5, 0);
                namemodel.LabelDirection = ELabelDirection.垂直;
                namemodel.LabelHeight = 0.4;
                namemodel.isGlow = false;
                namemodel.LabelColor = new SolidColorBrush(Colors.Yellow);
                namemodel.LabelText = LegendName;
                namemodel.X3D = model.model.Bounds.X + model.model.Bounds.SizeX / 2;
                namemodel.Z3D = 2;//model.model.Bounds.Z + model.model.Bounds.SizeZ + 1;
                //namemodel.SizeZ3D = 1.5;
                mg.Children.Add(namemodel.model);
            }

            //图例注释
            owner.grdMain.UpdateLayout();
            genSizeXLabel();
            genSizeYLabel();
            genSizeZLabel();
            genColorLabel();
            genGroupLabel();
        }

        ///<summary>生成X方向大小的标注</summary>
        private void genSizeXLabel()
        {
            if (SizeXName != null)
            {
                double distance;
                Point pb1, pb2, pb3, po1, po2, pomid, pomid2, pend;
                GeneralTransform3DTo2D gtransform = owner.modelMain.TransformToAncestor(owner.grdMain);
                Model3D lModel = model.model;
                pb1 = gtransform.Transform(new Point3D(lModel.Bounds.X, lModel.Bounds.Y + lModel.Bounds.SizeY, (lModel.Bounds.Z + lModel.Bounds.SizeZ) / 2));
                pb2 = gtransform.Transform(new Point3D(lModel.Bounds.X + lModel.Bounds.SizeX, lModel.Bounds.Y + lModel.Bounds.SizeY, (lModel.Bounds.Z + lModel.Bounds.SizeZ) / 2));
                pb3 = gtransform.Transform(new Point3D(lModel.Bounds.X + lModel.Bounds.SizeX / 2, lModel.Bounds.Y + lModel.Bounds.SizeY, lModel.Bounds.Z));
                distance = pb3.Y - pb1.Y;
                po1 = pb1; po1.Offset(0, distance - 10);
                po2 = pb2; po2.Offset(0, distance - 10);
                pomid = new Point((po1.X + po2.X) / 2, (po1.Y + po2.Y) / 2);
                pomid2 = pomid; pomid2.Offset(0, -10);
                pend = pomid2; pend.Offset(-20, -10);
                Path path;
                path = new Path() { StrokeThickness = 1, Stroke = Brushes.White };
                path.Data = getPathGeo(pb1, pb2, po1, po2, pomid, pomid2, pend);
                owner.grdFlag.Children.Add(path);
                path = new Path() { StrokeThickness = 1, Fill = Brushes.Blue };
                path.Data = new EllipseGeometry(pend, 5, 5);
                owner.grdFlag.Children.Add(path);
                TextBlock txt = new TextBlock() { Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0xFF, 0xCC)), VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
                txt.Text = SizeXName;
                txt.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                txt.Margin = new Thickness(pend.X - txt.DesiredSize.Width / 2, pend.Y - txt.DesiredSize.Height - 5, 0, 0);
                owner.grdFlag.Children.Add(txt);

            }
        }
        ///<summary>生成Y方向大小的标注</summary>
        private void genSizeYLabel()
        {
            if (SizeYName != null)
            {
                Point pb1, pb2, po1, po2, pomid, pomid2, pend;
                GeneralTransform3DTo2D gtransform = owner.modelMain.TransformToAncestor(owner.grdMain);
                Model3D lModel = model.model;
                pb1 = gtransform.Transform(new Point3D(lModel.Bounds.X, lModel.Bounds.Y + lModel.Bounds.SizeY, (lModel.Bounds.Z + lModel.Bounds.SizeZ)/2));
                pb2 = gtransform.Transform(new Point3D(lModel.Bounds.X, lModel.Bounds.Y, (lModel.Bounds.Z + lModel.Bounds.SizeZ) / 2));
                po1 = pb1; po1.Offset(-10, 0);
                po2 = pb2; po2.Offset(-10, 0);
                pomid = new Point((po1.X + po2.X) / 2, (po1.Y + po2.Y) / 2);
                pomid2 = pomid; pomid2.Offset(-20, 0);
                pend = pomid2; pend.Offset(-10, -10);
                Path path;
                path = new Path() { StrokeThickness = 1, Stroke = Brushes.White };
                path.Data = getPathGeo(pb1, pb2, po1, po2, pomid, pomid2, pend);
                owner.grdFlag.Children.Add(path);
                path = new Path() { StrokeThickness = 1, Fill = Brushes.Blue };
                path.Data = new EllipseGeometry(pend, 5, 5);
                owner.grdFlag.Children.Add(path);
                TextBlock txt = new TextBlock() { Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0xFF, 0xCC)), VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
                txt.Text = SizeYName;
                txt.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                txt.Margin = new Thickness(pend.X - txt.DesiredSize.Width / 2, pend.Y - txt.DesiredSize.Height - 5, 0, 0);
                owner.grdFlag.Children.Add(txt);
            }
        }
        private void genSizeZLabel()
        {
            if (SizeZName != null)
            {
                double distance;
                Point pb1, pb2, pb3, po1, po2, pomid, pomid2, pend;
                GeneralTransform3DTo2D gtransform = owner.modelMain.TransformToAncestor(owner.grdMain);
                Model3D lModel = model.model;
                pb1 = gtransform.Transform(new Point3D(lModel.Bounds.X + lModel.Bounds.SizeX / 2, lModel.Bounds.Y + lModel.Bounds.SizeY, lModel.Bounds.Z));
                pb2 = gtransform.Transform(new Point3D(lModel.Bounds.X + lModel.Bounds.SizeX / 2, lModel.Bounds.Y + lModel.Bounds.SizeY, lModel.Bounds.Z + lModel.Bounds.SizeZ));
                pb3 = gtransform.Transform(new Point3D(lModel.Bounds.X + lModel.Bounds.SizeX, lModel.Bounds.Y + lModel.Bounds.SizeY, lModel.Bounds.Z + lModel.Bounds.SizeZ / 2));
                distance = pb3.X - pb1.X;
                po1 = pb1; po1.Offset(distance + 10, 0);
                po2 = pb2; po2.Offset(distance + 10, 0);
                pomid = new Point((po1.X + po2.X) / 2, (po1.Y + po2.Y) / 2);
                pomid2 = pomid; pomid2.Offset(20, 0);
                pend = pomid2; pend.Offset(10, -10);
                Path path;
                path = new Path() { StrokeThickness = 1, Stroke = Brushes.White };
                path.Data = getPathGeo(pb1, pb2, po1, po2, pomid, pomid2, pend);
                owner.grdFlag.Children.Add(path);
                path = new Path() { StrokeThickness = 1, Fill = Brushes.Blue };
                path.Data = new EllipseGeometry(pend, 5, 5);
                owner.grdFlag.Children.Add(path);
                TextBlock txt = new TextBlock() { Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0xFF, 0xCC)), VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
                txt.Text = SizeZName;
                txt.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                txt.Margin = new Thickness(pend.X - txt.DesiredSize.Width / 2, pend.Y - txt.DesiredSize.Height - 5, 0, 0);
                owner.grdFlag.Children.Add(txt);
            }
        }
        private PathGeometry getPathGeo(Point pb1, Point pb2, Point po1, Point po2, Point pomid, Point pomid2, Point pend)
        {
            PathGeometry geo = new PathGeometry();
            PathFigure fig = new PathFigure();
            fig.StartPoint = pb1;
            fig.Segments.Add(new LineSegment(po1, true));
            fig.Segments.Add(new LineSegment(po2, true));
            fig.Segments.Add(new LineSegment(pb2, true));
            geo.Figures.Add(fig);
            fig = new PathFigure();
            fig.StartPoint = pomid;
            fig.Segments.Add(new LineSegment(pomid2, true));
            fig.Segments.Add(new LineSegment(pend, true));
            geo.Figures.Add(fig);
            return geo;
        }
        /// <summary>
        /// 生成组模型标注
        /// </summary>
        private void genGroupLabel()
        {
            if (LstGroupLabel.Count > 0)
            {
                int idx = 0;
                foreach (KeyValuePair<string,ModelBase> item in (model as GroupModelBase).models)
                {
                    double distance;
                    Point pb1, pb3, po1, po2;
                    GeneralTransform3DTo2D gtransform = owner.modelMain.TransformToAncestor(owner.grdMain);
                    Model3D lModel = item.Value.model;
                    Point3D pinner;
                    pinner = new Point3D(lModel.Bounds.X + lModel.Bounds.SizeX / 2, lModel.Bounds.Y + lModel.Bounds.SizeY / 2, lModel.Bounds.Z + lModel.Bounds.SizeZ);
                    pb1 = gtransform.Transform((model.model.Transform as MatrixTransform3D).Transform(pinner));
                    pinner = new Point3D(lModel.Bounds.X + lModel.Bounds.SizeX, lModel.Bounds.Y + lModel.Bounds.SizeY / 2, lModel.Bounds.Z + lModel.Bounds.SizeZ);
                    pb3 = gtransform.Transform((model.model.Transform as MatrixTransform3D).Transform(pinner));
                    distance = pb3.X - pb1.X;
                    po1 = pb1; po1.Offset(distance + 5, 0);
                    po2 = po1; po2.Offset(10, -10);
                    Path path;
                    path = new Path() { StrokeThickness = 1, Stroke = Brushes.White };
                    PathGeometry geo = new PathGeometry();
                    PathFigure fig = new PathFigure();
                    fig.StartPoint = pb1;
                    fig.Segments.Add(new LineSegment(po1, true));
                    fig.Segments.Add(new LineSegment(po2, true));
                    geo.Figures.Add(fig);
                    path.Data = geo;
                    owner.grdFlag.Children.Add(path);
                    path = new Path() { StrokeThickness = 1, Fill = Brushes.Blue };
                    path.Data = new EllipseGeometry(pb1, 5, 5);
                    owner.grdFlag.Children.Add(path);
                    TextBlock txt = new TextBlock() { Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0xFF, 0xCC)), VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
                    //txt.Text = LstGroupLabel[idx].Lname;
                    txt.Text = item.Key;
                    txt.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    txt.Margin = new Thickness(po2.X + 10, po2.Y - txt.DesiredSize.Height / 2, 0, 0);
                    owner.grdFlag.Children.Add(txt);
                    idx++;
                }
            }
        }

        /// <summary>
        /// 生成颜色标注
        /// </summary>
        private void genColorLabel()
        {
            if (ColorName != null)
            {
                double distance;
                Point pb1, pb3, po1, po2, ptop, pbuttom;
                GeneralTransform3DTo2D gtransform = owner.modelMain.TransformToAncestor(owner.grdMain);
                Model3D lModel = model.model;
                pb1 = gtransform.Transform(new Point3D(lModel.Bounds.X + lModel.Bounds.SizeX / 2, lModel.Bounds.Y + lModel.Bounds.SizeY / 2, lModel.Bounds.Z + lModel.Bounds.SizeZ));
                pb3 = gtransform.Transform(new Point3D(lModel.Bounds.X + lModel.Bounds.SizeX, lModel.Bounds.Y + lModel.Bounds.SizeY / 2, lModel.Bounds.Z + lModel.Bounds.SizeZ));
                ptop = gtransform.Transform(new Point3D(lModel.Bounds.X + lModel.Bounds.SizeX, lModel.Bounds.Y + lModel.Bounds.SizeY, lModel.Bounds.Z + lModel.Bounds.SizeZ / 2));
                pbuttom = gtransform.Transform(new Point3D(lModel.Bounds.X + lModel.Bounds.SizeX, lModel.Bounds.Y, lModel.Bounds.Z + lModel.Bounds.SizeZ / 2));
                distance = pb3.X - pb1.X;
                po1 = pb1; po1.Offset(distance + 5, 0);
                po2 = po1; po2.Offset(10, -10);
                Path path;
                path = new Path() { StrokeThickness = 1, Stroke = Brushes.White };
                PathGeometry geo = new PathGeometry();
                PathFigure fig = new PathFigure();
                fig.StartPoint = pb1;
                fig.Segments.Add(new LineSegment(po1, true));
                fig.Segments.Add(new LineSegment(po2, true));
                geo.Figures.Add(fig);
                path.Data = geo;
                owner.grdFlag.Children.Add(path);
                path = new Path() { StrokeThickness = 1, Fill = Brushes.Blue };
                path.Data = new EllipseGeometry(pb1, 5, 5);
                owner.grdFlag.Children.Add(path);
                TextBlock txt = new TextBlock() { Foreground = Brushes.Yellow, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
                txt.Text = ColorName;
                txt.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                txt.Margin = new Thickness(po2.X + 10, po2.Y - txt.DesiredSize.Height / 2, 0, 0);
                owner.grdFlag.Children.Add(txt);
                //色谱
                Rectangle rec = new Rectangle() { VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
                rec.StrokeThickness = 0.3;
                rec.Stroke = new SolidColorBrush(Colors.White);
                rec.Width = 6;
                rec.Height = pbuttom.Y - ptop.Y;
                LinearGradientBrush brush = new LinearGradientBrush();
                brush.StartPoint = new Point(0, 0);
                brush.EndPoint = new Point(0, 1);
                brush.GradientStops.Add(new GradientStop(colorStart, 1));
                brush.GradientStops.Add(new GradientStop(colorEnd, 0));
                rec.Fill = brush;
                rec.Margin = new Thickness(po2.X, ptop.Y, 0, 0);
                owner.grdFlag.Children.Add(rec);
                txt = new TextBlock() { Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0xFF, 0xFF)), VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
                txt.Text = colorStartValue;
                txt.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                txt.Margin = new Thickness(po2.X + 10, pbuttom.Y - txt.DesiredSize.Height / 2, 0, 0);
                owner.grdFlag.Children.Add(txt);
                txt = new TextBlock() { Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0xFF, 0xFF)), VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
                txt.Text = colorEndValue;
                txt.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                txt.Margin = new Thickness(po2.X + 10, ptop.Y - txt.DesiredSize.Height / 2, 0, 0);
                owner.grdFlag.Children.Add(txt);

            }
        }
    }


}
