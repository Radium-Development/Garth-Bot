using Discord.Commands;
using Garth.Helpers;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace Garth.Modules;

public class GPTModule : GarthModuleBase
{
    private OpenAIAPI _api;

    public GPTModule(OpenAIAPI api) { 
        _api = api;
    }
    
    [Command("gpt")]
    public async Task GPT([Remainder]string message) {
        var chat = _api.Chat.CreateConversation(new ChatRequest()
        {
            Model = Model.ChatGPTTurbo0613
        });
        
        chat.AppendUserInput(message);

        using (Context.Channel.EnterTypingState())
            _ = ReplyAsync(await chat.GetResponseFromChatbotAsync(), messageReference: CreateMessageReference(Context));
    }
}