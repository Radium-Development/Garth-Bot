using ChatGPTCommunicator;
using ChatGPTCommunicator.Models;
using ChatGPTCommunicator.Requests.Completion;
using Discord;
using Discord.Commands;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Helpers;
using Shared.Helpers;

namespace Garth.Modules;

public class TestModule : GarthModuleBase
{
    [Command("test")]
    public async Task Tag([Remainder]string text)
    {
        CompletionRequest request = new CompletionRequestBuilder()
            .AddMessage(MessageRole.user, text)
            .Build();

        ChatGPT chatGpt = new(EnvironmentVariables.Get("OPENAI_KEY", true)!);

        var response = await chatGpt.SendAsync(request);
        
        await ReplyAsync("Request message content: " + response.Choices.First().Message.Content);
    }
}