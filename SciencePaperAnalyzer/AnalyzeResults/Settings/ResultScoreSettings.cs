using System;
using System.Collections.Generic;
using WebPaperAnalyzer.Models;

namespace AnalyzeResults.Settings
{
    [Serializable]
    public class ResultScoreSettings
    {
        /// <summary>
        /// Критерий водности
        /// </summary>
        public BoundedCategory WaterCriteria { get; set; }

        /// <summary>
        /// Критерий ключевых слов
        /// </summary>
        public BoundedCategory KeyWordsCriteria { get; set; }

        /// <summary>
        /// Критерий Ципфа
        /// </summary>
        public BoundedCategory Zipf { get; set; }

        /// <summary>
        /// Пороговый критерий Использование личных местоимений
        /// </summary>
        public ThresholdCategory UseOfPersonalPronouns { get; set; }
        
        /// <summary>
        /// Пороговый критерий Используемый источник без ссылки
        /// </summary>
        public ThresholdCategory SourceNotReferenced { get; set; }
        
        /// <summary>
        /// Пороговый критерий "Слишком короткая секция/абзац"
        /// </summary>
        public ThresholdCategory ShortSection { get; set; }

        /// <summary>
        /// Пороговый критерий "Рисунки без подписи"
        /// </summary>
        public ThresholdCategory PictureNotReferenced { get; set; }

        /// <summary>
        /// Пороговый критерий "Таблица без подписи"
        /// </summary>
        public ThresholdCategory TableNotReferenced { get; set; }
        
        public double MaxScore { get; set; }
        public IEnumerable<ForbiddenWords> ForbiddenWords { get; set; }
        public double ForbiddenWordsCost { get; set; }
        public double ForbiddenWordsErrorCost { get; set; }
    }
}
