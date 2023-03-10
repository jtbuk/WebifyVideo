FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim AS build
RUN apt update
RUN apt install ffmpeg -y