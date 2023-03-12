#define AUTOCAST_ENABLE

using ChatGPTCommunicator.Models;

namespace ChatGPTCommunicator.Requests.Completion;

public class CompletionRequestBuilder : Builder<CompletionRequestBuilder, CompletionRequest>
{
    public CompletionRequestBuilder WithModel(string model) => Do(() =>
    {
        Instance.Model = model;
    });
    
    public CompletionRequestBuilder WithMessages(List<Message> messages) => Do(() =>
    {
        Instance.Messages = messages;
    });

    public CompletionRequestBuilder AddMessage(MessageRole role, String content) => Do(() =>
    {
        Instance.Messages.Add(new(role, content));
    });
    
    public CompletionRequestBuilder AddMessage(Message message) => Do(() =>
    {
        Instance.Messages.Add(message);
    });
}