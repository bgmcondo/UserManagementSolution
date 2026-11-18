# Hata ayıklama kapsayıcınızı özelleştirme ve Visual Studio’nun daha hızlı hata ayıklama için görüntülerinizi derlemek üzere bu Dockerfile'ı nasıl kullandığı hakkında bilgi edinmek için https://aka.ms/customizecontainer sayfasına bakın.

# Bu aşama, VS'den hızlı modda çalıştırıldığında kullanılır (Hata ayıklama yapılandırması için varsayılan olarak ayarlıdır)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Bu aşama, hizmet projesini oluşturmak için kullanılır
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UserManagement.API/UserManagement.API.csproj", "UserManagement.API/"]
COPY ["UserManagement.Infrastructure/UserManagement.Infrastructure.csproj", "UserManagement.Infrastructure/"]
COPY ["UserManagement.Application/UserManagement.Application.csproj", "UserManagement.Application/"]
COPY ["UserManagement/UserManagement.Domain.csproj", "UserManagement/"]
RUN dotnet restore "./UserManagement.API/UserManagement.API.csproj"
COPY . .
WORKDIR "/src/UserManagement.API"
RUN dotnet build "./UserManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Bu aşama, son aşamaya kopyalanacak hizmet projesini yayımlamak için kullanılır
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UserManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Bu aşama üretimde veya VS'den normal modda çalıştırıldığında kullanılır (Hata Ayıklama yapılandırması kullanılmazken varsayılan olarak ayarlıdır)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserManagement.API.dll"]