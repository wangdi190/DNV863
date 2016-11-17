using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Planning
{
    static class _Global
    {
        static _Global()
        {
            InstanceSelector = new Interact.Top() {VerticalAlignment= System.Windows.VerticalAlignment.Bottom};
            InstanceSelector.cmbTree.SelectItemChanged += new EventHandler(cmbTree_SelectItemChanged);

            //InstanceSelector.setCurInstance(1); //设置当前实例，控件不成功
        }

        ///<summary>当前选中实例id</summary>
        public static int curInstanceID;
        
        static void cmbTree_SelectItemChanged(object sender, EventArgs e)
        {
            Interact.Item selInstance = InstanceSelector.cmbTree.SelectedItem as Interact.Item;
            curInstanceID = selInstance.id;
            //改变网架显示
            List<WpfEarthLibrary.pLayer> layers = new List<WpfEarthLibrary.pLayer>() { distnet.scene.objManager.zLayers["规划层"], distnet.scene.objManager.zLayers["网格"] };
            global.showInstanceObject(layers, selInstance.id);



        }

        public static DistNetLibrary.DistNet distnet;
        public static Interact.Top InstanceSelector;

    }
}
