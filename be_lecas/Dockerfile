FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["be_lecas.csproj", "./"]
RUN dotnet restore "be_lecas.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "be_lecas.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "be_lecas.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "be_lecas.dll"]

