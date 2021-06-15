using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Microsoft.Graph;

using _425show.SecretManager;
using _425show.Msal.Extensions;

namespace isolated_rotator
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddEnvironmentVariables();
                    config.AddJsonFile(
                        System.IO.Path.Join(
                            hostContext.HostingEnvironment.ContentRootPath, @"..\..\appsettings.json"), optional: false);
                })
                 .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<SecretCredentialConfiguration>(hostContext.Configuration.GetSection("AzureAd:Credential"));
                    services.Configure<AzureAdConfiguration>(hostContext.Configuration.GetSection("AzureAd"));
                    services.Configure<TableCredStoreConfiguration>(hostContext.Configuration.GetSection("CredStoreConnection"));
                    services.AddSingleton<ICredentialStore, AzureTableCredStore>();
                    services.AddSingleton<IMsalCredential, SecretCredential>();
                    services.AddSingleton<MsalBuilder>();
                    services.AddSingleton<IAuthenticationProvider, MsalBuilderCredentialAuthenticationProvider>();
                    services.AddSingleton<GraphServiceClient>();
                    services.AddSingleton<AppSecretManager>();
                    //services.AddHostedService<SecretManagerService>();
                })
                .Build();

            host.Run();
        }
    }

}