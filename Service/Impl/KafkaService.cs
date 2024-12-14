using System.Text.Json;
using ASP_Chat.Exceptions;
using ASP_Chat.Service.Requests;
using Confluent.Kafka;

namespace ASP_Chat.Service.Impl
{
    public class KafkaService : IKafkaService
    {
        private readonly ILogger<KafkaService> _logger;
        private readonly string _bootstrapServers;
        private readonly string _topic;

        public KafkaService(ILogger<KafkaService> logger, string bootstrapServers)
        {
            _logger = logger;
            _bootstrapServers = bootstrapServers;
            _topic = "media-requests";
        }

        public async Task SendMessageAsync(FileRequest fileRequest)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _bootstrapServers
            };

            using var producer = new ProducerBuilder<string, string>(config).Build();

            var messageValue = JsonSerializer.Serialize(fileRequest);

            try
            {
                var deliveryResult = await producer.ProduceAsync(
                    _topic,
                    new Message<string, string>
                    {
                        Key = fileRequest.Operation,
                        Value = messageValue
                    });

                _logger.LogDebug("Message sent to topic '{_topic}' with key '{Operation}' and value '{MessageValue}'",
                                    _topic, fileRequest.Operation, messageValue);
            }
            catch (ProduceException<string, string> ex)
            {
                throw ServerExceptionFactory.KafkaException(ex.Message);
            }
        }

        public async Task<string> WaitForResponseAsync(string key, string responseTopic)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "response-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(responseTopic);

            var startTime = DateTime.Now;
            while (DateTime.Now.Subtract(startTime).TotalSeconds < 60)
            {
                var result = consumer.Consume();
                if (result.Message.Key == key)
                {
                    return result.Message.Value;
                }
                await Task.Delay(1000);
            }

            throw ServerExceptionFactory.RequestTimeout();
        }
    }
}
