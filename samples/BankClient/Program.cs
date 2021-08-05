using Dapr.Client;
using Google.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BankClient
{
    class Program
    {

        static async Task<int> Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddTransient<Example,HttpClientExample>();
            services.AddTransient<Example, RefitExample>();
            services.AddTransient<Example, WebapiClientExample>();

            services.AddLogging()
                .AddScoped<InvocationHandler>()
                .AddHttpApi<WebapiClientExample.IBank>(o => o.HttpHost = new Uri("http://bank"))
                .AddHttpMessageHandler<InvocationHandler>();
           var serviceprovider = services.BuildServiceProvider();

            Example[] Examples = serviceprovider.GetServices<Example>().ToArray();

            if (args.Length > 0 && int.TryParse(args[0], out var index) && index >= 0 && index < Examples.Length)
            {
                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) => cts.Cancel();

                await Examples[index].RunAsync(cts.Token);
                return 0;
            }

            Console.WriteLine("Hello, please choose a sample to run:");
            for (var i = 0; i < Examples?.Length; i++)
            {
                Console.WriteLine($"{i}: {Examples[i].DisplayName}");
            }
            Console.WriteLine();
            return 1;
        }
    }

    public abstract class Example
    {
        public abstract string DisplayName { get; }

        public abstract Task RunAsync(CancellationToken cancellationToken);
    }
}
