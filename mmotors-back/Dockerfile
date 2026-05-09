FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5049

ENV ASPNETCORE_URLS=http://+:5049

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["mmotors-backend/mmotors-back/mmotors-back.csproj", "mmotors-backend/mmotors-back/"]
RUN dotnet restore "mmotors-backend/mmotors-back/mmotors-back.csproj"
COPY . .
WORKDIR "/src/mmotors-backend/mmotors-back"
RUN dotnet build "mmotors-back.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "mmotors-back.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

USER app
COPY --from=publish /app/publish .
    
ENTRYPOINT ["dotnet", "mmotors-back.dll"]
