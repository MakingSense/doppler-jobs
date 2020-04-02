FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS restore
WORKDIR /src
COPY Doppler.Jobs.sln ./
COPY ["Doppler.Jobs.Server/Doppler.Service.Job.Server.csproj", "Doppler.Jobs.Server/"]
COPY ["CrossCutting/CrossCutting.csproj", "CrossCutting/"]
COPY ["Doppler.Sap.Job/Doppler.Sap.Job.Service.csproj", "Doppler.Sap.Job/"]
COPY ["Doppler.Sap.Job.Test/Doppler.Jobs.Test.csproj", "Doppler.Sap.Job.Test/"]
RUN dotnet restore

FROM restore AS build
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS test
RUN dotnet test

FROM build AS publish
RUN dotnet publish "Doppler.Sap.Job/Doppler.Sap.Job.Service.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ARG version=unknown
# TODO: configure static files in the service and copy version.txt to the right folder
RUN echo $version > /app/version.txt
ENTRYPOINT ["dotnet", "Doppler.Service.Job.Server.dll"]