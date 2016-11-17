using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MyClassLibrary.Share3D
{
    public class Plane3D : ModelVisual3D
    {
        public Plane3D()
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
            _content.Children.Add(new GeometryModel3D(mesh, BackMaterial));
            //<MeshGeometry3D x:Key="x1"
            TriangleIndices = "0,2,1,2,0,3 ";
            Normals = "1,0,0|1,0,0|1,0,0|1,0,0";
            TextureCoordinates = "1,1|0,1|0,0|1,0";
            Positions = "1,-1,-1|1,-1,1|1,1,1|1,1,-1";
            mesh = genMesh(TriangleIndices, Normals, TextureCoordinates, Positions);
            _content.Children.Add(new GeometryModel3D(mesh, BackMaterial));
            //<MeshGeometry3D x:Key="x0"
            TriangleIndices = "0,1,2,2,3,0";
            Normals = "-1,0,0|-1,0,0|-1,0,0|-1,0,0";
            TextureCoordinates = "1,1|0,1|0,0|1,0";
            Positions = "-1,-1,-1|-1,-1,1|-1,1,1|-1,1,-1";
            mesh = genMesh(TriangleIndices, Normals, TextureCoordinates, Positions);
            _content.Children.Add(new GeometryModel3D(mesh, BackMaterial));
            //<MeshGeometry3D x:Key="y1"
            TriangleIndices = "0,2,1,2,0,3";
            Normals = "0,1,0|0,1,0|0,1,0|0,1,0";
            TextureCoordinates = "0,0|1,0|1,1|0,1";
            Positions = "-1,1,-1|1,1,-1|1,1,1|-1,1,1";
            mesh = genMesh(TriangleIndices, Normals, TextureCoordinates, Positions);
            _content.Children.Add(new GeometryModel3D(mesh, BackMaterial));
            //<MeshGeometry3D x:Key="y0"
            TriangleIndices = "0,1,2,2,3,0";
            Normals = "0,-1,0|0,-1,0|0,-1,0|0,-1,0";
            TextureCoordinates = "1,1|0,1|0,0|1,0";
            Positions = "-1,-1,-1|1,-1,-1|1,-1,1|-1,-1,1";
            mesh = genMesh(TriangleIndices, Normals, TextureCoordinates, Positions);
            _content.Children.Add(new GeometryModel3D(mesh, BackMaterial));
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

        private string _info;
        public string info
        {
            get { return _info; }
            set { _info = value; }
        }

        //正面材质
        public static DependencyProperty planeMaterialProperty =
            DependencyProperty.Register(
                "planeMaterial",
                typeof(Material),
                typeof(Plane3D), new PropertyMetadata(
                    null, new PropertyChangedCallback(OnMaterialChanged)));
        public Material Material
        {
            get { return (Material)GetValue(planeMaterialProperty); }
            set { SetValue(planeMaterialProperty, value); }
        }
        //其它各面材质
        public static DependencyProperty planeBackMaterialProperty =
            DependencyProperty.Register(
                "planeBackMaterial",
                typeof(Material),
                typeof(Plane3D), new PropertyMetadata(
                    null, new PropertyChangedCallback(OnMaterialChanged)));
        public Material BackMaterial
        {
            get { return (Material)GetValue(planeBackMaterialProperty); }
            set { SetValue(planeBackMaterialProperty, value); }
        }


        internal static void OnMaterialChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            Plane3D p = ((Plane3D)sender);
            ((GeometryModel3D)p._content.Children[0]).Material = p.Material;
            ((GeometryModel3D)p._content.Children[1]).Material = p.BackMaterial;
            ((GeometryModel3D)p._content.Children[2]).Material = p.BackMaterial;
            ((GeometryModel3D)p._content.Children[3]).Material = p.BackMaterial;
            ((GeometryModel3D)p._content.Children[4]).Material = p.BackMaterial;
            ((GeometryModel3D)p._content.Children[5]).Material = p.BackMaterial;
        }



        internal double DegToRad(double degrees)
        {
            return (degrees / 180.0) * Math.PI;
        }

        public Rect3D getBouns()
        {
            return _content.Bounds;
        }


        //internal abstract Geometry3D Tessellate();

        //internal readonly GeometryModel3D _content = new GeometryModel3D();
        internal Model3DGroup _content = new Model3DGroup();
    }
}
