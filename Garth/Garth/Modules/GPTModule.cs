using ChatGPTCommunicator;
using ChatGPTCommunicator.Models;
using ChatGPTCommunicator.Requests.Completion;
using Discord.Commands;
using Garth.Helpers;

namespace Garth.Modules;

public class GPTModule : GarthModuleBase
{
    [Command("gpt")]
    public async Task GPT(ChatGPT api, [Remainder]string message) {
        CompletionRequestBuilder chatBuilder = new();
        
        chatBuilder.AddMessage(MessageRole.user, message);
  
        var response = await api.SendAsync(chatBuilder.Build());
        await ReplyAsync(response);
    }
}