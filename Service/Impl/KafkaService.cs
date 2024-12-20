using ASP_Chat.Exceptions;
using ASP_Chat.Service.Requests;
using Confluent.Kafka;
using System.Text.Json;

namespace ASP_Chat.Service.Impl
{
    public class KafkaService : IKafkaService
    {
        private readonly ILogger<KafkaService> _logger;
        private readonly string _bootstrapServers;
        private readonly string _topic;
        private readonly IConsumer<string, string> _consumer;
        private readonly IProducer<string, string> _producer;

        public KafkaService(ILogger<KafkaService> logger, string bootstrapServers)
        {
            _logger = logger;
            _bootstrapServers = bootstrapServers;
            _topic = "media-requests";

            ConsumerConfig consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "response-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

            ProducerConfig producerConfig = new ProducerConfig
            {
                BootstrapServers = _bootstrapServers
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        public async Task SendMessageAsync(FileRequest fileRequest)
        {
            var messageValue = JsonSerializer.Serialize(fileRequest);

            try
            {
                var deliveryResult = await _producer.ProduceAsync(
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
            _consumer.Subscribe(responseTopic);

            var startTime = DateTime.Now;
            while (DateTime.Now.Subtract(startTime).TotalSeconds < 1000)
            {
                var result = _consumer.Consume();
                if (result.Message.Key == key 
                    && result.Message.Timestamp.UtcDateTime.Subtract(startTime).TotalSeconds < 1000)
                {
                    _consumer.Commit(result);
                    return result.Message.Value;
                }
                await Task.Delay(500);
            }

            throw ServerExceptionFactory.RequestTimeout();
        }
    }
}
