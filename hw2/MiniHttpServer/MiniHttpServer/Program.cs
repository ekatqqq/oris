using System.Text.Json;
using HttpServer.Shared;
using Microsoft.VisualBasic;

class Program
{
    public static async Task Main()
    {
        try
        {
            if (!File.Exists("settings.json"))
            {
                Console.WriteLine("The settings.json file could not be found");
                return;
            }

            string settingsJson = File.ReadAllText("settings.json");
            var settings = JsonSerializer.Deserialize<SettingsModel>(settingsJson);

            if (settings == null)
            {
                Console.WriteLine("Error reading settings from settings.json");
                return;
            }

            if (!File.Exists(Path.Combine(settings.StaticDirectoryPath, "index.html")))
            {
                Console.WriteLine("index.html not found in the static folder");
                return;
            }

            var server = new MiniHttpServer(settings);
            var serverTask = server.StartAsync();

            Console.WriteLine("The server is running. Enter '/stop' to stop.");

            while (true)
            {
                var input = Console.ReadLine();
                if (input?.ToLower() == "/stop")
                {
                    Console.WriteLine("Stopping the server.");
                    server.Stop();
                    break;
                }
            }

            await serverTask;
            Console.WriteLine("The server is completely stopped.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error: {ex.Message}");
        }
    }
}