using System.ComponentModel.DataAnnotations;

namespace NewsApp.NewsAlerts
{
    public class UpdateNewsAlertListDto
    {
        [Required]
        [StringLength(256)]
        public string Name { get; set; }
        
        [StringLength(1024)]
        public string? Description { get; set; }
        
        [StringLength(512)]
        public string? Categories { get; set; }
        
        [Required]
        [StringLength(5)]
        public string LanguageCode { get; set; }
        
        [StringLength(256)]
        public string? Keyword { get; set; }
        
        public bool IsActive { get; set; }
    }
}
