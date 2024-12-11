using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.ServiceProcess;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Timers;

namespace PRTGService
{
    public partial class PRTGService : ServiceBase
    {
        private string sensorUpdateDir = @"C:\ProgramData\PRTGSensorStatus\SensorUpdates";
        private JObject config;
        private int maxSensorDataFiles;
        private List<Timer> serverTimers = new List<Timer>();

        public PRTGService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log("Dienst wird gestartet.");
            try
            {
                ResetLog();
                LoadConfiguration();

                if (!Directory.Exists(sensorUpdateDir))
                {
                    Directory.CreateDirectory(sensorUpdateDir);
                    Log($"Verzeichnis {sensorUpdateDir} wurde erstellt.");
                }

                maxSensorDataFiles = Math.Max(10, (int)config["Servers"].Count() * 5);

                foreach (var serverToken in (JArray)config["Servers"])
                {
                    JObject server = (JObject)serverToken;
                    int refreshInterval = server["RefreshInterval"]?.ToObject<int>() ?? 30;

                    Log($"Erstelle Timer für Server {server["ServerIP"]} mit Intervall {refreshInterval} Sekunden.");

                    Timer serverTimer = new Timer(refreshInterval * 1000);
                    serverTimer.Elapsed += async (sender, e) =>
                    {
                        await FetchSensorDataForServer(server);
                    };
                    serverTimer.Start();
                    serverTimers.Add(serverTimer);
                }

                Log("Alle Server-Timer wurden erfolgreich gestartet.");
            }
            catch (Exception ex)
            {
                Log($"Fehler beim Start des Dienstes: {ex.Message}");
            }
        }

        protected override void OnStop()
        {
            Log("Dienst wird gestoppt.");

            foreach (var timer in serverTimers)
            {
                timer.Stop();
                timer.Dispose();
            }
            serverTimers.Clear();

            Log("Dienst erfolgreich gestoppt.");
        }

        private void ResetLog()
        {
            string logFilePath = @"C:\ProgramData\PRTGSensorStatus\Logs\ServiceLog.txt";
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
                Log("Logs zurückgesetzt.");
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                string configFilePath = @"C:\ProgramData\PRTGSensorStatus\Appsettings\appsettings.json";
                if (File.Exists(configFilePath))
                {
                    config = JObject.Parse(File.ReadAllText(configFilePath));
                    Log("Konfigurationsdatei erfolgreich geladen.");
                }
                else
                {
                    Log("Konfigurationsdatei nicht gefunden.");
                    throw new FileNotFoundException("Konfigurationsdatei nicht gefunden.");
                }
            }
            catch (Exception ex)
            {
                Log("Fehler beim Laden der Konfigurationsdatei: " + ex.Message);
                throw;
            }
        }

        private async Task FetchSensorDataForServer(JObject server)
        {
            try
            {
                string serverIp = server["ServerIP"]?.ToString() ?? "N/A";
                bool useAPIToken = server["UseAPIToken"]?.ToObject<bool>() ?? false;
                string APIToken = server["APIToken"]?.ToString() ?? "";
                string username = server["Username"]?.ToString() ?? "";
                string password = server["Password"]?.ToString() ?? "";
                string protocol = server["Protocol"]?.ToString() ?? "http";
                bool useAlternatePort = server["UseAlternatePort"]?.ToObject<bool>() ?? false;
                string port = server["Port"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(port))
                {
                    port = protocol == "https" ? "443" : "80"; // Standardport setzen
                }

                string apiUrl = useAPIToken
                    ? $"{protocol}://{serverIp}:{port}/api/table.json?content=sensors&columns=objid,sensor,status,message,lastvalue,priority&filter_status=4&filter_status=5&apitoken={APIToken}"
                    : $"{protocol}://{serverIp}:{port}/api/table.json?content=sensors&columns=objid,sensor,status,message,lastvalue,priority&filter_status=4&filter_status=5&username={username}&password={password}";

                Log($"Beginne Datenabruf für Server {serverIp}: {apiUrl}");

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string sensorData = await response.Content.ReadAsStringAsync();
                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        string sensorFilePath = Path.Combine(sensorUpdateDir, $"SensorData_{serverIp}_{timestamp}.json");
                        File.WriteAllText(sensorFilePath, sensorData);
                        Log($"Sensordaten erfolgreich gespeichert: {sensorFilePath}");
                    }
                    else
                    {
                        Log($"Fehler bei der Abfrage von Server {serverIp}: {response.StatusCode}");
                    }
                }

                ManageSensorDataFiles();
            }
            catch (Exception ex)
            {
                Log($"Fehler bei der Verarbeitung von Server {server["ServerIP"]}: {ex.Message}");
            }
        }

        private void ManageSensorDataFiles()
        {
            var filesByServer = Directory.GetFiles(sensorUpdateDir, "SensorData_*.json")
                                          .GroupBy(file =>
                                          {
                                              var parts = Path.GetFileName(file).Split('_');
                                              return parts.Length > 1 ? parts[1] : "UnknownServer";
                                          });

            foreach (var group in filesByServer)
            {
                var server = group.Key;
                var files = group.OrderBy(f => File.GetLastWriteTime(f)).ToList();

                Log($"Prüfe Dateien für Server {server}. Insgesamt: {files.Count}");

                while (files.Count > maxSensorDataFiles)
                {
                    var fileToDelete = files.First();
                    File.Delete(fileToDelete);
                    Log($"Alte Sensordatei gelöscht: {fileToDelete}");
                    files.RemoveAt(0);
                }
            }
        }

        private static readonly object logLock = new object();

        private void Log(string message)
        {
            string logFilePath = @"C:\ProgramData\PRTGSensorStatus\Logs\ServiceLog.txt";
            string logMessage = $"{DateTime.Now}: {message}{Environment.NewLine}";

            lock (logLock)
            {
                try
                {
                    File.AppendAllText(logFilePath, logMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Schreiben in die Logdatei: {ex.Message}");
                }
            }
        }
    }
}
