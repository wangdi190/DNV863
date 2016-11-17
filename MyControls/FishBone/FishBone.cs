using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FishBone
{
    public enum EFishDirection { 头向右, 头向左 }

    ///<summary>参数类</summary>
    public class FishPara
    {
        public FishPara()
        {
        }

        internal double fishLen { get; set; }
        internal double fishHeight { get; set; }

        internal double fishHeadLen { get; set; }
        internal double fishTailLen { get; set; }
        internal double fishBodyLen { get; set; }

        ///<summary>鱼头所占比例</summary>
        public double fishHeadScale = 0.1;
        ///<summary>鱼尾所占比例</summary>
        public double fishTailScale = 0.2;
        ///<summary>子骨与父骨夹角</summary>
        public double boneAngle = 45;
        public double MainBoneThickness = 40;
        ///<summary>第一级鱼骨底部宽度</summary>
        public double FirstLayerBoneThickness = 12;
        ///<summary>叶级鱼骨宽度</summary>
        public double LeafBoneThickness = 1.5;

        public double HeadTailOpacity = 0.6;

        public EFishDirection fishDirection = EFishDirection.头向右;

        public Brush sortBrush = new SolidColorBrush(Colors.DodgerBlue);
        public Brush normalTextBrush = Brushes.DarkGray;
        public Brush noramlBranchBrush = new SolidColorBrush(Colors.Silver);
        public Brush MainBranchBrush = new SolidColorBrush(Colors.Silver);
        public Brush HeadTailBrush = new SolidColorBrush(Colors.Silver);
        public Brush UpBranchBrush = new SolidColorBrush(Colors.DarkGray);
        public Brush DownBranchBrush = new SolidColorBrush(Colors.DarkGray);
        public Brush warningBranchBrush = Brushes.OrangeRed;
        public Brush warningNameBrush = Brushes.Orange;
        public Brush warningValueBrush = Brushes.Red;

    }


    ///<summary>组织类</summary>
    public class Fish
    {
        public Fish(FishBoneControl pfishcontrol)
        {
            fishcontrol = pfishcontrol;
            this.para.MainBranchBrush = (Brush)fishcontrol.FindResource("mainbrush");
            this.para.HeadTailBrush = (Brush)fishcontrol.FindResource("HeadTailBrush");
            bone = new MainBone(this);
        }

        internal Random rd = new Random();

        public FishPara para = new FishPara();
        internal FishBoneControl fishcontrol;
        internal MainBone bone;
        internal Canvas canv = new Canvas();

        internal void cal()
        {
            para.fishHeadLen = para.fishLen * para.fishHeadScale;
            para.fishTailLen = para.fishLen * para.fishTailScale;
            para.fishBodyLen = para.fishLen - para.fishHeadLen - para.fishTailLen;

            bone.cal();

        }

        internal void draw()
        {
            canv.Children.Clear();
            canv.Width = para.fishLen;
            canv.Height = para.fishHeight;
            bone.draw();
        }



    }

    ///<summary>骨基类</summary>
    internal abstract class BoneBase
    {
        internal Fish fish;
        internal List<SubBone> bones = new List<SubBone>();


        #region 可视化相关
        internal Point from { get; set; }
        internal Point from2 { get; set; }
        internal Point to { get; set; }

        internal Point froma { get; set; }
        internal Point froma2 { get; set; }
        internal Point toa { get; set; }


        internal double len { get; set; }  //长度
        internal double thickness { get; set; }
        internal Brush branchBrush = Brushes.Gainsboro;
        internal Vector direct;  //方向
        internal int layer;

        internal double subMaxLen { get; set; }  //子骨最大长度

        internal abstract void cal();
        internal abstract void draw();



        #endregion

        internal SubBone addBone(string pid, string pname, string psort1, string psort2)
        {
            SubBone bone = new SubBone(fish, this) { id = pid, name = pname, sort1 = psort1, sort2 = psort2 };
            bones.Add(bone);
            return bone;
        }

    }


    ///<summary>主骨</summary>
    class MainBone : BoneBase
    {
        public MainBone(Fish pfish)
        {
            fish = pfish;
            branchBrush = fish.para.MainBranchBrush;
            layer = 0;
        }





        #region 可视化相关




        #endregion


        internal override void cal()
        {
            //计算主骨位置信息
            if (fish.para.fishDirection == EFishDirection.头向右)
            {
                from = new Point(fish.para.fishLen - fish.para.fishHeadLen, fish.para.fishHeight / 2);
                to = new Point(fish.para.fishTailLen, fish.para.fishHeight / 2);
            }
            else
            {
                from = new Point(fish.para.fishTailLen, fish.para.fishHeight / 2);
                to = new Point(fish.para.fishLen - fish.para.fishHeadLen, fish.para.fishHeight / 2);
            }
            direct = to - from;
            direct.Normalize();

            len = fish.para.fishBodyLen;
            thickness = fish.para.MainBoneThickness;
            subMaxLen = fish.para.fishHeight / 2;

            ////计算次骨长度
            //Point[] ap, cp1, cp2;
            //Point calpoint;
            //Point[] cp = new Point[4];
            //ap = new Point[3];
            //ap[0] = new Point(0, subMaxLen * 0.9);
            //ap[1] = new Point(bones.Count * 0.3, subMaxLen);
            //ap[2] = new Point(bones.Count, subMaxLen * 0.6);
            //MyClassLibrary.Share2D.MyGeometryHelper.GetCurveControlPoints(ap, out cp1, out cp2);

            //for (int i = 0; i < bones.Count; i++)
            //{
            //    if (1.0f * i / bones.Count < 0.3)
            //    {
            //        cp[0] = ap[0]; cp[1] = cp1[0]; cp[2] = cp2[0]; cp[3] = ap[1]; 
            //    }
            //    else
            //    { cp[0] = ap[1]; cp[1] = cp1[1]; cp[2] = cp2[1]; cp[3] = ap[2]; }
            //    calpoint = MyClassLibrary.Share2D.MyGeometryHelper.PointOnBezier(cp, 1.0f * i / bones.Count);
            //    bones[i].len = calpoint.Y;
            //}






            int ord = 0;
            foreach (SubBone bon in bones)
            {
                bon.idx = ord++;
                bon.layer = layer + 1;
                bon.thickness = fish.para.FirstLayerBoneThickness;
                bon.cal();
            }
        }
        internal override void draw()
        {
            //绘图形
            Path path = new Path();
            LineGeometry geo = new LineGeometry(from, to);
            path.Data = geo;
            path.Stroke = branchBrush;
            path.StrokeThickness = thickness;
            fish.canv.Children.Add(path);


            foreach (SubBone bon in bones)
                bon.draw();

            foreach (SubBone bon in bones)
                bon.text();

            drawHead();
            drawTail();
        }

        void drawHead()
        {
            Path path = new Path();
            PathGeometry pg = new PathGeometry();
            path.Data = pg;

            Point sp0, sp, cp1, cp2, ep;
            Vector vc, vc2;
            double zlen;
            RotateTransform r;
            PathFigure pf = new PathFigure();
            pg.Figures.Add(pf);
            sp0 = new Point(from.X - len / bones.Count * 1.2, from.Y - fish.para.fishHeight * 0.3);
            pf.StartPoint = sp0;
            //if (fish.para.fishDirection== EFishDirection.头向右)
            {
                ep = new Point(fish.para.fishLen - 20, from.Y - 20);
                vc = ep - sp0;
                zlen = vc.Length;
                vc.Normalize();
                r = new RotateTransform(-90);
                vc2 = (Vector)r.Transform((Point)vc);
                cp1 = sp0 + 0.6 * zlen * vc;
                cp1 = cp1 + 0.3 * zlen * vc2;
                cp2 = sp0 + 0.9 * zlen * vc;
                cp2 = cp2 - 0.1 * zlen * vc2;
                BezierSegment bs = new BezierSegment(cp1, cp2, ep, true);
                pf.Segments.Add(bs);

                sp = new Point(fish.para.fishLen - 20, from.Y + 20);
                ArcSegment aseg = new ArcSegment(sp, new Size(20, 20), 0, false, SweepDirection.Counterclockwise, true);
                pf.Segments.Add(aseg);

                ep = new Point(from.X - len / bones.Count * 1.2, from.Y + fish.para.fishHeight * 0.3);
                vc = ep - sp;
                zlen = vc.Length;
                vc.Normalize();
                r = new RotateTransform(-90);
                vc2 = (Vector)r.Transform((Point)vc);
                cp2 = sp + 0.4 * zlen * vc;
                cp2 = cp2 + 0.3 * zlen * vc2;
                cp1 = sp + 0.1 * zlen * vc;
                cp1 = cp1 - 0.1 * zlen * vc2;
                bs = new BezierSegment(cp1, cp2, ep, true);
                pf.Segments.Add(bs);

                vc = sp0 - ep;
                zlen = vc.Length;
                vc.Normalize();
                r = new RotateTransform(-90);
                vc2 = (Vector)r.Transform((Point)vc);
                cp1 = ep + 0.3 * zlen * vc;
                cp1 = cp1 - 0.18 * zlen * vc2;
                cp2 = ep + 0.7 * zlen * vc;
                cp2 = cp2 - 0.18 * zlen * vc2;
                bs = new BezierSegment(cp1, cp2, sp0, true);
                pf.Segments.Add(bs);

                path.Stroke = Brushes.Silver;
                path.StrokeThickness = 1;
                path.Fill = fish.para.HeadTailBrush;
                path.Opacity = fish.para.HeadTailOpacity;
                fish.canv.Children.Add(path);

                //眼
                sp = new Point(from.X + (fish.para.fishLen - from.X) / 3, from.Y - 30);
                EllipseGeometry eg = new EllipseGeometry(sp, 20, 30);
                path = new Path();
                path.Data = eg;
                path.Stroke = new SolidColorBrush(Colors.Black);
                path.StrokeThickness = 1;
                path.Fill = new SolidColorBrush(Colors.White);
                fish.canv.Children.Add(path);

                sp = new Point(from.X + (fish.para.fishLen - from.X) / 3 + 3, from.Y - 30 + 6);
                eg = new EllipseGeometry(sp, 12, 18);
                path = new Path();
                path.Data = eg;
                path.StrokeThickness = 0;
                path.Fill = new SolidColorBrush(Colors.Black);
                fish.canv.Children.Add(path);
            }


        }

        void drawTail()
        {
            Path path = new Path();
            PathGeometry pg = new PathGeometry();
            path.Data = pg;

            Point sp0, sp, cp1, cp2, ep;
            Vector vc, vc2;
            double zlen;
            RotateTransform r;
            PathFigure pf = new PathFigure();
            pg.Figures.Add(pf);
            sp0 = new Point(to.X-15, to.Y - fish.para.MainBoneThickness/2);
            pf.StartPoint = sp0;
            //if (fish.para.fishDirection== EFishDirection.头向右)
            {
                ep = new Point(20, 200);
                vc = ep - sp0;
                zlen = vc.Length;
                vc.Normalize();
                r = new RotateTransform(-90);
                vc2 = (Vector)r.Transform((Point)vc);
                cp1 = sp0 + 0.3 * zlen * vc;
                cp1 = cp1 + 0.1 * zlen * vc2;
                cp2 = sp0 + 0.7 * zlen * vc;
                cp2 = cp2 - 0.15* zlen * vc2;
                BezierSegment bs = new BezierSegment(cp1, cp2, ep, true);
                pf.Segments.Add(bs);

                sp = ep;
                ep = new Point(20, fish.para.fishHeight - 200);
                vc = ep - sp;
                zlen = vc.Length;
                vc.Normalize();
                r = new RotateTransform(-90);
                vc2 = (Vector)r.Transform((Point)vc);
                cp1 = sp + 0.4 * zlen * vc;
                cp1 = cp1 + 0.3 * zlen * vc2;
                cp2 = sp + 0.6 * zlen * vc;
                cp2 = cp2 + 0.3 * zlen * vc2;
                bs = new BezierSegment(cp1, cp2, ep, true);
                pf.Segments.Add(bs);

                sp = ep;
                ep = new Point(to.X-15, to.Y + fish.para.MainBoneThickness/2);
                vc = ep - sp;
                zlen = vc.Length;
                vc.Normalize();
                r = new RotateTransform(-90);
                vc2 = (Vector)r.Transform((Point)vc);
                cp1 = sp + 0.3 * zlen * vc;
                cp1 = cp1 - 0.13 * zlen * vc2;
                cp2 = sp + 0.7 * zlen * vc;
                cp2 = cp2 + 0.13 * zlen * vc2;
                bs = new BezierSegment(cp1, cp2, ep, true);
                pf.Segments.Add(bs);
                pf.Segments.Add(new LineSegment(sp0, true));

                path.Stroke = Brushes.Silver;
                path.StrokeThickness = 1;
                path.Fill = fish.para.HeadTailBrush;
                path.Opacity = fish.para.HeadTailOpacity;

                fish.canv.Children.Add(path);

            }
        }
    }

    ///<summary>细骨</summary>
    internal class SubBone : BoneBase
    {


        public SubBone(Fish pfish, BoneBase parentbone)
        {
            fish = pfish;
            parent = parentbone;
            nameBrush = fish.para.normalTextBrush;
            valueBrush = fish.para.normalTextBrush;
        }
        internal BoneBase parent;

        #region 业务数据相关
        internal string id;
        public string name { get; set; }
        internal string sort1 { get; set; }
        internal string sort2 { get; set; }

        internal double value;
        public string strvalue { get { return value.ToString(format) + unit; } }

        internal double ref1;
        internal double ref2;
        internal string reftype;
        internal string format = "f";
        internal string unit = "";
        public string refnote { get; set; }
        public string definition { get; set; }

        public bool isWarning;



        #endregion

        #region 可视化相关
        internal int idx; //序号
        StackPanel txtpanel = new StackPanel();
        ///<summary>是否是一级次骨</summary>
        internal bool isFirstLayer { get { return parent is MainBone; } }

        ///<summary>是否是上方的分支</summary>
        internal bool isUp { get; set; }

        Brush nameBrush;
        Brush valueBrush;

        //弧计算用
        Point center;

        #endregion


        internal override void cal()
        {
            //计算自身骨长度和子骨长度
            len = parent.subMaxLen;

            subMaxLen = parent.len * 0.8 / (parent.bones.Count + 1);
            //计算方向
            RotateTransform r;
            if (isFirstLayer)
            {
                //if (idx % 2 == 1)
                isUp = idx < parent.bones.Count / 2;
                if (isUp)
                    r = new RotateTransform(fish.para.boneAngle);
                else
                    r = new RotateTransform(-fish.para.boneAngle);
            }
            else
            {
                isUp = (parent as SubBone).isUp;
                if (isUp)
                    r = new RotateTransform(-fish.para.boneAngle);
                else
                    r = new RotateTransform(fish.para.boneAngle);

            }

            direct = (Vector)r.Transform((Point)parent.direct);

            //计算位置from
            Vector vec = parent.to - parent.from;
            vec.Normalize();
            if (isFirstLayer)
            {
                double div = parent.len / (parent.bones.Count) * 2;
                if (isUp)
                    from = parent.from + div * idx * vec;
                else
                    from = parent.from + div * (int)(idx - parent.bones.Count / 2) * vec;
                from2 = from + fish.para.FirstLayerBoneThickness * vec;

                froma = from + 4 * vec;
                froma2 = from2 - 4 * vec;
            }
            else
            {
                double div = parent.len / (parent.bones.Count + 1);
                from = parent.from + div * (idx + 1) * vec;
                //计算在弧形上的起点 (x-a)²+(y-b)²=r²  
                SubBone pb = parent as SubBone;
                double zr = parent.len * 1.5;
                double zy = from.Y;
                double zx = Math.Sqrt(zr * zr - (zy - pb.center.Y) * (zy - pb.center.Y)) + pb.center.X;
                from = new Point(zx - 2, zy);



            }
            //计算位置to
            to = from + len * direct;
            toa = from + 0.8 * len * direct;

            //求弧形圆心, 供子级使用
            double cr = len * 1.5;
            double cx = len / 2;
            double cy = Math.Sqrt(cr * cr - cx * cx);
            if (isUp)
                r = new RotateTransform(-90);
            else
                r = new RotateTransform(90);
            Vector cv1 = to - from;
            cv1.Normalize();
            Vector cv2 = (Vector)r.Transform((Point)cv1);
            Point cp1 = from + 0.5 * len * direct;
            center = cp1 + cy * cv2;



            //设置填充
            if (isFirstLayer)
                if (isUp)
                    branchBrush = fish.para.UpBranchBrush;
                else
                    branchBrush = fish.para.DownBranchBrush;
            else
                branchBrush = fish.para.noramlBranchBrush;


            //计算业务逻辑
            isWarning=false;
            if (!isFirstLayer)
            {
                switch (reftype)
                {
                    case ">":
                        isWarning = value < ref1;
                        break;
                    case "<":
                        isWarning = value > ref1;
                        break;
                    case "<>":
                        isWarning = value < ref1 || value > ref2;
                        break;
                    case "><":
                        isWarning = value > ref1 && value < ref2;
                        break;
                }
                
                if (isWarning)
                {
                    branchBrush = fish.para.warningBranchBrush;
                    nameBrush = fish.para.warningNameBrush;
                    valueBrush = fish.para.warningValueBrush;
                }
            }


            //子级
            int ord = 0;
            foreach (SubBone bon in bones)
            {
                bon.layer = layer + 1;
                bon.thickness = fish.para.LeafBoneThickness;
                bon.idx = ord++;
                bon.cal();
            }

        }

        internal override void draw()
        {
            // 绘图形
            Path path = new Path();
            Path patha = new Path();
            if (isFirstLayer)
            {
                //Data="M 100,100 A 200,200 45 0 0 0,0 A 200,200 45 0 1 90,100" />

                PathGeometry pg = new PathGeometry();
                PathFigure pf = new PathFigure();
                pg.Figures.Add(pf);
                pf.StartPoint = from;

                PathGeometry pga = new PathGeometry();
                PathFigure pfa = new PathFigure();
                pga.Figures.Add(pfa);
                pfa.StartPoint = froma;

                if (isUp)
                {
                    ArcSegment ps = new ArcSegment(to, new Size(len * 1.5, len * 1.5), 45, false, SweepDirection.Counterclockwise, false);
                    pf.Segments.Add(ps);
                    ps = new ArcSegment(from2, new Size(len * 1.5, len * 1.5), 45, false, SweepDirection.Clockwise, false);
                    pf.Segments.Add(ps);

                    ps = new ArcSegment(to, new Size(len * 1.5, len * 1.5), 45, false, SweepDirection.Counterclockwise, false);
                    pfa.Segments.Add(ps);
                    ps = new ArcSegment(froma2, new Size(len * 1.5, len * 1.5), 45, false, SweepDirection.Clockwise, false);
                    pfa.Segments.Add(ps);

                }
                else
                {
                    ArcSegment ps = new ArcSegment(to, new Size(len * 1.5, len * 1.5), 45, false, SweepDirection.Clockwise, false);
                    pf.Segments.Add(ps);
                    ps = new ArcSegment(from2, new Size(len * 1.5, len * 1.5), 45, false, SweepDirection.Counterclockwise, false);
                    pf.Segments.Add(ps);

                    ps = new ArcSegment(to, new Size(len * 1.5, len * 1.5), 45, false, SweepDirection.Clockwise, false);
                    pfa.Segments.Add(ps);
                    ps = new ArcSegment(froma2, new Size(len * 1.5, len * 1.5), 45, false, SweepDirection.Counterclockwise, false);
                    pfa.Segments.Add(ps);
                }

                path.Data = pg;
                path.Fill = branchBrush;
                path.StrokeThickness = 0;

                patha.Data = pga;
                byte gb = (byte)(255.0f * (1.0f - 1.0f * bones.Count(p => p.isWarning) / bones.Count));
                patha.Fill = new SolidColorBrush(Color.FromRgb(255, gb, gb));
                patha.StrokeThickness = 0;
                patha.Effect = new System.Windows.Media.Effects.BlurEffect() { Radius = 5 };

            }
            else
            {
                LineGeometry geo = new LineGeometry(from, to);
                path.Data = geo;
                path.Stroke = branchBrush;
                path.StrokeThickness = thickness;
                path.StrokeEndLineCap = PenLineCap.Triangle;
            }
            if (isWarning)
                path.Effect = new System.Windows.Media.Effects.DropShadowEffect() { ShadowDepth = 0, BlurRadius = 5, Color = Colors.Yellow };

            Canvas.SetZIndex(path, -1 * layer);
            fish.canv.Children.Add(path);

            Canvas.SetZIndex(patha, -1 * layer);
            fish.canv.Children.Add(patha);


            foreach (SubBone bon in bones)
                bon.draw();



        }

        internal void text()
        {

            // 绘文字
            if (isFirstLayer)
            {
                TextBlock txtname = new TextBlock() { Text = sort2, FontSize = 16, Foreground = fish.para.sortBrush };
                if (bones.Count(p => p.isWarning) > 0)
                {
                    txtname.Foreground = new SolidColorBrush(Colors.Red);
                    txtname.Effect = new System.Windows.Media.Effects.DropShadowEffect { Color = Colors.Yellow, ShadowDepth = 0, BlurRadius = 2 };
                }
                txtname.Measure(new Size(500, 500));
                if (isUp)
                    txtname.Margin = new Thickness(to.X - txtname.DesiredSize.Width / 2, to.Y - txtname.DesiredSize.Height - 2, 0, 0);
                else
                    txtname.Margin = new Thickness(to.X - txtname.DesiredSize.Width / 2, to.Y + 2, 0, 0);
                fish.canv.Children.Add(txtname);
            }
            else
            {
                TextBlock txtname = new TextBlock() { Text = name, FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, Foreground = nameBrush };
                txtname.MaxWidth = parent.subMaxLen * 2;
                txtname.TextWrapping = TextWrapping.Wrap;
                txtname.TextAlignment = TextAlignment.Center;

                TextBlock txtvalue = new TextBlock() { Text = value.ToString(format) + unit, FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, Foreground = valueBrush };
                txtpanel.Children.Add(txtname);
                txtpanel.Children.Add(txtvalue);

                txtname.Measure(new Size(500, 500));
                txtvalue.Measure(new Size(500, 500));
                txtpanel.Width = Math.Max(txtname.DesiredSize.Width, txtvalue.DesiredSize.Width);

                txtpanel.Margin = new Thickness(to.X - txtname.DesiredSize.Width / 2, to.Y + 2, 0, 0);

                fish.canv.Children.Add(txtpanel);

                txtpanel.MouseEnter += new System.Windows.Input.MouseEventHandler(txtpanel_MouseEnter);
                txtpanel.MouseLeave += new System.Windows.Input.MouseEventHandler(txtpanel_MouseLeave);

            }





            foreach (SubBone bon in bones)
                bon.text();



        }



        void txtpanel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point mouseposition = e.GetPosition(fish.fishcontrol);
            double ToolTipOffset = 5;
            fish.fishcontrol.Tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
            fish.fishcontrol.Tooltip.PlacementTarget = fish.fishcontrol;
            fish.fishcontrol.Tooltip.HorizontalOffset = mouseposition.X + ToolTipOffset + 5;
            fish.fishcontrol.Tooltip.VerticalOffset = mouseposition.Y + ToolTipOffset;

            fish.fishcontrol.tooptipdata.Content = this;

            fish.fishcontrol.Tooltip.IsOpen = true;
        }
        void txtpanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            fish.fishcontrol.Tooltip.IsOpen = false;
        }
    }


}
