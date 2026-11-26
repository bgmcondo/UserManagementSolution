namespace UserManagement.Infrastructure.Messaging
{
    // appsettings / docker-compose'daki "RabbitMq" bölümünü temsil eder.
    // Docker'da: RabbitMq__Host=rabbitmq, kullanıcı adı ve parola .env'den gelir.
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string Host { get; init; } = "localhost";

        // RabbitMQ içindeki "sanal sunucu". Tek uygulama için varsayılan "/" yeterli.
        public string VirtualHost { get; init; } = "/";

        public string Username { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;
    }
}