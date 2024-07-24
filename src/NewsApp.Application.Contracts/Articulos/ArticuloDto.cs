using System;
using Volo.Abp.Application.Dtos;

namespace NewsApp.Articulos;

public class ArticuloDto : AuditedEntityDto<Guid>
{
    public string? Title { get; set; } = string.Empty;

    public string? Author { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;

    public string? Url { get; set; } = string.Empty;

    public string? UrlToImage { get; set; } = string.Empty;

    public DateTime? PublishedAt { get; set; } = DateTime.Now;

    public string? Content { get; set; } = string.Empty;


}

