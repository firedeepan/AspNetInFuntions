using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.AspNetCore.TestHost;
using WebApiAspNetCore;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;

namespace WebApiProxyFunctionApp
{
    public static class AspNetCoreProxy
    {
        private static readonly HttpClient Client;

        static AspNetCoreProxy()
        {
            var functionPath = new FileInfo(typeof(AspNetCoreProxy).Assembly.Location).Directory.Parent.FullName;
            Directory.SetCurrentDirectory(functionPath);
            var server = CreateServer(functionPath);
            Client = server.CreateClient();
        }

        private static TestServer CreateServer(string functionPath)
        {



            return new TestServer(new WebHostBuilder()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config
                        .SetBasePath(functionPath)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{builderContext.HostingEnvironment.EnvironmentName}.json",
                            optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .UseStartup<Startup>()
                .UseContentRoot(Path.Combine(functionPath)));
        }

        [FunctionName("Proxy")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get", "post", "put", "patch", "options",
                Route = "{*x:regex(^(?!admin|debug|monitoring).*$)}")]
            HttpRequestMessage req,
            TraceWriter log)
        {
            log.Info("***HTTP trigger - ASP.NET Core Proxy: function processed a request.");

            var response = await Client.SendAsync(req);

            return response;
        }
    }
}
