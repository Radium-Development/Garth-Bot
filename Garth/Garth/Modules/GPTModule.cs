using ChatGPTCommunicator;
using ChatGPTCommunicator.Models;
using ChatGPTCommunicator.Requests.Completion;
using Discord.Commands;
using Garth.Helpers;

namespace Garth.Modules;

public class GPTModule : GarthModuleBase
{
    
    private ChatGPT _api;

    public GPTModule(ChatGPT api) { 
        _api = api;
    }
    
    [Command("gpt")]
    public async Task GPT([Remainder]string message) {
        CompletionRequestBuilder chatBuilder = new();
        
        chatBuilder.AddMessage(MessageRole.user, message);
  
        var response = await _api.SendAsync(chatBuilder.Build());
        await ReplyAsync(response!.Choices.First().Message.Content);
    }
}