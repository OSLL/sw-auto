using System;

namespace AnalyzeResults.Settings
{
	/// <summary>
	/// Критерий для оценивания, который предполагает наличие веса в общей оценке работы
	/// а также, нижней и верхней границ для значения критерия
	/// Например: используется для критерия водности, Ципф.
	/// </summary>
	[Serializable]
	public class BoundedCategory
	{
		/// <summary>
		/// Вес оценки критерия в общей оценке работы
		/// </summary>
		public double Weight { get; set; }
		
		/// <summary>
		/// Верхняя граница критерия
		/// </summary>
		public double UpperBound { get; set; }

		/// <summary>
		/// Нижняя граница критерия
		/// </summary>
		public double LowerBound { get; set; }
	}

}
