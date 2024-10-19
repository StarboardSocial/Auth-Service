FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 9000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["StarboardSocial.AuthService.Server/StarboardSocial.AuthService.Server.csproj", "StarboardSocial.AuthService.Server/"]
RUN dotnet restore "StarboardSocial.AuthService.Server/StarboardSocial.AuthService.Server.csproj"
COPY . .
WORKDIR "/src/StarboardSocial.AuthService.Server"
RUN dotnet build "StarboardSocial.AuthService.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StarboardSocial.AuthService.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:9000
ENTRYPOINT ["dotnet", "StarboardSocial.AuthService.Server.dll"]