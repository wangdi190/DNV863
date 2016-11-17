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
using System.Xml;
using System.Text.RegularExpressions;
using WpfEarthLibrary;


namespace DNV863
{
    /// <summary>
    /// UCSample2.xaml 的交互逻辑
    /// 示例非地理图形的呈现
    /// </summary>
    public partial class UCSample2 : UserControl
    {
        public UCSample2()
        {
            InitializeComponent();
        }

        Earth uc;
        private void UserControl_Initialized(object sender, EventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            uc = new Earth();
            uc.config.enableMinimap = false;
            grdMain.Children.Add(uc);

            uc.earthManager.mapType = EMapType.无;  //设置为无地图模式
            uc.earthManager.earthpara.setBackground(Color.FromArgb(0x00, 0x00, 0x00, 0x00)); //设置透明

            loadFromSvg("112.svg");
        }

        ///<summary>解析svg文件</summary>
        void loadFromSvg(string filename)
        {
            //示例从svg中读取非地理数据展现

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = false;

            ;
            //settings.LineNumberOffset = 2;
            XmlReader reader = XmlReader.Create(filename, settings);
            string elename;
            string id, fill, stroke, width, height, data, layer, symbol, translate, rotate, scale, zclass, shapetype, layerid;
            id = layerid = "";
            int depth;

            //解析图元用
            bool istext = false;
            Brush brush = null;
            DrawingGroup drawgroup = new DrawingGroup();
            GeometryDrawing aDrawing;
            WpfEarthLibrary.pSymbol psymbol = null;
            //层
            WpfEarthLibrary.pLayer player = null;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.CDATA)
                {
                    string tmp = reader.Value;
                    string[] stringSeparators = new string[] { "\n" };
                    string[] ss = tmp.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in ss)
                    {
                        Regex regex = new Regex(".(.*?) {stroke:(.*?); fill:(.*?) }", RegexOptions.Multiline);
                        Match m = regex.Match(s);
                        if (m.Success)
                        {
                            string stylename = m.Groups[1].Value;
                            stroke = m.Groups[2].Value;
                            fill = m.Groups[3].Value;

                            //zh注：style暂未实现

                        }
                    }
                }

                if (reader.IsStartElement())
                {
                    elename = reader.Name;
                    switch (elename)
                    {
                        case "svg":  //  svg
                            string tmp2 = getPropertyValue(reader, "viewBox");
                            if (tmp2 != null)
                            {
                                Rect svgviewbox = Rect.Parse(tmp2);
                                uc.earthManager.planeViewBox = svgviewbox; //必须设置，方能自动调整相机到适当位置，若显现看起来没居中，是图形与viewbox不完全匹配，可手动调整这个viewbox数值以居中
                            }
                            break;
                        case "symbol":  //  图元符号
                            depth = reader.Depth;
                            if (reader.HasAttributes)
                            {
                                string symbolid = getPropertyValue(reader, "id");
                                string viewbox = getPropertyValue(reader, "viewBox");
                                reader.MoveToElement();

                                //开始一个图元的处理，创建一个图元对象并加入到uc.objmanager的图元字典中，以便框架生成公用材质供厂站使用，在本示例中，仅breakor使用到了公用图元
                                drawgroup = new DrawingGroup();
                                Rect symbolviewbox = new Rect(0, 0, 2, 2);
                                if (!string.IsNullOrWhiteSpace(viewbox))
                                    symbolviewbox = Rect.Parse(viewbox);
                                psymbol = new pSymbol() { id = symbolid, name = symbolid, sizeX = symbolviewbox.Width, sizeY = symbolviewbox.Height };
                                uc.objManager.zSymbols.Add(symbolid, psymbol);
                                istext = false;

                            }

                            while (reader.Read())
                            {
                                if (reader.Depth == depth && reader.NodeType == XmlNodeType.EndElement)
                                {
                                    //结束一个图元处理，对图元的brush赋值
                                    if (!istext)
                                    {
                                        DrawingBrush myDrawingBrush = new DrawingBrush();
                                        myDrawingBrush.Drawing = drawgroup;
                                        psymbol.brush = myDrawingBrush;
                                    }

                                    break;
                                }
                                switch (reader.Name)
                                {
                                    case "circle":
                                        if (reader.HasAttributes)
                                        {
                                            fill = getPropertyValue(reader, "fill");
                                            stroke = "rgb(0, 0, 0)";
                                            width = getPropertyValue(reader, "stroke-width");
                                            data = getPropertyValue(reader, "cx") + "," + getPropertyValue(reader, "cy") + "|" + getPropertyValue(reader, "r");
                                            reader.MoveToElement();
                                            //绘图元元素
                                            Regex regex = new Regex("(\\d*.?\\d*,\\d*.?\\d*)\\|(\\d*.?\\d*)", RegexOptions.Multiline);
                                            Match m = regex.Match(data);
                                            if (m.Success)
                                            {
                                                Point pc = Point.Parse(m.Groups[1].Value);
                                                double r = double.Parse(m.Groups[2].Value);

                                                EllipseGeometry geo = new EllipseGeometry(pc, r, r);
                                                aDrawing = new GeometryDrawing();
                                                aDrawing.Geometry = geo;

                                                Pen pen = new Pen();
                                                double thickness = double.Parse(width);
                                                thickness = thickness < 2 ? 2 : thickness;
                                                pen.Thickness = thickness;
                                                //brush = ;//defaultbrush;//强制缺省用黄色//brush = anaBrush(item["fill"].ToString());
                                                brush = new SolidColorBrush(Colors.Red);
                                                pen.Brush = brush;
                                                aDrawing.Pen = pen;
                                                drawgroup.Children.Add(aDrawing);
                                            }



                                        }
                                        break;
                                    case "ellipse":
                                        if (reader.HasAttributes)
                                        {
                                            fill = getPropertyValue(reader, "fill");
                                            stroke = "rgb(0, 0, 0)";
                                            width = getPropertyValue(reader, "stroke-width");
                                            double cx = double.Parse(getPropertyValue(reader, "cx"));
                                            double cy = double.Parse(getPropertyValue(reader, "cy"));
                                            double rx = double.Parse(getPropertyValue(reader, "rx"));
                                            double ry = double.Parse(getPropertyValue(reader, "ry"));

                                            reader.MoveToElement();
                                            //绘图元元素
                                            Point pc = new Point(cx, cy);
                                            EllipseGeometry geo = new EllipseGeometry(pc, rx, ry);
                                            aDrawing = new GeometryDrawing();
                                            aDrawing.Geometry = geo;

                                            Pen pen = new Pen();
                                            double thickness = double.Parse(width);
                                            thickness = thickness < 2 ? 2 : thickness;
                                            pen.Thickness = thickness;
                                            brush = Brushes.Yellow;  //注，色彩应从fill取，示例没做转换，直接使用黄色
                                            pen.Brush = brush;
                                            aDrawing.Pen = pen;
                                            drawgroup.Children.Add(aDrawing);

                                        }
                                        break;
                                    case "rect":
                                        if (reader.HasAttributes)
                                        {
                                            fill = getPropertyValue(reader, "fill");
                                            stroke = "rgb(0, 0, 0)";
                                            width = getPropertyValue(reader, "width");
                                            height = getPropertyValue(reader, "height");
                                            data = getPropertyValue(reader, "cx") + "," + getPropertyValue(reader, "cy") + "|" + getPropertyValue(reader, "r");
                                            reader.MoveToElement();
                                            //绘图元元素
                                            RectangleGeometry geo = new RectangleGeometry();
                                            geo.Rect = new Rect(0, 0, double.Parse(width), double.Parse(height));

                                            aDrawing = new GeometryDrawing();
                                            aDrawing.Geometry = geo;

                                            aDrawing.Brush = new SolidColorBrush(Colors.Aqua);
                                            drawgroup.Children.Add(aDrawing);



                                        }
                                        break;
                                    case "path":
                                        if (reader.HasAttributes)
                                        {
                                            fill = getPropertyValue(reader, "fill");
                                            stroke = "rgb(0, 0, 0)";
                                            width = getPropertyValue(reader, "stroke-width");
                                            data = getPropertyValue(reader, "d");
                                            reader.MoveToElement();
                                            //绘图元元素
                                            Geometry geo = PathGeometry.Parse(data);
                                            aDrawing = new GeometryDrawing();
                                            aDrawing.Geometry = geo;

                                            brush = Brushes.Orange;
                                            aDrawing.Brush = brush;
                                            Pen pen = new Pen();
                                            pen.Thickness = double.Parse(width);
                                            brush = Brushes.Blue;
                                            pen.Brush = brush;
                                            aDrawing.Pen = pen;
                                            drawgroup.Children.Add(aDrawing);


                                        }
                                        break;
                                    case "line":
                                        if (reader.HasAttributes)
                                        {
                                            //<line stroke="rgb(0, 0, 0)" stroke-width="1.000000" x1="50.000000" y1="7.499999" x2="13.193920" y2="71.250001" />
                                            stroke = getPropertyValue(reader, "stroke");
                                            width = getPropertyValue(reader, "stroke-width");
                                            data = string.Format("M {0} {1} L {2} {3}", getPropertyValue(reader, "x1"), getPropertyValue(reader, "y1"), getPropertyValue(reader, "x2"), getPropertyValue(reader, "y2"));
                                            reader.MoveToElement();
                                            //绘图元元素


                                        }
                                        break;
                                    case "text":
                                        if (reader.HasAttributes)
                                        {
                                            stroke = getPropertyValue(reader, "stroke");


                                            reader.MoveToElement();
                                            //绘图元元素
                                            psymbol.brush = Brushes.SkyBlue;
                                            istext = true;
                                        }
                                        break;


                                }
                            }
                            break;
                        case "g":
                            depth = reader.Depth;
                            if (reader.HasAttributes)
                            {
                                reader.MoveToAttribute("id");
                                layer = reader.Value;
                                reader.MoveToElement();

                                //开始一个层，在uc.objmanager中创建一个层
                                player = uc.objManager.AddLayer(layer, layer, layer);


                            }
                            while (reader.Read())
                            {
                                if (reader.Depth == depth && reader.NodeType == XmlNodeType.EndElement)
                                    break;

                                if (reader.Name == "use" && reader.NodeType == XmlNodeType.Element)
                                {
                                    shapetype = "dot";
                                    id = getPropertyValue(reader, "id");
                                    fill = getPropertyValue(reader, "fill");
                                    stroke = getPropertyValue(reader, "stroke");
                                    symbol = getPropertyValue(reader, "xlink:href");
                                    zclass = getPropertyValue(reader, "class");
                                    //data = getPropertyValue(reader, "transform");
                                    double x = double.Parse(getPropertyValue(reader, "x"));
                                    double y = double.Parse(getPropertyValue(reader, "y"));
                                    double w = double.Parse(getPropertyValue(reader, "width"));
                                    double h = double.Parse(getPropertyValue(reader, "height"));
                                    reader.MoveToElement();



                                    //点类对象，此处示例代码假定点对象使用了图元
                                    symbol = symbol.Replace("#", "");
                                    double aspect = uc.objManager.zSymbols[symbol].sizeX / uc.objManager.zSymbols[symbol].sizeY;  //从图元取宽高比

                                    pSymbolObject obj = new pSymbolObject(player)
                                    {
                                        id = id,
                                        name = id,
                                        planeLocation = (new Point(x + w / 2, y + h / 2)).ToString(),  //注：应赋值到planeLocation(平面坐标用)，而不是location(经纬度坐标用)，另外，校验位置到中心点
                                        symbolid = symbol, 
                                        scaleX = 0.005f,  //  0.005为从svg平面空间到3D空间的缩放系数，可自行调整大小
                                        scaleY = (float)(0.005 * aspect),
                                        color=Colors.LightBlue,
                                        isH=true
                                    };
                                    player.AddObject(id, obj); 





                                }
                                else if (reader.Name == "path" && reader.NodeType == XmlNodeType.Element)
                                {
                                    shapetype = "path";
                                    id = getPropertyValue(reader, "id");
                                    fill = getPropertyValue(reader, "fill");
                                    stroke = getPropertyValue(reader, "stroke");
                                    width = getPropertyValue(reader, "stroke-width");
                                    data = getPropertyValue(reader, "d");
                                    zclass = getPropertyValue(reader, "class");
                                    reader.MoveToElement();

                                    //线对象
                                    if (!string.IsNullOrWhiteSpace(id))
                                    {
                                        pPowerLine obj = new pPowerLine(player)
                                        {
                                            id = id,
                                            name = id,
                                            color = Color.FromRgb(0xFF, 0x66, 0x00),
                                            isFlow = true, //是否显示潮流
                                            thickness = 0.002f, //线宽
                                            arrowSize = 0.005f  //潮流箭头大小
                                        };
                                        // 处理path短写，生成点集，对更复杂的短写，需借助path对象生成点集
                                        string tmp = data;
                                        tmp = tmp.Replace("M", "");
                                        string[] ps = tmp.Split('L');
                                        string points = "";
                                        int idx = 0;
                                        foreach (string s in ps)
                                        {
                                            if (idx != 0)
                                                points += " ";
                                            Point pt = Point.Parse(s);
                                            points += pt.ToString();
                                            idx++;
                                        }

                                        //zh注：对不合理的过近的相邻点进行处理，必须进行此步检查，否则在3D空间不能正常生成3D线条
                                        PointCollection pc = PointCollection.Parse(points);
                                        PointCollection newpc = new PointCollection();
                                        newpc.Add(pc[0]);
                                        for (int i = 1; i < pc.Count; i++)
                                        {
                                            if ((pc[i] - pc[i - 1]).Length > 20)
                                            {
                                                if (i < pc.Count - 1)
                                                {
                                                    Vector v1 = pc[i] - pc[i - 1]; v1.Normalize();
                                                    Vector v2 = pc[i + 1] - pc[i]; v2.Normalize();
                                                    if (v1 != v2)
                                                        newpc.Add(pc[i]);
                                                }
                                                else
                                                    newpc.Add(pc[i]);
                                            }
                                        }
                                        (obj as pPowerLine).strPoints = newpc.ToString();

                                        obj.planeStrPoints = newpc.ToString(); // //注：应赋值到planeStrPoints，而不是strPoints
                                        if (newpc.Count > 1)
                                            player.pModels.Add(id, obj);


                                        //处理潮流，在本demo中，ConnectNode_Layer层中的线不显示潮流箭头
                                        if (player.id == "ConnectNode_Layer")
                                            obj.isFlow = false;



                                    }

                                }
                                else if (reader.Name == "text" && reader.NodeType == XmlNodeType.Element)
                                {
                                    stroke = getPropertyValue(reader, "stroke");
                                    double x = double.Parse(getPropertyValue(reader, "x"));
                                    double y = double.Parse(getPropertyValue(reader, "y"));
                                    string fontfamily = getPropertyValue(reader, "font-family");
                                    string fontsize = getPropertyValue(reader, "font-size");
                                    zclass = getPropertyValue(reader, "class");
                                    reader.Read();
                                    string text = reader.Value;

                                    reader.MoveToElement();
                                    if (text== "线")
                                    {
                                    }

                                    //文字对象
                                    double w = 40;  // w 和 h用来修正位置
                                    double h = -10;
                                    float scalexy = float.Parse(fontsize) / 16f*0.8f;
                                    if (player.id=="Text_Layer")
                                    {
                                        pText obj = new pText(player)
                                        {
                                            id = Helpler.getGUID(), //生成guid串，确保key值唯一
                                            name = text,
                                            text=text,
                                            planeLocation = (new Point(x + w / 2, y + h / 2)).ToString(),  //注：应赋值到planeLocation(平面坐标用)，而不是location(经纬度坐标用)，另外，校验位置到中心点
                                            isH=true, //是否水平放置
                                            color=Colors.White,
                                            scaleX=scalexy,
                                            scaleY=scalexy,
                                        };
                                        player.AddObject(text, obj);
                                    }

                                }
                                else if (reader.Name == "rect" && reader.NodeType == XmlNodeType.Element)
                                {
                                    fill = getPropertyValue(reader, "fill");
                                    stroke = getPropertyValue(reader, "stroke");
                                    double x = double.Parse(getPropertyValue(reader, "x"));
                                    double y = double.Parse(getPropertyValue(reader, "y"));
                                    double w = double.Parse(getPropertyValue(reader, "width"));
                                    double h = double.Parse(getPropertyValue(reader, "height"));

                                    reader.Read();

                                    reader.MoveToElement();

                                    //厂站对象
                                    if (player.id == "Other_Layer")
                                    {
                                        pSymbolObject obj = new pSymbolObject(player)
                                        {
                                            id = Helpler.getGUID(), //生成guid串，确保key值唯一
                                            planeLocation = (new Point(x + w / 2, y + h / 2)).ToString(),  //注：应赋值到planeLocation(平面坐标用)，而不是location(经纬度坐标用)，另外，校验位置到中心点
                                            isH = true,
                                            brush=Brushes.White,
                                            color=Colors.Red,
                                            scaleX =(float)(w* 0.0005), //0.0005为映射到3D空间的尺寸调整系数
                                            scaleY =(float)(h * 0.0005)
                                        };
                                        player.AddObject(obj.id, obj);
                                    }

                                }
                            }

                            break;

                    }
                }
                
            }

            //==============清理空白层
            int idxi = 0;
            while (idxi < uc.objManager.zLayers.Count)
            {
                if (uc.objManager.zLayers.Values.ElementAt(idxi).pModels.Count == 0)
                {
                    uc.objManager.zLayers.Remove(uc.objManager.zLayers.Keys.ElementAt(idxi));
                }
                else
                    idxi++;
            }

        }

        string getPropertyValue(XmlReader reader, string PropertyName)
        {
            if (reader.MoveToAttribute(PropertyName))
                return reader.Value;
            else
                return "";
        }

    }
}
