namespace Lib.MessageBus.Kafka.Settings
{
    public abstract class BaseSetting
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();


        public string Topic { get; set; }

        public string BootstrapServers { get; set; }

        public int? MessageMaxBytes { get; set; }

        public int? ReceiveMessageMaxBytes { get; set; }

        public int? MessageCopyMaxBytes { get; set; }

        public int? MaxInFlight { get; set; }

        public int? MetadataMaxAgeMs { get; set; }

        public int? ConnectionsMaxIdleMs { get; set; }

        public int? ReconnectBackoffMaxMs { get; set; }

        public int? ReconnectBackoffMs { get; set; }

        public int? SocketConnectionSetupTimeoutMs { get; set; }

        public bool? SocketKeepaliveEnable { get; set; }

        public int? SocketMaxFails { get; set; }

        public bool? SocketNagleDisable { get; set; }

        public int? SocketReceiveBufferBytes { get; set; }

        public int? SocketSendBufferBytes { get; set; }

        public int? SocketTimeoutMs { get; set; }

        public string Debug { get; set; }

        public int? ApiVersionFallbackMs { get; set; }

        public bool? AllowAutoCreateTopics { get; set; }

        public bool? ApiVersionRequest { get; set; }

        public int? ApiVersionRequestTimeoutMs { get; set; }
    }

}
