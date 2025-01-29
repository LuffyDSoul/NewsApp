using System;
using System.Collections.Generic;
using System.Text;

namespace NewsApp.News
{    
    /// <summary>
    /// Este dto representa la respuesta de la api de noticias. Lo pongo en este proyecto ya que son diferentes a los dtos de application
    /// </summary>
    public class ArticleDto
    {        
        public string Author { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public string UrlToImage { get; set; }

        public DateTime? PublishedAt { get; set; }

        public string Content { get; set; }
    }
}
