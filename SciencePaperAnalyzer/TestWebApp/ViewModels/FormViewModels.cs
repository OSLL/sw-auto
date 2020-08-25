using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using AnalyzeResults.Settings;
using WebPaperAnalyzer.Models;

namespace WebPaperAnalyzer.ViewModels
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Не указан логин")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Не указан логин")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароль введен неверно")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Не указан логин")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class AddCriterion : WebPaperAnalyzer.Models.ResultCriterion
    {
        public List<DictionaryCheckBoxModel> Dictionaries { get; set; }

        public static AddCriterion FromResultCriterionToForm(ResultCriterion model,
            IEnumerable<ForbiddenWords> dictionary)
        {
            var criterion = new AddCriterion
            {
                Id = model.Id,
                Name = model.Name,
                Summary = model.Summary,
                TeacherLogin = model.TeacherLogin,
                MaxScore = model.MaxScore,
                WaterCriterionFactor = model.WaterCriterionFactor,
                WaterCriterionLowerBound = model.WaterCriterionLowerBound,
                WaterCriterionUpperBound = model.WaterCriterionUpperBound,
                KeyWordsCriterionFactor = model.KeyWordsCriterionFactor,
                KeyWordsCriterionLowerBound = model.KeyWordsCriterionLowerBound,
                KeyWordsCriterionUpperBound = model.KeyWordsCriterionUpperBound,
                ZipfFactor = model.ZipfFactor,
                ZipfFactorLowerBound = model.ZipfFactorLowerBound,
                ZipfFactorUpperBound = model.ZipfFactorUpperBound,
                KeywordsMentioningFactor = model.KeywordsMentioningFactor,
                UseOfPersonalPronounsCost = model.UseOfPersonalPronounsCost,
                UseOfPersonalPronounsErrorCost = model.UseOfPersonalPronounsErrorCost,
                SourceNotReferencedCost = model.SourceNotReferencedCost,
                SourceNotReferencedErrorCost = model.SourceNotReferencedErrorCost,
                ShortSectionCost = model.ShortSectionCost,
                ShortSectionErrorCost = model.ShortSectionErrorCost,
                PictureNotReferencedCost = model.PictureNotReferencedCost,
                PictureNotReferencedErrorCost = model.PictureNotReferencedErrorCost,
                TableNotReferencedCost = model.TableNotReferencedCost,
                TableNotReferencedErrorCost = model.TableNotReferencedErrorCost,
                ForbiddenWordDictionary = model.ForbiddenWordDictionary,
                ForbiddenWordsCost = model.ForbiddenWordsCost,
                ForbiddenWordsErrorCost = model.ForbiddenWordsErrorCost,
                Dictionaries = dictionary.Select(x => new DictionaryCheckBoxModel
                    { Name = x.Name, IsSelected = false }).ToList()
            };

            return criterion;
        }
    }

    public class DictionaryCheckBoxModel
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }

    public class DictionariesModel
    {
        public List<ForbiddenWordsList> Dictionaries { get; set; }
    }
}