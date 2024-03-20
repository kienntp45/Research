FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV http_proxy=http://proxy.fpts.com.vn:8080
ENV https_proxy=http://proxy.fpts.com.vn:8080
WORKDIR /src
COPY ["src/SM-Post/Post.Cmd/Post.Cmd.Api/Post.Cmd.Api.csproj", "src/SM-Post/Post.Cmd/Post.Cmd.Api/Post.Cmd.Api.csproj"]
COPY ["src/CQRS-ES/CQRS.Core/CQRS.Core.csproj", "src/CQRS-ES/CQRS.Core/CQRS.Core.csproj"]
COPY ["src/SM-Post/Post.Cmd/Post.Cmd.Domain/Post.Cmd.Domain.csproj", "src/SM-Post/Post.Cmd/Post.Cmd.Domain/Post.Cmd.Domain.csproj"]
COPY ["src/SM-Post/Post.Common/Post.Common.csproj", "src/SM-Post/Post.Common/Post.Common.csproj"]
COPY ["src/SM-Post/Post.Cmd/Post.Cmd.Infrastructure/Post.Cmd.Infrastructure.csproj", "src/SM-Post/Post.Cmd/Post.Cmd.Infrastructure/Post.Cmd.Infrastructure.csproj"]
RUN dotnet restore -s https://api.nuget.org/v3/index.json -r linux-x64 "./src/SM-Post/Post.Cmd/Post.Cmd.Api/./Post.Cmd.Api.csproj"
COPY . .
WORKDIR "/src/src/SM-Post/Post.Cmd/Post.Cmd.Api"
RUN dotnet publish -r linux-x64 -c Release -o /app --self-contained false --no-restore 

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
USER root
ENV TZ=Asia/Ho_Chi_Minh
COPY --from=build /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "Post.Cmd.Api.dll"]