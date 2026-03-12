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
