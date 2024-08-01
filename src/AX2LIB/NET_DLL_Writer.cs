using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AX2LIB
{
    /// <summary>
    /// 
    /// </summary>
    public class NET_DLL_Writer
    {
        private const string CoderName = "GeorgGrebenyuk";

        /// <summary>
        /// Внутренний метод для генерации процедуры нода
        /// </summary>
        /// <param name="inner_commands"></param>
        /// <returns></returns>
        private string Get_NodeResult_Execute(string[] inner_commands)
        {
            string start_instructions = 
                $"\t\t[NullableContext(1)]\n" +
                $"\t\tpublic NodeResult Execute(INVPData context, List<NodeResult> inputs)\n" +
                "{";
            foreach (string command in inner_commands)
            {
                start_instructions += $"\t\t\t{command}\n";
            }

            start_instructions += "\t\t}\n";
            return start_instructions;
        }

        private NET_DLL_PROTOTYPE proptotype;
        /// <summary>
        /// 
        /// </summary>
        private string save_path;
        private string tab = $"\t";
        private string line_sep = $"\r\n";
        private string LibName => proptotype.LIBRARY_INFO.Name;
        private string RootNsName => "Dyn" + LibName;
        public NET_DLL_Writer(NET_DLL_PROTOTYPE proptotype, string save_path)
        {
            this.proptotype = proptotype;
            if (!Directory.Exists(save_path)) throw new DirectoryNotFoundException("Can not save to that Directory");
            this.save_path = save_path;
        }
        public void Create()
        {
            save_path = Path.Combine(save_path, proptotype.LIBRARY_INFO.Name);
            //this.LibName = "Dyn" + proptotype.LIBRARY_INFO.Name;
            //first, let's create a new folder in save_path named "root namepsace of Library"
            XDocument doc_nodeitem = new XDocument();
            XElement doc_nodeitem_Nodes = new XElement("ArrayOfNodeInfo");
            string nodeitem_save_path = Path.Combine(save_path, proptotype.LIBRARY_INFO.Name + ".nodeitem");

            
            StringBuilder not_impl = new StringBuilder();
            string not_impl_save_path = Path.Combine(save_path, "not_implemented_info.txt");
            
            if (!Directory.Exists(save_path)) Directory.CreateDirectory(save_path);
            else
            {
                Directory.Delete(save_path, true);
                Directory.CreateDirectory(save_path);
            }

            // List of interfaces name which are inherited by other interfaces (in future, create public dynamic constructor in it classes)
            List<string> inherits_info = new List<string>();
            foreach (CLASS_PROTOTYPE class_wrapper in proptotype.CLASSES)
            {
                inherits_info.Concat(class_wrapper.Inherits);
            }
            inherits_info = inherits_info.Distinct().ToList();

            //List of all enum
            List<string> enums = new List<string>();
            foreach (CLASS_PROTOTYPE class_wrapper in proptotype.Enumerations)
            {
                string enum_name = class_wrapper.Name.TrimStart();
                if (enum_name.Length > 3) enums.Add(enum_name);

            }
            enums = enums.Distinct().ToList();

            foreach (CLASS_PROTOTYPE class_wrapper in proptotype.CLASSES)
            {
                StringBuilder cs_content = new StringBuilder();
                string class_name = class_wrapper.Name;
                if (class_name[0] == 'I') class_name = class_name.Substring(1);

                //add NVP namespace
                cs_content.AppendLine("using NVP.API.Nodes;");
                /*block of using nsmespaces*/

                //add namespace 

                cs_content.AppendLine($"namespace {RootNsName} \r\n" + "{");
                //add class
                cs_content.AppendLine(GetComment(class_wrapper.Description, true));
                cs_content.AppendLine($"{tab}public abstract class {class_name} \r\n" + tab + "{");
                //add original interface
                cs_content.AppendLine($"{tab}{tab}public {LibName}.{class_wrapper.Name} _i;");
                //add default internal constructor
                cs_content.AppendLine(
                    $"{tab}{tab}internal {class_name}(object {class_name}_object) " + line_sep +
                    $"{tab}{tab}" + "{" + line_sep +
                    $"{tab}{tab}{tab}" + $"this._i = {class_name}_object as {LibName}.{class_wrapper.Name};" + line_sep +
                    $"{tab}{tab}{tab}" + $"if (this._i == null) throw new System.Exception(\"Invalid casting to {class_wrapper.Name}\");" + line_sep +
                    $"{tab}{tab}" + "}");
                if (inherits_info.Contains(class_wrapper.Name))
                {
                    //add public dynamic constructor 
                    string dyn_constructor = Get_NodeResult_Execute(new string[]
                    {
                        $"this._i = {class_name}_object_to_cast._i as {LibName}.{class_wrapper.Name};",
                        "if (this._i == null) throw new System.Exception(\"Invalid casting\");",
                        "return this._i;"
                    });
                    cs_content.AppendLine(
                    $"{tab}{tab}[NodeInput(\"dynamic\", typeof(dynamic))]" + line_sep +
                    $"{tab}{tab}public class {class_name}_C : {class_name}, INode " + line_sep +
                    $"{tab}{tab}" + "{" + line_sep + dyn_constructor + line_sep +
                    $"{tab}{tab}" + "}");
                }
                //add other class content
                foreach (COMPONENT_PROTOTYPE class_element in class_wrapper.Members)
                {
                    if (class_element.Description!= null && class_element.Description.Contains("Not implemented")) 
                    { not_impl.AppendLine(class_wrapper.Name + "." + class_element.Name); }
                    //Exception of specific element names
                    if (class_element.Name[0] != '_')
                    {
                        string arguments_string = "";
                        string arguments_names_string = "";
                        List<string> arguments = new List<string>();
                        List<string> arguments_names = new List<string>();
                        List<string> NVP_args_attributes = new List<string>();
                        NVP_args_attributes.Add("[NodeInput(\"AcadDocument\", typeof(dynamic))]");
                        bool is_opt = false;
                        for (int i = 0; i < class_element.ArgumentsNames.Length; i++)
                        {
                            string arg_name = class_element.ArgumentsNames[i];
                            var arg_type_raw = class_element.ArgumentsTypes[i];
                            
                            string arg_type_source = class_element.ArgumentTypes_Source[i];
                            string arg_type = null;
                            foreach (string enum_def in enums)
                            {
                                if (arg_type_source.Contains(enum_def)) 
                                {
                                    arg_type = $"{LibName}.{enum_def}";
                                    break;
                                }
                            }
                            if (arg_type == null) arg_type = arg_type_raw.ToString().ToLower();

                            bool is_optional = class_element.OptionalArguments[i];
                            if (!is_opt) is_optional = true;
                            bool is_out = class_element.IsOutFlags[i];
                            string out_info = "";
                            if (is_out) out_info = "out ";

                            Type arg_type_Type = typeof(object);
                            if (arg_type == "int") arg_type_Type = typeof(int);
                            else if (arg_type == "double") arg_type_Type = typeof(double);
                            else if (arg_type == "string") arg_type_Type = typeof(string);

                            NVP_args_attributes.Add($"[NodeInput(\"{arg_name}\", typeof({arg_type_Type}))]");

                            arguments.Add(out_info + arg_type + " " + arg_name);
                            arguments_names.Add(out_info + arg_name);
                        }
                        arguments_string = string.Join(",", arguments);
                        arguments_names_string = string.Join(",", arguments_names);

                        string content_type = "NodeViewType.Default";
                        string element_instructions = "";
                        string element_name = class_element.Name;

                        //|| 
                        if (class_element.TYPE == NET_DLL_PROTOTYPE.NET_TYPE.TYPE_METHOD_VOID)
                        {
                            //content_type = "NodeViewType.Modifier";
                            element_instructions = $"inputs[0].Value._i.{class_element.Name}({arguments_names_string});";
                        }
                        else if (class_element.TYPE == NET_DLL_PROTOTYPE.NET_TYPE.TYPE_METHOD_SET)
                        {
                            element_name = "Set_" + element_name;
                            content_type = "NodeViewType.Modifier";
                            element_instructions = $"inputs[0].Value._i.{class_element.Name} = {arguments_names_string};";
                        }
                        else if (class_element.TYPE == NET_DLL_PROTOTYPE.NET_TYPE.TYPE_METHOD_GET)
                        {
                            //element_name = element_name;
                            content_type = "NodeViewType.Data";
                            string arguments_names_string2 = $"({arguments_names_string})";
                            if (arguments_names_string.Length < 2) arguments_names_string2 = "";
                            element_instructions = $"return inputs[0].Value._i.{class_element.Name}{arguments_names_string2};";
                        }
                        else if (class_element.TYPE == NET_DLL_PROTOTYPE.NET_TYPE.TYPE_FIELD)
                        {
                            //content_type = "public void";
                            element_name = "Put_" + element_name;
                            element_instructions = $"inputs[0].Value._i.{class_element.Name} = {arguments_names_string};";
                        }
                        else if (class_element.TYPE == NET_DLL_PROTOTYPE.NET_TYPE.TYPE_METHOD_PRIVATE_VOID)
                        {
                            //content_type = "private void";
                            element_name = "HiddenField_" + element_name;
                            //element_instructions = $"this._i.{class_element.Name}({arguments_names_string});";
                        }
                        else throw new Exception($"Invalid type of element of class {class_element.TYPE.ToString()}");

                        if (class_element.Name == "Item")
                        {
                            //replace () to [] when Method's name is 'Item' (IEnumerable)
                            //element_instructions = element_instructions.Replace(")", "]").Replace(".Item(", "[");
                        }

                        cs_content.AppendLine(GetComment(class_element.Description, false));
                        foreach (string ang_Attribute in NVP_args_attributes)
                        {
                            cs_content.AppendLine($"{tab}{tab}{ang_Attribute}" + line_sep);
                        }

                        //add get-propperty as =>
                        string opt_comment = "";
                        if (is_opt) opt_comment = " //optional_argument" + line_sep;

                        cs_content.AppendLine(opt_comment);
                        string exec = Get_NodeResult_Execute(new string[] { element_instructions });

                        cs_content.AppendLine(
                            $"{tab}{tab}public class {element_name} : AcadDocument, INode" + line_sep +
                            $"{tab}{tab}" + "{" + line_sep +
                            $"{tab}{tab}{tab}" + $"{exec}" + line_sep +
                            $"{tab}{tab}" + "}");

                    }

                }


                //close class
                cs_content.AppendLine($"{tab}" + "}");
                //close namespace
                cs_content.AppendLine("}");
                File.WriteAllText(Path.Combine(save_path, $"{class_name}.cs"), cs_content.ToString(), Encoding.UTF8);
            }

            //actions with created files
            foreach (string cs_path in Directory.GetFiles(save_path, "*.cs", SearchOption.TopDirectoryOnly))
            {
                string file_name = Path.GetFileName(cs_path);
                if (file_name == "interface.cs" ||
                    file_name.Contains("[") || file_name.Contains("]") || file_name.Contains(";") ||
                    file_name.Contains("(") || file_name.Contains(")") || file_name.EndsWith("Ex.cs")) File.Delete(cs_path);
            }

            File.WriteAllText(not_impl_save_path, not_impl.ToString(), Encoding.UTF8);

            doc_nodeitem.Add(doc_nodeitem_Nodes);
            doc_nodeitem.Save(nodeitem_save_path);
        }
        private string GetComment(string helpstring, bool is_class)
        {
            string tabs;
            if (is_class) tabs = tab;
            else tabs = tab + tab;
            return line_sep + $"{tabs}///<summary>" + line_sep +
                $"{tabs}///{helpstring}" + line_sep + 
                $"{tabs}///</summary>";
        }

    }
}
