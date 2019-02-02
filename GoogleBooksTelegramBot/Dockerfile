FROM microsoft/dotnet:2.2-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY GoogleBooksTelegramBot/GoogleBooksTelegramBot.csproj GoogleBooksTelegramBot/
RUN dotnet restore GoogleBooksTelegramBot/GoogleBooksTelegramBot.csproj
COPY . .
WORKDIR /src/GoogleBooksTelegramBot
RUN dotnet build GoogleBooksTelegramBot.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish GoogleBooksTelegramBot.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "GoogleBooksTelegramBot.dll"]
