using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MyClassLibrary.Share3D
{
    public class Ground3D : ModelVisual3D
    {
        internal Model3DGroup _content = new Model3DGroup();

        Transform3DGroup tg = new Transform3DGroup();
        TranslateTransform3D translate = new TranslateTransform3D(0, -1, 0);
        ScaleTransform3D scale = new ScaleTransform3D(100, 0.2, 100);  //模原型为2*2*2 ，初始化为200, 0.4, 200

        public Ground3D()
        {
            Content = _content;

            //<MeshGeometry3D x:Key="z1"
            string TriangleIndices = "0,1,2,2,3,0";
            string Normals = "0,0,1|0,0,1|0,0,1|0,0,1";
            string TextureCoordinates = "0,1|1,1|1,0|0,0";
            string Positions = "-1,-1,1|1,-1,1|1,1,1|-1,1,1";
            MeshGeometry3D mesh = genMesh(TriangleIndices, Normals, TextureCoordinates, Positions);
            _content.Children.Add(new GeometryModel3D(mesh, Material));
            //<MeshGeometry3D x:Key="z0"
            TriangleIndices = "0,2,1,2,0,3 ";
            Normals = "0,0,-1|0,0,-1|0,0,-1|0,0,-1";
            TextureCoordinates = "1,1|0,1|0,0|1,0";
            Positions = "-1,-1,-1|1,-1,-1|1,1,-1|-1,1,-1";
            mesh = genMesh(TriangleIndices, Normals, TextureCoordinates, Positions);
            _content.Children.Add(new GeometryModel3D(mesh, OtherMaterial));
            //<MeshGeometry3D x:Key="x1"
            TriangleIndices = "0,2,1,2,0,3 ";
            Normals = "1,0,0|1,0,0|1,0,0|1,0,0";
            TextureCoordinates = "1,1|0,1|0,0|1,0";
            Positions = "1,-1,-1|1,-1,1|1,1,1|1,1,-1";
            mesh = genMesh(TriangleIndices, Normals, TextureCoordinates, Positions);
            _content.Children.Add(new GeometryModel3D(mesh, OtherMaterial));
            //<MeshGeometry3D x:Key="x0"
            TriangleIndices = "0,1,2,2,3,0";
            Normals = "-1,0,0|-1,0,0|-1,0,0|-1,0,0";
            TextureCoordinates = "1,1|0,1|0,0|1,0";
            Positions = "-1,-1,-1|-1,-1,1|-1,1,1|-1,1,-1";
            mesh = genMesh(TriangleIndices, Normals, TextureCoordinates, Positions);
            _content.Children.Add(new GeometryModel3D(mesh, OtherMaterial));
            //<MeshGeometry3D x:Key="y1"
            TriangleIndices = "0,2,1,2,0,3";
            Normals = "0,1,0|0,1,0|0,1,0|0,1,0";
            TextureCoordinates = "0,0|1,0|1,1|0,1";
            Positions = "-1,1,-1|1,1,-1|1,1,1|-1,1,1";
            mesh = genMesh(TriangleIndices, Normals, TextureCoordinates, Positions);
            //mesh = Model3DHelper.genCylinder3DTopMesh();


            _content.Children.Add(new GeometryModel3D(mesh, OtherMaterial));
            //<MeshGeometry3D x:Key="y0"
            TriangleIndices = "0,1,2,2,3,0";
            Normals = "0,-1,0|0,-1,0|0,-1,0|0,-1,0";
            TextureCoordinates = "1,1|0,1|0,0|1,0";
            Positions = "-1,-1,-1|1,-1,-1|1,-1,1|-1,-1,1";
            mesh = genMesh(TriangleIndices, Normals, TextureCoordinates, Positions);
            _content.Children.Add(new GeometryModel3D(mesh, OtherMaterial));

            //==================
            tg.Children.Add(translate);
            tg.Children.Add(scale);
            this.Transform = tg;

            Material = getGroundMaterial();
            OtherMaterial = new DiffuseMaterial(Brushes.LightGray);
        }

        private MeshGeometry3D genMesh(string TriangleIndices, string Normals, string TextureCoordinates, string Positions)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            string str;
            str = Positions;
            string[] astr = str.Split('|');
            for (int i = 0; i < astr.Length; i++)
            {
                string one = astr[i];
                Point3D p3d = Point3D.Parse(one);
                mesh.Positions.Add(p3d);
            }
            str = TextureCoordinates;
            astr = str.Split('|');
            for (int i = 0; i < astr.Length; i++)
            {
                string one = astr[i];
                Point p2d = Point.Parse(one);
                mesh.TextureCoordinates.Add(p2d);
            }
            str = Normals;
            astr = str.Split('|');
            for (int i = 0; i < astr.Length; i++)
            {
                string one = astr[i];
                Vector3D v3d = Vector3D.Parse(one);
                mesh.Normals.Add(v3d);
            }
            str = TriangleIndices;
            astr = str.Split(',');
            for (int i = 0; i < astr.Length; i++)
            {
                string one = astr[i];
                int tri = int.Parse(one);
                mesh.TriangleIndices.Add(tri);
            }
            mesh.Freeze();
            return mesh;

        }

        #region 地面参数

        public static DependencyProperty GroundSizeProperty = DependencyProperty.Register("GroundSize", typeof(Size3D), typeof(Ground3D), new PropertyMetadata(new Size3D(200,0.4,200), new PropertyChangedCallback(OnGroundSizeChanged)));
        /// <summary>
        /// 地面立方体尺寸，缺省100,0.2,100
        /// </summary>
        public Size3D GroundSize
        {
            get { return (Size3D)GetValue(GroundSizeProperty); }
            set { SetValue(GroundSizeProperty, value); }
        }
        internal static void OnGroundSizeChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            Ground3D p = ((Ground3D)sender);
            p.scale.ScaleX = ((Size3D)e.NewValue).X / 2;
            p.scale.ScaleY = ((Size3D)e.NewValue).Y / 2;
            p.scale.ScaleZ = ((Size3D)e.NewValue).Z / 2;
        }

        public static DependencyProperty GridSizeProperty = DependencyProperty.Register("GridSize", typeof(Size), typeof(Ground3D), new PropertyMetadata(new Size(1,1), new PropertyChangedCallback(OnGridSizeChanged)));
        /// <summary>
        /// 网格尺寸，缺省1,1为一格, width为X方向
        /// </summary>
        public Size GridSize
        {
            get { return (Size)GetValue(GridSizeProperty); }
            set { SetValue(GridSizeProperty, value); }
        }
        internal static void OnGridSizeChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            Ground3D p = ((Ground3D)sender);
            p.Material = p.getGroundMaterial();
        }


        #endregion

        #region 材质
        //正面材质
        public static DependencyProperty GroundMaterialProperty = DependencyProperty.Register("GroundMaterial", typeof(Material), typeof(Ground3D), new PropertyMetadata(null, new PropertyChangedCallback(OnMaterialChanged)));
        public Material Material
        {
            get { return (Material)GetValue(GroundMaterialProperty); }
            set { SetValue(GroundMaterialProperty, value); }
        }
        //其它各面材质
        public static DependencyProperty GroundOtherMaterialProperty =
            DependencyProperty.Register(
                "GroundOtherMaterial",
                typeof(Material),
                typeof(Ground3D), new PropertyMetadata(
                    null, new PropertyChangedCallback(OnMaterialChanged)));
        public Material OtherMaterial
        {
            get { return (Material)GetValue(GroundOtherMaterialProperty); }
            set { SetValue(GroundOtherMaterialProperty, value); }
        }

        internal static void OnMaterialChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            Ground3D p = ((Ground3D)sender);
            ((GeometryModel3D)p._content.Children[4]).Material = p.Material;
            ((GeometryModel3D)p._content.Children[1]).Material = p.OtherMaterial;
            ((GeometryModel3D)p._content.Children[2]).Material = p.OtherMaterial;
            ((GeometryModel3D)p._content.Children[3]).Material = p.OtherMaterial;
            ((GeometryModel3D)p._content.Children[0]).Material = p.OtherMaterial;
            ((GeometryModel3D)p._content.Children[5]).Material = p.OtherMaterial;
        }

        private Material getGroundMaterial()
        {
            Grid grd = new Grid();
            int w = 128, h = 128;
            grd.Width = w; grd.Height = h;
            Border brd = new Border();
            brd.Background = new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0x00, 0xFF));
            brd.BorderBrush = new SolidColorBrush(Colors.LightGray);
            brd.BorderThickness = new Thickness(1);
            grd.Children.Add(brd);
            grd.Measure(new System.Windows.Size(w, h));
            grd.Arrange(new Rect(0, 0, w, h));

            ImageBrush brush = null;
            RenderTargetBitmap renderTarget = new RenderTargetBitmap(128, 128, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(grd);
            renderTarget.Freeze();
            brush = new ImageBrush(renderTarget);
            brush.TileMode = TileMode.Tile;
            double cellxpara = GridSize.Width / GroundSize.X;
            double cellzpara = GridSize.Height / GroundSize.Z;
            brush.Viewport = new Rect(0, 0, cellxpara, cellzpara);
            return new DiffuseMaterial(brush);
        }

        #endregion

        #region 其它
        internal double DegToRad(double degrees)
        {
            return (degrees / 180.0) * Math.PI;
        }

        public Rect3D Bouns()
        {
            return _content.Bounds;
        }

        //地面的Geomodel和整个的transform，供点击测试引用
        public GeometryModel3D GroundSurfaceModel
        { get { return _content.Children[4] as GeometryModel3D; } }
        public Transform3D GroundTransform
        { get { return this.Transform; } }//_content.Transform; } }


        #endregion
    }
}
