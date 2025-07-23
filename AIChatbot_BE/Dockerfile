# === STAGE 1: Base image with ASP.NET Runtime & Python ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# Cài Python + pip + thư viện hệ thống cần thiết
RUN apt-get update && \
    apt-get install -y python3 python3-pip && \
    ln -sf /usr/bin/python3 /usr/bin/python && \
    python -m pip install --upgrade pip --break-system-packages

# === STAGE 2: Build stage ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj & restore
COPY ["Web_API/Web_API.csproj", "Web_API/"]
RUN dotnet restore "Web_API/Web_API.csproj"

# Copy source
COPY . .

# Build project
WORKDIR "/src/Web_API"
RUN dotnet build "Web_API.csproj" -c Release -o /app/build

# === STAGE 3: Publish stage ===
FROM build AS publish
RUN dotnet publish "Web_API.csproj" -c Release -o /app/publish

# === STAGE 4: Final stage ===
FROM base AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish ./

# Copy Python scripts + requirements
COPY Web_API/PythonScripts ./PythonScripts/
COPY Web_API/requirements.txt .

# Install Python packages
RUN pip install --no-cache-dir -r requirements.txt --break-system-packages

ENTRYPOINT ["dotnet", "Web_API.dll"]
