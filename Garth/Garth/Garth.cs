#define RELEASE

using Discord;
using Discord.WebSocket;
using Garth.IO;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Interactions;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Services;
using GarthWebPortal;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using Shared.Helpers;
using Spectre.Console;

namespace Garth
{
    public class Garth
    {
        private DiscordSocketClient? _client;
        private Configuration.Config? _config;
        private CommandHandlingService? _commandHandlingService;
        private ComponentHandlingService? _componentHandlingService;
        private WebPortal? _webPortal;
        
        public Garth() =>
            StartBot().GetAwaiter().GetResult();

        private async Task StartBot()
        {
            using (var services = ConfigureServices())
            {
                _client = services.GetRequiredService<DiscordSocketClient>();
                _config = services.GetRequiredService<Configuration.Config>();
                _commandHandlingService = services.GetRequiredService<CommandHandlingService>();
                _componentHandlingService = services.GetRequiredService<ComponentHandlingService>();
                _webPortal = services.GetRequiredService<WebPortal>();

                _client.GuildAvailable += guild =>
                {
                    Console.WriteLine("Found Guild: " + guild.Name);
                    return Task.CompletedTask;
                };
                
                _client.Log += Log;
                
                #if RELEASE
                var token = _config.Token;
                #else
                var token = _config.TestingToken;
                #endif

                if (token is null)
                    throw new Exception("Bot token not set in config.json");

                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();
                await _commandHandlingService.InitializeAsync();
                await _componentHandlingService.InitializeAsync();

                _webPortal.Builder.Services.AddSingleton(_client);
                
                if(EnvironmentVariables.Get("GARTH_ENABLE_WEB_PORTAL", defaultValue: true))
                    _ = Task.Run(() => _webPortal.Start());
                
                await Task.Delay(-1);
            }
        }

        private ServiceProvider ConfigureServices()
        {
            Configuration configuration = new Configuration();

            var sqlConnectionString = EnvironmentVariables.Get("GarthConnectionString", true)!;

            return new ServiceCollection()
                .AddDbContext<GarthDbContext>(context =>
                {
                    context.UseMySql(sqlConnectionString, ServerVersion.AutoDetect(sqlConnectionString));
                })
                .AddSingleton(e =>
                {
                    var config = new DiscordSocketConfig()
                    {
                        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers | GatewayIntents.All
                    };

                    return new DiscordSocketClient(config);
                })
                .AddSingleton(configuration)
                .AddSingleton(configuration.Data)
                .AddSingleton<CommandService>()
                .AddSingleton<InteractionService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<ComponentHandlingService>()
                .AddSingleton<WebPortal>()
                .AddSingleton(new OpenAIAPI(EnvironmentVariables.Get("OPENAI_KEY", true)!))
                .BuildServiceProvider();
        }
        
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}