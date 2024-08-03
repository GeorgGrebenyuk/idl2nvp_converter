using System.Runtime.CompilerServices;

namespace AX2LIB
{
    public class AX2LIB_Config
    {
        public string projectName { get; set; }
        public string idlPath { get; set; }

        public string savePath { get; set; }

        public AX2LIB_Config(string _projectName, string _idlPath, string _savePath)
        {
            this.savePath = _savePath;
            this.projectName = _projectName;
            this.idlPath = _idlPath;
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {

            AX2LIB_Config[] configs = new AX2LIB_Config[2]
            {
                new AX2LIB_Config("NVP_nanoCAD_COM", @"C:\Users\Georg\Documents\GitHub\nvp_NodeLibs_ActiveX\src\_IDL\ncad", @"C:\Users\Georg\Documents\GitHub\nvp_NodeLibs_ActiveX\src\NVP_nanoCAD"),
                new AX2LIB_Config("NVP_Renga_COM", @"C:\Users\Georg\Documents\GitHub\nvp_NodeLibs_ActiveX\src\_IDL\renga", @"C:\Users\Georg\Documents\GitHub\nvp_NodeLibs_ActiveX\src\NVP_Renga_COM")
            };

            foreach (var config in configs)
            {
                CommonData._doc = new NVP_XML(config.projectName + ".dll", "GeorgGrebenyuk", Path.Combine(config.savePath, config.projectName + ".nodeitem"));

                foreach (string idl_path in Directory.GetFiles(config.idlPath, "*.IDL", SearchOption.AllDirectories))
                {
                    IDL_reader reader = new IDL_reader(idl_path);
                    reader.Start();
                    NET_DLL_Writer writer = new NET_DLL_Writer(config.projectName, reader.NET_prototype, config.savePath);
                    writer.Create();
                }
                CommonData._doc.Save();
            }



            

            Console.WriteLine("End!");
        }
    }
}