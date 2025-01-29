using NewsApp.EntityFrameworkCore;
using NewsApp.Themes;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow;
using Xunit;

namespace NewsApp.Theme
{
    public class ThemeAppService_Test : NewsAppApplicationTestBase
    {
        private readonly IThemeAppService _themeAppService;
        private readonly IDbContextProvider<NewsAppDbContext> _dbContextProvider;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ThemeAppService_Test()
        {
            _themeAppService = GetRequiredService<IThemeAppService>();
            _dbContextProvider = GetRequiredService<IDbContextProvider<NewsAppDbContext>>();
            _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
        }

        [Fact]
        public async Task Should_Get_All_Themes()
        {

            //Act
            var themes = await _themeAppService.GetThemesAsync();

            //Assert
            themes.ShouldNotBeNull();
            themes.Count.ShouldBeGreaterThan(1);
        }

        [Fact]
        public async Task Should_Create_Theme()
        {
            //Arrange            
            var input = new CretateThemeDto { Name = "nuevo tema" };

            //Act
            var newTheme = await _themeAppService.CreateAsync(input);

            //Assert
            // Se verifican los datos devueltos por el servicio
            newTheme.ShouldNotBeNull();
            newTheme.Id.ShouldBePositive();
            // se verifican los datos persistidos por el servicio
            using (var uow = _unitOfWorkManager.Begin())
            {
                var dbContext = await _dbContextProvider.GetDbContextAsync();
                dbContext.Themes.FirstOrDefault(t=> t.Id == newTheme.Id).ShouldNotBeNull();
                dbContext.Themes.FirstOrDefault(t=> t.Id == newTheme.Id).Name.ShouldBe(input.Name);
            }
        }

        [Fact]
        public async Task Should_Update_Theme()
        {
            //Arrange            
            var input = new CretateThemeDto { Name = "nuevo tema", Id = 1 };

            //Act
            var newTheme = await _themeAppService.CreateAsync(input);

            //Assert
            // Se verifican los datos devueltos por el servicio
            newTheme.ShouldNotBeNull();
            newTheme.Id.ShouldBePositive();
            // se verifican los datos persistidos por el servicio
            using (var uow = _unitOfWorkManager.Begin())
            {
                var dbContext = await _dbContextProvider.GetDbContextAsync();
                dbContext.Themes.FirstOrDefault(t => t.Id == newTheme.Id).ShouldNotBeNull();
                dbContext.Themes.FirstOrDefault(t => t.Id == newTheme.Id).Name.ShouldBe(input.Name);
            }
        }

        [Fact]
        public async Task Should_Create_Child_Theme()
        {
            //Arrange            
            var input = new CretateThemeDto { Name = "nuevo tema hijo", ParentId = 1 };

            //Act
            var newTheme = await _themeAppService.CreateAsync(input);

            //Assert
            // Se verifican los datos devueltos por el servicio
            newTheme.ShouldNotBeNull();
            newTheme.Id.ShouldBePositive();
            // se verifican los datos persistidos por el servicio
            using (var uow = _unitOfWorkManager.Begin())
            {
                var dbContext = await _dbContextProvider.GetDbContextAsync();
                dbContext.Themes.FirstOrDefault(t => t.Id == newTheme.Id).ShouldNotBeNull();
                dbContext.Themes.FirstOrDefault(t => t.Id == newTheme.Id).Name.ShouldBe(input.Name);
            }
        }
    }
}
