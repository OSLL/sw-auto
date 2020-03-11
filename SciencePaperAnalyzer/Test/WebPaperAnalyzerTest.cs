using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TestWebApp.Controllers;
using WebPaperAnalyzer.DAL;

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

            var controller = new HomeController(logger.Object, repository.Object);

            // Временное решение, загрузка pdf файла, который присутствует в репозитории
            // Путь windows формата
            var current = Directory.GetCurrentDirectory();
            var path = Path.GetFullPath(Path.Combine(current, @"..\..\..\..\..\paper_work\icc_2018\paper_short.pdf"));

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            file.Setup(f => f.Length).Returns(memoryStream.Length);
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
