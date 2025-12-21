using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealPrepHelper.Models
{
    public class ShoppingListItem
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public int IngredientId { get; set; }
        [ForeignKey(nameof(IngredientId))]
        public Ingredient Ingredient { get; set; } = null!;

        public double Amount { get; set; }
        public bool IsBought { get; set; }

        public string Unit { get; set; } = string.Empty;

    }
}