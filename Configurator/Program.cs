using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Configurator
{
    class Program
    {
        static void Main(string[] args)
        {
            string username = "N/A";
            string password = "N/A";
            string APIToken = "N/A";

            Console.WriteLine("PRTG Dienst Konfiguration");
            string appSettingsPath = Path.Combine(@"C:\ProgramData\PRTGSensorStatus\Appsettings", "appsettings.json");

            // Erstellen des Verzeichnisses, falls es nicht existiert
            if (!Directory.Exists(@"C:\ProgramData\PRTGSensorStatus\Appsettings"))
            {
                Directory.CreateDirectory(@"C:\ProgramData\PRTGSensorStatus\Appsettings");
            }

            // Liste für Server-Konfigurationen
            JArray serverConfigs = new JArray();

            bool addAnotherServer = true;

            while (addAnotherServer)
            {

                Console.Write("Server-IP: ");
                string serverIp = Console.ReadLine();

                Console.Write("Protokoll (http/https): ");
                string protocol = Console.ReadLine();

                Console.Write("Verwenden Sie einen alternativen Port? (j/n): ");
                bool useAlternatePort = Console.ReadLine().ToLower() == "j";

                string port = null;
                if (useAlternatePort)
                {
                    Console.Write("Geben Sie den alternativen Port ein: ");
                    port = Console.ReadLine();
                }

                Console.Write("Möchten Sie einen API Token Verwenden? (j/n): ");
                bool UseAPIToken = Console.ReadLine().ToLower() == "j";

                if (UseAPIToken == false)
                {

                    Console.Write("Benutzername: ");
                    username = Console.ReadLine();

                    Console.Write("Passwort: ");
                    password = Console.ReadLine();

                    APIToken = "N/A";

                }

                if (UseAPIToken == true)
                {
                    Console.Write("API Token: ");
                    APIToken = Console.ReadLine();

                    username = "N/A";
                    password = "N/A";
                }

                Console.Write("Aktualisierungsintervall (in Sekunden): ");
                string refreshInterval = Console.ReadLine();

                // Einzelne Server-Konfiguration erstellen
                var serverConfig = new JObject
                {
                    { "ServerIP", serverIp },
                    { "UseAPIToken", UseAPIToken },
                    { "APIToken", APIToken },
                    { "Username", username },
                    { "Password", password },
                    { "Protocol", protocol },
                    { "UseAlternatePort", useAlternatePort },
                    { "Port", port ?? "" },  // Leerer String, falls kein alternativer Port
                    { "RefreshInterval", refreshInterval }
                };

                // Server-Konfiguration zur Liste hinzufügen
                serverConfigs.Add(serverConfig);

                Console.Write("Möchten Sie einen weiteren Server hinzufügen? (j/n): ");
                addAnotherServer = Console.ReadLine().ToLower() == "j";
            }

            // Gesamte Konfiguration als JSON speichern
            var config = new JObject
            {
                { "Servers", serverConfigs }
            };

            // Konfiguration in der Datei speichern
            File.WriteAllText(appSettingsPath, config.ToString(Formatting.Indented));
            Console.WriteLine("Konfiguration gespeichert in: " + appSettingsPath);
        }
    }
}
