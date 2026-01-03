using MealPrepHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using MealPrepHelper.Services;

namespace MealPrepHelper.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {

            context.Database.EnsureCreated();
            // 2. Kontrola, zda už jsou v databázi suroviny. Pokud ano, seeding přeskočíme.
            if (context.Ingredients.Any()) return;

            // --- A. Vytvoření Surovin (Katalog) ---
            var ingredients = new List<Ingredient>
            {
                new Ingredient { Name = "Kuřecí prsa (syrová)", Unit = "g", Calories = 110, Proteins = 23, Carbs = 0, Fats = 1, DietaryFiber = 0 },
                new Ingredient { Name = "Rýže Basmati (syrová)", Unit = "g", Calories = 360, Proteins = 8, Carbs = 77, Fats = 1, DietaryFiber = 1.5 },
                new Ingredient { Name = "Vejce (M)", Unit = "ks", Calories = 70, Proteins = 6, Carbs = 0.5, Fats = 5, DietaryFiber = 0 },
                new Ingredient { Name = "Olivový olej", Unit = "ml", Calories = 884, Proteins = 0, Carbs = 0, Fats = 100, DietaryFiber = 0 },
                new Ingredient { Name = "Ovesné vločky", Unit = "g", Calories = 370, Proteins = 13, Carbs = 59, Fats = 7, DietaryFiber = 10 },
                new Ingredient { Name = "Mléko polotučné", Unit = "ml", Calories = 47, Proteins = 3.3, Carbs = 4.8, Fats = 1.5, DietaryFiber = 0 },
                new Ingredient { Name = "Těstoviny (syrové)", Unit = "g", Calories = 350, Proteins = 12, Carbs = 70, Fats = 1.5, DietaryFiber = 3 },
                new Ingredient { Name = "Rajčatové pyré", Unit = "g", Calories = 30, Proteins = 1.5, Carbs = 6, Fats = 0.2, DietaryFiber = 1.5 },
                new Ingredient { Name = "Cibule", Unit = "ks", Calories = 40, Proteins = 1, Carbs = 9, Fats = 0.1, DietaryFiber = 1.7 },
                new Ingredient { Name = "Česnek", Unit = "stroužek", Calories = 4, Proteins = 0.2, Carbs = 1, Fats = 0, DietaryFiber = 0.1 }
            };

            context.Ingredients.AddRange(ingredients);
            context.SaveChanges(); // Uložíme, aby suroviny dostaly ID

            // --- B. Vytvoření Uživatele (Demo) ---
            if (context.Users.Any()) return;

            var user = new User
            {
                Username = "Admin",
                Email = "admin@mealprep.com",

                // ZMĚNA: Tady heslo "admin123" rovnou zahashujeme a uložíme výsledek
                PasswordHash = PasswordHelper.HashPassword("admin123"),

                HeightCm = 180,
                WeightKg = 80,
                Age = 30,
                Gender = "Male",
                ActivityLevel = "Active",
                DailyCalorieGoal = 2500,
                Protein = 200,
                Carbs = 300,
                Fat = 70,
                DietaryFiber = 30,
                Settings = new UserSettings { DarkMode = false }
            };

            context.Users.Add(user);
            context.SaveChanges(); // Uložíme uživatele, abychom měli jeho ID

            // --- C. Vytvoření Receptů (Systémové = CreatedByUserId je null) ---
            if (context.Recipes.Any()) return;
            var recipes = new List<Recipe>();

            // 1. Míchaná vajíčka
            var eggs = new Recipe
            {
                Name = "Míchaná vajíčka",
                CreatedByUserId = null,
                Ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { Ingredient = ingredients[2], Amount = 3 } // 3 vejce
                }
            };
            CalculateRecipeNutrition(eggs); // <-- Důležitá metoda dole
            recipes.Add(eggs);

            // 2. Kuře s rýží
            var chickenRice = new Recipe
            {
                Name = "Kuře s rýží",
                CreatedByUserId = null,
                Ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { Ingredient = ingredients[0], Amount = 150 }, // 150g kuře
                    new RecipeIngredient { Ingredient = ingredients[1], Amount = 100 }  // 100g rýže
                }
            };
            CalculateRecipeNutrition(chickenRice);
            recipes.Add(chickenRice);
            
            context.Recipes.AddRange(recipes);
            context.SaveChanges();

            // --- D. Naplnění Spižírny (Pantry) ---
            // Uživatel už má doma nějaké zásoby
            if (context.Pantry.Any()) return;

            var pantryItems = new List<PantryItem>
            {
                new PantryItem { User = user, Ingredient = ingredients[2], Amount = 10, Unit = "ks" }, // Má 10 vajec
                new PantryItem { User = user, Ingredient = ingredients[4], Amount = 500, Unit = "g" }, // Má 500g vloček
                new PantryItem { User = user, Ingredient = ingredients[1], Amount = 1000, Unit = "g" } // Má 1kg rýže
            };
            context.Pantry.AddRange(pantryItems);

            // --- E. Vytvoření Ukázkového Plánu ---
            if (context.MealPlans.Any()) return;

            var mealPlan = new MealPlan
            {
                User = user,
                Name = "Startovací týden",
                Items = new List<PlanItem>
                {
                    new PlanItem 
                    { 
                        Recipe = eggs,
                        ScheduledFor = DateTime.Now.Date.AddHours(8), // Dnes v 8:00
                        MealType = "Snídaně",
                        IsEaten = false 
                    },
                    new PlanItem 
                    { 
                        Recipe = chickenRice, 
                        ScheduledFor = DateTime.Now.Date.AddHours(12), // Dnes ve 12:00
                        MealType = "Oběd",
                        IsEaten = false 
                    }
                }
            };
            context.MealPlans.Add(mealPlan);

            // --- F. Finální uložení ---
            context.SaveChanges();
        }
        // --- Pomocná metoda pro výpočet nutričních hodnot receptu ---
        private static void CalculateRecipeNutrition(Recipe recipe)
        {
            double cals = 0, prot = 0, carbs = 0, fat = 0, fiber = 0;

            foreach (var ri in recipe.Ingredients)
            {
                // Převedeme množství na koeficient (např. 150g = 1.5x hodnota na 100g)
                double multiplier = ri.Ingredient.Unit == "ks" ? ri.Amount : ri.Amount / 100.0;

                cals += ri.Ingredient.Calories * multiplier;
                prot += ri.Ingredient.Proteins * multiplier;
                carbs += ri.Ingredient.Carbs * multiplier;
                fat += ri.Ingredient.Fats * multiplier;
                fiber += ri.Ingredient.DietaryFiber * multiplier;
            }

            recipe.TotalCalories = (int)Math.Round(cals);
            recipe.TotalProtein = Math.Round(prot, 1);
            recipe.TotalCarbs = Math.Round(carbs, 1);
            recipe.TotalFat = Math.Round(fat, 1);
            recipe.TotalFiber = Math.Round(fat, 1);
        }
    }
}