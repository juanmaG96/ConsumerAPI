using ConsumerAPI.Models;

namespace ConsumerAPI.Services
{
    public interface IRabbitMQService<TClass> where TClass : ClassMessageRMQ
    {
        Task<List<TClass>> ReceiveMessage(string message);
        Task<List<TClass>> ReceiveMessageWithAcknowledgment(string message);
    }
}