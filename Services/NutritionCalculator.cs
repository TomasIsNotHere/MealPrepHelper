using MealPrepHelper.Models; // Upravte dle vašeho namespace
namespace MealPrepHelper.Services{

public static class NutritionCalculator
{
    public static void CalculateAndSetGoals(User user)
    {
        // 1. Převod ActivityLevel (string) na číslo (multiplier)
        double activityMultiplier = GetActivityMultiplier(user.ActivityLevel);

        // 2. Výpočet BMR (Mifflin-St Jeor rovnice)
        double bmr;
        if (user.Gender == "Muž")
        {
            // (10 x váha) + (6.25 x výška) - (5 x věk) + 5
            bmr = (10 * user.WeightKg) + (6.25 * user.HeightCm) - (5 * user.Age) + 5;
        }
        else
        {
            // (10 x váha) + (6.25 x výška) - (5 x věk) - 161
            bmr = (10 * user.WeightKg) + (6.25 * user.HeightCm) - (5 * user.Age) - 161;
        }

        // 3. Celkové denní kalorie (TDEE)
        int totalCalories = (int)(bmr * activityMultiplier);
        user.DailyCalorieGoal = totalCalories;

        // 4. Výpočet maker (Příklad: Balanced Diet - 30% Bílkoviny, 35% Tuky, 35% Sacharidy)
        // 1g Bílkoviny = 4 kcal
        // 1g Sacharidy = 4 kcal
        // 1g Tuky = 9 kcal

        user.Protein = (int)((totalCalories * 0.30) / 4);
        user.Carbs = (int)((totalCalories * 0.35) / 4);
        user.Fat = (int)((totalCalories * 0.35) / 9);
        
        // Vláknina je obvykle 14g na každých 1000 kcal
        user.DietaryFiber = (int)((totalCalories / 1000.0) * 14);
    }    private static double GetActivityMultiplier(string levelName)
    {
        return levelName switch
        {
            "Sedavý" => 1.2,      // Sedavé
            "Lehký" => 1.375,        // Lehké (1-3x týdně)
            "Střední" => 1.55,      // Střední (3-5x týdně)
            "Aktivní" => 1.725,       // Aktivní (6-7x týdně)
            "Velmi aktivní" => 1.9,     // Velmi aktivní (fyzická práce + trénink)
            _ => 1.2                 // Default
        };
    }
}
}