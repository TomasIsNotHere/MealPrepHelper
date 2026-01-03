using ReactiveUI;
using System.Reactive;
using MealPrepHelper.Data;
using MealPrepHelper.Models; 
using System.Collections.Generic;
using MealPrepHelper.Services; // Důležité: Import pro NutritionCalculator
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

        private int _weight;
        public int Weight { get => _weight; set => this.RaiseAndSetIfChanged(ref _weight, value); }

        private int _height;
        public int Height { get => _height; set => this.RaiseAndSetIfChanged(ref _height, value); }

        private int _age;
        public int Age { get => _age; set => this.RaiseAndSetIfChanged(ref _age, value); }

        // Cíle (Makra)
        private int _goalCalories;
        public int GoalCalories { get => _goalCalories; set => this.RaiseAndSetIfChanged(ref _goalCalories, value); }

        private int _goalProtein;
        public int GoalProtein { get => _goalProtein; set => this.RaiseAndSetIfChanged(ref _goalProtein, value); }

        private int _goalCarbs;
        public int GoalCarbs { get => _goalCarbs; set => this.RaiseAndSetIfChanged(ref _goalCarbs, value); }

        private int _goalFat;
        public int GoalFat { get => _goalFat; set => this.RaiseAndSetIfChanged(ref _goalFat, value); }
        
        private int _goalFiber;
        public int GoalFiber { get => _goalFiber; set => this.RaiseAndSetIfChanged(ref _goalFiber, value); }

        // Zpráva o uložení
        public string StatusMessage 
        { 
            get => _statusMessage; 
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value); 
        }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        private string _activityLevel;
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

        private void SaveProfile()
        {
            using (var db = new AppDbContext())
            {
                var user = db.Users.Find(_userId);
                if (user != null)
                {
                    // Mapování z ViewModelu do DB
                    user.Username = Username;
                    user.WeightKg = Weight;
                    user.HeightCm = Height;
                    user.Age = Age;
                    user.ActivityLevel = ActivityLevel;
                    
                    NutritionCalculator.CalculateAndSetGoals(user);

                    GoalCalories = user.DailyCalorieGoal;
                    GoalProtein = user.Protein;
                    GoalCarbs = user.Carbs;
                    GoalFat = user.Fat;
                    GoalFiber = user.DietaryFiber;

                    db.SaveChanges();
                    StatusMessage = "✅ Uloženo!";
                }
            }
        }
    }
}