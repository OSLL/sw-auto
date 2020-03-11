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

            var service = new PaperAnalyzerService();

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
            // Путь windows формата
            var current = Directory.GetCurrentDirectory();
            var sep = Path.DirectorySeparatorChar;
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
