# שלב הבנייה
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# העתקת קבצי הפרויקט - כולל ה-Shared!
COPY ["Kheper.Web/Kheper.Web.csproj", "Kheper.Web/"]
COPY ["Kheper.App/Kheper.App.csproj", "Kheper.App/"]
COPY ["Kheper.Shared/Kheper.Shared.csproj", "Kheper.Shared/"]

# הרצת Restore
RUN dotnet restore "Kheper.Web/Kheper.Web.csproj"

# העתקת שאר הקבצים
COPY . .
WORKDIR "/src/Kheper.Web"
RUN dotnet publish "Kheper.Web.csproj" -c Release -o /app/publish

# שלב ההרצה
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Kheper.Web.dll"]
