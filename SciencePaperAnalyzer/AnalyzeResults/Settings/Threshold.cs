using System;

namespace AnalyzeResults.Settings
{
	/// <summary>
	/// Пороговое значение для параметра
	/// </summary>
	[Serializable]
	public class Threshold
	{
		/// <summary>
		/// Минимальное значение параметра, после которого применяет штраф указанного размера
		/// </summary>
		public int ThresholdValue { get; set; }

		/// <summary>
		/// Размер штрафа
		/// </summary>
		public double ErrorCost { get; set; }
	}
}
