FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Ecommerce.sln .
COPY Ecommerce.Api/*.csproj Ecommerce.Api/
COPY Ecommerce.Application/*.csproj Ecommerce.Application/
COPY Ecommerce.Infrastructure/*.csproj Ecommerce.Infrastructure/
COPY Ecommerce.Domain/*.csproj Ecommerce.Domain/
RUN dotnet restore

COPY . .
RUN dotnet publish Ecommerce.Api/Ecommerce.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Api.dll"]
