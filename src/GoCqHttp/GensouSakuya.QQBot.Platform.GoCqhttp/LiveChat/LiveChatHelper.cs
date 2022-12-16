using GensouSakuya.GoCqhttp.Sdk.Sessions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GensouSakuya.QQBot.Platform.GoCqhttp.LiveChat
{
    internal class LiveChatHelper
    {
        public static async Task<WebApplication> Generate(WebsocketSession session)
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddSignalR();
            builder.Services.AddMemoryCache();
            builder.Services.AddLogging();
            builder.Services.AddSingleton<QQHelper>();
            builder.Services.AddSingleton<WebsocketSession>(p =>
            {
                return session;
            });

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.AllowAnyHeader()
                            .SetIsOriginAllowed(_ => true)
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();
            app.MapHub<ChatHub>("/chat");
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(builder.Environment.ContentRootPath, "web")),
            });
            app.UseCors();

            return app;
        }
    }
}
