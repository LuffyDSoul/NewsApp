using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsApp.ApiNews
{
    public class NewsResponse
    {
        public string Status { get; set; }
        public int TotalResults { get; set; }
        public ICollection<ArticuloDto> Articulos { get; set; }

    }
    public class ArticuloDto
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public string UrlToImage { get; set; }

        public DateTime? PublishedAt { get; set; }
        public string Content { get; set; }

    }
}
