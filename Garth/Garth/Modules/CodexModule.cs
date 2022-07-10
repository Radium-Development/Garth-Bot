using Discord.Commands;
using Garth.Enums;
using Garth.Helpers;
using Garth.Services;

namespace Garth.Modules;

public class CodexModule : GarthModuleBase
{
    private GptService _service;
    
    public CodexModule(GptService gptService)
    {
        _service = gptService;
    }
    
    [Command("codex")]
    public async Task Codex([Remainder]string content)
    {
        GptResponse response = await _service.GetResponse(content, Context.Message.Author.Username, Model.Codex);

        if (!response.Success)
            await ReplyErrorAsync(response.Error!);
        
        await ReplySuccessAsync($"```\n{response.Response!}\n```");
    }
}