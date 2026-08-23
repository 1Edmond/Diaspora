using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Shared.Security.Abstractions;
using Shared.Security.Handlers;
using Shared.Security.Models;
using Shared.Security.Persistence.Repository;
using Shared.Security.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine;
using Wolverine.Transports.SharedMemory;

namespace Shared.Security;

public static class DependencyInjection
{

    public static void AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {


        var vaultUrl = configuration["Vault:Url"]!;
        var vaultToken = configuration["Vault:Token"]!;
        var vaultKeyName = configuration["Vault:KeyName"]!;


        services.AddScoped<IHashingService, HashingService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IEncryptionMetadataRepository, EncryptionMetadataRepository>();
        services.AddScoped<IDataEncryptionKeyRepository, DataEncryptionKeyRepository>();
        services.AddSingleton<IVaultTokenProvider, FileVaultTokenProvider>();
        services.AddScoped<IKeyManagementService, VaultKeyManagementService>();
       

        services.AddTransient<VaultAuthenticationHandler>();
        services.AddHttpClient("VaultHttpClient", client =>
        {
            client.BaseAddress = new Uri(vaultUrl);

        }).AddHttpMessageHandler<VaultAuthenticationHandler>();
        services.AddTransient<IVaultTransitService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var logger = sp.GetRequiredService<ILogger<VaultTransitService>>();
            var httpClient = httpClientFactory.CreateClient("VaultHttpClient");

            return new VaultTransitService(httpClient, logger);
        });

        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings not configured");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services
            .AddAuthentication("Gateway")
            .AddScheme<AuthenticationSchemeOptions, GatewayAuthenticationHandler>(
                "Gateway",
                options => { });

        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder("Gateway")
                .RequireAuthenticatedUser()
                .Build())
            .SetFallbackPolicy(new AuthorizationPolicyBuilder("Gateway")
                .RequireAuthenticatedUser()
                .Build());

    }


    public static void UseSecurity(this IApplicationBuilder app)
    {
        
        app.UseAuthentication();
        app.UseAuthorization();

    }

  
}
