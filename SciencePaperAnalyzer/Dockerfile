FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["TestWebApp/WebPaperAnalyzer.csproj", "TestWebApp/"]
COPY ["TextExtractor/TextExtractor.csproj", "TextExtractor/"]
COPY ["PaperAnalyzer/PaperAnalyzer.csproj", "PaperAnalyzer/"]
COPY ["LangAnalyzerStd.Postagger/LangAnalyzerStd.Postagger.csproj", "LangAnalyzerStd.Postagger/"]
COPY ["LangAnalyzerStd.Core/LangAnalyzerStd.Core.csproj", "LangAnalyzerStd.Core/"]
COPY ["LangAnalyzerStd.Morphology/LangAnalyzerStd.Morphology.csproj", "LangAnalyzerStd.Morphology/"]
COPY ["LangAnalyzerStd.SentenceSplitter/LangAnalyzerStd.SentenceSplitter.csproj", "LangAnalyzerStd.SentenceSplitter/"]
COPY ["LangAnalyzerStd.Сrfsuite/LangAnalyzerStd.Сrfsuite.csproj", "LangAnalyzerStd.Сrfsuite/"]
COPY ["AnalyzeResults/AnalyzeResults.csproj", "AnalyzeResults/"]
# RUN dotnet restore "TestWebApp/WebPaperAnalyzer.csproj"
COPY . .
WORKDIR "/src/TestWebApp"
RUN dotnet build "WebPaperAnalyzer.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebPaperAnalyzer.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebPaperAnalyzer.dll"]