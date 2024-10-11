using Microsoft.AspNetCore.Builder.Extensions;
using OAuth2.Entities;
using OpenIddict.Abstractions;

namespace OAuth2
{
    public class AuthSeed : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        public AuthSeed(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OAuthContext>();
            await context.Database.EnsureCreatedAsync();

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            var appOptions = new AppOptions();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            configuration.GetSection("AppOptions").Bind(appOptions);

            if (await manager.FindByClientIdAsync(appOptions.ClientId) == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = appOptions.ClientId,
                    ClientSecret = appOptions.ClientSecret,
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken, // Allow refresh token grant type
                        OpenIddictConstants.Permissions.Prefixes.Scope + "api",
                    }
                }, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
