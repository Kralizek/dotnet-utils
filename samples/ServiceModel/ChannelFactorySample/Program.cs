﻿using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Contracts;
using InsightArchitectures.Utilities.ServiceModel;

namespace ChannelFactorySample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddLogging(l => l.AddConsole().SetMinimumLevel(LogLevel.Debug));

            services.AddSingleton(sp =>
            {
                var binding = new BasicHttpBinding();

                var endpointAddress = new EndpointAddress("http://localhost:8080");

                return ActivatorUtilities.CreateInstance<ChannelFactory<IEchoService>>(sp, binding, endpointAddress);
            });

            services.AddTransient<TestEchoProxyWrapper>();

            await using var serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            var client = serviceProvider.GetRequiredService<TestEchoProxyWrapper>();

            try
            {
                var result = client.Proxy.Echo("Hello world");

                logger.LogInformation($"Result: {result}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while performing a remote call");
            }
        }
    }

    public class TestEchoProxyWrapper : ChannelFactoryProxyWrapper<IEchoService>
    {
        public TestEchoProxyWrapper(ChannelFactory<IEchoService> channelFactory, ILogger<TestEchoProxyWrapper> logger) : base(channelFactory, logger) {}
    }
}