using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AX2LIB
{
    /// <summary>
    /// Auxiliary class for create a *.nodeitem file for description the nodes
    /// </summary>
    internal class NVP_XML
    {
        private string _PathAssembly;
        private string _CoderName;
        private const string CoderName = "GeorgGrebenyuk";

        public enum NodeViewType
        {
            Default,
            Data,
            ThreeD,
            Modifier
        }

        private XDocument _doc;
        private XElement _doc_nodeitem_Nodes;
        public NVP_XML(string assemblyName, string coderName)
        {
            _PathAssembly = assemblyName;
            _CoderName = coderName;

            _doc = new XDocument();
            _doc_nodeitem_Nodes = new XElement("ArrayOfNodeInfo");
        }
        public void Add(string PathExecuteClass, string Folder, string NodeName, bool NodeType, NodeViewType ViewType)
        {
            string NodeType2 = "True";
            if (NodeType == false) NodeType2 = "False";

            XElement el_NodeInfo = new XElement("NodeInfo");
            el_NodeInfo.Add(new XElement("Id",                  Guid.NewGuid().ToString("D").ToUpper()));
            el_NodeInfo.Add(new XElement("PathAssembly",        _PathAssembly));
            el_NodeInfo.Add(new XElement("PathExecuteClass",    PathExecuteClass));
            el_NodeInfo.Add(new XElement("CoderName",           _CoderName));
            el_NodeInfo.Add(new XElement("Folder",              Folder));
            el_NodeInfo.Add(new XElement("NodeName",            NodeName));
            el_NodeInfo.Add(new XElement("NodeType",            NodeType2));
            el_NodeInfo.Add(new XElement("CADType",             "None"));
            el_NodeInfo.Add(new XElement("ViewType",            ViewType.ToString()));

            _doc_nodeitem_Nodes.Add(el_NodeInfo);
        }
        public void Save(string savePath)
        {
            _doc.Add(_doc_nodeitem_Nodes);
            this._doc.Save(savePath);
        }



    }
}
