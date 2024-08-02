using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace AX2LIB
{

    [Serializable]
    public class NodeInfo
    {
        public string Id { get; set; }
        public string PathAssembly { get; set; }
        public string PathExecuteClass { get; set; }
        public string CoderName { get; set; }
        public string Folder { get; set; }
        public string NodeName { get; set; }
        public string NodeType { get; set; }
        public string CADType { get; set; }
        public string ViewType { get; set; }
    }

    [Serializable()]
    [XmlRoot("ArrayOfNodeInfo")]
    public class NVP_XML_File
    {
        public const string xml_raw = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfNodeInfo xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"/>";

        public List<NodeInfo> ArrayOfNodeInfo { get; set; }
        public NVP_XML_File() { ArrayOfNodeInfo = new List<NodeInfo>(); }

        public void Save(string SavePath)
        {

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(NVP_XML_File));

            using (FileStream fs = new FileStream(SavePath, FileMode.OpenOrCreate))
            {
                xmlSerializer.Serialize(fs, this);
            }

        }
    }
    /// <summary>
    /// Auxiliary class for create a *.nodeitem file for description the nodes
    /// </summary>
    public class NVP_XML
    {
        private string _PathAssembly;
        private string _CoderName;
        private string _SavePath;

        private const string _icon_method = "❓";
        private const string _icon_field = "❕";
        private const string _icon_constructor = "➕";

        public enum NodeViewType
        {
            Default,
            Data,
            ThreeD,
            Modifier
        }

        //private XDocument _doc;
        //private XElement _doc_nodeitem_Nodes;
        private NVP_XML_File _doc;

        private NVP_XML_GuidsMap _guids_map;
        private string _SavePath_GuidsMap;
        public NVP_XML() { }
        public NVP_XML(string assemblyName, string coderName, string savePath)
        {
            _PathAssembly = assemblyName;
            _CoderName = coderName;
            _SavePath = savePath;

            _doc = new NVP_XML_File();
            //ArrayOfNodeInfo = new List<NVP_XML_NodeInfo>();
            //_doc = new XDocument();
            //_doc_nodeitem_Nodes = new XElement("ArrayOfNodeInfo");

            //Идентифицируем guids map
            _SavePath_GuidsMap = savePath.Replace(".nodeitem", "_guids_map.json");
            _guids_map = NVP_XML_GuidsMap.LoadSchema(_SavePath_GuidsMap);
        }
        public void Add(string PathExecuteClass, string Folder, string NodeName, bool NodeType, NodeViewType ViewType)
        {
            NodeInfo info = new NodeInfo();
            string NodeType2 = "True";
            if (NodeType == false) NodeType2 = "False";

            string NodeName2 = NodeName;
            //if (ViewType == NodeViewType.Data) NodeName2 = _icon_field + NodeName;
            //else if (ViewType == NodeViewType.Modifier) NodeName2 = _icon_method + NodeName;
            //if (NodeName.Contains("Constructor")) NodeName2 = _icon_constructor + NodeName;

            string id = Guid.NewGuid().ToString("D").ToUpper();
            string compound_name = PathExecuteClass;
            bool is_find = false;
            foreach (var guidData_info in _guids_map.items)
            {
                if (guidData_info.Name == compound_name)
                {
                    is_find = true;
                    id = guidData_info.Id;
                    break;
                }
            }
            if (!is_find)
            {
                NVP_XML_GuidsMap_Item new_item = new NVP_XML_GuidsMap_Item();
                new_item.Name = compound_name;
                new_item.Id = id;
                _guids_map.items.Add(new_item);
            }

            info.Id = id;
            info.PathAssembly = _PathAssembly;
            info.PathExecuteClass = PathExecuteClass;
            info.CoderName = _CoderName;
            info.Folder = Folder;
            info.NodeName = NodeName2;
            info.NodeType = NodeType2;
            info.CADType = "None";
            info.ViewType = ViewType.ToString();


            //XElement el_NodeInfo = new XElement("NodeInfo");
            //el_NodeInfo.Add(new XElement("Id",                  id));
            //el_NodeInfo.Add(new XElement("PathAssembly",        _PathAssembly));
            //el_NodeInfo.Add(new XElement("PathExecuteClass",    PathExecuteClass));
            //el_NodeInfo.Add(new XElement("CoderName",           _CoderName));
            //el_NodeInfo.Add(new XElement("Folder",              Folder));
            //el_NodeInfo.Add(new XElement("NodeName",            NodeName2));
            //el_NodeInfo.Add(new XElement("NodeType",            NodeType2));
            //el_NodeInfo.Add(new XElement("CADType",             "None"));
            //el_NodeInfo.Add(new XElement("ViewType",            ViewType.ToString()));

            _doc.ArrayOfNodeInfo.Add(info);
            //_doc_nodeitem_Nodes.Add(el_NodeInfo);
        }
        public void Save()
        {
            _doc.Save(_SavePath);
            _guids_map.Save(this._SavePath_GuidsMap);
            //this._doc.Save(this._SavePath);
        }



    }
}
