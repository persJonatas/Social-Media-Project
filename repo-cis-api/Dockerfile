FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy project files (csproj) of dependencies
COPY ["CisApi.Presentation/CisApi.Presentation.csproj", "CisApi.Presentation/"]
COPY ["CisApi.Business/CisApi.Business.csproj", "CisApi.Business/"]
COPY ["CisApi.Data/CisApi.Data.csproj", "CisApi.Data/"]

# restore dependencies
RUN dotnet restore "CisApi.Presentation/CisApi.Presentation.csproj"

# copy the entire source code
COPY . .
WORKDIR "/src/CisApi.Presentation"

# build the application
RUN dotnet build "CisApi.Presentation.csproj" -c Release -o /app/build

# publish the app
FROM build AS publish
RUN dotnet publish "CisApi.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# create final image using the runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CisApi.Presentation.dll"]
