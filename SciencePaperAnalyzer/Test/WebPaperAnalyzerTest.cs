using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using PaperAnalyzer;
using PaperAnalyzer.Service;
using TestWebApp.Models;
using WebPaperAnalyzer.Controllers;
using WebPaperAnalyzer.DAL;

namespace Test
{
    /// <summary>
    /// End-to-end тесты для проверки работоспособности сервиса
    /// </summary>
    [TestClass]
    public class WebPaperAnalyzerTest
    {
        private Mock<ILogger<HomeController>> _mockLogger;
        private Mock<IResultRepository> _mockRepository;
        private Mock<IFormFile> _mockFile;
        private Mock<IConfiguration> _mockConfiguration;

        private readonly string _currentPath = Directory.GetCurrentDirectory();
        private readonly char _sep = Path.DirectorySeparatorChar;

        [TestInitialize]
        public void Init()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockRepository = new Mock<IResultRepository>();
            _mockFile = new Mock<IFormFile>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Для анализа требуется создания окружения анализатора (PaperAnalyzerEnvironment)
            // Для создания окружения требуются словари с запрещенными словами, морфологические словари и так далее
            // Исходные словари хранились в папке resources в xml и pdf файлах в двух проектах - PaperAnalyzer и TestWebApp (две идентичные копии)
            // Ссылки на словари подгружались из конфигурационных файлов соответствующих проектах
            // В тестах явно указывается, что словари следует подгрузить из папки resources проекта TestWebApp
            // TODO: Это временное решение, следует пересмотреть хранение словарей и их загрузку в окружение (issue не создан).
            var configPrefix = Path.GetFullPath(Path.Combine(_currentPath, $"..{_sep}..{_sep}..{_sep}..{_sep}TestWebApp"));

            ConfigurationManager.AppSettings["MORPHO_AMBIGUITY_MODEL_FILENAME"] = Path.Combine(configPrefix, $"resources{_sep}morphology{_sep}dsf_pa_(morpho_ambiguity)_5g.txt");
            ConfigurationManager.AppSettings["MORPHO_AMBIGUITY_TEMPLATE_FILENAME_5G"] = Path.Combine(configPrefix, $"resources{_sep}morphology{_sep}templateMorphoAmbiguity_5g.txt");
            ConfigurationManager.AppSettings["MORPHO_AMBIGUITY_TEMPLATE_FILENAME_3G"] = Path.Combine(configPrefix, $"resources{_sep}morphology{_sep}templateMorphoAmbiguity_3g.txt");

            ConfigurationManager.AppSettings["URL_DETECTOR_RESOURCES_XML_FILENAME"] = Path.Combine(configPrefix, $"resources{_sep}url-detector-resources.xml");
            ConfigurationManager.AppSettings["SENT_SPLITTER_RESOURCES_XML_FILENAME"] = Path.Combine(configPrefix, $"resources{_sep}sent-splitter-resources.xml");
            ConfigurationManager.AppSettings["TOKENIZER_RESOURCES_XML_FILENAME"] = Path.Combine(configPrefix, $"resources{_sep}crfsuite-tokenizer-resources.xml");
            ConfigurationManager.AppSettings["POSTAGGER_MODEL_FILENAME"] = Path.Combine(configPrefix, $"resources{_sep}model_ap_(minfreq-1)_ru");
            ConfigurationManager.AppSettings["POSTAGGER_TEMPLATE_FILENAME"] = Path.Combine(configPrefix, $"resources{_sep}templatePosTagger_ru.txt");
            ConfigurationManager.AppSettings["POSTAGGER_RESOURCES_XML_FILENAME"] = Path.Combine(configPrefix, $"resources{_sep}pos-tagger-resources.xml");

            ConfigurationManager.AppSettings["MORPHO_BASE_DIRECTORY"] = Path.Combine(configPrefix, $"resources{_sep}morphology{_sep}");

            // Эти словари не являются необходимыми для корректной работы предложения, в тестах их загрузка отключена
            //ConfigurationManager.AppSettings["MORPHO_MORPHOTYPES_FILENAMES"] = $"{Path.Combine(configPrefix, $"")}; {Path.Combine(configPrefix, $"")}";
            //ConfigurationManager.AppSettings["MORPHO_PROPERNAMES_FILENAMES"].ToFilesArray();
            //ConfigurationManager.AppSettings["MORPHO_COMMON_FILENAMES"].ToFilesArray();

            // Настройка весов для разных параметров оценки
            _mockConfiguration = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            _mockConfiguration.Setup(c => c.GetSection("ResultScoreSettings"))
                .Returns(configurationSection.Object);

            configurationSection.Setup(c => c["ErrorCost"]).Returns("2");
            configurationSection.Setup(c => c["KeyWordsCriterionFactor"]).Returns("35");
            configurationSection.Setup(c => c["WaterCriterionFactor"]).Returns("35");
            configurationSection.Setup(c => c["ZipfFactor"]).Returns("35");

            // TODO: Одновременно применяются два приема: явное указание значений AppSettings и использование mock
            // Следует выбрать один и использовать только его
        }

        /// <summary>
        /// Анализ корректного pdf файла должен заканчиваться со статусом 200
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task EndToEndSuccessTest()
        {
            // TODO: Если иерархия классов будет разростаться, следует заменить явное создание экземпляров на использование контейнера ServiceProvider
            var env = new PaperAnalyzerEnvironment();
            var analyzer = new PapersAnalyzer(env);
            var service = new PaperAnalyzerService(analyzer);

            var controller = new HomeController(_mockLogger.Object, service, _mockRepository.Object, 
                configuration: _mockConfiguration.Object);

            // Загрузка pdf файла для тестирования из текущей директории
            // TODO: Нестабильное решение с множеством сторонних эффектов (если кто-то поменяет структуру папок в решении?)
            // Следует использовать отдельную сборку с тестовыми ресурсами
            var path = Path.GetFullPath(Path.Combine(_currentPath, $"..{_sep}..{_sep}..{_sep}TestResources{_sep}paper_short.pdf"));

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            _mockFile.Setup(f => f.Length).Returns(memoryStream.Length);
            _mockFile.Setup(f => f.FileName).Returns("paper_short.pdf");
            _mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) => memoryStream.CopyTo(stream))
                .Returns(Task.CompletedTask);

            var result = await controller.UploadFile(_mockFile.Object, string.Empty, string.Empty, string.Empty);
            var okResult = result as OkObjectResult;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.IsNotNull(okResult.Value);
            
            fileStream.Close();
            memoryStream.Close();
        }

        /// <summary>
        /// Анализ файла с неподдерживаемым расширением должен заканчиваться с ошибкой.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task EndToEndUnsupportedFileTypeTest()
        {
            // TODO: Если иерархия классов будет разростаться, следует заменить явное создание экземпляров на использование контейнера ServiceProvider
            var env = new PaperAnalyzerEnvironment();
            var analyzer = new PapersAnalyzer(env);
            var service = new PaperAnalyzerService(analyzer);

            var controller = new HomeController(_mockLogger.Object, service, _mockRepository.Object,
                configuration: _mockConfiguration.Object);

            // Загрузка pdf файла для тестирования из текущей директории
            // TODO: Нестабильное решение с множеством сторонних эффектов (если кто-то поменяет структуру папок в решении?)
            // Следует использовать отдельную сборку с тестовыми ресурсами
            var path = Path.GetFullPath(Path.Combine(_currentPath, $"..{_sep}..{_sep}..{_sep}TestResources{_sep}paper_short.pdf"));

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            _mockFile.Setup(f => f.Length).Returns(memoryStream.Length);
            _mockFile.Setup(f => f.FileName).Returns("testfile.xls");
            _mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) => memoryStream.CopyTo(stream))
                .Returns(Task.CompletedTask);

            var result = await controller.UploadFile(_mockFile.Object, string.Empty, string.Empty, string.Empty);

            var errorResult = result as ViewResult;
            Assert.IsNotNull(errorResult);
            var errorModel = errorResult.Model as ErrorViewModel;
            // RequestID не будет присвоен
            Assert.IsNull(errorModel.RequestId);
            // Если RequestID не присвоен, то он не показывается
            Assert.IsFalse(errorModel.ShowRequestId);
            // Сообщение об ошибке должено быть непустым
            Assert.AreNotEqual(errorModel.Message, string.Empty);

            fileStream.Close();
            memoryStream.Close();
        }

        /// <summary>
        /// Анализ пустого файла должен заканчиваться с ошибкой.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task EndToEndEmptyFileTest()
        {
            // TODO: Если иерархия классов будет разростаться, следует заменить явное создание экземпляров на использование контейнера ServiceProvider
            var env = new PaperAnalyzerEnvironment();
            var analyzer = new PapersAnalyzer(env);
            var service = new PaperAnalyzerService(analyzer);

            var controller = new HomeController(_mockLogger.Object, service, _mockRepository.Object,
                configuration: _mockConfiguration.Object);

            var memoryStream = new MemoryStream();

            _mockFile.Setup(f => f.Length).Returns(memoryStream.Length);
            _mockFile.Setup(f => f.FileName).Returns("testfile.pdf");
            _mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) => memoryStream.CopyTo(stream))
                .Returns(Task.CompletedTask);

            var result = await controller.UploadFile(_mockFile.Object, string.Empty, string.Empty, string.Empty);

            var errorResult = result as ViewResult;
            Assert.IsNotNull(errorResult);
            var errorModel = errorResult.Model as ErrorViewModel;
            // RequestID не будет присвоен
            Assert.IsNull(errorModel.RequestId);
            // Если RequestID не присвоен, то он не показывается
            Assert.IsFalse(errorModel.ShowRequestId);
            // Сообщение об ошибке должено быть непустым
            Assert.AreNotEqual(errorModel.Message, string.Empty);

            memoryStream.Close();
        }

    }
}
