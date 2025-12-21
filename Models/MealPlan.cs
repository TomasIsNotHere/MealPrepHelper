using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPrepHelper.Models
{
    public class MealPlan
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public List<PlanItem> Items { get; set; } = new();
    }
}