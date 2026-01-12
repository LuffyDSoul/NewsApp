using System;
using Volo.Abp.Application.Dtos;

namespace NewsApp.NewsAlerts
{
    public class NewsAlertNotificationDto : CreationAuditedEntityDto<Guid>
    {
        public Guid UserId { get; set; }
        
        public Guid NewsAlertListId { get; set; }
        
        public string AlertListName { get; set; }
        
        public string Category { get; set; }
        
        public string LanguageCode { get; set; }
        
        public int NewArticlesCount { get; set; }
        
        public bool IsRead { get; set; }
        
        public bool EmailSent { get; set; }
        
        public DateTime? EmailSentAt { get; set; }
        
        public DateTime NewestArticleDate { get; set; }
    }
}
