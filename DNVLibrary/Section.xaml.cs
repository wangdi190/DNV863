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
using WpfEarthLibrary;

namespace DNVLibrary
{
    /// <summary>
    /// Section.xaml 的交互逻辑
    /// </summary>
    public partial class SectionFlowDiagram : UserControl
    {
        public SectionFlowDiagram(UCDNV863 Parent)
        {
            parent=Parent;
            earth = parent.earth;
            InitializeComponent();

            parent.grdMap.MouseDown += new MouseButtonEventHandler(map_MouseDown);
            parent.grdMap.MouseMove+=new MouseEventHandler(map_MouseMove);
            parent.grdMap.MouseWheel+=new MouseWheelEventHandler(map_MouseWheel);

            DataGenerator.UpdateSectionData = updateData;

        }

        public SectionFlowDiagram(UCDNV863 Parent, string XmlFile)
        {
            xmlfile = XmlFile;
            parent = Parent;
            earth = parent.earth;
            InitializeComponent();

            parent.grdMap.MouseDown += new MouseButtonEventHandler(map_MouseDown);
            parent.grdMap.MouseMove += new MouseEventHandler(map_MouseMove);
            parent.grdMap.MouseWheel += new MouseWheelEventHandler(map_MouseWheel);

            DataGenerator.UpdateSectionData = updateData;

        }

        ~SectionFlowDiagram()
        {
            DataGenerator.UpdateSectionData = null;
            parent.grdMap.MouseDown -= map_MouseDown;
            parent.grdMap.MouseMove -= map_MouseMove;
            parent.grdMap.MouseWheel -= map_MouseWheel;
            earth = null;
            parent = null;

        }


        public UCDNV863 parent { get; set; }
        WpfEarthLibrary.Earth earth;


        internal string xmlfile = "sectiondef.xml";

        //zhh注：以下应删除和改写
        Random rd = new Random();
        void initdata()
        {
            //zhh注: 以下应初始化数据
            foreach (section sec in viewmodel.sections)
            {
                foreach (line lin in sec.lines)
                {
                    lin.maxValue = earth.objManager.find(lin.id).busiData.busiRatingValue;
                }
            }



        }

        //-------



        ///<summary>潮流显示</summary>
        public void SetPowerFlow() //zhh注，请根据数据来修改
        {


            //if (viewmodel.isShowAllArrow) //全体显示, 显示所有潮流箭头
            //{
            //    foreach (pLayer layer in earth.objManager.zLayers.Values)
            //    {
            //        foreach (PowerBasicObject obj in layer.pModels.Values)
            //        {
            //            if (obj.busiData.busiSort == "线路")
            //            {
            //                (obj as pPowerLine).isFlow = true;
            //            }
            //        }
            //    }
            //}
            //else //仅显示断面线路的潮流箭头
            //{
            //    List<string> allsectionlineid = (from e0 in viewmodel.sections.Where(p => p.isShow)
            //                                    from e1 in e0.lines
            //                                    select new 
            //                                    {
            //                                        id=e1.id
            //                                    }).Select(p=>p.id).ToList();
                                                

            //    foreach (pLayer layer in earth.objManager.zLayers.Values)
            //    {
            //        foreach (PowerBasicObject obj in layer.pModels.Values)
            //        {
            //            if (obj.busiData.busiSort == "线路")
            //            {
            //                (obj as pPowerLine).isFlow = allsectionlineid.Contains(obj.id);
            //            }
            //        }
            //    }
            //}
        }
    



        viewmodel viewmodel;
        private void UserControl_Initialized(object sender, EventArgs e)
        {

            viewmodel = (viewmodel)XmlHelper.readFromXml(xmlfile, typeof(viewmodel));
            viewmodel.setFlow = SetPowerFlow;
            grdMain.DataContext = viewmodel;
            grdMap.Children.Add(viewmodel.grid);
            viewmodel.map = earth;

            initdata();            

            viewmodel.Initialize();
            

            //timer.Interval = TimeSpan.FromSeconds(2);
            //timer.Tick += new EventHandler(timer_Tick);
            //timer.Start();

        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //map.Zoom = 9;
            
            viewmodel.refreshLocation();


            //本项目特定绑定
            //Binding bind = new Binding();
            //bind.Source = parent.paneltransform;
            //bind.Path = new PropertyPath(TranslateTransform.YProperty);
            //bind.Mode = BindingMode.OneWay;
            //BindingOperations.SetBinding(rowdef, RowDefinition.HeightProperty, bind);


            updateData(true);

        }
        

    

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            BindingOperations.ClearBinding(rowdef, RowDefinition.HeightProperty);

            viewmodel.SaveToXml(xmlfile);
        }

        //void timer_Tick(object sender, EventArgs e)
        //{
        //    //zhh注: 以下应读取数据
        //    foreach (section sec in viewmodel.sections)
        //    {
        //        foreach (line lin in sec.lines)
        //        {
        //            lin.curValue = lin.maxValue * rd.NextDouble();
        //        }
        //    }

        //}



        internal void updateData(bool isupdate)
        {
            foreach (section sec in viewmodel.sections)
            {
                foreach (line lin in sec.lines)
                {
                    lin.curValue = rd.NextDouble()*lin.maxValue;//earth.objManager.find(lin.id).busiData.busiCurValue;
                }
            }
        }



        bool isAdding;
        private void btnAddLine_Click(object sender, RoutedEventArgs e)
        {
            isAdding = !isAdding;
            if (!isAdding)
            {
                btnAddLine.Content = "抓取线路";
                btnAddLine.ToolTip="在地图上抓取线路";
            }
            else
            {
                btnAddLine.Content = "停止抓取";
                btnAddLine.ToolTip = "停止抓取线路";
            }
        }


        private void map_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string id=null;
            //抓取线路
            PowerBasicObject pickobj= earth.objManager.pick(e.GetPosition(grdMain));
            VECTOR3D? tmpLocation = earth.earthManager.transformScreenToD3D(e.GetPosition(grdMain));

            if (pickobj != null && pickobj.busiData.busiSort == "线路")
                id = pickobj.id;
            else
                return;

            pPowerLine pline = pickobj as pPowerLine;

            if (isAdding && id != null )
            {
                if (viewmodel.curSection != null)
                {
                    if (viewmodel.curSection.lines.Count(p => p.id == id) == 0)
                    {
                        line nline= new line() { id = id, section=viewmodel.curSection}; 

                        //zhh注：此下应修改为从数据库中读取相应数据

                        //if (tmpLocation!=null)
                        //    nline.d3dLocation =(VECTOR3D)tmpLocation;
                        //else
                            nline.d3dLocation = pline.VecLocation;
                        nline.name = pline.name;
                        nline.maxValue = pline.busiData.busiRatingValue;
                        nline.curValue = pline.busiData.busiCurValue;

                        //---------
                        

                        viewmodel.curSection.lines.Add(nline); 
                        nline.InitVisualControl();

                        if (viewmodel.curSection.isShow)
                        {
                            viewmodel.grid.Children.Add(nline.vLinkLine);
                            viewmodel.grid.Children.Add(nline.vInfo);
                        }
                        viewmodel.curSection.refreshCollection();
                        viewmodel.curSection.refreshLocation();
                        viewmodel.curSection.refreshData();
                    }
                }
            }

        }

        private void btnAddLine_LostFocus(object sender, RoutedEventArgs e)
        {
            isAdding = false;
            btnAddLine.Content = "抓取线路";
            btnAddLine.ToolTip = "在地图上抓取线路";
        }

        private void btnSaveXml_Click(object sender, RoutedEventArgs e)
        {
            viewmodel.SaveToXml(xmlfile);
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton== MouseButtonState.Pressed)
            {
                viewmodel.refreshLocation();
            }
        }

        private void map_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            viewmodel.refreshLocation();

        }


        private void btnRebuild_Click(object sender, RoutedEventArgs e)
        {
            viewmodel.SaveToXml(xmlfile);

            //timer.Stop();
            viewmodel.grid.Children.Clear();
            grdMap.Children.Remove(viewmodel.grid);

            viewmodel = (viewmodel)XmlHelper.readFromXml(xmlfile, typeof(viewmodel));
            viewmodel.setFlow = SetPowerFlow;
            grdMain.DataContext = viewmodel;
            grdMap.Children.Add(viewmodel.grid);
            viewmodel.map = earth;

            initdata();

            viewmodel.Initialize();

            //timer.Start();

        }

     
        
    }
}
