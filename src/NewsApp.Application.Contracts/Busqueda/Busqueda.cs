using System;
using System.Collections.Generic;
using System.Text;
using NewsApp.Articulos;

namespace NewsApp.Busqueda
{
    public class Search
    {
        public string Keyword { get; set; }
        public ICollection<ArticuloDto> Articulo { get; set; }

    }
}
