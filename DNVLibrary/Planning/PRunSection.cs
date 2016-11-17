using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DNVLibrary.Planning
{
    class PRunSection : AppBase
    {
        public PRunSection(UCDNV863 Root)
            : base(Root)
        {
         
        }

        internal override void load()
        {
            if (panel != null) (panel as BaseIPanel).load();
            root.earth.camera.adjustCameraAngle(0);


                //if (parent.grdSection.Children.Count == 0)
            root.grdSection.Children.Add(new SectionFlowDiagram(root, ".\\xml\\sectiondefp.xml") { });

            root.grdSection.Visibility = Visibility.Visible;
            root.grdInfo.Visibility = Visibility.Collapsed;

        }

        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();

            root.grdSection.Children.Clear();
            root.grdSection.Visibility = Visibility.Collapsed;
            root.grdInfo.Visibility = Visibility.Visible;
        }

    }
}
