using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace DNVLibrary.Run
{
    public class NodeEntry : INotifyPropertyChanged
    {
        public NodeEntry()
        {
            this.NodeEntrys = new List<NodeEntry>();
            this.ParentID = -1;
            this.Value = "";
            this.Unit = "";
        }
        int id;
        public int ID
        {
            get { return id; }
            set { id = value; this.OnPropertyChanged("ID"); }
        }
        string name;
        public string Name
        {
            get { return name; }
            set { name = value; this.OnPropertyChanged("Name"); }
        }

        public string Value { get; set; }
        public string Unit { get; set; }
        public int ParentID { get; set; }
        List<NodeEntry> nodeEntrys;
        public List<NodeEntry> NodeEntrys
        {
            get { return nodeEntrys; }
            set
            {
                nodeEntrys = value;
                this.OnPropertyChanged("NodeEntrys");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    class TreeViewDataBind
    {
        public static List<NodeEntry> Bind(List<NodeEntry> nodes)
        {
            List<NodeEntry> outputList = new List<NodeEntry>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].ParentID == -1) outputList.Add(nodes[i]);
                else FindDownward(nodes, nodes[i].ParentID).NodeEntrys.Add(nodes[i]);
            }
            return outputList;
        }

        public static NodeEntry FindDownward(List<NodeEntry> nodes, int id)
        {
            if (nodes == null) return null;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].ID == id)
                {
                    return nodes[i];
                }
            }
            return null;
        }  
    }
}
