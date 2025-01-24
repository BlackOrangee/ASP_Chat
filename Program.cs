using ASP_Chat;
using ASP_Chat.Entity;
using ASP_Chat.Exceptions;
using ASP_Chat.Hubs;
using ASP_Chat.Service;
using ASP_Chat.Service.Impl;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.Text;

DotNetEnv.Env.Load();

string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
string? secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");

if (string.IsNullOrEmpty(connectionString))
{
    throw ServerExceptionFactory.DBNotSet();
}

if (string.IsNullOrEmpty(secretKey))
{
    throw ServerExceptionFactory.SecretKeyNotSet();
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)), ServiceLifetime.Scoped);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();

builder.Services.AddSignalR(options =>
{
    options.AddFilter<HubServerExceptionMiddleware>();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterType<AuthService>().As<IAuthService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<ChatService>().As<IChatService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<MessageService>().As<IMessageService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<JwtService>().As<IJwtService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<PasswordHasher<User>>().As<IPasswordHasher<User>>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<CommunicationService>().As<ICommunicationService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<MediaService>().As<IMediaService>().InstancePerLifetimeScope();

    containerBuilder.RegisterType<ChatHub>().InstancePerLifetimeScope();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };


    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer ", "");

            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

using (var connection = new MySqlConnection(connectionString))
{
    connection.Open();
    using (var command = new MySqlCommand("SELECT 1", connection))
    {
        command.ExecuteScalar();
    }
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
    try
    {
        dbContext.Database.CanConnect();
        Console.WriteLine("Connected to database");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Could not connect to database. Message: {ex.Message}");
    }
}

app.UseCors("AllowSpecificOrigin");

app.UseMiddleware<ServerExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");
app.MapControllers();

app.Run();
