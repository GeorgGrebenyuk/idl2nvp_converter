﻿using System.Runtime.CompilerServices;

namespace AX2LIB
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //E:\Code\NanosoftWork\trunk_API\ru\ModelStudio_COM_API\IDL
            //E:\Code\NanosoftWork\trunk_API\ru\ncX_NCAuto_and_OdaX_docomatic\source\23
            string dir_path = @"E:\Code\NanosoftWork\trunk_API\ru\ModelStudio_COM_API\IDL";
            string save_path = @"C:\Users\Georg\Documents\GitHub\Dyn_ActiveX_NCAD\src\IDL";
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