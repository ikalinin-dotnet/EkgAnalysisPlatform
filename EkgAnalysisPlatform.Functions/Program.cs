using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace EkgAnalysisPlatform.Functions;

public class Program
{
    public static void Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(services =>
            {
                services.AddHttpClient();
            })
            .Build();

        host.Run();
    }
}