using System.Diagnostics;
using Newtonsoft.Json;

namespace nesbRT
{
    public class nesbWorkspace
    {
        public bool Initialize()
        {
            try
            {
                CompilerProcess = new Process();
                LinkerProcess = new Process();
            }
            catch (Exception ex)
            {
                Console.WriteLine(nesbFail.INITIALIZEWORKSPFAIL_MSG + ex.Message);
                return false;
            }

            return true;
        }
        public bool GetSrcList()
        {
            try
            {
                src = Directory.GetFiles(srcFolderPath, "*.asm");
            }
            catch (Exception e)
            {
                Console.WriteLine(nesbFail.GETSRCFAIL_MSG + e.Message);
                return false;
            }

            return true;
        }

        public bool PerformCompilation()
        {
            uint free_idx = 0;

            obj = new string[src.Length];
            if (!_CreateObjFolder())
                Console.WriteLine(nesbFail.CREATEOBJFOLDERFAIL_MSG);

            foreach (string filename in src)
            {
                CompilerProcess.StartInfo.Arguments = filename + " -o " + srcFolderPath + "\\" + OBJFOLDER + "\\" + Path.GetFileNameWithoutExtension(filename) + ".o";
                obj[free_idx] = srcFolderPath + "\\" + OBJFOLDER + "\\" + Path.GetFileNameWithoutExtension(filename) + ".o";

                try
                {
                    CompilerProcess.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(nesbFail.PERFORMCOMPILEFAIL_MSG + e.Message);

                    return false;
                }
                free_idx++;
            }

            return true;
        }
        public bool PerformLinking()
        {
            if (!_CreateROMFolder())
            {
                Console.WriteLine(nesbFail.CREATEROMFOLDERFAIL_MSG);
                return false;
            }

            foreach (string objArg in obj)
            {
                LinkerProcess.StartInfo.Arguments += (objArg + " ");
            }
            LinkerProcess.StartInfo.Arguments += linkerArgs + " -o " + srcFolderPath + "\\" + "\\" + ROMFOLDER + "\\" + romName;

            try
            {
                LinkerProcess.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(nesbFail.PERFORMLINKINGFAIL_MSG + e.Message);
                return false;
            }

            return true;
        }

        public bool UseConfig(nesbConfig config)
        {
            try
            {
                linkerArgs = config.LinkerArgs;
                CompilerProcess.StartInfo.FileName = config.CompilerPath;
                LinkerProcess.StartInfo.FileName = config.LinkerPath;
                romName = config.RomName;
            }
            catch (Exception e)
            {
                Console.WriteLine(nesbFail.CONFIGUSEFAIL_MSG + e.Message);
                return false;
            }

            return true;
        }
        private bool _CreateObjFolder()
        {
            if (srcFolderPath != null)
                Directory.CreateDirectory(srcFolderPath + "\\" + OBJFOLDER);
            else
                return false;

            return true;
        }
        private bool _CreateROMFolder()
        {
            if (srcFolderPath != null)
                Directory.CreateDirectory(srcFolderPath + "\\" + ROMFOLDER);
            else
                return false;

            return true;
        }
        private string[] src = { "<empty-list>" };
        private string[] obj = { "<empty-list>" };
        private string srcFolderPath;
        private string linkerArgs = "-t nes ";
        private string romName = "build.nes";
        private const string OBJFOLDER = "obj";
        private const string ROMFOLDER = "rom";
        private Process CompilerProcess, LinkerProcess;

        public string SourcesFolderName
        {
            get { return srcFolderPath; }
            set { srcFolderPath = value; }
        }



        public string LinkerArgs
        {
            get { return linkerArgs; }
            set { linkerArgs = value; }
        }
        public string ObjFolderName
        {
            get { return OBJFOLDER; }
        }
    }

    public static class nesbFail
    {
        public const string GETSRCFAIL_MSG = "nesb.GetSrcList failed: ";
        public const string CREATEOBJFOLDERFAIL_MSG = "nesbWorksp.CreateObjFolder failed: SourceFolderPath is NULL!";
        public const string CREATEROMFOLDERFAIL_MSG = "nesbWorksp.CreateROMFolder failed: ";
        public const string INITIALIZEWORKSPFAIL_MSG = "nesbWorksp.Initialize failed: ";
        public const string PERFORMCOMPILEFAIL_MSG = "nesbWorksp.PerformCompilation failed: ";
        public const string PERFORMLINKINGFAIL_MSG = "nesbWorksp.PerformLinking failed: ";
        public const string CONFIGWRITEFAIL_MSG = "nesbConfig.Write failed: ";
        public const string CONFIGREADFAIL_MSG = "nesbConfig.Load failed: ";
        public const string CONFIGUSEFAIL_MSG = "nesbWorksp.UseConfig failed: ";
    }
    public class nesbConfig
    {
        private JsonSerializer configSerializer;

        private string _compilerPath;

        private string _linkerPath;

        private string _linkerArgs;

        private string _romName;

        public nesbConfig()
        {
            configSerializer = new JsonSerializer();

            CompilerPath = nesbConfigDefaults.PathToCompiler;
            _linkerPath = nesbConfigDefaults.PathToLinker;
            _linkerArgs = nesbConfigDefaults.LinkerArguments;
            _romName = nesbConfigDefaults.NameOfROM;
        }

        private static class nesbConfigDefaults
        {
            public const string PathToCompiler = "cc65\\bin\\ca65.exe";
            public const string PathToLinker = "cc65\\bin\\ld65.exe";
            public const string LinkerArguments = "-t nes";
            public const string NameOfROM = "image.nes";
        }

        public string CompilerPath
        {
            get { return _compilerPath; }
            set { _compilerPath = value; }
        }

        public string LinkerPath
        {
            set { _linkerPath = value; }
            get { return _linkerPath; }
        }

        public string LinkerArgs
        {
            set { _linkerArgs = value; }
            get { return _linkerArgs; }
        }

        public string RomName
        {
            get { return _romName; }
            set { _romName = value; }
        }
    }

    public static class nesbJSON
    {
        public static bool Load(string filename, ref nesbConfig config)
        {
            try
            {
                string JSONconfigStr = File.ReadAllText(filename);
                config = JsonConvert.DeserializeObject<nesbConfig>(JSONconfigStr);
            }
            catch (Exception e)
            {
                Console.WriteLine(nesbFail.CONFIGREADFAIL_MSG + e.Message);
                return false;
            }

            return true;
        }
        public static bool Create(string filename, nesbConfig config)
        {
            try
            {
                string JSONconfigStr = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(filename, JSONconfigStr);
            }
            catch (Exception e)
            {
                Console.WriteLine(nesbFail.CONFIGWRITEFAIL_MSG + e.Message);
                return false;
            }

            return true;
        }
    }
}
