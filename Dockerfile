FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Feed local com FCG.Contracts
COPY packages/ packages/
COPY NuGet.Config .

COPY src/FCG.PaymentsAPI/FCG.PaymentsAPI.csproj src/FCG.PaymentsAPI/
RUN dotnet restore src/FCG.PaymentsAPI/FCG.PaymentsAPI.csproj \
    --configfile ./NuGet.Config

COPY src/ src/
WORKDIR /src/src/FCG.PaymentsAPI
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FCG.PaymentsAPI.dll"]
