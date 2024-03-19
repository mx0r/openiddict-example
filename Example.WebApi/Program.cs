using System.Security.Cryptography.X509Certificates;
using Example.WebApi.Auth.Extensions;
using Example.WebApi.Auth.Options;
using Example.WebApi.Auth.Policies;
using Example.WebApi.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var services = builder.Services;

var authOptions = services.Configure<AuthOptions>(config, "Auth");

Console.WriteLine("*** Application is STARTING UP ***");

try
{
    #region --- Configure services ---

    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddControllersWithViews();

    // add mediatr to support handlers
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

    // add authentication
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opts =>
        {
            opts.Authority = authOptions.Authority;

            var signingCertificate = new X509Certificate2(authOptions.SigningCertificateFilePath);
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authOptions.Authority,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new X509SecurityKey(signingCertificate),
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidAudiences = [authOptions.Audience]
            };
        });

    services.AddAuthorizationBuilder()
        .AddScopeRequirementPolicy("read:weather", authOptions.Authority)
        .AddScopeRequirementPolicy("write:weather", authOptions.Authority)
        .AddScopeRequirementPolicy("read:users", authOptions.Authority);

    services.AddSingleton<IAuthorizationHandler, HasScopeRequirementHandler>();

    #endregion

    #region --- Configure application ---

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

#pragma warning disable ASP0014

// use endpoints instead of top level route registrations 
    app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });

#pragma warning restore ASP0014

    #endregion

    Console.WriteLine("*** Application is READY ***");
    app.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine("*** Application startup FAILED ***");
    Console.Error.WriteLine(ex);
}
