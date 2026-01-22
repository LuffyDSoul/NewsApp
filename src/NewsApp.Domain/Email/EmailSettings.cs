namespace NewsApp.Email
{
    public class EmailSettings
    {
        public SmtpSettings Smtp { get; set; } = new SmtpSettings();
        public string DefaultFromAddress { get; set; } = string.Empty;
        public string DefaultFromDisplayName { get; set; } = string.Empty;
    }

    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
