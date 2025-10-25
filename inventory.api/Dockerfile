FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/dotnet-webapi/dotnet-webapi.csproj", "src/dotnet-webapi/"]
RUN dotnet restore "src/dotnet-webapi/dotnet-webapi.csproj"
COPY . .
WORKDIR "/src/src/dotnet-webapi"
RUN dotnet build "dotnet-webapi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dotnet-webapi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "dotnet-webapi.dll"]