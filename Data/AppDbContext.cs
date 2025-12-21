using Microsoft.EntityFrameworkCore;
using MealPrepHelper.Models;

namespace MealPrepHelper.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserSettings> Settings { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<MealPlan> MealPlans { get; set; }
        public DbSet<PlanItem> PlanItems { get; set; }
        public DbSet<PantryItem> Pantry { get; set; }
        public DbSet<ShoppingListItem> ShoppingList { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Název výsledného souboru
            optionsBuilder.UseSqlite("Data Source=mealprep.db");
        }
    }
}