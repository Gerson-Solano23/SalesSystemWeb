namespace SalesSystem.DTO
{
    /// <summary>
    /// SMTP settings. They live only on the server: the password comes from the
    /// PASSGREATEINDUSTRIES environment variable (or Email__Password) and is never sent by clients.
    /// </summary>
    public class EmailSettings
    {
        public string Host { get; set; } = "smtp-mail.outlook.com";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string NotificationEmail { get; set; } = string.Empty;
    }
}
