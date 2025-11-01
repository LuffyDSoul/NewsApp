using Microsoft.EntityFrameworkCore;
using NewsApp.Themes;
using NewsApp.ReadingLists;
using NewsApp.Domain.UserProfile;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace NewsApp.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class NewsAppDbContext :
    AbpDbContext<NewsAppDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    #region Entidades de dominio

    public DbSet<Theme> Themes { get; set; }
    public DbSet<ReadingList> ReadingLists { get; set; }
    public DbSet<SavedArticle> SavedArticles { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }

    #endregion

    public NewsAppDbContext(DbContextOptions<NewsAppDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(NewsAppConsts.DbTablePrefix + "YourEntities", NewsAppConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});

        // Entidad Theme
        builder.Entity<Theme>(b =>
        {
            b.ToTable(NewsAppConsts.DbTablePrefix + "Themes", NewsAppConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);            
        });

        // Reading Lists
        builder.Entity<ReadingList>(b =>
        {
            b.ToTable(NewsAppConsts.DbTablePrefix + "ReadingLists", NewsAppConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(1024);
            b.Property(x => x.Color).HasMaxLength(50);
            
            // Relationship with User
            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for user queries
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
        });

        // Saved Articles
        builder.Entity<SavedArticle>(b =>
        {
            b.ToTable(NewsAppConsts.DbTablePrefix + "SavedArticles", NewsAppConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.Property(x => x.Source).HasMaxLength(256);
            b.Property(x => x.Title).IsRequired().HasMaxLength(512);
            b.Property(x => x.Description).HasMaxLength(1024);
            b.Property(x => x.Url).IsRequired().HasMaxLength(2048);
            b.Property(x => x.UrlToImage).HasMaxLength(2048);
            b.Property(x => x.LanguageCode).HasMaxLength(5);
            b.Property(x => x.Author).HasMaxLength(256);
            b.Property(x => x.Tags).HasMaxLength(1024);
            
            // Relationship with User
            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship with ReadingList
            b.HasOne(x => x.ReadingList)
                .WithMany(x => x.SavedArticles)
                .HasForeignKey(x => x.ReadingListId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes for performance
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.ReadingListId);
            // Allow same URL in different lists for the same user
            b.HasIndex(x => new { x.UserId, x.Url, x.ReadingListId }).IsUnique();
        });

        // User Preferences
        builder.Entity<UserPreferences>(b =>
        {
            b.ToTable(NewsAppConsts.DbTablePrefix + "UserPreferences", NewsAppConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.Property(x => x.NewsLanguageCode).IsRequired().HasMaxLength(10);
            b.Property(x => x.NewsLanguageName).IsRequired().HasMaxLength(50);
            b.Property(x => x.TimeZone).HasMaxLength(100);
            b.Property(x => x.Theme).HasMaxLength(20);
            
            // Index for user queries
            b.HasIndex(x => x.UserId).IsUnique();
        });
    }
}
