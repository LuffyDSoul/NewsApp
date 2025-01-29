using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NewsApp.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace NewsApp.Themes
{
    [Authorize]
    public class ThemeAppService : NewsAppAppService, IThemeAppService 
    {
        private readonly IRepository<Theme, int> _repository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ThemeManager _themeManager;

        public ThemeAppService(IRepository<Theme, int> repository, UserManager<IdentityUser> userManager, ThemeManager themeManager)            
        {
            _repository = repository;
            _userManager = userManager;
            _themeManager = themeManager;
        }

        public async Task<ICollection<ThemeDto>> GetThemesAsync()
        {
            var themes = await _repository.GetListAsync(includeDetails:true);

            return ObjectMapper.Map<ICollection<Theme>, ICollection<ThemeDto>>(themes);
        }

        public async Task<ThemeDto> GetThemesAsync(int id)
        {            
            var queryable = await _repository.WithDetailsAsync(x => x.Themes);

            var query = queryable.Where(x => x.Id == id);

            var theme = await AsyncExecuter.FirstOrDefaultAsync(query);

            return ObjectMapper.Map<Theme, ThemeDto>(theme);
            
        }    
        
        public async Task<ThemeDto> CreateAsync(CretateThemeDto input)
        {                        
            var userGuid = CurrentUser.Id.GetValueOrDefault();

            var identityUser = await _userManager.FindByIdAsync(userGuid.ToString());

            var theme = await _themeManager.CreateAsyncOrUpdate(input.Id, input.Name, input.ParentId, identityUser);

            if (input.Id is null)
            {
                theme = await _repository.InsertAsync(theme, autoSave: true);
            }
            else
            {
                await _repository.UpdateAsync(theme, autoSave: true);
            }            
            
            return ObjectMapper.Map<Theme, ThemeDto>(theme);
        }
    }
}
