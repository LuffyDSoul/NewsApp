using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace NewsApp.Email
{
    public class EmailService : IEmailService, ITransientDependency
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                _logger.LogInformation("=== EMAIL SERVICE: Starting email send process ===");
                _logger.LogInformation("EMAIL SERVICE: Recipient (TO): {To}", to);
                _logger.LogInformation("EMAIL SERVICE: Sender (FROM): {From}", _emailSettings.DefaultFromAddress);
                _logger.LogInformation("EMAIL SERVICE: Subject: {Subject}", subject);
                _logger.LogDebug("SMTP Settings - Host: {Host}, Port: {Port}, EnableSSL: {EnableSSL}, User: {User}", 
                    _emailSettings.Smtp.Host, 
                    _emailSettings.Smtp.Port, 
                    _emailSettings.Smtp.EnableSsl,
                    _emailSettings.Smtp.UserName);

                if (string.IsNullOrWhiteSpace(to))
                {
                    _logger.LogError("EMAIL SERVICE: Cannot send email - recipient address is null or empty");
                    throw new ArgumentException("Recipient email address cannot be null or empty", nameof(to));
                }

                if (!to.Contains("@"))
                {
                    _logger.LogError("EMAIL SERVICE: Invalid recipient email format: {To}", to);
                    throw new ArgumentException($"Invalid email format: {to}", nameof(to));
                }

                using var smtpClient = new SmtpClient(_emailSettings.Smtp.Host, _emailSettings.Smtp.Port)
                {
                    EnableSsl = _emailSettings.Smtp.EnableSsl,
                    UseDefaultCredentials = _emailSettings.Smtp.UseDefaultCredentials,
                    Credentials = new NetworkCredential(
                        _emailSettings.Smtp.UserName,
                        _emailSettings.Smtp.Password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        _emailSettings.DefaultFromAddress,
                        _emailSettings.DefaultFromDisplayName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                _logger.LogInformation("EMAIL SERVICE: Adding recipient {To} to mail message", to);
                mailMessage.To.Add(to);
                
                _logger.LogInformation("EMAIL SERVICE: Mail message configured - From: {From}, To: {To}, ToCount: {ToCount}", 
                    mailMessage.From.Address, 
                    string.Join(", ", mailMessage.To.Select(t => t.Address)),
                    mailMessage.To.Count);

                _logger.LogInformation("EMAIL SERVICE: Sending email via SMTP...");
                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("=== EMAIL SERVICE: Email sent successfully to {To} ===", to);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP Error sending email to {To}. StatusCode: {StatusCode}", to, smtpEx.StatusCode);
                throw new Exception($"Error SMTP al enviar email: {smtpEx.Message}", smtpEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} with subject: {Subject}. Error: {ErrorMessage}", to, subject, ex.Message);
                throw new Exception($"Error al enviar email: {ex.Message}", ex);
            }
        }

        public async Task SendNewsNotificationAsync(
            string to,
            string userName,
            string themeName,
            ICollection<News.ArticleDto> articles)
        {
            var subject = $"NewsApp: Nuevas noticias sobre '{themeName}'";
            var body = BuildNewsNotificationHtml(userName, themeName, articles);

            await SendEmailAsync(to, subject, body, true);
        }

        private string BuildNewsNotificationHtml(
            string userName,
            string themeName,
            ICollection<News.ArticleDto> articles)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            sb.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            sb.AppendLine(".header { background-color: #4CAF50; color: white; padding: 20px; text-align: center; }");
            sb.AppendLine(".article { border: 1px solid #ddd; margin: 15px 0; padding: 15px; border-radius: 5px; }");
            sb.AppendLine(".article h3 { margin-top: 0; color: #2196F3; }");
            sb.AppendLine(".article img { max-width: 100%; height: auto; border-radius: 5px; }");
            sb.AppendLine(".article-meta { color: #666; font-size: 0.9em; margin: 10px 0; }");
            sb.AppendLine(".read-more { display: inline-block; margin-top: 10px; padding: 8px 16px; background-color: #2196F3; color: white; text-decoration: none; border-radius: 3px; }");
            sb.AppendLine(".footer { margin-top: 30px; padding: 20px; background-color: #f5f5f5; text-align: center; font-size: 0.9em; color: #666; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            
            sb.AppendLine("<div class='header'>");
            sb.AppendLine($"<h1>?? NewsApp</h1>");
            sb.AppendLine($"<p>Nuevas noticias sobre: {themeName}</p>");
            sb.AppendLine("</div>");
            
            sb.AppendLine($"<p>Hola {userName},</p>");
            sb.AppendLine($"<p>Hemos encontrado <strong>{articles.Count}</strong> nueva(s) noticia(s) sobre tu tema de interés: <strong>{themeName}</strong></p>");

            foreach (var article in articles.Take(5)) // Limitar a 5 artículos por email
            {
                sb.AppendLine("<div class='article'>");
                sb.AppendLine($"<h3>{article.Title}</h3>");
                
                if (!string.IsNullOrEmpty(article.UrlToImage))
                {
                    sb.AppendLine($"<img src='{article.UrlToImage}' alt='Article image' />");
                }
                
                sb.AppendLine($"<div class='article-meta'>");
                sb.AppendLine($"Por: {article.Author} | Publicado: {article.PublishedAt:dd/MM/yyyy HH:mm}");
                sb.AppendLine("</div>");
                
                sb.AppendLine($"<p>{article.Description}</p>");
                sb.AppendLine($"<a href='{article.Url}' class='read-more' target='_blank'>Leer más</a>");
                sb.AppendLine("</div>");
            }

            if (articles.Count > 5)
            {
                sb.AppendLine($"<p style='text-align: center; color: #666;'>... y {articles.Count - 5} noticia(s) más</p>");
            }

            sb.AppendLine("<div class='footer'>");
            sb.AppendLine("<p>Este es un email automático de NewsApp.</p>");
            sb.AppendLine("<p>Has recibido este email porque tienes activadas las notificaciones para tus temas de interés.</p>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}
