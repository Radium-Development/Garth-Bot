using Newtonsoft.Json;

namespace ChatGPTCommunicator.Extensions;

public static class HttpClientExtensions
{
    public static async Task<T?> GetAsync<T>(this HttpClient client, string requestUri) =>
        JsonConvert.DeserializeObject<T>(await client.GetStringAsync(requestUri));

    public static async Task<T?> GetAsync<T>(this HttpClient client, HttpRequestMessage request)
    {
        var res = await client.SendAsync(request);
        var req = await res.RequestMessage.Content.ReadAsStringAsync();
        var content = await res.Content.ReadAsStringAsync();
        var convert = JsonConvert.DeserializeObject<T>(content);
        return convert;
    }
}