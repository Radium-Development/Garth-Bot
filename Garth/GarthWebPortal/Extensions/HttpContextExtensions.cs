using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GarthWebPortal.Extensions;

public static class HttpContextExtensions
{
    public static async Task<AuthenticationScheme[]> GetExternalProvidersAsync(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var schemes = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

        return (from scheme in await schemes.GetAllSchemesAsync()
            where !string.IsNullOrEmpty(scheme.DisplayName)
            select scheme).ToArray();
    }

    public static async Task<bool> IsProviderSupportedAsync(this HttpContext context, string provider)
    {
        ArgumentNullException.ThrowIfNull(context);

        return (from scheme in await context.GetExternalProvidersAsync()
            where string.Equals(scheme.Name, provider, StringComparison.OrdinalIgnoreCase)
            select scheme).Any();
    }
    
    public static async Task<T?> Discord<T>(this HttpContext context, string route, HttpMethod method)
    {
        HttpClient client = new()
        {
            BaseAddress = new Uri("https://discord.com/")
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await context.GetTokenAsync("access_token"));
        var result = await client.SendAsync(new HttpRequestMessage(method, "api/v10/" + route));
        var content = await result.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync());
    }

    public static async Task<dynamic?> Discord(this HttpContext context, string route, HttpMethod method) => await Discord<dynamic>(context, route, method);
}