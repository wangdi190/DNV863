using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Run
{
    class RHistoryStat : AppBase
    {
        public RHistoryStat(UCDNV863 Root)
            : base(Root)
        {
        }


        internal override void load()
        {
            if (panel != null) (panel as BaseIPanel).load();
            root.earth.camera.adjustCameraAngle(50);
        }




        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();
        }

    }

}
