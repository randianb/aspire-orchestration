# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["IotPlatform.Presentation/IotPlatform.Presentation.csproj", "IotPlatform.Presentation/"]
RUN dotnet restore "./IotPlatform.Presentation/IotPlatform.Presentation.csproj"
COPY . .
WORKDIR "/src/IotPlatform.Presentation"
RUN dotnet build "IotPlatform.Presentation.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "IotPlatform.Presentation.csproj" -c Release -o /app/publish

# Final stage
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY IotPlatform.Presentation/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80