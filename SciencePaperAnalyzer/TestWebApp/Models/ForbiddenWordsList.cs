using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPaperAnalyzer.Models
{
	public class ForbiddenWordsList
	{
		// Наименование словаря
		public string Name { get; set; }
		// Превью, первые n слов из словаря
		public string PreviewString { get; set; }
		// Количество слов в словаре
		public int WordCount { get; set; }
	}
}
