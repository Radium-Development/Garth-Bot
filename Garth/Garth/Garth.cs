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
using Garth.Services;

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

        var token = _config.Token;

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
      
      return new ServiceCollection()
        .AddSingleton<DiscordSocketClient>()
        .AddSingleton<Configuration>(configuration)
        .AddSingleton<Configuration.Config>(configuration.Data)
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
