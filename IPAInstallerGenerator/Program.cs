using System;
using System.Collections.Generic;
using System.Text;
using IPATools;
using System.Globalization;
using System.IO;

namespace IPAInstallerGenerator
{
    class Program
    {
        const string TAG = "iwigen";

        static void Main(string[] args)
        {
            List<string> ipaList = new List<string>();
            string outputDir = string.Empty;
            string baseUrl = string.Empty;
            bool infoOnly = false;
            bool skipCopy = false;

            try
            {
                if (args.Length == 0)
                {
                    ShowHelp();
                    return;
                }

                for (int i = 0; i < args.Length; ++i)
                {
                    string arg = args[i];

                    if (arg.StartsWith("-"))
                        arg = arg.Substring(1);

                    if (arg == "help")
                    {
                        ShowHelp();
                        return;
                    }

                    try
                    {
                        switch (arg)
                        {
                            case "ipa": break;
                            case "outputDir": outputDir = args[++i]; break;
                            case "baseUrl": baseUrl = args[++i]; break;
                            case "info": infoOnly = true; break;
                            case "skipCopy": skipCopy = true; break;
                            default:
                                if (File.Exists(arg) && Path.GetExtension(arg).Equals(".ipa", StringComparison.InvariantCultureIgnoreCase))
                                    ipaList.Add(arg);
                                else
                                    throw new ArgumentException(string.Format("unrecognized argument \"{0}\".", args[i]));
                                break;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentException(string.Format("missing value for \"{0}\" argument.", arg));
                    }
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(TAG + ": " + e.Message);
                Console.WriteLine("Try " + TAG + " --help for more information.");
                Environment.Exit(-1);
            }


            List<IPAInfo> infoList = new List<IPAInfo>();

            try
            {
                foreach (var ipaPath in ipaList)
                {
                    Console.Write("Parsing IPA archive \"" + Path.GetFileName(ipaPath) + "\"...");
                    IPAInfo info = IPAParser.Parse(ipaPath);

                    Console.WriteLine(" Done!");
                    Console.WriteLine();
                    Console.WriteLine("  Bundle details");
                    Console.WriteLine("    Display Name:          " + info.BundleDisplayName);
                    Console.WriteLine("    Name:                  " + info.BundleName);
                    Console.WriteLine("    ID:                    " + info.BundleIdentifier);
                    Console.WriteLine("    Version:               " + info.BundleVersion);
                    Console.WriteLine("    Date:                  " + info.BuildDate.ToString(CultureInfo.InvariantCulture));
                    Console.WriteLine();
                    Console.WriteLine("  Restrictions");
                    Console.WriteLine("    Platform:              " + info.PlatformName);
                    Console.WriteLine("    Minimum OS version:    " + info.MinimumOSVersion);
                    Console.WriteLine("    Device:                " + info.DeviceFamily.ToString());
                    Console.WriteLine();
                    Console.WriteLine();

                    infoList.Add(info);
                }

                if (infoOnly)
                    return;

                Console.Write("Generating installer...");
                IPATools.IPAInstallerGenerator generator = new IPATools.IPAInstallerGenerator(
                    infoList.ToArray(), outputDir, baseUrl, null);
                generator.SkipCopying = skipCopy;
                generator.Run();
                Console.WriteLine(" Done!");
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine(TAG + ": " + e.Message);
                Environment.Exit(-1);
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: " + TAG + " -ipa IPA -outputDir OUTPUT_DIR -baseUrl BASE_URL");
            Console.WriteLine("  or:  " + TAG + " -ipa IPA [-info]");
            Console.WriteLine();
            Console.WriteLine("Generate web installer in OUTPUT_DIR directory with links");
            Console.WriteLine("beginning with BASE_URL for IPA file.");
            Console.WriteLine();
            Console.WriteLine("Mandatory arguments:");
            Console.WriteLine("  -ipa                         path to IPA. Application archive file");
            Console.WriteLine("                               generated by Xcode for iOS projects.");
            Console.WriteLine("  -outputDir                   output directory where installer");
            Console.WriteLine("                               files will be placed");
            Console.WriteLine("  -baseUrl                     URL address which will serve as a base");
            Console.WriteLine("                               for all links in installer files.");
            Console.WriteLine("  -info                        Prints info about IPA archive and exit");
            Console.WriteLine();
            Console.WriteLine("Report bugs to <thedmd at artifexmundi.com>");
            Console.WriteLine();
        }
    }
}
