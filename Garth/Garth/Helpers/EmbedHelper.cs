using Discord;

namespace Garth.Helpers;

public class EmbedHelper : EmbedBuilder
{
    public EmbedHelper() : base()
    {
        this.WithColor(Discord.Color.LightGrey);
    }

    public EmbedHelper AsError()
    {
        this.WithColor(Discord.Color.Red);
        return this;
    }

    public EmbedHelper AsSuccess()
    {
        this.WithColor(Discord.Color.Green);
        return this;
    }

    public EmbedHelper AsWarning()
    {
        this.WithColor(new Discord.Color(235, 192, 52));
        return this;
    }

    public EmbedHelper AsInfo()
    {
        this.WithColor(new Discord.Color(36, 90, 227));
        return this;
    }

    public EmbedHelper Compact()
    {
        this.AddField($"**{this.Title}**", this.Description);
        this.Title = string.Empty;
        this.Description = string.Empty;
        return this;
    }

    public new EmbedHelper WithDescription(string msg)
    {
      return (EmbedHelper)base.WithDescription(msg);  
    }
    
    public new EmbedHelper WithTitle(string title)
    {
        return (EmbedHelper)base.WithTitle(title);  
    }
    
    public static Embed Error(string msg, string title = "Error") =>
        new EmbedHelper().AsError().WithDescription(msg).WithTitle(title).Build();
    
    public static Embed Error(Exception exception, string title = "Error") =>
        new EmbedHelper().AsError().WithDescription(exception.Message).WithTitle(title).Build();
    
    public static Embed Success(string msg, string title = "Success") =>
        new EmbedHelper().AsSuccess().WithDescription(msg).WithTitle(title).Build();
    
    public static Embed Warning(string msg, string title = "Warning") =>
        new EmbedHelper().AsWarning().WithDescription(msg).WithTitle(title).Build();
}

