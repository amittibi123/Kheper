FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Kheper.Web/Kheper.Web.csproj", "Kheper.Web/"]
COPY ["Kheper.App/Kheper.App.csproj", "Kheper.App/"]
COPY ["Kheper.Shared/Kheper.Shared.csproj", "Kheper.Shared/"]

RUN dotnet restore "Kheper.Web/Kheper.Web.csproj"

COPY . .
WORKDIR "/src/Kheper.Web"
RUN dotnet publish "Kheper.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN apt-get update && apt-get install -y \
    python3 python3-pip python3-venv \
    && python3 -m venv /opt/libretranslate-venv \
    && /opt/libretranslate-venv/bin/pip install libretranslate \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["/entrypoint.sh"]
