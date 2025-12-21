using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MealPrepHelper.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public int HeightCm { get; set; }
        public int WeightKg { get; set; }
        public int Age { get; set; }
        public string ActivityLevel { get; set; } = string.Empty; // Sedentary, Lightly Active, Active, Very Active
        public int DailyCalorieGoal { get; set; }

        public int Fat { get; set; }
        public int Carbs { get; set; }
        public int DietaryFiber { get; set; }
        public int Protein { get; set; }


        // Vazby
        public UserSettings? Settings { get; set; }
        public List<MealPlan> Plans { get; set; } = new();
        public List<PantryItem> Pantry { get; set; } = new();
        public List<ShoppingListItem> ShoppingList { get; set; } = new();
    }
}