# 🌱 Base image cho app runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# ✅ Cài Python vào base image
RUN apt-get update && \
    apt-get install -y python3 python3-pip && \
    ln -s /usr/bin/python3 /usr/bin/python

# 🛠 Stage build app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file .csproj trước để cache restore
COPY ["Web_API/Web_API.csproj", "Web_API/"]
RUN dotnet restore "Web_API/Web_API.csproj"

# Copy toàn bộ source
COPY . .

WORKDIR "/src/Web_API"
RUN dotnet build "Web_API.csproj" -c Release -o /app/build

# 🚀 Publish bản release
FROM build AS publish
RUN dotnet publish "Web_API.csproj" -c Release -o /app/publish

# 🧊 Final image để chạy app + python
FROM base AS final
WORKDIR /app

# Copy app đã publish từ stage publish
COPY --from=publish /app/publish ./

# Copy Python scripts & requirements
COPY Web_API/PythonScripts/ ./PythonScripts/
COPY Web_API/requirements.txt .

# Cài thư viện Python
RUN pip install --no-cache-dir -r requirements.txt

# ✅ Entry point
ENTRYPOINT ["dotnet", "Web_API.dll"]
