using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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

    public class AddCriterion
    {
        [Required(ErrorMessage = "Не указан критерий")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Не указан критерий")]
        public string ErrorCost { get; set; }

        [Required(ErrorMessage = "Не указан критерий")]
        public string WaterCriterionFactor { get; set; }
        public string WaterCriterionLowerBound { get; set; }
        public string WaterCriterionUpperBound { get; set; }

        [Required(ErrorMessage = "Не указан критерий")]
        public string KeyWordsCriterionFactor { get; set; }
        public string KeyWordsCriterionLowerBound { get; set; }
        public string KeyWordsCriterionUpperBound { get; set; }

        [Required(ErrorMessage = "Не указан критерий")]
        public string ZipfFactor { get; set; }
        public string ZipfFactorLowerBound { get; set; }
        public string ZipfFactorUpperBound { get; set; }
    }
}