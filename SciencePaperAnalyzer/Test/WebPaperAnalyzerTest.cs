using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PaperAnalyzer.Service;
using WebPaperAnalyzer.Controllers;
using WebPaperAnalyzer.DAL;
using Microsoft.Extensions.Configuration;
using PaperAnalyzer;
using System.Configuration;

namespace Test
{
    [TestClass]
    public class WebPaperAnalyzerTest
    {
        /// <summary>
        /// end-to-end тест для оценки работоспособности сервиса
        /// Анализ pdf файла
        /// После покрытия кода unit-тестами и корректными интеграционными тестами, следует удалить этот тест
        /// </summary>
        [TestMethod]
        public async Task EndToEndSuccessTest()
        {
            // Arrange
            var logger = new Mock<ILogger<HomeController>>();
            var repository = new Mock<IResultRepository>();
            var file = new Mock<IFormFile>();

            // Заглушка для словарей
            var current = Directory.GetCurrentDirectory();
            var sep = Path.DirectorySeparatorChar;
            var configPrefix = Path.GetFullPath(Path.Combine(current, $"..{sep}..{sep}..{sep}..{sep}TestWebApp"));

            ConfigurationManager.AppSettings["MORPHO_AMBIGUITY_MODEL_FILENAME"] = Path.Combine(configPrefix, $"resources{sep}morphology{sep}dsf_pa_(morpho_ambiguity)_5g.txt");
            ConfigurationManager.AppSettings["MORPHO_AMBIGUITY_TEMPLATE_FILENAME_5G"] = Path.Combine(configPrefix, $"resources{sep}morphology{sep}templateMorphoAmbiguity_5g.txt");
            ConfigurationManager.AppSettings["MORPHO_AMBIGUITY_TEMPLATE_FILENAME_3G"] = Path.Combine(configPrefix, $"resources{sep}morphology{sep}templateMorphoAmbiguity_3g.txt");

            ConfigurationManager.AppSettings["URL_DETECTOR_RESOURCES_XML_FILENAME"] = Path.Combine(configPrefix, $"resources{sep}url-detector-resources.xml");
            ConfigurationManager.AppSettings["SENT_SPLITTER_RESOURCES_XML_FILENAME"] = Path.Combine(configPrefix, $"resources{sep}sent-splitter-resources.xml");
            ConfigurationManager.AppSettings["TOKENIZER_RESOURCES_XML_FILENAME"] = Path.Combine(configPrefix, $"resources{sep}crfsuite-tokenizer-resources.xml");
            ConfigurationManager.AppSettings["POSTAGGER_MODEL_FILENAME"] = Path.Combine(configPrefix, $"resources{sep}model_ap_(minfreq-1)_ru");
            ConfigurationManager.AppSettings["POSTAGGER_TEMPLATE_FILENAME"] = Path.Combine(configPrefix, $"resources{sep}templatePosTagger_ru.txt");
            ConfigurationManager.AppSettings["POSTAGGER_RESOURCES_XML_FILENAME"] = Path.Combine(configPrefix, $"resources{sep}pos-tagger-resources.xml");

            ConfigurationManager.AppSettings["MORPHO_BASE_DIRECTORY"] = Path.Combine(configPrefix, $"resources{sep}morphology{sep}");
            //ConfigurationManager.AppSettings["MORPHO_MORPHOTYPES_FILENAMES"] = $"{Path.Combine(configPrefix, $"")}; {Path.Combine(configPrefix, $"")}";
            //ConfigurationManager.AppSettings["MORPHO_PROPERNAMES_FILENAMES"].ToFilesArray();
            //ConfigurationManager.AppSettings["MORPHO_COMMON_FILENAMES"].ToFilesArray();

        // явное создание экземпляров (стоит заменить на пересборку контейнера?)
        var env = new PaperAnalyzerEnvironment();
            var analyzer = new PapersAnalyzer(env);
            var service = new PaperAnalyzerService(analyzer);

            var configuration = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            configuration.Setup(c => c.GetSection("ResultScoreSettings"))
                .Returns(configurationSection.Object);

            configurationSection.Setup(c => c["ErrorCost"]).Returns("2");
            configurationSection.Setup(c => c["KeyWordsCriterionFactor"]).Returns("35");
            configurationSection.Setup(c => c["WaterCriterionFactor"]).Returns("35");
            configurationSection.Setup(c => c["ZipfFactor"]).Returns("35");


            var controller = new HomeController(logger.Object, service, repository.Object, 
                configuration: configuration.Object);

            // Временное решение, загрузка pdf файла, который присутствует в репозитории
            var path = Path.GetFullPath(Path.Combine(current, $"..{sep}..{sep}..{sep}..{sep}..{sep}paper_work{sep}icc_2018{sep}paper_short.pdf"));

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            file.Setup(f => f.Length).Returns(memoryStream.Length);
            file.Setup(f => f.FileName).Returns("paper-short.pdf");
            file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) => memoryStream.CopyTo(stream))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.UploadFile(file.Object, string.Empty, string.Empty, string.Empty);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.IsNotNull(okResult.Value);
            
            fileStream.Close();
            memoryStream.Close();
        }
    }
}
