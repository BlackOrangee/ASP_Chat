using System.Text;
using ASP_Chat;
using ASP_Chat.Entity;
using ASP_Chat.Exceptions;
using ASP_Chat.Service;
using ASP_Chat.Service.Impl;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

DotNetEnv.Env.Load();

string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
string? bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS");

string? secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");

if (string.IsNullOrEmpty(connectionString))
{
    throw ServerExceptionFactory.DBNotSet();
}

if (string.IsNullOrEmpty(bootstrapServers))
{
    throw ServerExceptionFactory.KafkaNotSet();
}

if (string.IsNullOrEmpty(secretKey))
{
    throw ServerExceptionFactory.SecretKeyNotSet();
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddControllers();

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterType<AuthService>().As<IAuthService>().SingleInstance();
    containerBuilder.RegisterType<UserService>().As<IUserService>().SingleInstance();
    containerBuilder.RegisterType<ChatService>().As<IChatService>().SingleInstance();
    containerBuilder.RegisterType<MessageService>().As<IMessageService>().SingleInstance();
    containerBuilder.RegisterType<JwtService>().As<IJwtService>().SingleInstance();
    containerBuilder.RegisterType<PasswordHasher<User>>().As<IPasswordHasher<User>>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<KafkaService>()
                    .As<IKafkaService>()
                    .WithParameter("bootstrapServers", bootstrapServers)
                    .SingleInstance();
    containerBuilder.RegisterType<MediaService>().As<IMediaService>().SingleInstance();
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
});

var app = builder.Build();

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

app.UseMiddleware<ServerExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();