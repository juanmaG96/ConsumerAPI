using System;

namespace ConsumerAPI.Models
{
    [Serializable]
    public class ClassMessageRMQ
    {
        public string? QueueName { get; set; }
    }
}