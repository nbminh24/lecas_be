using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Microsoft.AspNetCore.Identity;
using be_lecas.Models;
using be_lecas.Services;
using be_lecas.Repositories;
using be_lecas.Common;
using AutoMapper;
using FluentValidation.AspNetCore;
using FluentValidation;
using System.Security.Claims;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from appsettings.json and environment variables
var appSettingsJson = Environment.GetEnvironmentVariable("APPSETTINGS_JSON");
if (!string.IsNullOrEmpty(appSettingsJson))
{
    var tempFile = Path.GetTempFileName();
    File.WriteAllText(tempFile, appSettingsJson);
    builder.Configuration.AddJsonFile(tempFile, optional: false, reloadOnChange: false);
}
else
{
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
}
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.

// Configure MongoDB
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB");
var mongoUrl = new MongoUrl(mongoConnectionString);

var settings = MongoClientSettings.FromUrl(mongoUrl);
settings.ServerApi = new ServerApi(ServerApiVersion.V1);
settings.SslSettings = new SslSettings
{
    EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12,
    CheckCertificateRevocation = false
};

builder.Services.AddSingleton<IMongoClient>(new MongoClient(settings));
builder.Services.AddTransient<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoUrl.DatabaseName ?? "lecas");
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Add FluentValidation
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LECAS Fashion API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: Authorization: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JWT Authentication
var secretKey = builder.Configuration["JWT:SecretKey"];
if (string.IsNullOrEmpty(secretKey))
    throw new InvalidOperationException("JWT:SecretKey is not configured");

var issuer = builder.Configuration["JWT:Issuer"];
if (string.IsNullOrEmpty(issuer))
    throw new InvalidOperationException("JWT:Issuer is not configured");

var audience = builder.Configuration["JWT:Audience"];
if (string.IsNullOrEmpty(audience))
    throw new InvalidOperationException("JWT:Audience is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
})
.AddGoogle(googleOptions =>
{
    var clientId = builder.Configuration["GoogleOAuth:ClientId"];
    var clientSecret = builder.Configuration["GoogleOAuth:ClientSecret"];
    
    if (!string.IsNullOrEmpty(clientId))
        googleOptions.ClientId = clientId;
    if (!string.IsNullOrEmpty(clientSecret))
        googleOptions.ClientSecret = clientSecret;
        
    googleOptions.CallbackPath = "/signin-google";
    googleOptions.AccessDeniedPath = "/AccessDenied";
});


// Add CORS
var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();
if (allowedOrigins == null)
    throw new InvalidOperationException("CORS:AllowedOrigins is not configured");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins(allowedOrigins)
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Add JWT Helper
builder.Services.AddSingleton<JwtHelper>(serviceProvider =>
{
    var secretKey = builder.Configuration["JWT:SecretKey"];
    if (string.IsNullOrEmpty(secretKey))
        throw new InvalidOperationException("JWT:SecretKey is not configured");
        
    var issuer = builder.Configuration["JWT:Issuer"];
    if (string.IsNullOrEmpty(issuer))
        throw new InvalidOperationException("JWT:Issuer is not configured");
        
    var audience = builder.Configuration["JWT:Audience"];
    if (string.IsNullOrEmpty(audience))
        throw new InvalidOperationException("JWT:Audience is not configured");
        
    return new JwtHelper(secretKey, issuer, audience);
});

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add Redis Cache (with fallback to memory cache)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "lecas_";
});

// Add Cache Service
builder.Services.AddScoped<ICacheService, CacheService>();

// Add Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

// Add custom services and repositories
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

var app = builder.Build();

// Seed test data
using (var scope = app.Services.CreateScope())
{
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
    
    // Check if test user exists
    var testUser = await userRepository.GetByEmailAsync("test@gmail.com");
    if (testUser == null)
    {
        // Create test user
        var newUser = new User
        {
            Email = "test@gmail.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await userRepository.CreateAsync(newUser);
        Console.WriteLine("Test user created: test@gmail.com / 123456");
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigins"); // Use CORS policy

// Add Response Compression
app.UseResponseCompression();

// Add custom JWT middleware for flexible token format
app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run(); 