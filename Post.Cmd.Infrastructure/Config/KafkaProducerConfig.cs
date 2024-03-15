namespace Post.Cmd.Infrastructure.Config;

public class KafkaProducerConfig
{
    public string BootstrapServers { get; set; }
    public int Acks { get; set; }
    public int RetryBackoffMs { get; set; }
    public int LingerMs { get; set; }
    public int MaxInFlight { get; set; }
    public int BatchNumMessages { get; set; }
    public int MessageSendMaxRetries { get; set; }
    public int MessageTimeoutMs { get; set; }
    public int QueueBufferingMaxMessages { get; set; }
    public bool EnableIdempotence { get; set; }
}
