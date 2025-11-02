using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrderService.Application.Services;
using OrderService.Application.Validators;
using OrderService.Core.Interfaces;
using OrderService.Infrastructure.Context;
using OrderService.Infrastructure.Interfaces.Base;
using OrderService.Infrastructure.Interfaces.Entities;
using OrderService.Infrastructure.Repositories.Base;
using OrderService.Infrastructure.Repositories.Entities;
using OrderService.Shared.Dtos.Jwt;
using OrderService.Shared.Protos.GrpcProductService;
using OrderService.Shared.Protos.GrpcShopService;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpcClient<ProductService.ProductServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["gRPC:ProductService"]); 
});
builder.Services.AddGrpcClient<ShopService.ShopServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["gRPC:ShopService"]); 
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 5012, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });
    

    options.Listen(IPAddress.Any, 5004, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; 
    });
}); 


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:3001", "http://localhost:3000", "http://localhost:5040", "http://localhost")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("X-Total-Count", "X-Page-Number", "X-Page-Size");
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ShopCustomer", policy => policy.RequireClaim(ClaimTypes.Role, "ShopCustomer"));
    options.AddPolicy("ShopOwner", policy => policy.RequireClaim(ClaimTypes.Role, "ShopOwner"));
    options.AddPolicy("SuperAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "SuperAdmin"));
    options.AddPolicy("CustomerAndOwner", policy => policy.RequireClaim(ClaimTypes.Role, "ShopOwner", "ShopCustomer"));
    options.AddPolicy("OwnerAndSuperAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "ShopOwner", "SuperAdmin"));
});




//Jwt
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        RoleClaimType = ClaimTypes.Role,
        ValidateActor = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        RequireExpirationTime = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.HttpContext.Request.Cookies["accessToken"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = async context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                var httpContext = context.HttpContext;
                var accessToken =
                    httpContext.Request.Cookies["accessToken"];

                var refreshToken =
                    httpContext.Request.Cookies["refreshToken"];

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var refreshEndpoint =
                        $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/v1/Auth/Refresh";
                    var client = httpContext.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();

                    var response =
                        await client.PostAsJsonAsync(refreshEndpoint, new TokenDto(accessToken, refreshToken));

                    if (response.IsSuccessStatusCode)
                    {
                        var newTokens = await response.Content.ReadFromJsonAsync<RefreshDto>();
                        if (newTokens != null)
                        {
                            httpContext.Response.Cookies.Append("accessToken", newTokens.AccessToken,
                                new CookieOptions { HttpOnly = true });
                            httpContext.Response.Cookies.Append("refreshToken", newTokens.RefreshToken,
                                new CookieOptions { HttpOnly = true });

                            httpContext.Request.Headers["Authorization"] = $"Bearer {newTokens.AccessToken}";

                            var newToken = new JwtSecurityToken(newTokens.AccessToken);
                            var principal = new ClaimsPrincipal(new ClaimsIdentity(newToken.Claims, "jwt"));
                            

                            context.Principal = principal;
                            context.Success();
                        }
                    }
                }
            }
        }
    };
});


builder.Services.AddGrpc();
builder.Services.AddControllers();


//Cookie
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});

builder.Services.AddHttpClient("MyClient");

builder.Services.AddApiVersioning(options => { options.ReportApiVersions = true; }
).AddApiExplorer(
    options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddAutoMapper(cfg => { }, typeof(OrderService.Core.Profiles.OrderProfile));
builder.Services.AddAutoMapper(cfg => { }, typeof(OrderService.Core.Profiles.PaymentProfile));
builder.Services.AddAutoMapper(cfg => { }, typeof(OrderService.Core.Profiles.OrderItemProfile));

builder.Services.AddScoped<PaymentSystemValidator>();
builder.Services.AddScoped<IOrderService, OrderService.Application.Services.OrderService>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOrderConfirmationEmail, OrderConfirmationEmail>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();



builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("OrderService.Infrastructure"))
);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCookiePolicy();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<OrderService.Infrastructure.gRPC.GrpcOrderService>();
app.MapControllers();

app.Run();


 