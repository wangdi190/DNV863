using System;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MyClassLibrary.Share2D;
using MyClassLibrary.Share3D;
using DataLayer;

namespace MyControlLibrary.Controls3D.Index3D
{
    /// <summary>
    /// newIndexMain.xaml 的交互逻辑
    /// </summary>
    public partial class IndexMain : UserControl
    {
        public IndexMain()
        {
            InitializeComponent();
        }

        #region 模型定义


        [CategoryAttribute("数据"), Description("数据源视图，具有特定格式")]
        ///<summary>数据源</summary>
        public DataView DataSource
        {
            get { return (DataView)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(DataView), typeof(IndexMain), new UIPropertyMetadata(null, new PropertyChangedCallback(OnDataSourceChanged)));
        private static void OnDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IndexMain Sender = (IndexMain)d;
            Sender.updateData();
        }

        ///<summary>数据源更新，重构数据类</summary>
        private void updateData()
        {
            
            initdata();
            initmodel();


        }



        private string _controlTitle;
        [CategoryAttribute("外观"), Description("控件标题")]
        ///<summary>标件标题</summary>
        public string controlTitle
        {
            get { return _controlTitle; }
            set { _controlTitle = value; }
        }





        #endregion





        private void UserControl_Initialized(object sender, EventArgs e)
        {
            initSource();

            //initdata();
            //initmodel();

            //sb.Children.Add(anirx);
            //sb.Children.Add(aniry);
            //this.RegisterName("rotation1", rotation1);
            //this.RegisterName("rotation2", rotation2);
            //Storyboard.SetTargetName(aniry, "rotation1");
            //Storyboard.SetTargetName(anirx, "rotation2");
            //Storyboard.SetTargetProperty(aniry,new PropertyPath(AxisAngleRotation3D.AngleProperty));
            //Storyboard.SetTargetProperty(anirx, new PropertyPath(AxisAngleRotation3D.AngleProperty));

            //anir.KeyFrames.Add(new LinearQuaternionKeyFrame(new Quaternion(new Vector3D(0, 1, 1), 120)));
            //anir.KeyFrames.Add(new LinearQuaternionKeyFrame(new Quaternion(new Vector3D(0, 1, 1), 240)));
            //anir.KeyFrames.Add(new LinearQuaternionKeyFrame(new Quaternion(new Vector3D(0, 1, 1), 360)));
            //sb.Children.Add(anir);
            //this.RegisterName("rotation", rotation);
            //Storyboard.SetTargetName(anir, "rotation");
            //Storyboard.SetTargetProperty(anir,new PropertyPath(QuaternionRotation3D.QuaternionProperty));


            //sb.CurrentTimeInvalidated += new EventHandler(sb_CurrentTimeInvalidated);
            //sb.Begin(this,true);

            autoanitimer.Tick += new EventHandler(autoanitimer_Tick);
            autoanitimer.Start();
        }

        void autoanitimer_Tick(object sender, EventArgs e)
        {
            Vector3D axis = new Vector3D(0.3,1,0.7);

            double angle = 0.2;
                Quaternion delta = new Quaternion(axis, angle);
                rotation.Quaternion = delta * rotation.Quaternion;

                autoRotationText();
        }

        //void sb_CurrentTimeInvalidated(object sender, EventArgs e)
        //{
        //    autoRotationText();
        //}


      

        #region 自动动画
        System.Windows.Threading.DispatcherTimer autoanitimer = new System.Windows.Threading.DispatcherTimer() {Interval=TimeSpan.FromMilliseconds(20) };

        //Storyboard sb = new Storyboard();

        //DoubleAnimation anirx = new DoubleAnimation() { Duration = TimeSpan.FromSeconds(107), RepeatBehavior = RepeatBehavior.Forever ,From=0, To=360};
        //DoubleAnimation aniry = new DoubleAnimation() { Duration = TimeSpan.FromSeconds(60), RepeatBehavior = RepeatBehavior.Forever ,From=0, To=360};
        //QuaternionAnimation anir = new QuaternionAnimation() { Duration = TimeSpan.FromSeconds(20), RepeatBehavior = RepeatBehavior.Forever, By = new Quaternion(new Vector3D(0,1,1),180) };
        //QuaternionAnimationUsingKeyFrames anir = new QuaternionAnimationUsingKeyFrames() { Duration = TimeSpan.FromSeconds(20), RepeatBehavior = RepeatBehavior.Forever  };
        #endregion






        #region 定义
        indexPlane[] idxdata = new indexPlane[12];
        static readonly DependencyProperty HitTestInfoProperty = DependencyProperty.Register("HitTestInfo", typeof(indexPoint), typeof(IndexMain));

        Boolean _status_ani = false;  //是否动画中
        Boolean _status_view = false;  //是否显示细节视图中
        Boolean _status_panel = false; //是否处于平板显示状态

        DateTime dateb = new DateTime(2011, 1, 1);  //指标日期范围
        DateTime datee = new DateTime(2011, 9, 10);

        #endregion
        #region 初始化
        ///<summary>初始化模型资源</summary>
        public void initSource()
        {
            MeshGeometry3D mesh = Model3DHelper.genCylinder3DMesh();
            this.Resources.Add("meshCyl", mesh);
            mesh = Model3DHelper.genCylinder3DTopMesh();
            this.Resources.Add("meshCylTop", mesh);
            mesh = Model3DHelper.genCube3DMesh();
            this.Resources.Add("meshCube", mesh);
        }

        ///<summary>初始化数据</summary>
        public void initdata()   //初始化数据
        {
            for (int i = 0; i < 12; i++)
            {
                idxdata[i] = new indexPlane();
            }
            //填入指标数据
            DataTable dtIndex;

            dtIndex = DataSource.Table;

            var l = from one in dtIndex.AsEnumerable()
                    group one by one.Field<string>("sort2") into g
                    select new
                    {
                        sort1 = g.First().Field<string>("sort1"),
                        sort2 = g.First().Field<string>("sort2")
                    };

            int count = 0;
            string oldsort1 = "";
            int sort1count = 0;
            if (l.Count() < 12)  // 附加首页面  KPI
            {
                idxdata[0].planeidx = 0;
                idxdata[0].title = "KPI";
                idxdata[count].matidx = sort1count;
                var l2 = from dr in dtIndex.AsEnumerable()
                         where dr.Field<int>("kpi") == 1
                         select dr;
                foreach (var e in l2)  //填入指标数据
                {

                    indexPoint ip = idxdata[count].addChildren(e.Field<string>("indexname"));
                    //ip.indexdata = e;
                    ip.indexlayer =(int) e.Field<decimal>("important");
                    ip.format = e.Field<string>("format");
                    ip.refer1 = (Nullable<double>)e.Field<Nullable<decimal>>("refer1");
                    ip.refer2 = (Nullable<double>)e.Field<Nullable<decimal>>("refer2");
                    ip.referNote = e.Field<string>("refernote");
                    ip.referType = e.Field<string>("refertype");
                    ip.modelType = e.Field<string>("modeltype");
                    ip.definition = e.Field<string>("definition");
                    ip.viewType = e.Field<string>("viewType");
                    ip.viewInfo = e.Field<string>("viewInfo");
                    ip.viewSQL = e.Field<string>("viewSQL");
                    ip.valueSQL = e.Field<string>("valueSQL");

                    ip.gaugeInfo.startValue =(double) e.Field<decimal>("minValue");
                    ip.gaugeInfo.endValue =(double) e.Field<decimal>("maxValue");
                    ip.gaugeInfo.labelFormat = "{0:" + ip.format + "}";

                    ip.value =double.Parse(e["value"].ToString()) ; //(double)e.calIndexValue(vdata.startTime, vdata.dataTime);
                    ip.gaugeInfo.value = ip.value;
                }
                count++;
            }
            foreach (var one in l)
            {
                if (count > 12) return;
                if (oldsort1 != one.sort1)
                {
                    sort1count++;
                    oldsort1 = one.sort1;
                }
                idxdata[count].planeidx = count;
                idxdata[count].title = one.sort2;
                idxdata[count].matidx = sort1count;
                var l2 = from dr in dtIndex.AsEnumerable()
                         where dr.Field<string>("sort2") == one.sort2
                         select dr;
                foreach (var e in l2)  //填入指标数据
                {
                    indexPoint ip = idxdata[count].addChildren(e.Field<string>("indexname"));
                    //ip.indexdata = e;
                    ip.indexlayer =(int) e.Field<decimal>("important");
                    ip.value =(double) e.Field<decimal>("value");
                    ip.format = e.Field<string>("format");
                    ip.refer1 =(Nullable<double>) e.Field<Nullable<decimal>>("refer1");
                    ip.refer2 =(Nullable<double>) e.Field<Nullable<decimal>>("refer2");
                    ip.referNote = e.Field<string>("refernote");
                    ip.referType = e.Field<string>("refertype");
                    ip.modelType = e.Field<string>("modeltype");
                    ip.definition = e.Field<string>("definition");
                    ip.viewType = e.Field<string>("viewType");
                    ip.viewInfo = e.Field<string>("viewInfo");
                    ip.viewSQL = e.Field<string>("viewSQL");
                    ip.valueSQL = e.Field<string>("valueSQL");

                    ip.gaugeInfo.startValue =(double) e.Field<decimal>("minValue");
                    ip.gaugeInfo.endValue =(double) e.Field<decimal>("maxValue");
                    ip.gaugeInfo.labelFormat="{0:"+ip.format+"}";

                    ip.gaugeInfo.value = ip.value;

                }
                count++;
            }
            // 指标值计算
            //calIndexValue();

            // 参数计算
            foreach (indexPlane one in idxdata)
            {
                if (one != null)
                    one.calPoint();
            }

        }


        private void addControlcube()
        {
            //Cube3D cube = new Cube3D();
            //cube.Transform = new ScaleTransform3D(100, 100, 100);
            //cube.BackMaterial = new DiffuseMaterial(Brushes.Transparent);
            //ModelMain.Children.Add(cube);
        }

        ///<summary>初始化模型</summary>
        private void initmodel()
        {
            addControlcube();
            //面对应模型
            for (int i = 0; i < 12; i++)
            {
                idxdata[i].modelgroup = (ModelVisual3D)this.FindName("mg" + i.ToString());
                idxdata[i].colgroup = (ModelVisual3D)this.FindName("mc" + i.ToString());

                ((idxdata[i].modelgroup.Content as Model3DGroup).Children[0] as GeometryModel3D).Material = (MaterialGroup)this.FindResource("mat"+idxdata[i].matidx.ToString());
            }
            //====各面材质
            int w, h;
            //double w0, h0;
            //w0 = 100;
            //h0 = 95.10565164;
            TextBlock txt = new TextBlock();
            txt.Text = "一二三四五";
            txt.FontSize = 96;
            txt.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            w = (int)(Math.Ceiling(txt.DesiredSize.Width) * 1.5);
            h = (int)(95.10565164 / 100 * w);
            foreach (indexPlane one in idxdata)
            {
                //背景大类文字
                MyVisualHost myvh = new MyVisualHost();
                Grid grd = new Grid();
                txt = new TextBlock();
                txt.Foreground = Brushes.LightGray;
                txt.FontSize = 96;
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;
                grd.Width = h;//w;
                grd.Height = h;
                //grd.Background = getColorBrush(one.matidx);

                txt.Text = one.title;
                grd.Children.Add(txt);

                double scale = h / 2;
                Point pc = new Point(h / 2, h / 2); //getTraceCenter(w,h,new Point(0,0));
                double radius;
                foreach (double r in one.radius)
                {
                    radius = scale * r;
                    myvh.CreateDrawingVisualEllipses(Brushes.Transparent, new Pen(Brushes.Black, 0.2), pc, radius, radius);
                }
                // 指标点及文字
                double maxRadius = h / 2;// *1.384957593 / 3.07768354 * 0.9 / 0.9; //界定范围所用最大半径

                Point ppc;
                Rect txtrect = new Rect();
                Brush pointbrush;
                foreach (indexPoint ei in one.getChildren())
                {
                    ppc = transpoint(ei.pcenter, scale, pc);
                    radius = ei.radius * scale * 0.8;  // 备忘：加入渐变色
                    pointbrush = getPointBrush(ei);
                    if (pointbrush != null)
                        myvh.CreateDrawingVisualEllipses(pointbrush, null, ppc, radius, radius);
                    //文字
                    txt = new TextBlock();
                    txt.Text = ei.indexname;
                    txt.FontSize = 20;
                    txt.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    txtrect.Size = txt.DesiredSize;
                    txtrect.Y = ppc.Y + txtrect.Height / 2;
                    txtrect.X = ppc.X - txtrect.Width / 2;
                    //分四象限调整位置
                    double calx;
                    if (ei.angle < 90)
                    {
                        calx = Math.Pow(Math.Pow(maxRadius, 2) - Math.Pow(txtrect.Top - pc.Y, 2), 0.5) + pc.X;
                        if (calx < txtrect.Right) txtrect.X = txtrect.X - (txtrect.Right - calx);
                    }
                    else if (ei.angle < 180)
                    {
                        calx = -Math.Pow(Math.Pow(maxRadius, 2) - Math.Pow(txtrect.Top - pc.Y, 2), 0.5) + pc.X;
                        if (calx > txtrect.Left) txtrect.X = txtrect.X + (calx - txtrect.Left);
                    }
                    else if (ei.angle < 270)
                    {
                        if (txtrect.Bottom > ppc.Y + maxRadius) txtrect.Y = txtrect.Y - (txtrect.Bottom - ppc.Y - maxRadius);
                        calx = -Math.Pow(Math.Pow(maxRadius, 2) - Math.Pow(txtrect.Bottom - pc.Y, 2), 0.5) + pc.X;
                        if (calx > txtrect.Left) txtrect.X = txtrect.X + (calx - txtrect.Left);
                    }
                    else
                    {
                        if (txtrect.Bottom > ppc.Y + maxRadius) txtrect.Y = txtrect.Y - (txtrect.Bottom - ppc.Y - maxRadius);
                        calx = Math.Pow(Math.Pow(maxRadius, 2) - Math.Pow(txtrect.Bottom - pc.Y, 2), 0.5) + pc.X;
                        if (calx < txtrect.Right) txtrect.X = txtrect.X - (txtrect.Right - calx);
                    }
                    myvh.CreateDrawingVisualText(ei.indexname, null, txt.FontSize, Brushes.Black, txtrect.TopLeft);



                    // 指标点的 3d模型, 分类表现，百分比

                    genPointModel(ei); //备忘：改进到单独的modelvisual3D大组中，可获半透效果，可改进为程序化生成12个面


                }
                grd.Children.Add(myvh);
                if (one.title == "首页")
                {
                    //补入首页图片
                }
                grd.Measure(new System.Windows.Size(h, h));
                grd.Arrange(new Rect(0, 0, h, h));
                System.Windows.Media.Brush brush = null;
                RenderTargetBitmap renderTarget = new RenderTargetBitmap(h, h, 96, 96, PixelFormats.Pbgra32);
                renderTarget.Render(grd);
                renderTarget.Freeze();
                brush = new ImageBrush(renderTarget);
                MaterialGroup matgroup = new MaterialGroup();
                matgroup.Children.Add(new DiffuseMaterial(brush));
                //matgroup.Children.Add(new SpecularMaterial(Brushes.White, 90));

                ((GeometryModel3D)(one.modelgroup.Content as Model3DGroup).Children[0]).Material =(MaterialGroup)this.FindResource("mat"+one.matidx.ToString()) ;//new DiffuseMaterial(getColorBrush(one.matidx));


                //=======底圆盘model
                MeshGeometry3D ellmesh = (MeshGeometry3D)this.FindResource("meshCylTop");
                GeometryModel3D ellmodel = new GeometryModel3D(ellmesh, matgroup);
                Transform3DGroup tg = new Transform3DGroup();
                tg.Children.Add(new ScaleTransform3D(1.384957593 * 0.98, 1, 1.384957593 * 0.98, 0, 1, 0));
                tg.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90), new Point3D(0, 1, 0)));
                tg.Children.Add(new TranslateTransform3D(0, 0.384957593, 0.01));
                ellmodel.Transform = tg;
                ((Model3DGroup)(one.modelgroup.Content as Model3DGroup).Children[1]).Children.Add(ellmodel);


            }
        }
        ///<summary>返回轨迹中心点</summary>
        private Point getTraceCenter(double w, double h, Point bp)  //返回轨迹中心点
        {
            Vector offset = new Vector(w / 2, h - h * 1.384957593 / 3.07768354);
            Point pc = bp + offset;
            return pc;
        }
        ///<summary>指标点位置换算，h为所在平面的高，pc为轨迹圆心点</summary>
        private Point transpoint(Point ppc, double scale, Point pc) //指标点位置换算，h为所在平面的高，pc为轨迹圆心点
        {
            ppc.X = ppc.X * scale;
            ppc.Y = ppc.Y * scale;
            ppc = ppc + (Vector)pc;
            return ppc;
        }
        ///<summary>根据值的情况返回提示brush,null则不绘</summary>
        private Brush getPointBrush(indexPoint ip)  //根据值的情况返回提示brush,null则不绘
        {
            Brush brush = null;
            int sel = 0; //0正常，1黄，2红
            double vmin, vmax;
            if (ip.refer1 != null)
            {
                vmin = vmax = (double)ip.refer1;
                if (ip.refer2 != null)
                {
                    vmin = Math.Min(vmin, (double)ip.refer2);
                    vmax = Math.Max(vmax, (double)ip.refer2);
                }

                switch (ip.referType)
                {
                    case ">":
                        if (ip.value >= vmax) sel = 0;
                        else if (ip.value < vmin) sel = 2;
                        else
                            sel = 1;
                        break;
                    case "<":
                        if (ip.value <= vmin) sel = 0;
                        else if (ip.value > vmax) sel = 2;
                        else
                            sel = 1;
                        break;
                    case "<>":
                        if (ip.value >= vmin && ip.value <= vmax)
                            sel = 0;
                        else
                            sel = 2;
                        break;
                }
                if (sel == 1)
                {
                    brush = (Brush)this.FindResource("brushYellow");
                    ip.gaugeInfo.titleBrush = Brushes.Orange;
                }
                else if (sel == 2) 
                {
                    brush = (Brush)this.FindResource("brushRed");
                    ip.gaugeInfo.titleBrush = Brushes.Red;
                }
            }
            ip.flag = sel;
            return brush;
        }
        private SolidColorBrush getColorBrush(int idx)   // 面的底色
        {
            int selcolor = idx % 5;
            SolidColorBrush brush = null;
            switch (selcolor)
            {
                case 0:
                    brush = new SolidColorBrush(Color.FromRgb(0xE5, 0xF3, 0xFF));
                    break;
                case 1:
                    brush = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xE5));
                    break;
                case 2:
                    brush = new SolidColorBrush(Color.FromRgb(0xE5, 0xFF, 0xE8));
                    break;
                case 3:
                    brush = new SolidColorBrush(Color.FromRgb(0xFF, 0xE5, 0xE6));
                    break;
                case 4:
                    brush = Brushes.PowderBlue;
                    break;
            }

            return brush;
        }
        ///<summary>生成单个指标模型</summary>
        private void genPointModel(indexPoint ip)
        {
            double h = 3.07768354, w = 1.61803399 * 2;  //1,0,0 1.61803399,1.90211303,0 0,3.07768354,0 -1.61803399,1.90211303,0 -1,0,0 0,1.384957593,0
            double scale = 1.384957593 * 0.98;//= h / 2 *0.9;
            Point pc = getTraceCenter(w, h, new Point(-w / 2, 0));

            //基本柱
            MeshGeometry3D mesh;//= (MeshGeometry3D)this.FindResource("cyl");
            Material mat;//= (Material)this.FindResource("matTransparent");
            GeometryModel3D model;// = new GeometryModel3D(mesh, mat);

            Point ppc = transpoint(ip.pcenter, scale, pc);
            ppc.Y = h - ppc.Y; //2D y转换到3D y

            Transform3DGroup tg;// = new Transform3DGroup();
            double scale3d = 0.05;
            //tg.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90)));
            //tg.Children.Add(new ScaleTransform3D(scale3d,scale3d,0.6,0,0,-1));
            //tg.Children.Add(new TranslateTransform3D(ppc.X, ppc.Y, 1));
            //model.Transform = tg;

            //ip.model3d = model;

            //((ip.owner.modelgroup.Content as Model3DGroup).Children[1] as Model3DGroup).Children.Add(model);

            // 计算模型参数

            double maxScaleY = 0.6, baseScaleY = 0.3;
            double miny = double.PositiveInfinity, maxy = double.NegativeInfinity, devy, avgy;
            double secb, sec1 = double.NegativeInfinity, sec2 = double.NegativeInfinity, sect;
            if (ip.refer1 != null)
            {
                miny = Math.Min((double)ip.value, (double)ip.refer1);
                maxy = Math.Max((double)ip.value, (double)ip.refer1);
                sec1 = (double)ip.refer1;
                if (ip.refer2 != null)
                {
                    miny = Math.Min(miny, (double)ip.refer2);
                    maxy = Math.Max(maxy, (double)ip.refer2);
                    sec1 = Math.Min((double)ip.refer1, (double)ip.refer2);
                    sec2 = Math.Max((double)ip.refer1, (double)ip.refer2);
                }
                devy = maxy - miny;
                secb = miny - devy;
                sect = maxy + devy;
                avgy = (maxy - miny) / 2;
            }
            else
            {
                secb = 0; sect = ip.value * 2;
                avgy = ip.value;
            }
            //确定可用高的scaley
            double canusescale = baseScaleY;
            switch (ip.modelType)
            {
                case "百分比":
                    canusescale = (sect - secb) * (maxScaleY - baseScaleY) + baseScaleY;
                    break;
                case "比值":  //对数逼近y = 0.1346ln(x) + 0.4265
                    if (avgy != 0)
                        canusescale = (0.1346 * Math.Log10(avgy) + 0.4265) * (maxScaleY - baseScaleY) + baseScaleY;
                    else
                        canusescale = baseScaleY;
                    break;
                case "数值":  //对数逼近
                    if (avgy != 0)
                        canusescale = (0.0786 * Math.Log10(avgy) + 0.2541) * (maxScaleY - baseScaleY) + baseScaleY;
                    else
                        canusescale = baseScaleY;
                    break;
            }
            //确定分段模型参数,兼计算仪表盘参数
            List<struModelPara> modelpara = new List<struModelPara>();
            double tmp, sumz = 0; //sumz为累积出z方向位移
            //string strmat = "";
            struModelPara para;
            if (sec1 != double.NegativeInfinity)
            {
                para = new struModelPara();
                para.vecTranslate = new Vector3D(ppc.X, ppc.Y, 1 + sumz);
                tmp = canusescale * (sec1 - secb) / (sect - secb);
                para.vecScale = new Vector3D(scale3d, scale3d, tmp);
                sumz = sumz + tmp * 2;
                ip.gaugeInfo.range[0, 0] = ip.gaugeInfo.startValue;
                ip.gaugeInfo.range[0, 1] = sec1;
                if (ip.referType == ">" || ip.referType == "<>")
                {
                    para.mat = (Material)this.FindResource(ip.flag > 0 ? "matRed" : "matRedTranslucent");
                    ip.gaugeInfo.rangeBrush[0] = Brushes.Red;
                }
                else
                {
                    para.mat = (Material)this.FindResource(ip.flag > 0 ? "matGreen" : "matGreenTranslucent");
                    ip.gaugeInfo.rangeBrush[0] = Brushes.DarkGreen;
                }
                modelpara.Add(para);
            }
            if (sec2 != double.NegativeInfinity)
            {
                para = new struModelPara();
                para.vecTranslate = new Vector3D(ppc.X, ppc.Y, 1 + sumz);
                tmp = canusescale * (sec2 - sec1) / (sect - secb);
                para.vecScale = new Vector3D(scale3d, scale3d, tmp);
                sumz = sumz + tmp * 2;
                ip.gaugeInfo.range[1, 0] = sec1;
                ip.gaugeInfo.range[1, 1] = sec2;
                if (ip.referType == "<>")
                {
                    para.mat = (Material)this.FindResource(ip.flag > 0 ? "matGreen" : "matGreenTranslucent");
                    ip.gaugeInfo.rangeBrush[1] = Brushes.DarkGreen;
                }
                else
                {
                    para.mat = (Material)this.FindResource(ip.flag > 0 ? "matYellow" : "matYellowTranslucent");
                    ip.gaugeInfo.rangeBrush[1] = Brushes.Orange;
                }
                modelpara.Add(para);
            }
            if (sec1 != double.NegativeInfinity)
            {
                para = new struModelPara();
                para.vecTranslate = new Vector3D(ppc.X, ppc.Y, 1 + sumz);
                tmp = canusescale * (sect - (sec2 == double.NegativeInfinity ? sec1 : sec2)) / (sect - secb);
                para.vecScale = new Vector3D(scale3d, scale3d, tmp);
                ip.gaugeInfo.range[2, 0] = sec2 == double.NegativeInfinity ? sec1 : sec2;
                ip.gaugeInfo.range[2, 1] = ip.gaugeInfo.endValue;
                if (ip.referType == "<" || ip.referType == "<>")
                {
                    para.mat = (Material)this.FindResource(ip.flag > 0 ? "matRed" : "matRedTranslucent");
                    ip.gaugeInfo.rangeBrush[2] = Brushes.Red;
                }
                else
                {
                    para.mat = (Material)this.FindResource(ip.flag > 0 ? "matGreen" : "matGreenTranslucent");
                    ip.gaugeInfo.rangeBrush[2] = Brushes.DarkGreen;
                }
                modelpara.Add(para);
            }
            else
            {
                para = new struModelPara();
                para.vecTranslate = new Vector3D(ppc.X, ppc.Y, 1);
                tmp = canusescale;
                para.vecScale = new Vector3D(scale3d, scale3d, tmp);
                para.mat = (Material)this.FindResource("matWhiteTranslucent");
                modelpara.Add(para);
            }
            //值标
            para = new struModelPara();
            tmp = canusescale * (ip.value - secb) / (sect - secb);
            para.vecTranslate = new Vector3D(ppc.X, ppc.Y, 1 + tmp * 2);
            para.vecScale = new Vector3D(scale3d * 1.5, scale3d * 1.5, 0.001);
            para.mat = (Material)this.FindResource("matWhite");
            modelpara.Add(para);


            //  建立模型
            Model3DGroup mg = new Model3DGroup();
            foreach (struModelPara one in modelpara)
            {
                mesh = (MeshGeometry3D)this.FindResource("meshCyl");
                //mat = (Material)this.FindResource("matTransparent");
                mat = one.mat;
                model = new GeometryModel3D(mesh, mat);


                tg = new Transform3DGroup();
                tg.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90)));
                tg.Children.Add(new ScaleTransform3D(one.vecScale, new Point3D(0, 0, -1)));
                tg.Children.Add(new TranslateTransform3D(one.vecTranslate));
                model.Transform = tg;
                model.SetValue(IndexMain.HitTestInfoProperty, ip);
                mg.Children.Add(model);
            }
            ip.model3d = mg;
            //((ip.owner.modelgroup.Content as Model3DGroup).Children[1] as Model3DGroup).Children.Add(mg);
            ((ip.owner.colgroup.Content as Model3DGroup).Children[0] as Model3DGroup).Children.Add(mg);

        }
        private struct struModelPara  // 分段模型参数
        {
            public Material mat;
            public Vector3D vecScale;
            public Vector3D vecTranslate;
        }
        #endregion



        #region 交互控制
        private string _controlStatus = "";
        private Point _oldPoint;
        private void mainViewport3D_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            ScaleTransform3D scale = (ScaleTransform3D)(((Transform3DGroup)((sender as Viewport3D).Camera).Transform).Children[1]);
            double dev;
            if (e.Delta > 0)
                dev = 1.05;
            else
                dev = 0.95;
            scale.ScaleX *= dev;
            scale.ScaleY *= dev;
            scale.ScaleZ *= dev;
        }

        private void mainViewport3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            closeView();
            if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
            {
                _controlStatus = "rotation";
            }
            else if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Pressed)
            {
                _controlStatus = "move";
            }
            mouseDownTime = e.Timestamp;
            _oldPoint = e.GetPosition(sender as Viewport3D);
        }

        private void mainViewport3D_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_status_panel) autoanitimer.Start();
            _controlStatus = "";
        }


        int mouseDownTime;

        //HitTestResult hitresult;

        private void mainViewport3D_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.Timestamp - mouseDownTime < 500) //左键单击
                {
                    Point newPoint = e.GetPosition(sender as Viewport3D);
                   HitTestResult result = VisualTreeHelper.HitTest(sender as Viewport3D, newPoint);
                    //showDetail(result,newPoint);  //显示细节视图
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                if (e.Timestamp - mouseDownTime < 500) //右键单击
                {
                    Point newPoint = e.GetPosition(sender as Viewport3D);
                    HitTestResult result = VisualTreeHelper.HitTest(sender as Viewport3D, newPoint);
                    changeShowStatus(result);  //切换显示状态，平板和12面体
                }
            }

            _controlStatus = "";
        }


        private void mainViewport3D_MouseMove(object sender, MouseEventArgs e)
        {
            autoanitimer.Stop();


            Point newPoint = e.GetPosition(sender as Viewport3D);
            Vector vec = newPoint - _oldPoint;
            //double mgxs = ((OrthographicCamera)((sender as Viewport3D).Camera)).Width/ (sender as Viewport3D).ActualWidth;
            double mgxs = (((PerspectiveCamera)((sender as Viewport3D).Camera)).Position - (new Point3D(0, 1.5, 0))).Length * Math.Tan(22.5 / 180 * Math.PI) * 2 / (sender as Viewport3D).ActualWidth;
            //mgxs = mgxs * ((((Transform3DGroup)camera.Transform).Children[1]) as ScaleTransform3D).ScaleX;
            if (_controlStatus == "move")
            {
                Vector3D vec3d = new Vector3D(vec.X * mgxs, -vec.Y * mgxs, 0);
                ((PerspectiveCamera)((sender as Viewport3D).Camera)).Position -= vec3d;
            }
            else if (_controlStatus == "rotation")
            {
                //改用模型旋转
                RotateTransform3D rota = (RotateTransform3D)(((Transform3DGroup)((sender as Viewport3D).Camera).Transform).Children[0]);

                Vector3D newPoint3D = ProjectToTrackball((sender as Viewport3D).ActualWidth, (sender as Viewport3D).ActualHeight, newPoint);
                Vector3D oldPoint3D = ProjectToTrackball((sender as Viewport3D).ActualWidth, (sender as Viewport3D).ActualHeight, _oldPoint);


                Vector3D axis = Vector3D.CrossProduct(oldPoint3D, newPoint3D);

                double angle = Vector3D.AngleBetween(oldPoint3D, newPoint3D);
                if (axis.Length > 0)
                {
                    Quaternion delta = new Quaternion(axis, angle);
                    //AxisAngleRotation3D r = (AxisAngleRotation3D)rotation;
                    //Quaternion q = new Quaternion(r.Axis, r.Angle);
                    //r.Axis = q.Axis;
                    //r.Angle = q.Angle;
                    rotation.Quaternion = delta * rotation.Quaternion;

                    autoRotationText();
                }

            }
            else
            {
                HitTestResult result = VisualTreeHelper.HitTest(sender as Viewport3D, newPoint);
                if (result is RayMeshGeometry3DHitTestResult)
                {
                    RayMeshGeometry3DHitTestResult ray3DResult = (RayMeshGeometry3DHitTestResult)result;
                    indexPoint zslice = ray3DResult.ModelHit.GetValue(IndexMain.HitTestInfoProperty) as indexPoint;
                    if (zslice != null && !_status_view) //tooltips
                    {
                        double ToolTipOffset = 5;
                        Point position = e.GetPosition(mainViewport3D);
                        this.Cursor = Cursors.Hand;

                        ttContent.Inlines.Clear();
                        Run tt= new Run(zslice.indexname + "：" + zslice.value.ToString(zslice.format));

                        switch (zslice.flag)
                        {
                            case 0:
                                tt.Foreground = Brushes.DarkGreen;
                                break;
                            case 1:
                                tt.Foreground = Brushes.Orange;
                                break;
                            case 2:
                                tt.Foreground = Brushes.Red;
                                break;
                        }
                        ttContent.Inlines.Add(tt);
                        ttContent.Inlines.Add(new LineBreak());
                        ttContent.Inlines.Add(new Run("定义：" + zslice.definition));
                        ttContent.Inlines.Add(new LineBreak());
                        ttContent.Inlines.Add(new Run("参考值：" + zslice.referNote));
                        ttGauge.gaugeInfo = zslice.gaugeInfo;
                        pointTooltip.Placement = PlacementMode.RelativePoint;
                        pointTooltip.PlacementTarget = mainViewport3D;
                        pointTooltip.HorizontalOffset = position.X + ToolTipOffset;
                        pointTooltip.VerticalOffset = position.Y + ToolTipOffset;
                        pointTooltip.IsOpen = true;

                    }
                    else
                    {
                        pointTooltip.IsOpen = false;
                        this.Cursor = Cursors.Arrow;
                    }

                }

            }
            _oldPoint = newPoint;

            e.Handled = true;
        }

        private Vector3D ProjectToTrackball(double width, double height, Point point)
        {
            double x = point.X / (width / 2);    // Scale so bounds map to [0,0] - [2,2]
            double y = point.Y / (height / 2);

            x = (x - 1);                           // Translate 0,0 to the cente
            y = (1 - y);                           // Flip so +Y is up instead of down

            double z2 = 1 - x * x - y * y;       // z^2 = 1 - x^2 - y^2
            double z = z2 > 0 ? Math.Sqrt(z2) : 0;
            return new Vector3D(x, y, z);
        }

        private void autoRotationText()  // 自动旋转保持文字向上
        {
            foreach (indexPlane one in idxdata)
            {
                //indexPlane one = idxdata[0];
                Point3D newp0 = one.modelgroup.TransformToAncestor(ModelMain).Transform(new Point3D(0, 0, 0));
                Point3D newp1 = one.modelgroup.TransformToAncestor(ModelMain).Transform(new Point3D(0, 1, 0));
                Vector3D vecold = new Vector3D(0, 1, 0);
                Vector3D vecnew = newp1 - newp0;
                vecnew = new Vector3D(vecnew.X, vecnew.Y, 0);
                Vector3D axis2 = Vector3D.CrossProduct(vecold, vecnew);
                double angle2 = -Vector3D.AngleBetween(vecold, vecnew) * Math.Sign(axis2.Z);
                Quaternion q = new Quaternion(new Vector3D(0, 0, 1), angle2);
                RotateTransform3D rt = ((RotateTransform3D)((one.modelgroup.Content as Model3DGroup).Children[1].Transform));
                rt.Rotation = new QuaternionRotation3D(q);
                RotateTransform3D rt2 = ((RotateTransform3D)((one.colgroup.Content as Model3DGroup).Children[0].Transform));
                rt2.Rotation = new QuaternionRotation3D(q);

            }
        }


        #endregion 交互控制
        #region 细节视图
        const double viewscale = 0.8; // 细节视图的scale大小
        Point viewpoint; //细节视图的固定左上角坐标
        private void showDetail(HitTestResult result, Point orgPoint)  //显示细节视图
        {
            if (result is RayMeshGeometry3DHitTestResult)
            {
                RayMeshGeometry3DHitTestResult ray3DResult = (RayMeshGeometry3DHitTestResult)result;
                indexPoint ip = ray3DResult.ModelHit.GetValue(IndexMain.HitTestInfoProperty) as indexPoint;
                if (ip != null)
                {
                    //装载细节视图
                    if (ip.viewType!= null && ip.viewType!= "")
                    {
                        pointTooltip.IsOpen = false;
                        indexView ivNew = findIndexView(ip.indexname);
                        if (ivNew == null) { ivNew = createIndexView(ip); }

                        viewpoint = new Point((mainGrid.ActualWidth - viewscale * 1024) / 2, (mainGrid.ActualHeight - viewscale * 768) / 2);

                        ivNew.orgPoint = orgPoint;
                        ivNew.Visibility = Visibility.Visible;
                        _status_ani = true;
                        _status_view = true;
                        ivNew.setActive(false);
                        this.Cursor = Cursors.Arrow;
                        
                        DoubleAnimation myaniNew = new DoubleAnimation(0, viewscale, new Duration(TimeSpan.FromMilliseconds(200)));
                        myaniNew.Name = "id" + ip.indexname;
                        myaniNew.Completed += new EventHandler(myaniNew_Completed);
                        ivNew.scale.BeginAnimation(ScaleTransform.ScaleXProperty, myaniNew);
                        ivNew.scale.BeginAnimation(ScaleTransform.ScaleYProperty, myaniNew);

                        DoubleAnimation taniX = new DoubleAnimation(orgPoint.X, viewpoint.X, new Duration(TimeSpan.FromMilliseconds(200)));
                        DoubleAnimation taniY = new DoubleAnimation(orgPoint.Y, viewpoint.Y, new Duration(TimeSpan.FromMilliseconds(200)));
                        ivNew.translate.BeginAnimation(TranslateTransform.XProperty, taniX);
                        ivNew.translate.BeginAnimation(TranslateTransform.YProperty, taniY);

                        //Panel.SetZIndex(canvIndexView, 2);
                    }
                }
            }
        }
        void myaniNew_Completed(object sender, EventArgs e)
        {
            _status_ani = false;
            string indexID = (sender as AnimationClock).Timeline.Name;
            indexID = indexID.Substring(2);
            indexView iv = findIndexView(indexID);
            if (iv != null)
            {
                iv.setActive(true);
                iv.scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                iv.scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                iv.translate.BeginAnimation(TranslateTransform.XProperty, null);
                iv.translate.BeginAnimation(TranslateTransform.YProperty, null);
                iv.scale.ScaleX = viewscale;
                iv.scale.ScaleY = viewscale;
                iv.translate.X = viewpoint.X;
                iv.translate.Y = viewpoint.Y;
            }
        }

        private indexView findIndexView(string indexID)  //查找index视图
        {
            indexView iv = null;
            for (int i = 0; i < canvIndexView.Children.Count; i++)
            {
                if ((canvIndexView.Children[i] as indexView).indexID == indexID)
                {
                    iv = (indexView)canvIndexView.Children[i];
                    break;
                }
            }
            return iv;
        }
        private indexView createIndexView(indexPoint ip) //创建细节视图
        {
            //indexView iv = new indexView(ip.indexname, ip.viewType, ip.viewInfo, ip.indexdata.getDetailData(vdata.startTime, vdata.dataTime), ip.value.ToString(ip.format));
            indexView iv = null; //zhnote
            iv.CloseEvent += new EventHandler(iv_CloseEvent);
            canvIndexView.Children.Add(iv);
            return iv;
        }

        void iv_CloseEvent(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            viewClose(sender as indexView);
        }
        private void closeView()
        {
            foreach (indexView one in canvIndexView.Children)
            {
                if (one.Visibility == Visibility.Visible)
                {
                    viewClose(one);
                    _status_view = false;
                    break;
                }
            }
        }
        private void viewClose(indexView iv)  //细节视图关闭
        {

            _status_ani = true;
            _status_view = true;
            iv.setActive(false);


            DoubleAnimation aniClose = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(200)));
            aniClose.Name = "id" + iv.indexID;
            aniClose.Completed += new EventHandler(aniClose_Completed);
            iv.scale.BeginAnimation(ScaleTransform.ScaleXProperty, aniClose);
            iv.scale.BeginAnimation(ScaleTransform.ScaleYProperty, aniClose);

            DoubleAnimation taniX = new DoubleAnimation(iv.orgPoint.X, new Duration(TimeSpan.FromMilliseconds(200)));
            DoubleAnimation taniY = new DoubleAnimation(iv.orgPoint.Y, new Duration(TimeSpan.FromMilliseconds(200)));
            iv.translate.BeginAnimation(TranslateTransform.XProperty, taniX);
            iv.translate.BeginAnimation(TranslateTransform.YProperty, taniY);



            //_status_ani = true;
            //DoubleAnimation aniClose = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(500)));
            //aniClose.FillBehavior = FillBehavior.Stop;
            //aniClose.Name = "id" + iv.indexID;
            //aniClose.Completed += new EventHandler(aniClose_Completed);
            //iv.BeginAnimation(indexView.OpacityProperty, aniClose);
        }
        void aniClose_Completed(object sender, EventArgs e)
        {
            string indexID = (sender as AnimationClock).Timeline.Name;
            indexID = indexID.Substring(2);
            indexView iv = findIndexView(indexID);
            if (iv != null)
            {
                iv.Visibility = Visibility.Hidden;
                //iv.BeginAnimation(indexView.OpacityProperty, null);
                iv.scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                iv.scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                iv.translate.BeginAnimation(TranslateTransform.XProperty, null);
                iv.translate.BeginAnimation(TranslateTransform.YProperty, null);

            }
            _status_ani = false;
            _status_view = false;
            //Panel.SetZIndex(canvIndexView, 0);
        }

        #endregion

        #region 切换显示状态
        Quaternion quaterPanel = new Quaternion(new Vector3D(1, 0.00001, 0), -50);
        double toScale,toX;
        private void changeShowStatus(HitTestResult result)
        {
            if (!_status_ani && !_status_view)
            {
                _status_panel = !_status_panel;
                _status_ani = true;
                if (_status_panel)  //切换平板
                {
                    QuaternionAnimation qani = new QuaternionAnimation(quaterPanel, new Duration(TimeSpan.FromMilliseconds(1000)));
                    qani.FillBehavior = FillBehavior.Stop;
                    rotation.BeginAnimation(QuaternionRotation3D.QuaternionProperty, qani);

                    DoubleAnimation dra = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(1000)));
                    rA1.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rA2.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rA3.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rA4.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rAkey.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rB1.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rB2.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rB3.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rB4.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);

                    rAB1.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rAB2.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rBB.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    toScale = 1;
                    toX = -3.7;
                    DoubleAnimation drat = new DoubleAnimation(toX, new Duration(TimeSpan.FromMilliseconds(1000)));
                    DoubleAnimation dras = new DoubleAnimation(toScale, new Duration(TimeSpan.FromMilliseconds(1000)));
                    dras.Completed += new EventHandler(toPanelEnd);
                    dras.FillBehavior = FillBehavior.Stop;
                    drat.FillBehavior = FillBehavior.Stop;
                    translate.BeginAnimation(TranslateTransform3D.OffsetXProperty, drat);
                    mScale.BeginAnimation(ScaleTransform3D.ScaleXProperty, dras);
                    mScale.BeginAnimation(ScaleTransform3D.ScaleYProperty, dras);
                    mScale.BeginAnimation(ScaleTransform3D.ScaleZProperty, dras);

                }
                else  // 切换至12面体
                {
                    DoubleAnimation dra = new DoubleAnimation(-63.43495, new Duration(TimeSpan.FromMilliseconds(1000)));
                    rA1.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rA2.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rA3.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rA4.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rAkey.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rB1.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rB2.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rB3.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rB4.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);

                    rAB1.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rAB2.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    rBB.BeginAnimation(AxisAngleRotation3D.AngleProperty, dra);
                    toScale = 0.5;
                    toX = 0;
                    DoubleAnimation drat = new DoubleAnimation(toX, new Duration(TimeSpan.FromMilliseconds(1000)));
                    DoubleAnimation dras = new DoubleAnimation(toScale, new Duration(TimeSpan.FromMilliseconds(1000)));
                    dras.Completed += new EventHandler(to12End);
                    dras.FillBehavior = FillBehavior.Stop;
                    drat.FillBehavior = FillBehavior.Stop;
                    translate.BeginAnimation(TranslateTransform3D.OffsetXProperty, drat);
                    mScale.BeginAnimation(ScaleTransform3D.ScaleXProperty, dras);
                    mScale.BeginAnimation(ScaleTransform3D.ScaleYProperty, dras);
                    mScale.BeginAnimation(ScaleTransform3D.ScaleZProperty, dras);
                }

            }
        }

        private void toPanelEnd(object sender, EventArgs e)
        {
            rotation.Quaternion = quaterPanel;
            mScale.ScaleX = toScale;
            mScale.ScaleY = toScale;
            mScale.ScaleZ = toScale;
            translate.OffsetX = toX;
            _status_ani=false;
            autoRotationText();
        }

        private void to12End(object sender, EventArgs e)
        {
            mScale.ScaleX = toScale;
            mScale.ScaleY = toScale;
            mScale.ScaleZ = toScale;
            translate.OffsetX = toX;
            _status_ani = false;
            autoRotationText();
        }


#endregion


        #region 计算

        private void calIndexValue()
        {

            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < idxdata[i].getChildren().Count; j++)
                {
                    indexPoint ip = idxdata[i].getChildren()[j];
                    if (ip.valueSQL != null && ip.valueSQL != "")
                    {
                        ip.value = (double)calSqlValue(ip.valueSQL);
                        ip.gaugeInfo.value = ip.value;
                    }
                }
            }
        }

        private Nullable<double> calSqlValue(string strsql)
        {
            if (strsql == "") return null;
   
            OleDbConnection conn;
            //if (strsql.Substring(0, 5) == "vids:")
                conn = new OleDbConnection("Provider=MSDAORA;Data Source=vids;Persist Security Info=True;Password=vids;User ID=vids");
            //else if (strsql.Substring(0, 5) == "pmos:")
                //conn = new OleDbConnection("Provider=MSDAORA;Data Source=pmos;Persist Security Info=True;Password=pmos;User ID=pmos");
            //else
                //return null;

                string sql = strsql; //strsql.Substring(5);



            string sdb = dateb.ToString("yyyy/MM/dd");
            string sde = datee.AddDays(1).ToString("yyyy/MM/dd");

            DataContext db = new DataContext(conn);
            sql = sql.Replace("{0}", sdb);
            sql = sql.Replace("{1}", sde);


            return (double)db.ExecuteQuery<decimal>(sql).First();

        }

        #endregion

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            pointTooltip.IsOpen = false;
            this.Cursor = Cursors.Arrow;
        }


    }







}
