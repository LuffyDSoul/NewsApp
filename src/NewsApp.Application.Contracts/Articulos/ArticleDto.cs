using System;
using Volo.Abp.Application.Dtos;

namespace NewsApp.Articulos;

public class ArticleDto : EntityDto<int>
{
    public string? Title { get; set; } ="";

    public string? Author { get; set; } = "";

    public string? Description { get; set; } = "";

    public string? Url { get; set; } = "";

    public string? UrlToImage { get; set; } = "";

    public DateTime? Date { get; set; } = DateTime.Now;

    public string? Content { get; set; } = "";


}

