using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Services;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using CaotinhoAuMiau.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddRazorPages();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<NotificacaoServico>();
builder.Services.AddScoped<UsuarioServico>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "CaotinhoAuMiau.Auth";
        options.LoginPath = "/usuario/login";
        options.LogoutPath = "/usuario/logout";
        options.AccessDeniedPath = "/";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.SessionStore = new MemoryCacheTicketStore(new MemoryCache(new MemoryCacheOptions()));
        
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }
                context.Response.Headers.Append("X-Redirect-Origin", "auth-redirect");
                
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                }
                
                context.Response.Redirect("/");
                return Task.CompletedTask;
            },
            OnValidatePrincipal = async context =>
            {
                var userPrincipal = context.Principal;
                if (userPrincipal?.Identity?.IsAuthenticated == true)
                {
                    var userId = userPrincipal.ObterIdUsuario();
                    var userRole = userPrincipal.ObterValorClaim(ClaimTypes.Role);
                    
                    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        return;
                    }

                    var validationTimestamp = context.Properties.GetString("LastValidated");
                    var shouldValidate = string.IsNullOrEmpty(validationTimestamp) || 
                        (DateTime.TryParse(validationTimestamp, out var lastValidated) && 
                         DateTime.UtcNow.Subtract(lastValidated).TotalMinutes > 5);

                    if (shouldValidate)
                    {
                        var scopeFactory = context.HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
                        using (var scope = scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            bool usuarioExiste = false;

                            if (userRole == "Administrador")
                            {
                                usuarioExiste = await dbContext.Colaboradores.AnyAsync(c => c.Id.ToString() == userId);
                            }
                            else
                            {
                                usuarioExiste = await dbContext.Usuarios.AnyAsync(u => u.Id.ToString() == userId);
                            }

                            if (!usuarioExiste)
                            {
                                context.RejectPrincipal();
                                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                                return;
                            }
                            
                            context.Properties.SetString("LastValidated", DateTime.UtcNow.ToString("o"));
                            context.ShouldRenew = true;
                        }
                    }
                }
            }
        };
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Erro");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    if (context != null)
    {
        context.Request.EnableBuffering();
        var feature = context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();
        if (feature != null)
        {
            feature.MaxRequestBodySize = 100 * 1024 * 1024;
        }
    }
    
    await next.Invoke();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "explorarPetsComFiltros",
    pattern: "usuario/pets/explorar",
    defaults: new { controller = "Pet", action = "ExplorarPets" });



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


public class MemoryCacheTicketStore : ITicketStore
{
    private const string KeyPrefix = "AuthTicket_";
    private readonly IMemoryCache _cache;

    public MemoryCacheTicketStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = $"{KeyPrefix}{Guid.NewGuid()}";
        _cache.Set(key, ticket, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ticket.Properties.ExpiresUtc - DateTimeOffset.UtcNow
        });
        return Task.FromResult(key);
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        _cache.Set(key, ticket, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ticket.Properties.ExpiresUtc - DateTimeOffset.UtcNow
        });
        return Task.CompletedTask;
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        _cache.TryGetValue(key, out AuthenticationTicket? ticket);
        return Task.FromResult(ticket);
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}