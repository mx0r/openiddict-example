#define SEED_APPLICATIONS

using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using Example.AuthServer.Api.Auth.Extensions;
using Example.AuthServer.Api.Auth.Policies;
using Example.AuthServer.Domain;
using Example.AuthServer.Domain.DataSources;
using Example.AuthServer.Domain.DataSources.Impl;
using Example.AuthServer.Domain.Seeders;
using Example.AuthServer.Extensions;
using Example.AuthServer.OpenIddict.Extensions;
using Example.AuthServer.OpenIddict.Managers;
using Example.AuthServer.Options;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;
var services = builder.Services;

var serverOptions = services.Configure<ServerOptions>(config, "Server");

services.AddDbContext<ServerDbContext>(options =>
{
    options.UseSqlite(config.GetConnectionString("Server"));
    options.UsePartnerAwareOpenIddict();
});

services.AddAuthorizationBuilder()
    .AddScopeRequirementPolicy("admin:auth", serverOptions.Issuer);

services.AddSingleton<IAuthorizationHandler, HasScopeRequirementHandler>();

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddControllersWithViews();
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

services.AddSingleton<IPartnerDataSource, MockPartnerDataSource>();
services.AddSingleton<ICustomerDataSource, MockCustomerDataSource>();

services.AddOpenIddict()
    .AddCore(opts =>
    {
        opts.AddApplicationStore<ExampleOpenIdApplicationStore>();
        opts.ReplaceApplicationManager<ExampleOpenIdApplicationManager>();

        opts.UseEntityFrameworkCore()
            .UseDbContext<ServerDbContext>()
            .UsePartnerAwareEntities();
    })
    .AddServer(opts =>
    {
        opts.AllowClientCredentialsFlow();
        opts.AllowAuthorizationCodeFlow();
        opts.AllowPasswordFlow();
        opts.AllowRefreshTokenFlow();

        opts.SetIssuer(serverOptions.Issuer);

        opts.SetAuthorizationEndpointUris("/oauth/authorize")
            .SetTokenEndpointUris("/oauth/token");

        var encryptionCertificate = new X509Certificate2(serverOptions.EncryptionCertificateFilePath);
        opts.AddEncryptionCertificate(encryptionCertificate);

        var signingCertificate = new X509Certificate2(serverOptions.SigningCertificateFilePath);
        opts.AddSigningCertificate(signingCertificate);

        // allow the token to be readable without encryption
        opts.DisableAccessTokenEncryption();
        
        // disable scope parameter check
        opts.RemoveEventHandler(OpenIddictServerHandlers.Exchange.ValidateScopeParameter.Descriptor);

        // initialize the token endpoint pass-through
        opts.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableTokenEndpointPassthrough();
    });

#if SEED_APPLICATIONS

// use application data seeder to seed the database with POC applications
services.AddHostedService<ApplicationSeederService>();

#endif

// add authentication
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
        opts =>
        {
            opts.Authority = serverOptions.Issuer;

            var signingCertificate = new X509Certificate2(serverOptions.SigningCertificateFilePath);
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = serverOptions.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new X509SecurityKey(signingCertificate),
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidAudiences = ["auth-server"]
            };
        })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
        opts =>
        {
            opts.LoginPath = "/account/login";
            opts.ExpireTimeSpan = TimeSpan.FromDays(30);
            opts.SlidingExpiration = true;
        });

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Strict });

#pragma warning disable ASP0014

// use endpoints instead of top level route registrations 
app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });

#pragma warning restore ASP0014

app.Run();
