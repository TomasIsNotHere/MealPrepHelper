using ReactiveUI;
using System.Reactive;
using MealPrepHelper.Data;
using MealPrepHelper.Models; 
using System.Collections.Generic;
using MealPrepHelper.Services; 
using System.Linq;// Důležité: Import pro NutritionCalculator
// Zde je váš User model

namespace MealPrepHelper.ViewModels
{
    public class UserProfileViewModel : ViewModelBase
    {
        private readonly int _userId;
        private string _statusMessage = "";

        // Osobní údaje (Property názvy upravujeme pro View, ale mapujeme na Model)
        private string _username = "";
        public string Username { get => _username; set => this.RaiseAndSetIfChanged(ref _username, value); }

        private int? _weight;
        public int? Weight { get => _weight; set => this.RaiseAndSetIfChanged(ref _weight, value); }

        private int? _height;
        public int? Height { get => _height; set => this.RaiseAndSetIfChanged(ref _height, value); }

        private int? _age;
        public int? Age { get => _age; set => this.RaiseAndSetIfChanged(ref _age, value); }

        // Cíle (Makra)
        private int? _goalCalories;
        public int? GoalCalories { get => _goalCalories; set => this.RaiseAndSetIfChanged(ref _goalCalories, value); }

        private int? _goalProtein;
        public int? GoalProtein { get => _goalProtein; set => this.RaiseAndSetIfChanged(ref _goalProtein, value); }

        private int? _goalCarbs;
        public int? GoalCarbs { get => _goalCarbs; set => this.RaiseAndSetIfChanged(ref _goalCarbs, value); }

        private int? _goalFat;
        public int? GoalFat { get => _goalFat; set => this.RaiseAndSetIfChanged(ref _goalFat, value); }
        
        private int? _goalFiber;
        public int? GoalFiber { get => _goalFiber; set => this.RaiseAndSetIfChanged(ref _goalFiber, value); }

        // Zpráva o uložení
        public string StatusMessage 
        { 
            get => _statusMessage; 
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value); 
        }

        private string _statusColor = "Black";
public string StatusColor { get => _statusColor; set => this.RaiseAndSetIfChanged(ref _statusColor, value); }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        public ReactiveCommand<Unit, Unit> RecalculateCommand { get; }

        private string _activityLevel = "";
        public string ActivityLevel
        { 
            get => _activityLevel; 
            set => this.RaiseAndSetIfChanged(ref _activityLevel, value); 
        }
        public List<string> AvailableActivityLevels { get; } = new() 
        { 
            "Sedavý", 
            "Lehký", 
            "Střední", 
            "Aktivní", 
            "Velmi aktivní" 
        };

        public UserProfileViewModel(int userId)
        {
            _userId = userId;
            SaveCommand = ReactiveCommand.Create(SaveProfile);
            RecalculateCommand = ReactiveCommand.Create(RecalculateGoals);
            LoadData();
        }

        public void LoadData()
        {
            using (var db = new AppDbContext())
            {
                var user = db.Users.Find(_userId);
                if (user != null)
                {
                    // Mapování z DB do ViewModelu
                    Username = user.Username; // Podle obrázku je to Username
                    Weight = user.WeightKg;   // Podle obrázku je to WeightKg
                    Height = user.HeightCm;   // Podle obrázku je to HeightCm
                    Age = user.Age;
ActivityLevel = string.IsNullOrEmpty(user.ActivityLevel) ? "Střední" : user.ActivityLevel;
                    GoalCalories = user.DailyCalorieGoal;
                    GoalProtein = user.Protein;
                    GoalCarbs = user.Carbs;
                    GoalFat = user.Fat;
                    GoalFiber = user.DietaryFiber;
                }
            }
        }
        private void RecalculateGoals()
    {
        // Potřebujeme validaci, abychom nepočítali s nulami
        if (Weight == null || Height == null || Age == null)
        {
            StatusMessage = "⚠️ Pro výpočet vyplňte věk, váhu a výšku.";
            StatusColor = "Red";
            return;
        }

        using (var db = new AppDbContext())
        {
            var user = db.Users.Find(_userId);
            if (user != null)
            {
                // Dočasně nastavíme uživateli data z formuláře (pro výpočet)
                user.WeightKg = Weight.Value;
                user.HeightCm = Height.Value;
                user.Age = Age.Value;
                user.ActivityLevel = ActivityLevel;

                // Zavoláme kalkulačku
                NutritionCalculator.CalculateAndSetGoals(user);

                // Výsledek propíšeme DO FORMULÁŘE (do ViewModelu)
                GoalCalories = user.DailyCalorieGoal;
                GoalProtein = user.Protein;
                GoalCarbs = user.Carbs;
                GoalFat = user.Fat;
                GoalFiber = user.DietaryFiber;

                StatusMessage = "ℹ️ Hodnoty přepočítány (uložte změny).";
                StatusColor = "Blue";
            }
        }
    }

        private void SaveProfile()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            StatusMessage = "⚠️ Jméno nesmí být prázdné.";
            StatusColor = "Red";
            return;
        }
        
        // Tady validaci čísel nutně nepotřebujeme, pokud chcete dovolit uložit prázdné
        // Ale pro jistotu ji nechme, nebo použijte ?? 0
        
        using (var db = new AppDbContext())
        {
            // Kontrola jména...
            bool nameTaken = db.Users.Any(u => u.Username == Username && u.Id != _userId);
            if (nameTaken)
            {
                StatusMessage = "⚠️ Jméno je obsazené.";
                StatusColor = "Red";
                return;
            }

            var user = db.Users.Find(_userId);
            if (user != null)
            {
                // Uložení osobních údajů
                user.Username = Username;
                user.WeightKg = Weight ?? 0;
                user.HeightCm = Height ?? 0;
                user.Age = Age ?? 0;
                user.ActivityLevel = ActivityLevel;

                // --- ZMĚNA ZDE ---
                // Už nevoláme NutritionCalculator.CalculateAndSetGoals(user);
                // Místo toho vezmeme to, co je napsané v kolonkách, a uložíme to do DB.
                
                user.DailyCalorieGoal = GoalCalories ?? 0; // Uložíme vaši ruční hodnotu
                user.Protein = GoalProtein ?? 0;
                user.Carbs = GoalCarbs ?? 0;
                user.Fat = GoalFat ?? 0;
                user.DietaryFiber = GoalFiber ?? 0;

                db.SaveChanges();
                
                StatusMessage = "✅ Vše uloženo!";
                StatusColor = "#4CAF50";
            }
        }
    }
    }
}