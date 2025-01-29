using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace NewsApp.Themes
{
    public class ThemeManager : DomainService
    {
        private readonly IRepository<Theme, int> _repository;
        public ThemeManager(IRepository<Theme, int> repository)
        {
            _repository = repository;
        }

        public async Task<Theme> CreateAsyncOrUpdate(int? id, string name, int? parentId, IdentityUser identityUser)
        {
            Theme theme = null;            

            if (id is not null)
            {
                // Si el id no es nulo significa que se modifica el tema
                theme = await _repository.GetAsync(id.Value, includeDetails: true);

                theme.Name = name;
            }
            else
            {
                //Si el id es nulo, es un tema nuevo
                theme = new Theme { Name = name, User = identityUser };

                if (parentId is not null)
                {
                    // Si el parent id no es nulo, es un tema hijo de un tema padre.
                    var parentTheme = await _repository.GetAsync(parentId.Value, includeDetails: true);
                    parentTheme.Themes.Add(theme);
                }               
            };

            return theme;
        }
    }
}
