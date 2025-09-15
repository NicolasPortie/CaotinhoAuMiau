using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Services;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using CaotinhoAuMiau.Utils;
using CaotinhoAuMiau.Configuration;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var mvcBuilder = builder.Services.AddControllersWithViews();

if (builder.Environment.IsDevelopment())
{
    mvcBuilder = mvcBuilder.AddRazorRuntimeCompilation();
}

mvcBuilder.AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddRazorPages();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100 * 1024 * 1024;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    if (!builder.Environment.IsDevelopment())
    {
        options.EnableServiceProviderCaching();
        options.EnableSensitiveDataLogging(false);
        options.LogTo(message => { }, LogLevel.None);
    }
});

var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddDistributedMemoryCache();
}
else
{
    try
    {
        using var muxer = StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnection);
        if (muxer.IsConnected)
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "PortalAdocao_";
            });
        }
        else
        {
            builder.Services.AddDistributedMemoryCache();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao conectar com Redis: {ex.Message}");
        Console.WriteLine("Fallback: Usando MemoryCache local");
        builder.Services.AddDistributedMemoryCache();
    }
}

builder.Services.Configure<AdminConfig>(builder.Configuration.GetSection("AdminConfig"));

builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<HistoricoAdocaoService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IColaboradorService, ColaboradorService>();
builder.Services.AddScoped<ContratoService>();
builder.Services.AddScoped<AssinaturaDigitalService>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<AdocaoAutomacaoService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();

builder.Services.AddHostedService<EmailBackgroundService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("Administrador", "Colaborador", "Voluntário"));
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "CaotinhoAuMiau.Auth";
        options.LoginPath = "/autenticacao/login";
        options.LogoutPath = "/autenticacao/logout";
        options.AccessDeniedPath = "/";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        
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

                    // Evita validar o usuário a cada requisição, só revalida quando necessário
                    var validationTimestamp = context.Properties.GetString("LastValidated");
                    var isFirstValidation = string.IsNullOrEmpty(validationTimestamp);
                    var needsRevalidation = false;

                    if (!isFirstValidation && DateTime.TryParse(validationTimestamp, out var lastValidated))
                    {
                        needsRevalidation = DateTime.UtcNow.Subtract(lastValidated).TotalHours > 2;
                    }

                    if (isFirstValidation || needsRevalidation)
                    {
                        var scopeFactory = context.HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
                        using (var scope = scopeFactory.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            bool usuarioExiste = false;

                            if (int.TryParse(userId, out var userIdInt))
                            {
                                usuarioExiste = await dbContext.Usuarios.AnyAsync(u => u.Id == userIdInt && u.Ativo) ||
                                               await dbContext.Colaboradores.AnyAsync(c => c.Id == userIdInt && c.Ativo);
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

using (var scope = app.Services.CreateScope())
{
    var colaboradorService = scope.ServiceProvider.GetRequiredService<IColaboradorService>();
    await colaboradorService.CriarAdminPadraoAsync();
}

app.UseMiddleware<CaotinhoAuMiau.Middleware.GlobalErrorHandlingMiddleware>();


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

app.MapControllers();


app.MapControllerRoute(
    name: "explorarPetsComFiltros",
    pattern: "usuario/pets/explorar",
    defaults: new { controller = "Pet", action = "ExplorarPets" });



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();