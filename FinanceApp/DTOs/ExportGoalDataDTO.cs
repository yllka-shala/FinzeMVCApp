using FinanceApp.Models;
using System.ComponentModel.DataAnnotations;

namespace FinanceApp.DTOs
{
    public class ExportGoalDataDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double TargetAmount { get; set; }
        public double CurrentAmount { get; set; }
        public string DueDate { get; set; }
        public string CreatedDate { get; set; }
        public string User { get; set; }
        public double Progress { get; set; }
    }
}
