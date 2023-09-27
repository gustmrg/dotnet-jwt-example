using System.Security.Claims;
using System.Text;
using JwtAspNetCourse;
using JwtAspNetCourse.Extensions;
using JwtAspNetCourse.Models;
using JwtAspNetCourse.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<TokenService>();
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.PrivateKey)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
builder.Services.AddAuthorization(x =>
{
    x.AddPolicy("admin", p => p.RequireRole("admin"));
});

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", (TokenService service) =>
{
    var user = new User(
        1,
        "Gustavo Miranda",
        "email@gustavomiranda.com",
        "https://gustavomiranda.dev",
        "xywzxasd",
        new[] { "admin", "user" }
    );

    return service.Create(user);
});

app.MapGet("/restricted", (ClaimsPrincipal user) => new
{
    id = user.Id(),
    name = user.Name(),
    email = user.Email(),
    givenName = user.GivenName(),
    image = user.Image()
}).RequireAuthorization();

app.MapGet("/admin", () => "Você tem acesso!").RequireAuthorization("admin");

app.Run();
