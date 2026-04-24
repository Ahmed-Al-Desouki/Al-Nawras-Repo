namespace Al_Nawras.Infrastructure.Notifications
{
    public sealed class EmailOptions
    {
        public string? SmtpHost { get; set; }
        public int Port { get; set; } = 587;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FromEmail { get; set; }
        public string? FromName { get; set; }
        public bool EnableSsl { get; set; } = true;
    }
}
