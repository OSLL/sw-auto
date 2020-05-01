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
        public double ErrorCost { get; set; }

        [Required(ErrorMessage = "Сумма весов должна равняться 100")]
        public double WaterCriterionFactor { get; set; }
        public double WaterCriterionLowerBound { get; set; }
        public double WaterCriterionUpperBound { get; set; }

        [Required(ErrorMessage = "Сумма весов должна равняться 100")]
        public double KeyWordsCriterionFactor { get; set; }
        public double KeyWordsCriterionLowerBound { get; set; }
        public double KeyWordsCriterionUpperBound { get; set; }

        [Required(ErrorMessage = "Сумма весов должна равняться 100")]
        public double ZipfFactor { get; set; }
        public double ZipfFactorLowerBound { get; set; }
        public double ZipfFactorUpperBound { get; set; }

        public bool IsValid()
        {
            return (Math.Abs(WaterCriterionFactor + KeyWordsCriterionFactor + ZipfFactor - 100) < 0.001) &&
                   WaterCriterionLowerBound < WaterCriterionUpperBound &&
                   KeyWordsCriterionLowerBound < KeyWordsCriterionUpperBound &&
                   ZipfFactorLowerBound < ZipfFactorUpperBound;
        }
    }
}