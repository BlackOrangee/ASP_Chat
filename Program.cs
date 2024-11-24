using ASP_Chat;
using ASP_Chat.Exceptions;
using ASP_Chat.Service;
using ASP_Chat.Service.Impl;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
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
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");

if (string.IsNullOrEmpty(secretKey))
{
    throw new ServerException("JWT secret key is not set",
        ServerException.ExceptionCodes.SecretKeyNotSet,
        ServerException.StatusCodes.InternalServerError);
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

var app = builder.Build();

app.UseMiddleware<ServerExceptionMiddleware>();

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


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();