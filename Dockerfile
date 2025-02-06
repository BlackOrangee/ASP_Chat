FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

COPY . /app

ENTRYPOINT ["dotnet", "ASP_Chat.dll"]