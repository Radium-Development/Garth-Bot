using System.Text;
using ChatGPTCommunicator;
using ChatGPTCommunicator.Models;
using ChatGPTCommunicator.Requests.Completion;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Enums;
using Garth.Helpers;
using Shared.Helpers;

namespace Garth.Modules;

public class TestModule : GarthModuleBase
{
    private readonly DiscordSocketClient _client; 
    
    public TestModule(DiscordSocketClient client)
    {
        _client = client;
    }
    
    [Command("test")]
    public async Task Tag()
    {
        var roles = _client.GetGuild(748209749329182851).GetUser(201582886137233409).AddRoleAsync(910220351923953705);

        await ReplyAsync("ok");
        
        return;
        using (Context.Channel.EnterTypingState())
        {
            var events = await Calendar.GetEventsTomorrow();
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("FOL Content Due Tomorrow")
                .WithColor(new Color(255, 255, 255));
            
            var classes = events.Select(x => x.Class).Distinct();
            
            foreach(var @class in classes)
            {
                StringBuilder sb = new();
                var eventsForClass = events.Where(x => x.Class == @class);
                foreach (var @event in eventsForClass)
                {
                    var emoji = Emoji.Parse(@event.Type switch
                    {
                        EventType.QuizDue => "📝",
                        EventType.DiscussionDue => "📖",
                        EventType.SubmissionDue => "🗓️",
                        EventType.AvailabilityEnds => "⏰",
                        EventType.ContentAvailable => "📚",
                        EventType.Other => "🏫"
                    });

                    var subject = @event.Type switch
                    {
                        EventType.QuizDue => "Quiz Due:",
                        EventType.DiscussionDue => "Discussion Due:",
                        EventType.SubmissionDue => "Submission Due:",
                        EventType.AvailabilityEnds => "Availability Ends:",
                        EventType.ContentAvailable => "Content Available:",
                        EventType.Other => ""
                    };

                    sb.AppendLine($"{emoji} **{subject}** *{@event.Summary}* `{@event.End.ToString("hh:mm tt")}`");
                }

                builder.AddField(@class, sb.ToString());
            }

            ReplyAsync("", embed: builder.Build());
        }
    }
}