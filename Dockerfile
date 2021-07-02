FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["DotaDataExtractor.fsproj", "./"]
RUN dotnet restore "DotaDataExtractor.fsproj"
COPY . .
WORKDIR "/src/DotaDataExtractor"
RUN dotnet build "DotaDataExtractor.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DotaDataExtractor.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotaDataExtractor.dll"]
