FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ToDo_LudusAstra/ToDo_LudusAstra.csproj", "ToDo_LudusAstra/"]
RUN dotnet restore "ToDo_LudusAstra/ToDo_LudusAstra.csproj"
COPY . .
WORKDIR "/src/ToDo_LudusAstra"
RUN dotnet build "ToDo_LudusAstra.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ToDo_LudusAstra.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToDo_LudusAstra.dll"]
