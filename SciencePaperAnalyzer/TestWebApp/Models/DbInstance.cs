using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace WebPaperAnalyzer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public int? RoleId { get; set; }
        public Role Role { get; set; }
        public string TeacherLogin { get; set; }
    }
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<User> Users { get; set; }
        public Role()
        {
            Users = new List<User>();
        }
    }

    public class ResultCriterion
    {
        public int Id { get; set; }
        public string TeacherLogin { get; set; }
        public string Name { get; set; }
        public double ErrorCost { get; set; }
        public double WaterCriterionFactor { get; set; }
        public double KeyWordsCriterionFactor { get; set; }
        public double ZipfFactor { get; set; }
    }
}