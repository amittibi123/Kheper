# שלב הבנייה
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# העתקת קבצי הפרויקט
COPY ["Kheper.Web/Kheper.Web.csproj", "Kheper.Web/"]
COPY ["Kheper.App/Kheper.App.csproj", "Kheper.App/"]

RUN dotnet restore "Kheper.Web/Kheper.Web.csproj"

# העתקת שאר הקבצים
COPY . .
WORKDIR "/src/Kheper.Web"
RUN dotnet publish "Kheper.Web.csproj" -c Release -o /app/publish

# שלב ההרצה
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# הגדרת הפורט ל-Render
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Kheper.Web.dll"]