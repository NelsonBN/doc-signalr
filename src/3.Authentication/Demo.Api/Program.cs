using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Demo.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateSlimBuilder(args);

const string SECURITY_KEY = "a550bcba6c25ce27753056c97d272c63376b9dfde0e63d76a731ce61b78d59fb5a3c4855e663c37038e431ad8901468c308e02a69a748374775c01f36aa8fe42";
const string ISSUER = "my-custom-issue";
const string AUDIENCE = "my-custom-audience";
const string TOKEN_TYPE = "at+jwt";
const string HUB_ENDPOINT = "/my-hub";


builder.Services.AddSignalR();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => true)
              .AllowCredentials()));

builder.Services
    .AddSingleton<IUserIdProvider, UserIdProvider>()
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = ISSUER,

            ValidateAudience = true,
            ValidAudience = AUDIENCE,

            RequireExpirationTime = true,
            RequireSignedTokens = true,

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SECURITY_KEY)),

            ClockSkew = TimeSpan.Zero,
            ValidTypes = [TOKEN_TYPE]
        };

        options.Events = new JwtBearerEvents()
        {
            OnMessageReceived = context =>
            {
                if(context.Request.Query.TryGetValue("access_token", out var accessToken) &&
                   context.HttpContext.Request.Path.StartsWithSegments(HUB_ENDPOINT) &&
                   context.Scheme.Name == JwtBearerDefaults.AuthenticationScheme)
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });


var app = builder.Build();

app.UseCors()
   .UseAuthentication()
   .UseAuthorization();

app.MapHub<MyHub>(HUB_ENDPOINT);


app.MapGet("/login", (string username) =>
{
    if(string.IsNullOrWhiteSpace(username))
    {
        return Results.BadRequest("Username is required");
    }

    var userId = Guid.NewGuid();

    var claims = new ClaimsIdentity(new Claim[]
    {
        new(ClaimTypes.Name, username),
        new(ClaimTypes.NameIdentifier, userId.ToString()),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    });

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SECURITY_KEY));

    var handler = new JwtSecurityTokenHandler();

    var token = handler.CreateToken(new()
    {
        Issuer = ISSUER,
        Audience = AUDIENCE,
        SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512),
        Subject = claims,
        NotBefore = DateTime.UtcNow,
        Expires = DateTime.UtcNow.AddSeconds(30),
        IssuedAt = DateTime.UtcNow,
        TokenType = TOKEN_TYPE
    });
    var accessToken = handler.WriteToken(token);

    return Results.Ok(new LoginResult(
        userId,
        accessToken,
        token.ValidTo));
});

await app.RunAsync();
