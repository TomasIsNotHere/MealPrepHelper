using MealPrepHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using MealPrepHelper.Services;

namespace MealPrepHelper.Data
{
    public static class DbInitializer
    {
        // db initialization method
        public static void Initialize(AppDbContext context)
        {
            // db creation if needed
            context.Database.EnsureCreated();

            if (context.Ingredients.Any()) return;

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
                new Ingredient { Name = "Česnek", Unit = "stroužek", Calories = 4, Proteins = 0.2, Carbs = 1, Fats = 0, DietaryFiber = 0.1 },
                new Ingredient { Name = "Sýr Eidam 30%", Unit = "g", Calories = 260, Proteins = 27, Carbs = 1, Fats = 16, DietaryFiber = 0 },
                new Ingredient { Name = "Jablko", Unit = "ks", Calories = 52, Proteins = 0.3, Carbs = 14, Fats = 0.2, DietaryFiber = 2.4 },
                new Ingredient { Name = "Máslo", Unit = "g", Calories = 717, Proteins = 0.9, Carbs = 0.1, Fats = 81, DietaryFiber = 0 }
            };

            context.Ingredients.AddRange(ingredients);
            context.SaveChanges();

            if (context.Users.Any()) return;

            var user = new User
            {
                Username = "Admin",
                Email = "admin@mealprep.com",
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
            };

            context.Users.Add(user);
            context.SaveChanges();

            if (context.Recipes.Any()) return;
            var recipes = new List<Recipe>();

            var eggs = new Recipe
            {
                Name = "Míchaná vajíčka na cibulce",
                CreatedByUserId = null,
                Instructions = "1. Cibuli nakrájejte nadrobno. Vejce rozklepněte do hrnku a lehce prošlehejte vidličkou.\n2. Na pánvi rozehřejte máslo/olej a nechte na něm zesklovatět cibulku.\n3. Ztlumte plamen na minimum a vlijte vajíčka.\n4. Za stálého míchání je pomalu zahřívejte do zhoustnutí.\n5. Osolte, opepřete a podávejte s pečivem.",
                Ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { Ingredient = ingredients[2], Amount = 3 },
                    new RecipeIngredient { Ingredient = ingredients[8], Amount = 0.5 },
                    new RecipeIngredient { Ingredient = ingredients[12], Amount = 10 }
                }
            };
            CalculateRecipeNutrition(eggs);
            recipes.Add(eggs);

            var chickenRice = new Recipe
            {
                Name = "Kuřecí plátek s rýží",
                CreatedByUserId = null,
                Instructions = "1. Maso omyjte, osušte a nakrájejte na plátky.\n2. Smíchejte s trochou oleje a kořením.\n3. Rýži propláchněte a dejte vařit (1 díl rýže na 1.5 dílu vody).\n4. Maso zprudka orestujte na pánvi, poté stáhněte plamen a nechte dojít.\n5. Podávejte maso s rýží a výpekem.",
                Ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { Ingredient = ingredients[0], Amount = 150 },
                    new RecipeIngredient { Ingredient = ingredients[1], Amount = 100 },
                    new RecipeIngredient { Ingredient = ingredients[3], Amount = 10 }
                },
            };
            CalculateRecipeNutrition(chickenRice);
            recipes.Add(chickenRice);

            var oatmeal = new Recipe
            {
                Name = "Ranní ovesná kaše s jablkem",
                CreatedByUserId = null,
                Instructions = "1. V hrnci smíchejte mléko a vločky.\n2. Přiveďte k varu a za stálého míchání vařte cca 2-3 minuty do zhoustnutí.\n3. Jablko nastrouhejte nebo nakrájejte na kostičky.\n4. Kaši stáhněte z plotny, vmíchejte jablko a případně doslaďte medem nebo skořicí.",
                Ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { Ingredient = ingredients[4], Amount = 60 },
                    new RecipeIngredient { Ingredient = ingredients[5], Amount = 250 },
                    new RecipeIngredient { Ingredient = ingredients[11], Amount = 1 }
                }
            };
            CalculateRecipeNutrition(oatmeal);
            recipes.Add(oatmeal);

            var pastaTomato = new Recipe
            {
                Name = "Těstoviny s rajčatovou omáčkou",
                CreatedByUserId = null,
                Instructions = "1. Těstoviny uvařte v osolené vodě al dente.\n2. Na pánvi orestujte nakrájenou cibuli a česnek na oleji.\n3. Přidejte rajčatové pyré, osolte, opepřete a nechte 5 minut provařit.\n4. Smíchejte uvařené těstoviny s omáčkou a posypte sýrem.",
                Ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { Ingredient = ingredients[6], Amount = 120 },
                    new RecipeIngredient { Ingredient = ingredients[7], Amount = 150 },
                    new RecipeIngredient { Ingredient = ingredients[8], Amount = 0.5 },
                    new RecipeIngredient { Ingredient = ingredients[9], Amount = 1 },
                    new RecipeIngredient { Ingredient = ingredients[3], Amount = 10 },
                    new RecipeIngredient { Ingredient = ingredients[10], Amount = 30 }
                }
            };
            CalculateRecipeNutrition(pastaTomato);
            recipes.Add(pastaTomato);

            var omelette = new Recipe
            {
                Name = "Vaječná omeleta se sýrem",
                CreatedByUserId = null,
                Instructions = "1. Vejce rozšlehejte v misce se špetkou soli a pepře.\n2. Rozehřejte pánev s trochou másla.\n3. Vlijte vejce a nechte je ztuhnout (bez míchání).\n4. Když jsou vejce téměř hotová, posypte je nastrouhaným sýrem.\n5. Omeletu přeložte napůl a nechte sýr rozpustit.",
                Ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { Ingredient = ingredients[2], Amount = 3 },
                    new RecipeIngredient { Ingredient = ingredients[10], Amount = 50 },
                    new RecipeIngredient { Ingredient = ingredients[12], Amount = 10 }
                }
            };
            CalculateRecipeNutrition(omelette);
            recipes.Add(omelette);

            var garlicChicken = new Recipe
            {
                Name = "Kuřecí nudličky na česneku",
                CreatedByUserId = null,
                Instructions = "1. Kuřecí maso nakrájejte na tenké nudličky.\n2. Na pánvi rozpalte olej, přidejte maso a zprudka orestujte dozlatova.\n3. Přidejte prolisovaný česnek a krátce orestujte (aby nezhořkl).\n4. Podlijte trochou vody, osolte a nechte chvíli podusit.\n5. Podávejte s rýží.",
                Ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { Ingredient = ingredients[0], Amount = 150 },
                    new RecipeIngredient { Ingredient = ingredients[9], Amount = 3 },
                    new RecipeIngredient { Ingredient = ingredients[3], Amount = 15 },
                    new RecipeIngredient { Ingredient = ingredients[1], Amount = 80 }
                }
            };
            CalculateRecipeNutrition(garlicChicken);
            recipes.Add(garlicChicken);

            var pancakes = new Recipe
            {
                Name = "Fit ovesné lívance",
                CreatedByUserId = null,
                Instructions = "1. Ovesné vločky rozmixujte na mouku (nebo použijte jemné).\n2. Smíchejte s vejcem a trochou mléka, aby vzniklo hustší těstíčko.\n3. Nechte 5 minut odležet, aby vločky nasákly.\n4. Na pánvi tvořte lžící malé lívanečky a opékejte z obou stran dozlatova.",
                Ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { Ingredient = ingredients[4], Amount = 70 },
                    new RecipeIngredient { Ingredient = ingredients[2], Amount = 1 },
                    new RecipeIngredient { Ingredient = ingredients[5], Amount = 50 },
                    new RecipeIngredient { Ingredient = ingredients[3], Amount = 5 }
                }
            };
            CalculateRecipeNutrition(pancakes);
            recipes.Add(pancakes);

            var bakedPasta = new Recipe
            {
                Name = "Rychlé zapečené těstoviny",
                CreatedByUserId = null,
                Instructions = "1. Těstoviny uvařte asi na 70% (budou se ještě péct).\n2. Vejce rozšlehejte s mlékem, solí a pepřem.\n3. Těstoviny smíchejte s nastrouhaným sýrem a dejte do pekáčku.\n4. Zalijte vaječnou směsí.\n5. Pečte v troubě na 180°C cca 20-30 minut dozlatova.",
                Ingredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { Ingredient = ingredients[6], Amount = 100 },
                    new RecipeIngredient { Ingredient = ingredients[2], Amount = 2 },
                    new RecipeIngredient { Ingredient = ingredients[5], Amount = 100 },
                    new RecipeIngredient { Ingredient = ingredients[10], Amount = 50 }
                }
            };
            CalculateRecipeNutrition(bakedPasta);
            recipes.Add(bakedPasta);
            
            context.Recipes.AddRange(recipes);
            context.SaveChanges();

            if (context.Pantry.Any()) return;

            var pantryItems = new List<PantryItem>
            {
                new PantryItem { User = user, Ingredient = ingredients[2], Amount = 10, Unit = "ks" },
                new PantryItem { User = user, Ingredient = ingredients[4], Amount = 500, Unit = "g" },
                new PantryItem { User = user, Ingredient = ingredients[1], Amount = 1000, Unit = "g" },
                new PantryItem { User = user, Ingredient = ingredients[6], Amount = 500, Unit = "g" },
                new PantryItem { User = user, Ingredient = ingredients[7], Amount = 400, Unit = "g" }
            };
            context.Pantry.AddRange(pantryItems);

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
                        ScheduledFor = DateTime.Now.Date.AddHours(8), 
                        MealType = "Snídaně",
                        IsEaten = false 
                    },
                    new PlanItem 
                    { 
                        Recipe = chickenRice, 
                        ScheduledFor = DateTime.Now.Date.AddHours(12),
                        MealType = "Oběd",
                        IsEaten = false 
                    },
                    new PlanItem 
                    { 
                        Recipe = pastaTomato, 
                        ScheduledFor = DateTime.Now.Date.AddHours(18),
                        MealType = "Večeře",
                        IsEaten = false 
                    }
                }
            };
            context.MealPlans.Add(mealPlan);

            context.SaveChanges();
        }
        // calculation of recipe nutrition values
        private static void CalculateRecipeNutrition(Recipe recipe)
        {
            double cals = 0, prot = 0, carbs = 0, fat = 0, fiber = 0;

            foreach (var ri in recipe.Ingredients)
            {
                double multiplier = ri.Ingredient.Unit == "ks" || ri.Ingredient.Unit == "stroužek"
                    ? ri.Amount
                    : ri.Amount / 100.0;

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
            recipe.TotalFiber = Math.Round(fiber, 1);
        }
    }
}