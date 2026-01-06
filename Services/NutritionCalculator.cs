using MealPrepHelper.Models;
namespace MealPrepHelper.Services{

public static class NutritionCalculator
{
    public static void CalculateAndSetGoals(User user)
        {
            double activityMultiplier = GetActivityMultiplier(user.ActivityLevel);
            // bmi calculation
            double bmr;
            if (user.Gender == "Muž")
            {
                bmr = (10 * user.WeightKg) + (6.25 * user.HeightCm) - (5 * user.Age) + 5;
            }
            else
            {
                bmr = (10 * user.WeightKg) + (6.25 * user.HeightCm) - (5 * user.Age) - 161;
            }


            double calories = bmr * activityMultiplier;

            double targetCalories;

            string goal = user.Goal ?? "Udržení váhy";
            // total calores based on goal
            switch (goal)
            {
                case "Hubnutí":
                    targetCalories = calories - 500;
                    break;
                case "Nabírání svalů":
                    targetCalories = calories + 300;
                    break;
                case "Udržení váhy":
                default:
                    targetCalories = calories;
                    break;
            }
            // minimum calories
            if (targetCalories < 1200) targetCalories = 1200;

            user.DailyCalorieGoal = (int)targetCalories;

            user.Protein = (int)((targetCalories * 0.30) / 4);
            user.Carbs = (int)((targetCalories * 0.35) / 4);
            user.Fat = (int)((targetCalories * 0.35) / 9);
            user.DietaryFiber = (int)((targetCalories / 1000.0) * 14);
        }

    // activity level to multiplier
    private static double GetActivityMultiplier(string levelName)
    {
        return levelName switch
        {
            "Sedavý" => 1.2,
            "Lehký" => 1.375,
            "Střední" => 1.55,
            "Aktivní" => 1.725,
            "Velmi aktivní" => 1.9,
            _ => 1.2
        };
    }
}
}