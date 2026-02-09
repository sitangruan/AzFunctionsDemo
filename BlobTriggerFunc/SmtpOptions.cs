namespace BlobTriggerFunc
{
    public class SmtpOptions
    {
        public const string SectionName = "SmtpSettings";
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string? User { get; set; }
        public string? Pass { get; set; }
        public string From { get; set; } = "noreply@example.test";
        public bool UseSsl { get; set; } = true;
        public string DefaultRecipient { get; set; } = "admin@example.test";
    }
}
