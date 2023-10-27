using Discord;
using Discord.WebSocket;
using Garth.Helpers;
using OpenAI.Utilities.FunctionCalling;

namespace Garth.Services.GPT.Functions;

public class GptContext
{
    private GarthCommandContext _context;
    
    public GptContext(GarthCommandContext context)
    {
        _context = context;
    }

    [FunctionDescription("Reply to the current channel with a message")]
    public async Task SendMessageToCurrentChannel(
        [ParameterDescription("The content of the message")] string messageContent)
    {
        await _context.Message.ReplyAsync(messageContent);
    }

    [FunctionDescription("Add a reaction to a specific message")]
    public async Task AddReactionToMessage(
        [ParameterDescription("The ID of the message to react to")] string messageId,
        [ParameterDescription("The emote to react with. :heart: for example.")] string emote)
    {
        ulong messageIdAsULong = ulong.Parse(messageId);
        var message = await _context.Channel.GetMessageAsync(messageIdAsULong);
        var emoji = Emote.Parse(emote);
        await message.AddReactionAsync(emoji);
    }

    /*[FunctionDescription("Creates a new thread channel")]
    public async Task CreateThreadFromMessage(
        [ParameterDescription("The ID of the message to create the thread from")] string messageId,
        [ParameterDescription("The title of the thread")] string threadTitle)
    {
        ulong messageIdAsULong = ulong.Parse(messageId);
        var message = await _context.Channel.GetMessageAsync(messageIdAsULong);
        SocketTextChannel channel = (SocketTextChannel)_context.Channel;
        await channel.CreateThreadAsync(threadTitle, ThreadType.PublicThread, ThreadArchiveDuration.OneDay, message);
    }*/
    
    [FunctionDescription("Creates a new thread channel and sends a reply message there")]
    public async Task CreateThreadFromMessageAndReply(
        [ParameterDescription("The ID of the message to create the thread from")] string messageId, 
        [ParameterDescription("The title of the thread")] string threadTitle, 
        [ParameterDescription("The content of the reply")] string replyContent)
    {
        if (_context.Channel is IThreadChannel)
        {
            await SendMessageToCurrentChannel(replyContent);
            return;
        }
        ulong messageIdAsULong = ulong.Parse(messageId);
        var message = await _context.Channel.GetMessageAsync(messageIdAsULong);
        SocketTextChannel channel = (SocketTextChannel)_context.Channel;
        var thread = await channel.CreateThreadAsync(threadTitle, ThreadType.PublicThread, ThreadArchiveDuration.OneDay, message);
        await thread.SendMessageAsync(replyContent);
    }
}