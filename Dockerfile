FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# העתקת קבצי הפרויקט
COPY ["Kheper.Web/Kheper.Web.csproj", "Kheper.Web/"]
COPY ["Kheper.App/Kheper.App.csproj", "Kheper.App/"]
COPY ["Kheper.Shared/Kheper.Shared.csproj", "Kheper.Shared/"]

# הרצת Restore (עכשיו זה יעבוד כי ה-SDK תואם)
RUN dotnet restore "Kheper.Web/Kheper.Web.csproj"

# העתקת שאר הקבצים
COPY . .
WORKDIR "/src/Kheper.Web"
RUN dotnet publish "Kheper.Web.csproj" -c Release -o /app/publish

# שלב ההרצה
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Kheper.Web.dll"]