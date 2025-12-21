using System.ComponentModel.DataAnnotations;

namespace MealPrepHelper.Models
{
    public class Ingredient
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Nutriční hodnoty
        public double Calories { get; set; }
        public double Proteins { get; set; }
        public double Carbs { get; set; }
        public double Fats { get; set; }
        public double DietaryFiber { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
//upravene modely jen naplnit data a  enum