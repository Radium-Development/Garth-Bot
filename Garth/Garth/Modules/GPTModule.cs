using Discord.Commands;
using Garth.Helpers;
using OpenAI.Managers;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;

namespace Garth.Modules;

public class GPTModule : GarthModuleBase
{
    private OpenAIService _openAiService;

    public GPTModule(OpenAIService api) { 
        _openAiService = api;
    }
    
    [Command("gpt")]
    public async Task GPT([Remainder]string message) {
        var response = await _openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = new List<ChatMessage>
            {
                new ChatMessage("user", message)
            },
            Temperature = 0.4f,
            Model = Models.Gpt_4
        });

        using (Context.Channel.EnterTypingState())
            _ = ReplyAsync(response.Choices.First().Message.Content, messageReference: CreateMessageReference(Context));
    }
}