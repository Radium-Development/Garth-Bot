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
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Services;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace Garth
{
    public class Garth
    {
        private DiscordSocketClient? _client;
        private Configuration.Config? _config;
        private CommandHandlingService? _commandHandlingService;

        public Garth() =>
            StartBot().GetAwaiter().GetResult();

        private async Task StartBot()
        {
            using (var services = ConfigureServices())
            {
                _client = services.GetRequiredService<DiscordSocketClient>();
                _config = services.GetRequiredService<Configuration.Config>();
                _commandHandlingService = services.GetRequiredService<CommandHandlingService>();

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

                await Task.Delay(-1);
            }
        }

        private ServiceProvider ConfigureServices()
        {
            Configuration configuration = new Configuration();

            var sqlConnectionString =
                Environment.GetEnvironmentVariable("GarthConnectionString", EnvironmentVariableTarget.Process);
            sqlConnectionString ??=
                Environment.GetEnvironmentVariable("GarthConnectionString", EnvironmentVariableTarget.User);
            sqlConnectionString ??=
                Environment.GetEnvironmentVariable("GarthConnectionString", EnvironmentVariableTarget.Machine);
            if (sqlConnectionString is null)
                throw new Exception("Environment variable 'GarthConnectionString' is not set!");

            return new ServiceCollection()
                .AddDbContext<GarthDbContext>(context =>
                {
                    context.UseMySql(sqlConnectionString, ServerVersion.AutoDetect(sqlConnectionString));
                })
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<Configuration>(configuration)
                .AddSingleton<Configuration.Config>(configuration.Data)
                .AddSingleton<GptService>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .BuildServiceProvider();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}