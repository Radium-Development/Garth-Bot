using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using ChatGPTCommunicator.Extensions;
using ChatGPTCommunicator.Models;
using ChatGPTCommunicator.Requests.Completion;
using Newtonsoft.Json;

namespace ChatGPTCommunicator;

public class ChatGPT
{
    private readonly HttpClient _client;

    public ChatGPT(string OpenAIKey)
    {
        _client = new();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Authorization = new("Bearer", OpenAIKey);
    }

    public async Task<ChatGptResponse?> SendAsync(CompletionRequest request)
    {
        var response = await _client.GetAsync<ChatGptResponse>(new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://api.openai.com/v1/chat/completions"),
            Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")
        });
        return response;
    }
}