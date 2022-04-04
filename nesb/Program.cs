using System;
using System.Diagnostics;
using Newtonsoft.Json;
using nesbRT;

public class nesbUtil
{
    public const int NESB_UTIL_SUCCESS = 0;
    public const int NESB_UTIL_FAIL = -1;
    public const string CREATECONFIG_COMMAND = "configCreate";
    public const string CONFIG_FILENAME = "config.json";
    public const string USING_HINT = "Usage:\n\n\tnesb <folder-with-asm-sources> [configCreate]\n\n*configCreate is meant to be used to create default JSON config file";

    public static nesbWorkspace ctx;
    public static nesbConfig ctxConfig;

    public static int Main(string[] args)
    {
        ctxConfig = new nesbConfig();
        if (args.Length > 1 && args[1] == CREATECONFIG_COMMAND)
        {
            if(!nesbJSON.Create(args[0] + "\\" + CONFIG_FILENAME, ctxConfig))
                return NESB_UTIL_FAIL;
            
            return NESB_UTIL_SUCCESS;
        }
        else if(args.Length > 1 && args[1] != CONFIG_FILENAME)
        {
            Console.WriteLine(USING_HINT);
            return NESB_UTIL_SUCCESS;
        }
        
        ctx = new nesbWorkspace();  
        
        if(!ctx.Initialize())
        {
            Console.WriteLine("Failed while initializing. Abort.");
            return NESB_UTIL_FAIL;
        }
        ctx.SourcesFolderName = args[0];

        if(!nesbJSON.Load(args[0] + "\\" + CONFIG_FILENAME, ref ctxConfig))
        {
            Console.WriteLine("Unable to load JSON file.");
            return NESB_UTIL_FAIL;
        }
        if(!ctx.UseConfig(ctxConfig))
        {
            Console.WriteLine("FATAL: " + CONFIG_FILENAME + " is unable to be used. Probably? it`s corrupted. Please, reset it with command: \n\n\tnesb <source-folder> configCreate.");
            return NESB_UTIL_FAIL;
        }

        if (!ctx.GetSrcList())
        {
            Console.WriteLine("failed");
        }
        else
        {
            if (!ctx.PerformCompilation())
            { 
                Console.WriteLine("compilation failed"); 
            }
            else
            {
                if (!ctx.PerformLinking())
                    Console.WriteLine("linking failed");
            }
        }
        return NESB_UTIL_SUCCESS;
    }
}


