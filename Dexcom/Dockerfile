FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS base

WORKDIR /app

COPY . .

RUN dotnet publish -c Release -o /app/publish

WORKDIR /app/publish

EXPOSE 8080

CMD ["dotnet", "Dexcom.dll"]    