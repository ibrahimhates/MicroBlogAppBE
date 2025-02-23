FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MicroBlog.Api/MicroBlog.Api.csproj", "MicroBlog.Api/"]
COPY ["MicroBlog.Repository/MicroBlog.Repository.csproj", "MicroBlog.Repository/"]
COPY ["MicroBlog.Core/MicroBlog.Core.csproj", "MicroBlog.Core/"]
COPY ["MicroBlog.Service/MicroBlog.Service.csproj", "MicroBlog.Service/"]
RUN dotnet restore "MicroBlog.Api/MicroBlog.Api.csproj"
COPY . .
WORKDIR "/src/MicroBlog.Api"
RUN dotnet build "MicroBlog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MicroBlog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroBlog.Api.dll"]
