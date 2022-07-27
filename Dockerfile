#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
#EXPOSE 443 

ENV ASPNETCORE_URLS=http://+:5000;http://+:80;
ENV ASPNETCORE_ENVIRONMENT=Development


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MyMovieScore.Api/MyMovieScore.Api.csproj", "MyMovieScore.Api/"]
COPY ["MyMovieScore.Application/MyMovieScore.Application.csproj", "MyMovieScore.Application/"]
COPY ["MyMovieScore.Core/MyMovieScore.Core.csproj", "MyMovieScore.Core/"]
COPY ["MyMovieScore.Infrastructure/MyMovieScore.Infrastructure.csproj", "MyMovieScore.Infrastructure/"]
RUN dotnet restore "MyMovieScore.Api/MyMovieScore.Api.csproj"
COPY . .
WORKDIR "/src/MyMovieScore.Api"
RUN dotnet build "MyMovieScore.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyMovieScore.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyMovieScore.Api.dll"]