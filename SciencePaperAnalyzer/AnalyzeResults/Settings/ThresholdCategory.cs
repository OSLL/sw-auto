using System.Collections.Generic;

namespace AnalyzeResults.Settings
{
	/// <summary>
	/// Критерий для оценивания, который предполагает наличие "порогов"
	/// От общей оценки за работу будет отниматься значение, соответствующее достигнутому порогу
	/// </summary>
	public class ThresholdCategory
	{
		/// <summary>
		/// Набор пороговых значений штрафов
		/// </summary>
		public IEnumerable<Threshold> Thresholds { get; set; }

		/// <summary>
		/// Вес критерия 
		/// </summary>
		public double Weight { get; set; }
	}
}
