using Microsoft.AspNetCore.Http;

using System.ComponentModel.DataAnnotations;

namespace WebPaperAnalyzer.ViewModels
{
	public class DictionaryModel
	{
		[Required]
		public IFormFile File { get; set; }

		[Required]
		public string Name { get; set; }
	}
}
