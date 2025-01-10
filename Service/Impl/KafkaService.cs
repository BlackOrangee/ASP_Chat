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
                AutoOffsetReset = AutoOffsetReset.Latest
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            _consumer.Subscribe("media-responses");

            ProducerConfig producerConfig = new ProducerConfig
            {
                BootstrapServers = _bootstrapServers,
                EnableIdempotence = true
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
            var timeout = TimeSpan.FromSeconds(100);
            var startTime = DateTime.UtcNow;
            Console.WriteLine("Date time utc now" + DateTime.UtcNow);

            while (DateTime.UtcNow - startTime < timeout)
            {
                try
                {
                    Console.WriteLine("Try to consume");
                    var result = _consumer.Consume(TimeSpan.FromMilliseconds(500));
                    Console.WriteLine("Consumed");

                    if (result != null && result.Message.Key == key)
                    {
                        Console.WriteLine("Result is not null");
                        Console.WriteLine("Result message timestamp: " + result.Message.Timestamp.UtcDateTime.ToString("o"));
                        Console.WriteLine("Start time: " + startTime.ToString("o"));
                        if (result.Message.Timestamp.UtcDateTime >= startTime.AddMilliseconds(-500))
                        {
                            Console.WriteLine("Commit");
                            _consumer.Commit(result);
                            return result.Message.Value;
                        }
                        Console.WriteLine("Message is too old");
                        Console.WriteLine("Result: " + result);
                    }
                    Console.WriteLine("Result is null");
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error while consuming Kafka message");
                    throw ServerExceptionFactory.KafkaException("Error consuming response");
                }

                await Task.Delay(500);
            }

            throw ServerExceptionFactory.RequestTimeout();
        }
    }
}
