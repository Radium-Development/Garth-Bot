using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Garth.Helpers;
using Garth.Services;

namespace Garth.Modules;

public class ChatGPTModule : GarthModuleBase
{
    private ChatGPTCommunicator _service;
    
    public ChatGPTModule(ChatGPTCommunicator gptService)
    {
        _service = gptService;
    }
    
    [Command("chatgpt"), Alias("chat", "gpt")]
    public async Task Codex([Remainder]string content)
    {
        var thread = await ((SocketTextChannel)Context.Channel).CreateThreadAsync($"ChatGPT - {content}", ThreadType.PublicThread, ThreadArchiveDuration.OneDay, Context.Message, true, 10);

        await thread.SendMessageAsync("", embed: EmbedHelper.Warning("ChatGPT functionality is currently in **beta**!\nSome functionality may still be limited.\n\n**Please do not spam messages!**"));

        using (thread.EnterTypingState())
        {
            var GPTResponse = await _service.GetResponse(content, thread.Id);

            await thread.SendMessageAsync(GPTResponse.response, messageReference: Context.Message.Reference);
        }
    }
}