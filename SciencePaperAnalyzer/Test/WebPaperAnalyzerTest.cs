using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using TestWebApp.Controllers;
using WebPaperAnalyzer.DAL;
using WebPaperAnalyzer.Models;
using Xunit.Sdk;


namespace Test
{
    [TestClass]
    public class WebPaperAnalyzerTest
    {
        /// <summary>
        /// end-to-end тест дл¤ оценки работоспособности сервиса
        /// ѕосле покрыти¤ кода unit-тестами и корректными интеграционными тестами, следует удалить этот тест
        /// 
        /// TODO: проблемы с тестированием
        /// 1. Ћогика размазана между контроллером, textExtractor и PaperAnalyzer
        /// -- оставить в контроллере только первичную валидацию, вызов сервиса и оборачивание ошибок
        /// -- может разделить шаги UploadFile и AnalyzeFile на уровне fe?
        /// 
        /// 2. ”брать статический класс PaperAnalyzer из контроллера (и сервиса)! ≈го невозможно замокать!
        /// </summary>
        [TestMethod]
        public async Task EndToEndSuccessTest()
        {
            // Arrange
            var logger = new Mock<ILogger<HomeController>>();
            var repository = new Mock<IResultRepository>();

            var controller = new HomeController(logger.Object, repository.Object);

            var file = new Mock<IFormFile>();

            // ¬озможно, стоит добавить файл в testResources непосредственно в проект
            // ѕока на временной основе загружу файл из родительской директории (pdf включен в репозиторий)
            var current = Directory.GetCurrentDirectory();
            // Ќе уверен, что это будет работать на unix-системе или из докер-контейнера
            var path = Path.GetFullPath(Path.Combine(current, @"..\..\..\..\..\paper_work\icc_2018\paper_short.pdf"));

            var testFile = new FileInfo(path);
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write(testFile.OpenRead());
            writer.Flush();
            memoryStream.Position = 0;

            file.Setup(f => f.Length).Returns(memoryStream.Length);

            file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None))
                .Callback<Stream, CancellationToken>((stream, token) => memoryStream.CopyTo(stream))
                .Returns(Task.CompletedTask);


            // Act
            var result = await controller.UploadFile(file.Object, string.Empty, string.Empty, string.Empty);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.IsNotNull(okResult.Value);
        }
    }
}
