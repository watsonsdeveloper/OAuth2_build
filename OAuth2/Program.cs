using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OAuth2;
using OAuth2.Entities;
using Watsons.Common;
using Watsons.Common.ConnectionHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OAuthContext>(options =>
{
    options.UseInMemoryDatabase("AuthDb"); // Use SQL Server or any other database in production
    options.UseOpenIddict(); //options.UseOpenIddict();
});

builder.Services.AddHostedService<AuthSeed>();

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<OAuthContext>();
    })
    .AddServer(options =>
    {
        options.AllowAuthorizationCodeFlow();
        options.AllowClientCredentialsFlow();

        options.SetAuthorizationEndpointUris("/connect/authorize");
        options.SetTokenEndpointUris("/connect/token");

        //options.AddDevelopmentEncryptionCertificate()
        //       .AddDevelopmentSigningCertificate();

        options.AddSigningCertificate(AuthHelper.LoadSigningCertificateFromStore());
        options.AddEncryptionCertificate(AuthHelper.LoadCertificateFromStore());

        //options.AddEncryptionKey(new SymmetricSecurityKey(
        //    Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

        //options.AddEphemeralEncryptionKey()
        //    .AddEphemeralSigningKey();

        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough();
    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
});
builder.Services.AddAuthorization();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
