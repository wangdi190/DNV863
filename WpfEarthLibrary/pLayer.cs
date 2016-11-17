using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfEarthLibrary
{
    public class pLayer
    {
        public pLayer(pObjectManager Parent)
        {
            parent = Parent;
        }

        public pObjectManager parent;
        public string id;
        public string name;
        //public Dictionary<string, pLayer> subLayers = new Dictionary<string, pLayer>();  //zhh注：嵌套层暂不实现

        public Dictionary<string, PowerBasicObject> pModels = new Dictionary<string, PowerBasicObject>();

        ///<summary>可视最大距离，缺省100，当大于相机远端距离时，全可见。一般不用设置，当对象大于5万时，应针对每层进行设置以启用层的范围可见性检查提高性能</summary>
        public double visualMaxDistance = 100;
        ///<summary>可视最小距离，缺省0。一般不用设置，当对象大于5万时，应针对每层进行设置以启用层的范围可见性检查提高性能</summary>
        public double visualMinDistance = 0;


        ///<summary>此层是否显示，由logicVisibility与rangeVisibility共同决定</summary>
        public bool isShow
        {
            get { return logicVisibility && (rangeVisibility || !parent.earth.config.isDynShow); }
        }


        private bool _logicVisibility = true;
        ///<summary>业务逻辑可见性，实际呈现的必要条件之一，缺省true</summary>
        public bool logicVisibility
        {
            get { return _logicVisibility; }
            set { _logicVisibility = value; }
        }

        private bool _rangeVisibility = true;
        ///<summary>范围可见性，动态显示模式下实际呈现的必要条件之一，缺省true</summary>
        public bool rangeVisibility
        {
            get { return _rangeVisibility; }
            set { _rangeVisibility = value; }
        }


        private int _deepOrder;
        ///<summary>深度顺序，已无效</summary>
        public int deepOrder
        {
            get { return _deepOrder; }
            set { _deepOrder = value; }
        }

        ///<summary>向层添加对象，以obj.id为键值, 若已存在相同ID则忽略添加，返回false</summary>
        public bool AddObject(PowerBasicObject obj)
        {
            if (!pModels.Keys.Contains(obj.id))
            {
                pModels.Add(obj.id, obj);
                return true;
            }
            else
                return false;
        }

        ///<summary>向层添加对象，以key为键值, 若已存在相同ID则忽略添加，返回false</summary>
        public bool AddObject(string key, PowerBasicObject obj)
        {
            if (!pModels.Keys.Contains(key))
            {
                pModels.Add(key, obj);
                return true;
            }
            else
                return false;
        }

        ///<summary>检查层的范围可见性</summary>
        internal void checkVisualization()
        {
            float curCameraDistance = parent.earth.camera.curCameraDistanceToGround;
            rangeVisibility = (curCameraDistance > visualMinDistance && curCameraDistance < visualMaxDistance);
        }
    }
}
