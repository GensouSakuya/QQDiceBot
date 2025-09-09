using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Agent;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using GensouSakuya.QQBot.Core.Handlers;
using GensouSakuya.QQBot.Core.Agent.Tools;
using BotSharp.Abstraction.Functions;

namespace GensouSakuya.QQBot.Core
{
    public static class QQBotExtensions
    {
        public static IApplicationBuilder UseQQBot(this IApplicationBuilder builder)
        {
            builder.UseAgent();
            return builder;
        }

        public static IServiceCollection AddQQBot(this IServiceCollection services, IConfiguration configuration, BaseConfig baseConfig)
        {
            services.AddLogging(p =>
            {
                p.AddConfiguration(configuration).AddSerilog(new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger());
            });
            services.AddMemoryCache();
            services.AddSingleton(p => baseConfig);
            services.AddSingleton<Core>();
            services.AddSingleton<CacheService>();
            services.AddScoped<IFunctionCallback, GetBilispaceToolFn>();
            services
                .AddAiAgent(configuration)
                .AddSingleton<DataManager>()
                .AddSingleton<HandlerResolver>();
            var handlerResolver = new HandlerResolver();
            handlerResolver.RegisterHandlers(services);
            services.AddSingleton(handlerResolver);
            return services;
        }
    }
}
