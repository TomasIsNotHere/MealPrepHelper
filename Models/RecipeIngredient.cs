using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPrepHelper.Models
{
    public class RecipeIngredient
    {
        [Key]
        public int Id { get; set; }
        public int RecipeId { get; set; }
        [ForeignKey(nameof(RecipeId))]
        public Recipe Recipe { get; set; } = null!;
        public int IngredientId { get; set; }
        [ForeignKey(nameof(IngredientId))]
        public Ingredient Ingredient { get; set; } = null!;
        public double Amount { get; set; }
    }
}