using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
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
        /// После покрытия кода unit-тестами и корректными интеграционными тестами, следует удалить этот тест
        /// 
        /// TODO: проблемы с тестированием
        /// 1. Логика размазана между контроллером, textExtractor и PaperAnalyzer
        /// -- оставить в контроллере только первичную валидацию, вызов сервиса и оборачивание ошибок
        /// -- может разделить шаги UploadFile и AnalyzeFile на уровне fe?
        /// 
        /// 2. Убрать статический класс PaperAnalyzer из контроллера (и сервиса)! Его невозможно замокать!
        /// </summary>
        [TestMethod]
        public async Task EndToEndSuccessTest()
        {
            // Arrange
            var logger = new Mock<ILogger<HomeController>>();
            var repository = new Mock<IResultRepository>();

            var controller = new HomeController(logger.Object, repository.Object);

            // Возможно, стоит добавить файл в testResources непосредственно в проект
            // Пока на временной основе загружу файл из родительской директории (pdf включен в репозиторий)
            var current = Directory.GetCurrentDirectory();
            // Не уверен, что это будет работать на unix-системе или из докер-контейнера
            var path = Path.GetFullPath(Path.Combine(current, @"..\..\..\..\..\paper_work\icc_2018\paper_short.pdf"));

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);         
            var formFile = new FormFile(fileStream, 0, fileStream.Length, "file", "paper_short.pdf");

            // Act
            var result = await controller.UploadFile(formFile, string.Empty, string.Empty, string.Empty);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.IsNotNull(okResult.Value);
            
            fileStream.Close();          
        }
    }
}
