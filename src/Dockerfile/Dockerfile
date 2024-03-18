#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SM-Post/Post.Cmd/Post.Cmd.Api/Post.Cmd.Api.csproj", "src/SM-Post/Post.Cmd/Post.Cmd.Api/Post.Cmd.Api.csproj"]
COPY ["src/CQRS-ES/CQRS.Core/CQRS.Core.csproj", "src/CQRS-ES/CQRS.Core/CQRS.Core.csproj"]
COPY ["src/SM-Post/Post.Cmd/Post.Cmd.Domain/Post.Cmd.Domain.csproj", "src/SM-Post/Post.Cmd/Post.Cmd.Domain/Post.Cmd.Domain.csproj"]
COPY ["src/SM-Post/Post.Common/Post.Common.csproj", "src/SM-Post/Post.Common/Post.Common.csproj"]
COPY ["src/SM-Post/Post.Cmd/Post.Cmd.Infrastructure/Post.Cmd.Infrastructure.csproj", "src/SM-Post/Post.Cmd/Post.Cmd.Infrastructure/Post.Cmd.Infrastructure.csproj"]
RUN dotnet restore "./src/SM-Post/Post.Cmd/Post.Cmd.Api/./Post.Cmd.Api.csproj"
COPY . .
WORKDIR "/src/src/SM-Post/Post.Cmd/Post.Cmd.Api"
RUN dotnet publish -r linux-x64 -c Release -o /app --self-contained false --no-restore 

FROM base AS final
ENV TZ=Asia/Ho_Chi_Minh
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Post.Cmd.Api.dll"]