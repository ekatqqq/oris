using System.Net;
using System.Text;
using HttpServer.Shared;

class MiniHttpServer
{
    private readonly HttpListener _listener;
    private readonly SettingsModel _settings;
    private bool _isRunning;

    public MiniHttpServer(SettingsModel settings)
    {
        _settings = settings;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://{_settings.Domain}:{_settings.Port}/");
    }

    public async Task StartAsync()
    {
        _isRunning = true;
        _listener.Start();
        Console.WriteLine($"The server is running on http://{_settings.Domain}:{_settings.Port}/");

        while (_isRunning)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => ProcessRequestAsync(context));
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when getting the context: {ex.Message}");
            }
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        var response = context.Response;
        
        try
        {
            string filePath = Path.Combine(_settings.StaticDirectoryPath, "index.html");
            string responseText = await File.ReadAllTextAsync(filePath);
            
            byte[] buffer = Encoding.UTF8.GetBytes(responseText);
            response.ContentLength64 = buffer.Length;
            response.ContentType = "text/html; charset=utf-8";

            await response.OutputStream.WriteAsync(buffer);
            await response.OutputStream.FlushAsync();

            Console.WriteLine("The request was successfully processed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request processing error: {ex.Message}");
            
            try
            {
                response.StatusCode = 500;
                string errorText = "Internal Server Error";
                byte[] errorBuffer = Encoding.UTF8.GetBytes(errorText);
                response.ContentLength64 = errorBuffer.Length;
                await response.OutputStream.WriteAsync(errorBuffer);
                await response.OutputStream.FlushAsync();
            }
            catch (Exception innerEx)
            {
                Console.WriteLine($"Error when sending an error to the client: {innerEx.Message}");
            }
        }
        finally
        {
            response.Close();
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _listener.Stop();
        Console.WriteLine("The server is shutting down.");
    }
}