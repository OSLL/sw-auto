namespace AnalyzeResults.Settings
{
	/// <summary>
	/// Критерий для оценивания работы, который предполагает наличие веса в общей оценке,
	/// а также верхней и нижней границы значения критерия
	/// Например, критерий водности или Ципфа.
	/// </summary>
	public class BoundedCriteria
	{
		/// <summary>
		/// Вес оценки критерия в общей оценке работы (в процентах)
		/// Должен принимать значения в диапазоне 0-100
		/// TODO: Добавить автоматическую проверку корректности значения
		/// </summary>
		public double Weight { get; set; }

		/// <summary>
		/// Верхняя граница значения критерия
		/// </summary>
		public double UpperBound { get; set; }

		/// <summary>
		/// Нижняя граница значения критерия
		/// </summary>
		public double LowerBound { get; set; }
	}
}
