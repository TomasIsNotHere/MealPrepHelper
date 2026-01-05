using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MealPrepHelper.Models
{
    public class Recipe
    {
        [Key]
        public int Id { get; set; }
        public int? CreatedByUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public int PrepTimeMinutes { get; set; }
        public int TotalCalories { get; set; }
        public double TotalProtein { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalFat { get; set; }
        public double TotalFiber { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public List<RecipeIngredient> Ingredients { get; set; } = new();
    }
}