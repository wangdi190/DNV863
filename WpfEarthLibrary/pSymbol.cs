using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfEarthLibrary
{
    public class pSymbol
    {
        string _id;
        public string id
        {
            get { return _id; }
            set
            {
                _id = value;
                hashcode = value.GetHashCode();
            }
        }
        internal int hashcode;

        public string sort { get;set;}
        public string name { get; set; }
        public double sizeX;
        public double sizeY;

        
        private System.Windows.Media.Brush _brush;
        public System.Windows.Media.Brush brush
        {
            get { 
                if (_brush==null && !string.IsNullOrWhiteSpace(imagefile))
                {
                    System.Windows.Media.Imaging.BitmapImage bi = new System.Windows.Media.Imaging.BitmapImage();
                    Uri overlayuri = new Uri(Environment.CurrentDirectory + "\\"+imagefile);
                    bi.BeginInit();
                    bi.UriSource = overlayuri;
                    bi.EndInit();
                    _brush = new System.Windows.Media.ImageBrush(bi);
                }
                return _brush; 
            }
            set { _brush = value; }
        }
                      

        ///<summary>图片文件名</summary>
        public string imagefile;
        ///<summary>纹理文件名</summary>
        public string texturefile;
    }

    public class pStyle
    {
    }

    public class pGeometry
    {
        string _id;
        public string id
        { get { return _id; }
            set { _id = value; hashcode = value.GetHashCode(); }
        }
        public EGeometryType goetype;
        public float pf1;
        public float pf2;
        public float pf3;
        public int pi1;
        public int pi2;

        internal int hashcode;
    }

    public class pXModel
    {
        public enum EModelType {X模型,Custom模型}

        string _id;
        public string id
        {
            get { return _id; }
            set { _id = value; hashcode = value.GetHashCode(); }
        }
        public string note;
        public string filepath;

        ///<summary>初始旋转轴</summary>
        public VECTOR3D rotationAxis=new VECTOR3D(0,0,1);
        ///<summary>初始旋转角度，弧度值</summary>
        public float rotationAngle=0;

        internal int hashcode;

        public EModelType eModelType;



        public List<VECTOR3D> VecVertices; 
        public List<VECTOR3D> VecNormals;
        public List<ushort> VecIndexes;
        public List<VECTOR2D> uvs;
        public string texture;

    }
}
