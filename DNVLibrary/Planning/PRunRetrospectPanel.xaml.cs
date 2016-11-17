using System;
using System.Data;
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

namespace DNVLibrary.Planning
{
    /// <summary>
    /// PRetrospectPanel.xaml 的交互逻辑
    /// </summary>
    public partial class PRunRetrospectPanel : UserControl, BaseIPanel
    {
        public PRunRetrospectPanel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
        }
        Random rd = new Random();

        UCDNV863 root;
        public void load()
        {


            Run.DataGenerator.initRunData(root.distnet); //需要开关运行数据，借用实时模拟
            Run.DataGenerator.StartGenData(root.distnet);

            lstStation.MouseDoubleClick += new MouseButtonEventHandler(lstStation_MouseDoubleClick);

            //读取开关站列表
       
            lstStation.ItemsSource =root.distnet.getAllObjListByObjType( DistNetLibrary.EObjectType.配电室) ;//root.earth.objManager.getObjList("开关站").Where(p=>(p as pSymbolObject).relationID.Count>0);



            //补充台账信息
            //foreach (PowerBasicObject obj in root.earth.objManager.getObjList("开关站"))
            //{
            //    //重要负荷
            //    (obj.busiAccount as AccountSwicth).cusomers = new System.ComponentModel.BindingList<AccountCustomer>(); 
            //    for (int i = 0; i < rd.Next(3); i++)
            //    {
            //        (obj.busiAccount as AccountSwicth).cusomers.Add(new AccountCustomer() { name = "客户" + i, vl = 380, ot = "其它业务数据..." });
            //    }
            //    //电源追溯
            //    (obj.busiAccount as AccountSwicth).Stations = new System.ComponentModel.BindingList<AccountStation>();
            //    pPowerLine line;
            //    string lineotherid;
            //    foreach (string rid in (obj as pSymbolObject).relationID)
            //    {
            //        line =(pPowerLine)root.earth.objManager.find(rid);
            //        lineotherid = line.fromID == obj.id ? line.toID : line.fromID;
            //        PowerBasicObject sobj = root.earth.objManager.find(lineotherid);
            //        if (sobj.busiData.busiSort == "变电站")
            //            (obj.busiAccount as AccountSwicth).Stations.Add((AccountStation)sobj.busiAccount);
            //        else //注：续接待定
            //        {}
                    
            //    }
            //}





        }

        List<pPowerLine> anilines = new List<pPowerLine>();
        pSymbolObject selobj;
        pData additionObj;
        void lstStation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            root.distnet.clearTraceSupplyRange();
            DistNetLibrary.DNSwitchHouse pds = lstStation.SelectedItem as DistNetLibrary.DNSwitchHouse;
            root.distnet.showSourceTrace(pds);
            propObj.SelectedObject = pds.busiAccount;
            root.earth.camera.aniLook(pds.VecLocation);

            ////恢复之前
            //foreach (pPowerLine lin in anilines)
            //{
            //    lin.lineColor = Colors.Cyan;
            //    lin.thickness = 0.002f;
            //    lin.isShowLabel = false;
            //}
            //anilines.Clear();
            //if (selobj != null) selobj.submodels.Clear();
            ////设置当前
            //selobj = lstStation.SelectedItem as pSymbolObject;

            //selobj.LabelColor = Colors.Yellow;
            //selobj.LabelSizeX = selobj.LabelSizeY = 2.2f;
            //selobj.Label = selobj.name;
            //selobj.isShowLabel = true;
            


            //propObj.SelectedObject = selobj.busiAccount;
            //root.earth.camera.aniLook(selobj.VecLocation);

            //foreach (string lid in selobj.relationID)
            //{
            //    pPowerLine lin = (pPowerLine)root.earth.objManager.find(lid);
            //    anilines.Add(lin);
            //    lin.lineColor=Colors.Red;
            //    lin.thickness=0.008f;
            //    lin.AnimationBegin(pPowerLine.EAnimationType.绘制);
                
            //    lin.LabelColor = Colors.Yellow;
            //    lin.LabelSizeX=lin.LabelSizeY = 1.8f;
            //    lin.Label = lin.name;
            //    //lin.LabelPanelKey = "文字面板";
            //    lin.isShowLabel = true;//初始化时放最后(暂时)

            //}

            //additionObj = new pData(selobj.parent) { id = selobj.id + "追溯", location=selobj.location, valueScale=0.1f, radScale=0.025f };
            //additionObj.datas.Add(new Data() { id = selobj.id + "数据", value = 1, argu = selobj.name, color = Color.FromArgb(0xC3, 0x00, 0x00, 0xFF), geokey = "倒锥体" });
            //selobj.AddSubObject("sf",additionObj);
            //additionObj.aniRotation.isDoAni = true;
            //root.earth.UpdateModel();


        }

        public void unload()
        {
            lstStation.MouseDoubleClick -= new MouseButtonEventHandler(lstStation_MouseDoubleClick);
            Run.DataGenerator.StopGenData();
            Run.DataGenerator.clearAll();
            
            ////恢复外观
            //foreach (pPowerLine lin in anilines)
            //{
            //    lin.lineColor = Colors.Cyan;
            //    lin.thickness = 0.002f;
            //    lin.isShowLabel = false;
            //}
            //anilines.Clear();

            ////移除对象
            //if (selobj != null) selobj.submodels.Clear();
            //root.earth.UpdateModel();

            root.distnet.clearTraceSupplyRange();
        }
    }
}
