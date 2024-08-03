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
        /// <summary>
        /// Внутренний метод для генерации процедуры нода
        /// </summary>
        /// <param name="inner_commands"></param>
        /// <returns></returns>
        private string Get_NodeResult_Execute(string[] inner_commands)
        {
            string start_instructions =
                //$"\t\t\t[NullableContext(1)]\n" +
                $"\t\tpublic NodeResult Execute(INVPData context, List<NodeResult> inputs)\n" +
                "\t\t{" + line_sep;
            foreach (string command in inner_commands)
            {
                start_instructions += $"\t\t\t{command}\n";
            }

            //start_instructions += $"\t\t\t\treturn new \n";

            start_instructions += "\t\t}\n";
            return start_instructions;
        }

        /// <summary>
        /// Вспомогательный метод для генерации набора атрибутов класса NVP_Manifest (пользовательская реализация атрибутов для генерации файла манифеста библиотеки нодов nodeitem)
        /// </summary>
        /// <returns></returns>
        private string Get_ClassAttributes_NVP_Manifest(NodeInfo info)
        {
            return
                $"{line_sep}{tab}[NVP_Manifest(" +
                $"{line_sep}{tab}{tab}Id = \"{info.Id}\", " +
                $"{line_sep}{tab}{tab}PathAssembly = \"{info.PathAssembly}\", " +
                $"{line_sep}{tab}{tab}PathExecuteClass = \"{info.PathExecuteClass}\", " +
                $"{line_sep}{tab}{tab}CoderName = \"{info.CoderName}\", " +
                $"{line_sep}{tab}{tab}Folder = \"{info.Folder}\", " +
                $"{line_sep}{tab}{tab}NodeName = \"{info.NodeName}\", " +
                $"{line_sep}{tab}{tab}NodeType = \"{info.NodeType}\", " +
                $"{line_sep}{tab}{tab}CADType = \"{info.CADType}\", " +
                $"{line_sep}{tab}{tab}ViewType = \"{info.ViewType}\")]";
        }

        private string _projectName = "";
        private NET_DLL_PROTOTYPE proptotype;
        private string save_path;
        private string tab = $"\t";
        private string line_sep = $"\r\n";
        private string LibName => proptotype.LIBRARY_INFO.Name;
        private string RootNsName => LibName;
        public NET_DLL_Writer(string projectName, NET_DLL_PROTOTYPE proptotype, string save_path)
        {
            _projectName = projectName;
            this.proptotype = proptotype;
            if (!Directory.Exists(save_path)) throw new DirectoryNotFoundException("Can not save to that Directory");
            this.save_path = save_path;

        }
        public void Create()
        {
            save_path = Path.Combine(save_path, proptotype.LIBRARY_INFO.Name);
            //this.LibName = "Dyn" + proptotype.LIBRARY_INFO.Name;
            //first, let's create a new folder in save_path named "root namepsace of Library"
            

            
            StringBuilder not_impl = new StringBuilder();
            string not_impl_save_path = Path.Combine(save_path, "not_implemented_info.txt");
            
            if (!Directory.Exists(save_path)) Directory.CreateDirectory(save_path);
            else
            {
                Directory.Delete(save_path, true);
                Directory.CreateDirectory(save_path);
            }

            //List of all enum
            List<string> enums = new List<string>();
            foreach (CLASS_PROTOTYPE class_wrapper in proptotype.Enumerations)
            {
                string enum_name = class_wrapper.Name.TrimStart();
                if (enum_name.Length > 3) enums.Add(enum_name);
            }
            enums = enums.Distinct().ToList();
            //TODO: create for all enum nodes

            foreach (CLASS_PROTOTYPE class_wrapper in proptotype.CLASSES)
            {
                StringBuilder cs_content = new StringBuilder();
                string class_name = class_wrapper.Name;
                if (class_name[0] == 'I') class_name = class_name.Substring(1);

                //add NVP namespace
                cs_content.AppendLine("using NVP.API.Nodes;" + line_sep);
                //add NVP_Manifest_Creator namespace (for attributes to create manifest)
                cs_content.AppendLine("using NVP_Manifest_Creator;");
                //add namespace (for that IDL file and for class) and comment for it 
                cs_content.AppendLine(GetComment(class_wrapper.Description, true));
                cs_content.AppendLine($"namespace {RootNsName}.{class_name} {line_sep}" + "{");
                //adding class content

                string original_interface = $"{tab}{tab}public {LibName}.{class_wrapper.Name} _i;" + line_sep;
                string NVP_Folder = _projectName + "." + RootNsName + "." + class_name;

                //add default dynamic constructor (from COM object)
                var doc_info1 = CommonData._doc.Add(
                    RootNsName + "." + class_name + "." + class_name + "_Constructor",
                    NVP_Folder,
                    "_" + class_name + "_Constructor",
                    true,
                    NVP_XML.NodeViewType.Modifier);
                string nvp_manifest_1 = Get_ClassAttributes_NVP_Manifest(doc_info1);

                string dyn_constructor = Get_NodeResult_Execute(new string[]
                {
                    "dynamic _input0 = inputs[0].Value;",
                    $"this._i = _input0 as {LibName}.{class_wrapper.Name};",
                    "if (this._i == null) throw new Exception(\"Invalid casting\");",
                    "return new NodeResult(this);"
                });
                cs_content.AppendLine(nvp_manifest_1 + line_sep + 
                    $"{tab}[NodeInput(\"dynamic\", typeof(object))]" + line_sep +
                    $"{tab}public class {class_name}_Constructor : INode " + line_sep +
                    $"{tab}" + "{" + line_sep + original_interface + dyn_constructor  +
                    $"{tab}" + "}");



                //add default cast-constructor (from _i field)
                var doc_info2 = CommonData._doc.Add(
                    RootNsName + "." + class_name + "." + class_name + "_ConstructorCast",
                    NVP_Folder,
                    "_" + class_name + "_ConstructorCast",
                    true,
                    NVP_XML.NodeViewType.Modifier);
                string nvp_manifest_2 = Get_ClassAttributes_NVP_Manifest(doc_info2);

                string dyn_constructor2 = Get_NodeResult_Execute(new string[]
                {
                    "dynamic _input0 = inputs[0].Value;",
                    $"this._i = _input0._i as {LibName}.{class_wrapper.Name};",
                    "if (this._i == null) throw new Exception(\"Invalid casting\");",
                    "return new NodeResult(this);"
                });
                cs_content.AppendLine(nvp_manifest_2 + line_sep +
                    $"{tab}[NodeInput(\"dynamic\", typeof(object))]" + line_sep +
                    $"{tab}public class {class_name}_ConstructorCast : INode " + line_sep +
                    $"{tab}" + "{" + line_sep + original_interface + dyn_constructor2 +
                    $"{tab}" + "}");

                


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
                        //Резерв для типов enum для приведения
                        List<string> arguments_types = new List<string>();
                        List<bool> arguments_is_enum = new List<bool>();

                        List<string> arguments_names = new List<string>();
                        List<string> NVP_args_attributes = new List<string>();
                        NVP_args_attributes.Add($"[NodeInput(\"{class_name}\", typeof(object))]");
                        bool is_opt = false;
                        for (int i = 0; i < class_element.ArgumentsNames.Length; i++)
                        {
                            string arg_name = class_element.ArgumentsNames[i];
                            var arg_type_raw = class_element.ArgumentsTypes[i];
                            
                            string arg_type_source = class_element.ArgumentTypes_Source[i];
                            string arg_type = null;
                            bool is_enum = false;
                            foreach (string enum_def in enums)
                            {
                                if (arg_type_source.Contains(enum_def)) 
                                {
                                    arg_type = $"{LibName}.{enum_def}";
                                    is_enum = true;
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
                            if (arg_type == "dynamic") arg_type_Type = typeof(object);

                            string arg_name2 = arg_name;
                            if (is_enum) arg_name2 = arg_type;
                            NVP_args_attributes.Add($"[NodeInput(\"{arg_name2}\", typeof({arg_type_Type}))]");

                            arguments.Add(out_info + arg_type + " " + arg_name);
                            arguments_names.Add(out_info + arg_name);
                            arguments_types.Add(arg_type);
                            arguments_is_enum.Add(is_enum);
                        }
                        arguments_string = string.Join(",", arguments);
                        arguments_names_string = string.Join(",", arguments_names);

                        List<string> NVP_arguments_raw = new List<string>();
                        for (int arg_i = 0; arg_i  < arguments.Count; arg_i++)
                        {
                            string pre_cast = "";
                            if (arguments_is_enum[arg_i] is true) pre_cast =arguments_types[arg_i];

                            if (pre_cast != "") NVP_arguments_raw.Add($"(({pre_cast})inputs[{arg_i + 1}].Value)");
                            else NVP_arguments_raw.Add($"inputs[{arg_i + 1}].Value");

                        }
                        string NVP_arguments = string.Join (",", NVP_arguments_raw);

                        NVP_XML.NodeViewType content_type = NVP_XML.NodeViewType.Default;
                        //string content_type = "NodeViewType.Default";
                        string element_instructions = "dynamic _input0 = inputs[0].Value;" + line_sep + "\t\t\t";
                        string element_name = class_element.Name;

                        //|| 
                        if (class_element.TYPE == NET_DLL_PROTOTYPE.NET_TYPE.TYPE_METHOD_VOID)
                        {
                            content_type = NVP_XML.NodeViewType.Modifier;
                            //content_type = "NodeViewType.Modifier";
                            element_instructions += $"_input0._i.{class_element.Name}({NVP_arguments});" + line_sep + "\t\t\t";
                            element_instructions += "return null;";
                        }
                        else if (class_element.TYPE == NET_DLL_PROTOTYPE.NET_TYPE.TYPE_METHOD_SET || class_element.TYPE == NET_DLL_PROTOTYPE.NET_TYPE.TYPE_FIELD)
                        {
                            string prefix = "Set_";
                            if (class_element.TYPE == NET_DLL_PROTOTYPE.NET_TYPE.TYPE_FIELD) prefix = "Put_" ;
                            element_name = prefix + element_name;

                            content_type = NVP_XML.NodeViewType.Modifier;
                            //content_type = "NodeViewType.Modifier";
                            if (arguments.Count == 1) element_instructions += $"_input0._i.{class_element.Name} = {NVP_arguments};";
                            else
                            {
                                element_instructions += $"_input0._i.{class_element.Name}[inputs[1]] = inputs[2];";
                            }
                            element_instructions += line_sep + "\t\t\t";

                            element_instructions += "return null;";
                        }
                        else if (class_element.TYPE == NET_DLL_PROTOTYPE.NET_TYPE.TYPE_METHOD_GET)
                        {
                            //element_name = element_name;
                            content_type = NVP_XML.NodeViewType.Data;
                            //content_type = "NodeViewType.Data";
                            string arguments_names_string2 = $"({NVP_arguments})";
                            if (NVP_arguments.Length < 2) arguments_names_string2 = "";
                            element_instructions += $"return new NodeResult(_input0._i.{class_element.Name}{arguments_names_string2});" + line_sep;
                        }
                        else if (class_element.TYPE == NET_DLL_PROTOTYPE.NET_TYPE.TYPE_METHOD_PRIVATE_VOID)
                        {
                            //content_type = "private void";
                            content_type = NVP_XML.NodeViewType.Modifier;
                            element_name = "HiddenField_" + element_name;
                            element_instructions += "return null;";
                            //element_instructions = $"this._i.{class_element.Name}({arguments_names_string});";
                        }
                        else throw new Exception($"Invalid type of element of class {class_element.TYPE.ToString()}");

                        if (class_element.Name == "Item")
                        {
                            //replace () to [] when Method's name is 'Item' (IEnumerable)
                            //element_instructions = element_instructions.Replace(")", "]").Replace(".Item(", "[");
                        }

                        //add get-propperty as =>
                        string opt_comment = "";
                        if (is_opt) opt_comment = " //optional_arguments" + line_sep;

                        cs_content.AppendLine(opt_comment);

                        var doc_info_item = CommonData._doc.Add(
                            RootNsName + "." + class_name + "." + element_name,
                            NVP_Folder,
                            element_name,
                            true,
                            content_type);
                        string nvp_manifest_item = Get_ClassAttributes_NVP_Manifest(doc_info_item);
                        cs_content.AppendLine(nvp_manifest_item);

                        
                        foreach (string ang_Attribute in NVP_args_attributes)
                        {
                            cs_content.AppendLine($"{tab}{ang_Attribute}");
                        }
                        cs_content.AppendLine(GetComment(class_element.Description, false));


                        string exec = Get_NodeResult_Execute(new string[] { element_instructions });

                        

                        cs_content.AppendLine( 
                            $"{tab}public class {element_name} : INode" + line_sep +
                            $"{tab}" + "{" + line_sep +
                            $"" + $"{exec}" +
                            $"{tab}" + "}");

                        
                    }
                }

                //close class
                //cs_content.AppendLine($"{tab}" + "}");
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
        }
        private string GetComment(string helpstring, bool is_namespace)
        {
            string tabs;
            if (is_namespace) tabs = "";
            else tabs = tab;
            return line_sep + $"{tabs}///<summary>" + line_sep +
                $"{tabs}///{helpstring}" + line_sep + 
                $"{tabs}///</summary>";
        }

    }
}
