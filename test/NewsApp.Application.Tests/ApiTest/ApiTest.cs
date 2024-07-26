using NewsApp.Articulos;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NewsApp.ApiNews
{
    public class ApiTest : NewsAppApplicationTestBase
    {
        private readonly IApiNewsService _apiAppService;

        public ApiTest()
        {
            _apiAppService = GetRequiredService<IApiNewsService>();
        }

        [Fact]
        public async Task Should_Search_News()
        {
            //Arrage
            var search = "Apple";

            //Act
            var response = await _apiAppService.Search(search);

            //Assert
            response.ShouldNotBeNull();
            response.Count.ShouldBe(5);
        }

    }
}
