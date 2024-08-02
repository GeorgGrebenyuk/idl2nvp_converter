using System.Runtime.CompilerServices;

namespace AX2LIB
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string dir_path = @"D:\PROCESSING\nanosoft\trunk_API\ru\ncX_NCAuto_and_OdaX_docomatic\source";
            string save_path = @"C:\Users\Georg\Documents\GitHub\nvp_NodeLibs_ActiveX\src\nanoCAD";
            foreach (string idl_path in Directory.GetFiles(dir_path, "*.IDL", SearchOption.AllDirectories))
            {
                IDL_reader reader = new IDL_reader(idl_path);
                reader.Start();
                NET_DLL_Writer writer = new NET_DLL_Writer(reader.NET_prototype, save_path);
                writer.Create();
            }
            Console.WriteLine("End!");
        }
    }
}