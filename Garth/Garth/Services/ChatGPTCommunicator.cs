using System.Web;
using Garth.Helpers;
using Newtonsoft.Json;

namespace Garth.Services;

public class ChatGPTCommunicator
{
    private HttpClient APIClient = new HttpClient()
    {
        BaseAddress = new Uri("http://127.0.0.1:5666/")
    };

    private Dictionary<ulong, ChatGPTResponse> threads = new();
    private Dictionary<ulong, bool> threadStates = new();

    public async Task<ChatGPTResponse> GetResponse(string message, ulong threadId)
    {   
        if (threads.ContainsKey(threadId))
        {
            var thread = threads[threadId];
            threadStates[threadId] = true;

            var response = await APIClient.GetAsync("?message=" + HttpUtility.UrlEncode(message)+
                                                    "&conversationId=" + HttpUtility.UrlEncode(thread.conversationId) +
                                                    "&messageId=" + HttpUtility.UrlDecode(thread.messageId)
                                                    );
            var parsedResponse = JsonConvert.DeserializeObject<ChatGPTResponse>(await response.Content.ReadAsStringAsync());
            
            threads[threadId] = parsedResponse;
            threadStates[threadId] = false;

            return parsedResponse;
        }
        else
        {
            threadStates[threadId] = true;
            var response = await APIClient.GetAsync("?message=" + message);
            var parsedResponse = JsonConvert.DeserializeObject<ChatGPTResponse>(await response.Content.ReadAsStringAsync());

            threads[threadId] = parsedResponse;
            threadStates[threadId] = false;
            
            return parsedResponse;
        }
    }

    public bool isChatGPTThread(ulong id) => threads.ContainsKey(id);
    public bool isThreadBusy(ulong id) => threadStates[id];
}