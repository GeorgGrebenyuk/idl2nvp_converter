# idl2nvp_converter
Translator ActiveX library (from IDL file) to C# NET DLL for NVP (https://nvp-studio.ru/) as node-package library. Next there is a summary how use it:

## Summary about using

1. Download Windows SDK (f.e. with Visual Studio Installer any supported version, f.e. 10.0.19041.0);
2. Run oleview.exe (by default from path `C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\oleview.exe`);
3. Navigate to `Type Libraries` tab and find in List of installed COM servers needing you, f.e. `AutoCAD 2021 Type Library` and go to `view` representation of that COM server;
4. Save representation to IDL file and retry step 3-4 for other target COM servers;
5. Using source code that library, navigate to `.\src\AX2LIB_Runner\Program.cs` and create `NET_DLL_Writer` for each created IDL;
6. Profit!

## Summary about logic

Original IDL file (COM Server) consists of Interfaces, Enums, Delegates and some other elements. Because of in NVP using only classes, all COM-interfaces's content are transformed in class-elements.
* For each intreface the will creating sub-namespace and two node-constructor (for casting unmanaged COM-object to class and for casting the property `_i` of each class to target object), in each of two constructors there is public field with `_i` name;
* All `HRESULT` are considered as methods or fields (if have not any arguments), it looks as classes, of cource;
* All other interfaces in arguments are marking as `dynamic` type in `[NodeInput...]` attributes;
* All enums in argument's list marking as `int`-variables, their names are in Full-namespace mode in `[NodeInput...]` attribute;
* For each HRESULT and interface saving original `helpstring` data that transforming to Visual Studio's comment of class;
* For methods that using optional arguments created a comment before class;
* Original logic with using `ref`, `out` argumens are save, but in library you need to edit all `ref` and `out` logic.
* Because of in NVP using GUID-ids of each node, the logic of link class name (and path) to Guid is exists and info stored in auxiliary file next with generating class-wrappers. Look class `NVP_XML_GuidsMap.cs` for details;

Other information (and discussing) you can fing at https://nvp-studio.ru/.

# Sample packages as result

* https://github.com/GeorgGrebenyuk/nvp_NodeLibs_ActiveX -- packages for CAD-systme with ActiveX (COM) API: Renga, nanoCAD and etc. 