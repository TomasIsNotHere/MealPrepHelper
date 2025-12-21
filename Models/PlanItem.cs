using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPrepHelper.Models
{
    public class PlanItem
    {
        [Key]
        public int Id { get; set; }

        public int MealPlanId { get; set; }
        [ForeignKey(nameof(MealPlanId))]
        public MealPlan MealPlan { get; set; } = null!;

        public DateTime ScheduledFor { get; set; }
        public string MealType { get; set; } = string.Empty;

        public int RecipeId { get; set; }
        [ForeignKey(nameof(RecipeId))]
        public Recipe Recipe { get; set; } = null!;
        public bool IsEaten { get; set; }
    }
}