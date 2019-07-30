FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["TestWebApp/WebPaperAnalyzer.csproj", "TestWebApp/"]
COPY ["TextExtractor/TextExtractor.csproj", "TextExtractor/"]
COPY ["PaperAnalyzer/PaperAnalyzer.csproj", "PaperAnalyzer/"]
COPY ["LangAnalyzer.Postagger/LangAnalyzer.Postagger.csproj", "LangAnalyzer.Postagger/"]
COPY ["LangAnalyzer.Core/LangAnalyzer.Core.csproj", "LangAnalyzer.Core/"]
COPY ["LangAnalyzer.Morphology/LangAnalyzer.Morphology.csproj", "LangAnalyzer.Morphology/"]
COPY ["LangAnalyzer.SentenceSplitter/LangAnalyzer.SentenceSplitter.csproj", "LangAnalyzer.SentenceSplitter/"]
COPY ["LangAnalyzer.Сrfsuite/LangAnalyzer.Сrfsuite.csproj", "LangAnalyzer.Сrfsuite/"]
COPY ["AnalyzeResults/AnalyzeResults.csproj", "AnalyzeResults/"]
RUN dotnet restore "TestWebApp/WebPaperAnalyzer.csproj"
COPY . .
WORKDIR "/src/TestWebApp"
RUN dotnet build "WebPaperAnalyzer.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebPaperAnalyzer.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebPaperAnalyzer.dll"]