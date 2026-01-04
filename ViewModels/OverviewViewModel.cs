using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;
using System.Reactive;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using MealPrepHelper.Data;
using MealPrepHelper.Models;
using System.Windows.Input;

namespace MealPrepHelper.ViewModels
{
   public class OverviewViewModel : ViewModelBase
    {
        private readonly int _currentUserId;
        
        // === DATA PRO KALENDÁŘ (Týdenní lišta) ===
        private DateTime _currentDate;
        public DateTime CurrentDate 
        {
            get => _currentDate;
            set 
            {
                this.RaiseAndSetIfChanged(ref _currentDate, value);
                LoadData();
            }
        }

        private string _weekRangeText = string.Empty;
        public string WeekRangeText 
        {
            get => _weekRangeText;
            set => this.RaiseAndSetIfChanged(ref _weekRangeText, value);
        }

        public ObservableCollection<CalendarDayViewModel> WeekDays { get; } = new();
        public ObservableCollection<PlanItem> TodayMeals { get; } = new();
        public ICommand UpdateMealStatusCommand { get; }  = null!;

        // === HODNOTY (Values) ===
        private int _currentCalories; public int CurrentCalories { get => _currentCalories; set => this.RaiseAndSetIfChanged(ref _currentCalories, value); }
        private int _goalCalories; public int GoalCalories { get => _goalCalories; set => this.RaiseAndSetIfChanged(ref _goalCalories, value); }
        
        private double _currentProtein; public double CurrentProtein { get => _currentProtein; set => this.RaiseAndSetIfChanged(ref _currentProtein, value); }
        private int _goalProtein; public int GoalProtein { get => _goalProtein; set => this.RaiseAndSetIfChanged(ref _goalProtein, value); }

        private double _currentCarbs; public double CurrentCarbs { get => _currentCarbs; set => this.RaiseAndSetIfChanged(ref _currentCarbs, value); }
        private int _goalCarbs; public int GoalCarbs { get => _goalCarbs; set => this.RaiseAndSetIfChanged(ref _goalCarbs, value); }

        private double _currentFat; public double CurrentFat { get => _currentFat; set => this.RaiseAndSetIfChanged(ref _currentFat, value); }
        private int _goalFat; public int GoalFat { get => _goalFat; set => this.RaiseAndSetIfChanged(ref _goalFat, value); }

        private double _currentFiber; public double CurrentFiber { get => _currentFiber; set => this.RaiseAndSetIfChanged(ref _currentFiber, value); }
        private int _goalFiber; public int GoalFiber { get => _goalFiber; set => this.RaiseAndSetIfChanged(ref _goalFiber, value); }

        // === ÚHLY PRO GRAFY (0 až 360) ===
        private double _angleCalories; public double AngleCalories { get => _angleCalories; set => this.RaiseAndSetIfChanged(ref _angleCalories, value); }
        private double _angleProtein; public double AngleProtein { get => _angleProtein; set => this.RaiseAndSetIfChanged(ref _angleProtein, value); }
        private double _angleCarbs; public double AngleCarbs { get => _angleCarbs; set => this.RaiseAndSetIfChanged(ref _angleCarbs, value); }
        private double _angleFat; public double AngleFat { get => _angleFat; set => this.RaiseAndSetIfChanged(ref _angleFat, value); }
        private double _angleFiber; public double AngleFiber { get => _angleFiber; set => this.RaiseAndSetIfChanged(ref _angleFiber, value); }

        // === BARVY GRAFŮ (Dynamické - chyběly vám) ===
        // Toto opraví chyby "Unable to resolve property ColorCalories"
        private string _colorCalories = "#FF5722"; public string ColorCalories { get => _colorCalories; set => this.RaiseAndSetIfChanged(ref _colorCalories, value); }
        private string _colorProtein = "#2196F3"; public string ColorProtein { get => _colorProtein; set => this.RaiseAndSetIfChanged(ref _colorProtein, value); }
        private string _colorCarbs = "#FFC107"; public string ColorCarbs { get => _colorCarbs; set => this.RaiseAndSetIfChanged(ref _colorCarbs, value); }
        private string _colorFat = "#4CAF50"; public string ColorFat { get => _colorFat; set => this.RaiseAndSetIfChanged(ref _colorFat, value); }
        private string _colorFiber = "#9C27B0"; public string ColorFiber { get => _colorFiber; set => this.RaiseAndSetIfChanged(ref _colorFiber, value); }

        // === PŘÍKAZY ===
        public ReactiveCommand<Unit, Unit> NextWeekCommand { get; } = null!;
        public ReactiveCommand<Unit, Unit> PrevWeekCommand { get; } = null!;
        public ReactiveCommand<CalendarDayViewModel, Unit> SelectDayCommand { get; } = null!;

// === NOVÉ: DETAILNÍ POPUP ===
        
        private bool _isInfoVisible;
        public bool IsInfoVisible 
        { 
            get => _isInfoVisible; 
            set => this.RaiseAndSetIfChanged(ref _isInfoVisible, value); 
        }

        private PlanItem? _selectedPlanItem;
        public PlanItem? SelectedPlanItem 
        { 
            get => _selectedPlanItem; 
            set => this.RaiseAndSetIfChanged(ref _selectedPlanItem, value); 
        }

        // === NOVÉ PŘÍKAZY ===
        public ReactiveCommand<PlanItem, Unit> OpenInfoCommand { get; } = null!;
        public ReactiveCommand<Unit, Unit> CloseInfoCommand { get; } = null!;
        public ReactiveCommand<PlanItem, Unit> DeleteItemCommand { get; } = null!;
        
        // (Editaci zde připravíme jako příkaz, ale logika by vyžadovala formulář jako v kalendáři.
        // Prozatím uděláme alespoň mazání, které je snadné).
        public ReactiveCommand<PlanItem, Unit> EditItemCommand { get; } = null!;

        public ObservableCollection<IngredientCheckViewModel> RecipeIngredientsCheck { get; } = new();


        public OverviewViewModel(int userId)
        {
            _currentUserId = userId;
            _currentDate = DateTime.Today;

            NextWeekCommand = ReactiveCommand.Create(() => ChangeDate(7));
            PrevWeekCommand = ReactiveCommand.Create(() => ChangeDate(-7));
            SelectDayCommand = ReactiveCommand.Create<CalendarDayViewModel>(day => 
            {
                ChangeDate((day.Date - CurrentDate).Days);
            });
            OpenInfoCommand = ReactiveCommand.Create<PlanItem>(item => 
            {
                SelectedPlanItem = item;
                IsInfoVisible = true;
            });

            CloseInfoCommand = ReactiveCommand.Create(() => 
            {
                IsInfoVisible = false;
                SelectedPlanItem = null;
            });

            DeleteItemCommand = ReactiveCommand.Create<PlanItem>(DeleteItem);
            
            // Editace je složitější (potřebuje formulář), pro teď necháme prázdné nebo jen zavření
            EditItemCommand = ReactiveCommand.Create<PlanItem>(item => 
            {
                // Zde by se musel otevřít editor. Pro jednoduchost zatím jen logujeme nebo nic.
                // Ideálně by to přepnulo na Kalendář do editačního režimu.
            });
            UpdateMealStatusCommand = ReactiveCommand.Create<PlanItem>(UpdateMealStatus);

            OpenInfoCommand = ReactiveCommand.Create<PlanItem>(OpenInfoWithCheck); // Změna metody
            LoadData(); 
        }
        public OverviewViewModel() {}
        private void OpenInfoWithCheck(PlanItem item)
{
    SelectedPlanItem = item;
    
    // Načteme a zkontrolujeme ingredience
    CheckIngredientsAvailability(item);
    
    IsInfoVisible = true;
}
private void CheckIngredientsAvailability(PlanItem item)
{
    RecipeIngredientsCheck.Clear();

    using (var db = new AppDbContext())
    {
        // 1. Načteme ingredience receptu (pokud nejsou načtené)
        var recipeIngredients = db.RecipeIngredients
            .Include(ri => ri.Ingredient)
            .Where(ri => ri.RecipeId == item.RecipeId)
            .ToList();

        // 2. Načteme aktuální stav spižírny pro tyto suroviny
        // Získáme seznam ID ingrediencí v receptu
        var ingredientIds = recipeIngredients.Select(r => r.IngredientId).ToList();
        
        var pantryItems = db.Pantry
            .Where(p => p.UserId == _currentUserId && ingredientIds.Contains(p.IngredientId))
            .ToList();

        // 3. Spárujeme a vytvoříme ViewModel
        foreach (var ri in recipeIngredients)
        {
            // Najdeme odpovídající položku ve spižírně
            var pantryItem = pantryItems.FirstOrDefault(p => p.IngredientId == ri.IngredientId);
            double amountInPantry = pantryItem?.Amount ?? 0;

RecipeIngredientsCheck.Add(new IngredientCheckViewModel(ri, amountInPantry, _currentUserId));        }
    }
}        private void DeleteItem(PlanItem item)
        {
            if (item == null) return;

            using (var db = new AppDbContext())
            {
                var dbItem = db.PlanItems.Find(item.Id);
                if (dbItem != null)
                {
                    db.PlanItems.Remove(dbItem);
                    db.SaveChanges();
                }
            }
            
            // Zavřít popup a obnovit data
            IsInfoVisible = false;
            LoadData(); // Toto překreslí seznam i makra
        }
        private void ChangeDate(int daysToAdd)
        {
            CurrentDate = CurrentDate.AddDays(daysToAdd);
            // LoadData se volá automaticky v setteru CurrentDate
        }

        public void LoadData()
        {
            GenerateWeek();
            TodayMeals.Clear();
            
            using (var db = new AppDbContext())
            {
                var user = db.Users.Find(_currentUserId);
                if (user != null)
                {
                    GoalCalories = user.DailyCalorieGoal;
                    GoalProtein = user.Protein;      // Používám vaše názvy z User modelu
                    GoalCarbs = user.Carbs;
                    GoalFat = user.Fat;
                    GoalFiber = user.DietaryFiber;
                }

                var meals = db.PlanItems
                    .Include(p => p.Recipe)
                    .ThenInclude(r => r.Ingredients) // 2. Načte seznam ingrediencí (RecipeIngredient)
                    .ThenInclude(ri => ri.Ingredient)
                    .Where(p => p.MealPlan.UserId == _currentUserId 
                             && p.ScheduledFor.Date == CurrentDate.Date)
                    .OrderBy(p => p.ScheduledFor)
                    .ToList();

                foreach (var meal in meals) TodayMeals.Add(meal);

                RecalculateEaten();
            }
        }

        private void GenerateWeek()
        {
            WeekDays.Clear();
            int diff = (7 + (CurrentDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            var monday = CurrentDate.Date.AddDays(-diff);
            var sunday = monday.AddDays(6);

            WeekRangeText = $"{monday:d.M.} - {sunday:d.M. yyyy}";
            var czechCulture = new CultureInfo("cs-CZ");

            for (int i = 0; i < 7; i++)
            {
                var dayDate = monday.AddDays(i);
                bool isSelected = dayDate == CurrentDate.Date;
                bool isToday = dayDate == DateTime.Today;

                var dayVm = new CalendarDayViewModel
                {
                    Date = dayDate,
                    DayName = czechCulture.DateTimeFormat.GetAbbreviatedDayName(dayDate.DayOfWeek),
                    DayNumber = dayDate.Day.ToString(),
                    Background = isSelected ? "#4CAF50" : (isToday ? "#E8F5E9" : "White"),
                    Foreground = isSelected ? "White" : "Black",
                    FontWeight = isSelected ? "Bold" : "Normal"
                };
                WeekDays.Add(dayVm);
            }
        }

        private void RecalculateEaten()
        {
            var eatenMeals = TodayMeals.Where(x => x.IsEaten).ToList();

            // 1. Součty
            CurrentCalories = eatenMeals.Sum(x => x.Recipe.TotalCalories);
            CurrentProtein = Math.Round(eatenMeals.Sum(x => x.Recipe.TotalProtein), 1);
            CurrentCarbs = Math.Round(eatenMeals.Sum(x => x.Recipe.TotalCarbs), 1);
            CurrentFat = Math.Round(eatenMeals.Sum(x => x.Recipe.TotalFat), 1);
            CurrentFiber = Math.Round(eatenMeals.Sum(x => x.Recipe.TotalFiber), 1);

            // 2. Úhly
            AngleCalories = CalculateAngle(CurrentCalories, GoalCalories);
            AngleProtein = CalculateAngle(CurrentProtein, GoalProtein);
            AngleCarbs = CalculateAngle(CurrentCarbs, GoalCarbs);
            AngleFat = CalculateAngle(CurrentFat, GoalFat);
            AngleFiber = CalculateAngle(CurrentFiber, GoalFiber);

            // 3. Barvy (Červená při překročení)
            string warningColor = "#D32F2F"; // Červená

            ColorCalories = (CurrentCalories > GoalCalories) ? warningColor : "#FF5722";
            ColorProtein = (CurrentProtein > GoalProtein) ? warningColor : "#2196F3";
            ColorCarbs = (CurrentCarbs > GoalCarbs) ? warningColor : "#FFC107";
            ColorFat = (CurrentFat > GoalFat) ? warningColor : "#4CAF50";
            ColorFiber = (CurrentFiber > GoalFiber) ? warningColor : "#9C27B0";
        }

        private double CalculateAngle(double current, double goal)
        {
            if (goal <= 0) return 0;
            double pct = current / goal;
            if (pct > 1) pct = 1;
            return pct * 360;
        }

        public void UpdateMealStatus(PlanItem item)
        {
            using (var db = new AppDbContext())
            {
                var dbItem = db.PlanItems.Find(item.Id);
                if (dbItem != null)
                {
                    dbItem.IsEaten = item.IsEaten;
                    db.SaveChanges();
                }
            }
            RecalculateEaten();
        }
    }

    // Pomocná třída pro týdenní lištu v Overview
    public class CalendarDayViewModel : ViewModelBase
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = string.Empty;
        public string DayNumber { get; set; } = string.Empty;
        
        private string _background = "White";
        public string Background { get => _background; set => this.RaiseAndSetIfChanged(ref _background, value); }
        
        private string _foreground = "Black";
        public string Foreground { get => _foreground; set => this.RaiseAndSetIfChanged(ref _foreground, value); }

        private string _fontWeight = "Normal";
        public string FontWeight { get => _fontWeight; set => this.RaiseAndSetIfChanged(ref _fontWeight, value); }
    }
}