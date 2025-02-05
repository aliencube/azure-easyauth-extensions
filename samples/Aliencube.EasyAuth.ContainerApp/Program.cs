using Aliencube.Azure.Extensions.EasyAuth;
using Aliencube.Azure.Extensions.EasyAuth.EntraID;
using Aliencube.Azure.Extensions.EasyAuth.GitHub;
using Aliencube.EasyAuth.Components.Services;
using Aliencube.EasyAuth.ContainerApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<IRequestService, RequestService>((sp, client) =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext;
    var request = httpContext!.Request;
    var baseUrl = $"{request.Scheme}://{request.Host}";

    client.BaseAddress = new Uri(baseUrl);
});

// To use EasyAuth with EntraID, uncomment the following line
builder.Services.AddAuthentication(EasyAuthAuthenticationScheme.Name)
                .AddAzureEasyAuthHandler<EntraIDEasyAuthAuthenticationHandler>();

// To use EasyAuth with GitHub, uncomment the following line
// builder.Services.AddAuthentication(EasyAuthAuthenticationScheme.Name)
//                 .AddAzureEasyAuthHandler<GitHubEasyAuthAuthenticationHandler>();

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
