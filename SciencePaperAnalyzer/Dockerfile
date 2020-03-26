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
COPY ["Test/Test.csproj", "Test/"]
COPY . .
RUN dotnet test -c Release

WORKDIR "/src/TestWebApp"

FROM build AS publish
RUN dotnet publish "WebPaperAnalyzer.csproj" -c Release -o /app
RUN rm /app/NLog.config
RUN cp NLog.config /app/NLog.config
RUN cp -avr resources /app/resources
FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN mkdir app
RUN cp NLog.config app/
RUN cp NLog.config wwwroot/
ENTRYPOINT ["dotnet", "WebPaperAnalyzer.dll"]