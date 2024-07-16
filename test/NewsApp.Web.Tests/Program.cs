using Microsoft.AspNetCore.Builder;
using NewsApp;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
await builder.RunAbpModuleAsync<NewsAppWebTestModule>();

public partial class Program
{
}
