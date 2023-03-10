FROM mcr.microsoft.com/dotnet/runtime:7.0-bullseye-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim AS build
WORKDIR /src
COPY ["VideoProcessor.csproj", "."]
RUN dotnet restore "./VideoProcessor.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "VideoProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoProcessor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt update
RUN apt install ffmpeg -y

ENTRYPOINT ["dotnet", "VideoProcessor.dll"]