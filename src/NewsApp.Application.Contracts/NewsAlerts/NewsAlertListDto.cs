using System;
using Volo.Abp.Application.Dtos;

namespace NewsApp.NewsAlerts
{
    public class NewsAlertListDto : FullAuditedEntityDto<Guid>
    {
        public Guid UserId { get; set; }
        
        public string Name { get; set; }
        
        public string? Description { get; set; }
        
        public string Categories { get; set; }
        
        public string LanguageCode { get; set; }
        
        public string? Keyword { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime? LastCheckedAt { get; set; }
        
        public DateTime? LastNewsFoundAt { get; set; }
    }
}
