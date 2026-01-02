using MealPrepHelper.Models;

namespace MealPrepHelper.ViewModels
{
    public class IngredientCheckViewModel
    {
        public string Name { get; set; } = "";
        public double AmountNeeded { get; set; }
        public string Unit { get; set; } = "";
        
        // Kolik toho má uživatel ve spižírně
        public double AmountInPantry { get; set; }
        
        // Máme toho dost? (Skladem >= Potřeba)
        public bool HasEnough => AmountInPantry >= AmountNeeded;

        // Pomocné vlastnosti pro barvy a ikony
        public string StatusColor => HasEnough ? "#4CAF50" : "#F44336"; // Zelená vs Červená
        public string StatusIcon => HasEnough ? "✅" : "❌";
        public string StatusText => HasEnough 
            ? $"Máte: {AmountInPantry} {Unit}" 
            : $"Chybí (Máte jen {AmountInPantry} {Unit})";

        public IngredientCheckViewModel(RecipeIngredient ri, double pantryAmount)
        {
            Name = ri.Ingredient.Name;
            AmountNeeded = ri.Amount;
            Unit = ri.Ingredient.Unit;
            AmountInPantry = pantryAmount;
        }
    }
}