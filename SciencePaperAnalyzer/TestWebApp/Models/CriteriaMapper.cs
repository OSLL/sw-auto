using AnalyzeResults.Settings;
using System;
using System.Collections.Generic;

namespace WebPaperAnalyzer.Models
{
	/// <summary>
	/// Преобразует критерии оценивания из формата слоя представления данных в формат
	/// слоя обработки данных (и обратно)
	/// </summary>
	public static class CriteriaMapper
	{
		public static ResultCriterion GetUICriteia(ResultScoreSettings criteria)
		{
			throw new NotImplementedException();
		}

		public static ResultScoreSettings GetAnalyzeCriteria(ResultCriterion criteria)
		{
            var result = new ResultScoreSettings
            {
                WaterCriteria = new BoundedCriteria
                {
                    Weight = criteria.WaterCriterionFactor,
                    UpperBound = criteria.WaterCriterionUpperBound,
                    LowerBound = criteria.WaterCriterionLowerBound,
                },
                KeyWordsCriteria = new BoundedCriteria
				{
                    Weight = criteria.KeyWordsCriterionFactor,
                    UpperBound = criteria.KeyWordsCriterionUpperBound,
                    LowerBound = criteria.KeyWordsCriterionLowerBound,
				},
                Zipf = new BoundedCriteria
				{
                    Weight = criteria.ZipfFactor,
                    LowerBound = criteria.ZipfFactorLowerBound,
                    UpperBound = criteria.ZipfFactorUpperBound,
                },                
                UseOfPersonalPronounsCost = criteria.UseOfPersonalPronounsCost,
                UseOfPersonalPronounsErrorCost = criteria.UseOfPersonalPronounsErrorCost,
                UseOfPersonalPronounsGrading = new Dictionary<int, double>(),
                UseOfPersonalPronounsGradingType = criteria.UseOfPersonalPronounsGradingType,
                SourceNotReferencedCost = criteria.SourceNotReferencedCost,
                SourceNotReferencedErrorCost = criteria.SourceNotReferencedErrorCost,
                SourceNotReferencedGrading = new Dictionary<int, double>(),
                SourceNotReferencedGradingType = criteria.SourceNotReferencedGradingType,
                ShortSectionCost = criteria.ShortSectionCost,
                ShortSectionErrorCost = criteria.ShortSectionErrorCost,
                ShortSectionGrading = new Dictionary<int, double>(),
                ShortSectionGradingType = criteria.ShortSectionGradingType,
                PictureNotReferencedCost = criteria.PictureNotReferencedCost,
                PictureNotReferencedErrorCost = criteria.PictureNotReferencedErrorCost,
                PictureNotReferencedGrading = new Dictionary<int, double>(),
                PictureNotReferencedGradingType = criteria.PictureNotReferencedGradingType,
                TableNotReferencedCost = criteria.TableNotReferencedCost,
                TableNotReferencedErrorCost = criteria.TableNotReferencedErrorCost,
                TableNotReferencedGrading = new Dictionary<int, double>(),
                TableNotReferencedGradingType = criteria.TableNotReferencedGradingType,
                MaxScore = criteria.MaxScore,
                ForbiddenWords = criteria.ForbiddenWords,
                ForbiddenWordsCost = criteria.ForbiddenWordsCost,
                ForbiddenWordsErrorCost = criteria.ForbiddenWordsErrorCost,
                ForbiddenWordsGrading = new Dictionary<int, double>(),
                ForbiddenWordsGradingType = criteria.ForbiddenWordsGradingType,
            };
            return result;
		}
	}
}
