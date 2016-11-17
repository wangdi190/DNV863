using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfEarthLibrary
{
    public class Config
    {
        public Config(Earth pearth)
        {
            earth = pearth;
        }
        Earth earth;

        ///<summary>地图IP瓦片地址, 如http://localhost:8080/img.aspx</summary>
        public static string MapIP = "http://localhost:8080/img.aspx";
        ///<summary>地图IP瓦片路径</summary>
        public static string MapPath = "E:\\map\\googlemaps\\satellite\\";
        ///<summary>地图IP瓦片路径</summary>
        public static string MapPath2 = "E:\\map\\googlemaps\\overlay\\";
        ///<summary>卫星瓦片最大层</summary>
        public static int satmaxLayer = 18; //扫描最大层数
        ///<summary>道路瓦片最大层</summary>
        public static int roadmaxLayer = 15;


        ///<summary>是否编辑模式</summary>
        public bool isEditMode { get; set; }

        #region ----- tooltips和pick相关 ------
        ///<summary>是否激活mouse move形态的tooltip显示</summary>
        public bool tooltipMoveEnable { get; set; }

        private int _tooltipMoveDelay = 200;
        ///<summary>move move形态时，延时多少毫秒开始tooltip的对象拾取和tooltip显示，缺省200毫秒</summary>
        public int tooltipMoveDelay
        {
            get { return _tooltipMoveDelay; }
            set { _tooltipMoveDelay = value; earth.tooltiptimer.Interval = TimeSpan.FromMilliseconds(tooltipMoveDelay); }
        }


        ///<summary>是否激活mouse click形态的tooltip显示（鼠标左键）</summary>
        public bool tooltipClickEnable { get; set; }
        ///<summary>是否激活mouse right click形态的tooltip显示（鼠标右键）</summary>
        public bool tooltipRightClickEnable { get; set; }
        ///<summary>是否激活拾取对象（鼠标左键）</summary>
        public bool pickEnable { get; set; }


        private bool _doubleClickPickEnable;
        ///<summary>是否激活双击拾取对象，注：将改写主窗体双击事件</summary>
        public bool doubleClickPickEnable
        {
            get { return _doubleClickPickEnable; }
            set
            {
                _doubleClickPickEnable = value;
                earth.setDoubleClick(value);
            }
        }

        #endregion

        
        private bool _enableMinimap;
        ///<summary>导航地图是否生效, 用于代码控制小地图是否生效，与小地图自身的可见性配置共同决定小地图是否可见</summary>
        public bool enableMinimap
        {
            get { return _enableMinimap; }
            set
            {
                _enableMinimap = value; 
                if (value)
                    earth.showMinimap();
                else
                    earth.hideMinimap();
            }
        }
      


        private bool _enableToolbox = true;
        ///<summary>设置内置工具栏是否可见</summary>
        public bool enableToolbox
        {
            get { return _enableToolbox; }
            set { _enableToolbox = value; earth.grdtoolbox.Visibility = (value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed); }
        }

        private bool _enableSat = true;
        ///<summary>设置内置工具栏是否卫星地图可见</summary>
        public bool enableSat
        {
            get { return _enableSat; }
            set { _enableSat = value; earth.btnSat.Visibility = (value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed); }
        }

        private bool _enableRoad = true;
        ///<summary>设置内置工具栏是否道路地图可见</summary>
        public bool enableRoad
        {
            get { return _enableRoad; }
            set { _enableRoad = value; earth.btnRoad.Visibility = (value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed); }
        }

        private bool _enableTerrain = true;
        ///<summary>设置内置工具栏是否地形地图可见</summary>
        public bool enableTerrain
        {
            get { return _enableTerrain; }
            set { _enableTerrain = value; earth.btnTerrain.Visibility = (value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed); }
        }

        
        private bool _enableLightSetPanel;
        ///<summary>设置内置工具栏是否显示光源设置按钮</summary>
        public bool enableLightSetPanel
        {
            get { return _enableLightSetPanel; }
            set { _enableLightSetPanel = value; earth.btnLightSet.Visibility = (value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed); }
        }
      


        #region 动态显示相关

        private bool _isDynShow;
        ///<summary>是否动态显示对象，缺省false</summary>
        public bool isDynShow
        {
            get { return _isDynShow; }
            set { _isDynShow = value; }
        }

        private bool _isDynLoad;
        ///<summary>是否动态加载对象，缺省false</summary>
        public bool isDynLoad
        {
            get { return _isDynLoad; }
            set { _isDynLoad = value; }
        }

        #endregion


        private bool _isShowDebugInfo;
        ///<summary>是否显示调试信息</summary>
        public bool isShowDebugInfo
        {
            get { return _isShowDebugInfo; }
            set
            {
                _isShowDebugInfo = value;
                //earth.txtDebug.Visibility = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }



        private bool _isShowCoordinate = true;
        ///<summary>是否在状态栏中显示坐标</summary>
        public bool isShowCoordinate
        {
            get { return _isShowCoordinate; }
            set { _isShowCoordinate = value; }
        }


     
    }
}
