using System;
using AutoMapper;
using NewsApp.News;
using NewsApp.Themes;
using NewsApp.User;
using NewsApp.Lists;
using NewsApp.Alerts;
using NewsApp.Monitoring;
using NewsApp.Domain.News;
using NewsApp.Domain.Lists;
using NewsApp.Domain.Alerts;
using NewsApp.Domain.Monitoring;
using NewsApp.Domain.News.Services;
using Volo.Abp.Identity;

namespace NewsApp;

public class NewsAppApplicationAutoMapperProfile : Profile
{
    public NewsAppApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        
        // Legacy mappings
        CreateMap<Theme, ThemeDto>();
        CreateMap<IdentityUser, UserDto>();

        // News mappings
        CreateMap<NewsArticle, NewsArticleDto>()
            .ForMember(dest => dest.CreationTime, opt => opt.MapFrom(src => src.CreationTime));
        CreateMap<CreateNewsArticleDto, NewsArticle>()
            .ForCtorParam("id", opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForCtorParam("source", opt => opt.MapFrom(src => src.Source))
            .ForCtorParam("title", opt => opt.MapFrom(src => src.Title))
            .ForCtorParam("url", opt => opt.MapFrom(src => src.Url))
            .ForCtorParam("publishedAt", opt => opt.MapFrom(src => src.PublishedAt))
            .ForCtorParam("languageCode", opt => opt.MapFrom(src => src.LanguageCode))
            .ForCtorParam("description", opt => opt.MapFrom(src => src.Description))
            .ForCtorParam("urlToImage", opt => opt.MapFrom(src => src.UrlToImage))
            .ForCtorParam("content", opt => opt.MapFrom(src => src.Content))
            .ForCtorParam("author", opt => opt.MapFrom(src => src.Author));
        CreateMap<NewsSource, NewsSourceDto>();

        // Reading Lists mappings
        CreateMap<ReadingList, ReadingListDto>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count))
            .ForMember(dest => dest.UnreadCount, opt => opt.MapFrom(src => src.GetUnreadCount()))
            .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Name : null))
            .ForMember(dest => dest.OwnerUserName, opt => opt.Ignore()); // Will be filled by application service

        CreateMap<CreateReadingListDto, ReadingList>()
            .ForCtorParam("id", opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForCtorParam("name", opt => opt.MapFrom(src => src.Name))
            .ForCtorParam("description", opt => opt.MapFrom(src => src.Description))
            .ForCtorParam("isPublic", opt => opt.MapFrom(src => src.IsPublic));

        CreateMap<ReadingListItem, ReadingListItemDto>();
        CreateMap<ReadingList, ReadingListWithItemsDto>()
            .IncludeBase<ReadingList, ReadingListDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children));

        CreateMap<ReadingList, ReadingListHierarchyDto>()
            .IncludeBase<ReadingList, ReadingListDto>()
            .ForMember(dest => dest.Level, opt => opt.Ignore()) // Will be calculated by service
            .ForMember(dest => dest.Path, opt => opt.Ignore()) // Will be calculated by service
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children));

        // Alerts mappings
        CreateMap<Alert, AlertDto>()
            .ForMember(dest => dest.NextRunTime, opt => opt.MapFrom(src => src.GetNextRunTime()))
            .ForMember(dest => dest.ListName, opt => opt.Ignore()) // Will be filled by application service
            .ForMember(dest => dest.OwnerUserName, opt => opt.Ignore()); // Will be filled by application service

        CreateMap<CreateAlertDto, Alert>()
            .ForCtorParam("id", opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForCtorParam("name", opt => opt.MapFrom(src => src.Name))
            .ForCtorParam("keywords", opt => opt.MapFrom(src => src.Keywords))
            .ForCtorParam("description", opt => opt.MapFrom(src => src.Description));

        CreateMap<AlertResult, AlertResultDto>()
            .ForMember(dest => dest.AlertName, opt => opt.MapFrom(src => src.Alert.Name))
            .ForMember(dest => dest.AlertType, opt => opt.MapFrom(src => src.Alert.AlertType))
            .ForMember(dest => dest.RunAt, opt => opt.MapFrom(src => src.CreationTime))
            .ForMember(dest => dest.FoundCount, opt => opt.MapFrom(src => src.ArticlesFound))
            .ForMember(dest => dest.Success, opt => opt.MapFrom(src => src.Status == AlertExecutionStatus.Success));

        CreateMap<Alert, AlertWithResultsDto>()
            .IncludeBase<Alert, AlertDto>()
            .ForMember(dest => dest.RecentResults, opt => opt.Ignore()); // Will be filled by application service

        // Monitoring mappings
        CreateMap<ApiCallMetric, ApiCallMetricDto>();
    }
}
