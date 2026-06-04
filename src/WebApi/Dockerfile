# src/WebApi/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copiar archivos de proyecto
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/WebApi/WebApi.csproj src/WebApi/

# Restaurar paquetes
RUN dotnet restore src/WebApi/WebApi.csproj

# Copiar todo el código fuente
COPY . .

# Publicar la aplicación
RUN dotnet publish src/WebApi/WebApi.csproj -c Release -o /app/publish

# Imagen de runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copiar los archivos publicados
COPY --from=build /app/publish .

# Variable de entorno para el puerto
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "WebApi.dll"]