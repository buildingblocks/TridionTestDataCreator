using System;
using System.Configuration;
using System.Net;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;
using Tridion.ContentManager.CoreService.Client;
using log4net;
using log4net.Config;

namespace TridionTestData
{
    internal class Program
    {
        private static ILog _log;

        private static string TridionDomain { get; set; }
        private static string TridionPort { get; set; }

        private static string Username { get; set; }
        private static string Password { get; set; }

        private static void Main(string[] args)
        {
            string schemaPublicationTcmId = "tcm:0-123-1";
            var testContentFolderTcmId = new TcmId("tcm:136-2476-2");
            var version = "1.2.3";

            BasicConfigurator.Configure();
            _log = LogManager.GetLogger(typeof (Program));

            bool settingsLoaded = ReadInSettings();

            PrintOutSettings();

            var componentFactory = new ComponentFactory(TridionDomain, TridionPort, Username, Password, version,_log);

            componentFactory.CreateComponents(schemaPublicationTcmId, testContentFolderTcmId);

            Console.WriteLine("-- FIN --");
            Console.ReadLine();
        }
        
        private static void PrintOutSettings()
        {
            _log.InfoFormat("TridionDomain - {0}", TridionDomain);
            _log.InfoFormat("TridionPort - {0}", TridionPort);
            _log.InfoFormat("Username - {0}", Username);
            _log.InfoFormat("Password - {0}", Password);
        }

        private static bool ReadInSettings()
        {
            Console.WriteLine("Do you want to read in form the app config : Y/N");
            string settingsFromConfig = Console.ReadLine();

            if (settingsFromConfig != null && settingsFromConfig.ToLower() == "y")
            {
                return ReadInSettingsFromConfig();
            }

            return false;
        }

        private static bool ReadInSettingsFromConfig()
        {
            TridionDomain = ConfigurationManager.AppSettings.Get("TridionDomain");
            TridionPort = ConfigurationManager.AppSettings.Get("TridionPort");

            Username = ConfigurationManager.AppSettings.Get("Username");
            Password = ConfigurationManager.AppSettings.Get("Password");

            _log.Debug("Read In Settings From Config.");

            return true;
        }

    }
}