# Dùng base image .NET 8 để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# Dùng SDK image để build project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj và restore
COPY ["Web_API/Web_API.csproj", "Web_API/"]
RUN dotnet restore "Web_API/Web_API.csproj"

# Copy toàn bộ source và build
COPY . .
WORKDIR "/src/Web_API"
RUN dotnet build "Web_API.csproj" -c Release -o /app/build

# Publish bản release
FROM build AS publish
RUN dotnet publish "Web_API.csproj" -c Release -o /app/publish

# Image final để chạy app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Web_API.dll"]
