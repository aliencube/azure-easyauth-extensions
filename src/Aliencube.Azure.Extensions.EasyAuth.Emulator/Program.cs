using Aliencube.Azure.Extensions.EasyAuth.Emulator.Components;
using Aliencube.Azure.Extensions.EasyAuth.Emulator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

// builder.Services.AddDistributedMemoryCache();
// builder.Services.AddSession(options =>
// {
//     options.IdleTimeout = TimeSpan.FromMinutes(30);
//     options.Cookie.HttpOnly = true;
//     options.Cookie.IsEssential = true;
// });
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IdGenerator>();
builder.Services.AddScoped<IEasyAuthService, EasyAuthService>();

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

// app.UseSession();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();
