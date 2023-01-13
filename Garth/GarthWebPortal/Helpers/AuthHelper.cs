using GarthWebPortal.Entities.Discord.oauth2;
using GarthWebPortal.Extensions;
using Microsoft.AspNetCore.Authentication;

namespace GarthWebPortal.Helpers;

public class AuthHelper
{
    private HttpContext Context;
    
    public AuthHelper(IHttpContextAccessor context)
    {
        Context = context.HttpContext;
    }

    public bool IsAuthenticated => (Context.GetTokenAsync("access_token").GetAwaiter().GetResult() is not null);

    public User CurrentUser => Context.Discord<Me>("/oauth2/@me", HttpMethod.Get).GetAwaiter().GetResult().user;

    public async Task<User> GetCurrentUser()
    {
        var obj = await Context.Discord<Me>("/oauth2/@me", HttpMethod.Get);
        return obj.user;
    }
}