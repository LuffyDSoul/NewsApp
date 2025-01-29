using NewsApp.User;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace NewsApp.Themes
{ 
    public class ThemeDto : EntityDto<int>
    {
        public string Name { get; set; }
        public UserDto User { get; set; }
        public ICollection<ThemeDto> Themes { get; set; }
    }
}
