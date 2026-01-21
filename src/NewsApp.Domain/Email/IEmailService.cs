using System.Threading.Tasks;

namespace NewsApp.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isHtml = true);

        Task SendNewsNotificationAsync(
            string to,
            string userName,
            string themeName,
            System.Collections.Generic.ICollection<NewsApp.News.ArticleDto> articles);
    }
}
