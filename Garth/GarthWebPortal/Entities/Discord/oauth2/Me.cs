namespace GarthWebPortal.Entities.Discord.oauth2;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Application
{
    public string id { get; set; }
    public string name { get; set; }
    public string icon { get; set; }
    public string description { get; set; }
    public string summary { get; set; }
    public object type { get; set; }
    public bool hook { get; set; }
    public bool bot_public { get; set; }
    public bool bot_require_code_grant { get; set; }
    public string verify_key { get; set; }
    public int flags { get; set; }
}

public class Me
{
    public Application application { get; set; }
    public List<string> scopes { get; set; }
    public DateTime expires { get; set; }
    public User user { get; set; }
}

public class User
{
    public string id { get; set; }
    public string username { get; set; }
    public string avatar { get; set; }
    public object avatar_decoration { get; set; }
    public string discriminator { get; set; }
    public int public_flags { get; set; }
}

