﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewsApp.Articulos;

namespace NewsApp.ApiNews
{
    public interface IApiNewsAppService
    {
        Task<ICollection<ArticleDto>> Search(string? Search);

    }
}
