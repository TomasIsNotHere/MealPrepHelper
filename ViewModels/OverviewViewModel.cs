using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;
using System.Reactive;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using MealPrepHelper.Data;
using MealPrepHelper.Models;

namespace MealPrepHelper.ViewModels
{
    public class OverviewViewModel : ViewModelBase
    {
        private readonly int _currentUserId;

        // data
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

        // goals and current values
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

        // ui specs
        private double _angleCalories; public double AngleCalories { get => _angleCalories; set => this.RaiseAndSetIfChanged(ref _angleCalories, value); }
        private double _angleProtein; public double AngleProtein { get => _angleProtein; set => this.RaiseAndSetIfChanged(ref _angleProtein, value); }
        private double _angleCarbs; public double AngleCarbs { get => _angleCarbs; set => this.RaiseAndSetIfChanged(ref _angleCarbs, value); }
        private double _angleFat; public double AngleFat { get => _angleFat; set => this.RaiseAndSetIfChanged(ref _angleFat, value); }
        private double _angleFiber; public double AngleFiber { get => _angleFiber; set => this.RaiseAndSetIfChanged(ref _angleFiber, value); }

        private string _colorCalories = "#FF5722"; public string ColorCalories { get => _colorCalories; set => this.RaiseAndSetIfChanged(ref _colorCalories, value); }
        private string _colorProtein = "#2196F3"; public string ColorProtein { get => _colorProtein; set => this.RaiseAndSetIfChanged(ref _colorProtein, value); }
        private string _colorCarbs = "#FFC107"; public string ColorCarbs { get => _colorCarbs; set => this.RaiseAndSetIfChanged(ref _colorCarbs, value); }
        private string _colorFat = "#4CAF50"; public string ColorFat { get => _colorFat; set => this.RaiseAndSetIfChanged(ref _colorFat, value); }
        private string _colorFiber = "#9C27B0"; public string ColorFiber { get => _colorFiber; set => this.RaiseAndSetIfChanged(ref _colorFiber, value); }

        // popup detail
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

        public ObservableCollection<IngredientCheckViewModel> RecipeIngredientsCheck { get; } = new();

        // commands
        public ReactiveCommand<Unit, Unit> NextWeekCommand { get; } = null!;
        public ReactiveCommand<Unit, Unit> PrevWeekCommand { get; } = null!;
        public ReactiveCommand<CalendarDayViewModel, Unit> SelectDayCommand { get; } = null!;

        public ReactiveCommand<PlanItem, Unit> OpenInfoCommand { get; } = null!;
        public ReactiveCommand<Unit, Unit> CloseInfoCommand { get; } = null!;

        public ReactiveCommand<PlanItem, Unit> DeleteItemCommand { get; } = null!;
        public ReactiveCommand<PlanItem, Unit> EditItemCommand { get; } = null!;
        public ReactiveCommand<PlanItem, Unit> UpdateMealStatusCommand { get; } = null!;

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

            OpenInfoCommand = ReactiveCommand.Create<PlanItem>(OpenInfoWithCheck);

            CloseInfoCommand = ReactiveCommand.Create(() =>
            {
                IsInfoVisible = false;
                SelectedPlanItem = null;
            });

            DeleteItemCommand = ReactiveCommand.Create<PlanItem>(DeleteItem);
            UpdateMealStatusCommand = ReactiveCommand.Create<PlanItem>(UpdateMealStatus);
            EditItemCommand = ReactiveCommand.Create<PlanItem>(item => { });

            LoadData();
        }

        // detail logic
        private void OpenInfoWithCheck(PlanItem item)
        {
            SelectedPlanItem = item;
            CheckIngredientsAvailability(item);
            IsInfoVisible = true;
        }

        private void CheckIngredientsAvailability(PlanItem item)
        {
            RecipeIngredientsCheck.Clear();

            using (var db = new AppDbContext())
            {
                var recipeIngredients = db.RecipeIngredients
                    .Include(ri => ri.Ingredient)
                    .Where(ri => ri.RecipeId == item.RecipeId)
                    .ToList();

                var ingredientIds = recipeIngredients.Select(r => r.IngredientId).ToList();

                var pantryItems = db.Pantry
                    .Where(p => p.UserId == _currentUserId && ingredientIds.Contains(p.IngredientId))
                    .ToList();

                foreach (var ri in recipeIngredients)
                {
                    var pantryItem = pantryItems.FirstOrDefault(p => p.IngredientId == ri.IngredientId);
                    double amountInPantry = pantryItem?.Amount ?? 0;

                    RecipeIngredientsCheck.Add(new IngredientCheckViewModel(ri, amountInPantry, _currentUserId));
                }
            }
        }

        private void DeleteItem(PlanItem item)
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

            IsInfoVisible = false;
            LoadData();
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

        // calendar logic
        private void ChangeDate(int daysToAdd)
        {
            CurrentDate = CurrentDate.AddDays(daysToAdd);
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
                    GoalProtein = user.Protein;
                    GoalCarbs = user.Carbs;
                    GoalFat = user.Fat;
                    GoalFiber = user.DietaryFiber;
                }

                var meals = db.PlanItems
                    .Include(p => p.Recipe)
                    .ThenInclude(r => r.Ingredients)
                    .ThenInclude(ri => ri.Ingredient)
                    .Where(p => p.MealPlan.UserId == _currentUserId && p.ScheduledFor.Date == CurrentDate.Date)
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

        // calculation logic
        private void RecalculateEaten()
        {
            var eatenMeals = TodayMeals.Where(x => x.IsEaten).ToList();

            CurrentCalories = eatenMeals.Sum(x => x.Recipe.TotalCalories);
            CurrentProtein = Math.Round(eatenMeals.Sum(x => x.Recipe.TotalProtein), 1);
            CurrentCarbs = Math.Round(eatenMeals.Sum(x => x.Recipe.TotalCarbs), 1);
            CurrentFat = Math.Round(eatenMeals.Sum(x => x.Recipe.TotalFat), 1);
            CurrentFiber = Math.Round(eatenMeals.Sum(x => x.Recipe.TotalFiber), 1);

            AngleCalories = CalculateAngle(CurrentCalories, GoalCalories);
            AngleProtein = CalculateAngle(CurrentProtein, GoalProtein);
            AngleCarbs = CalculateAngle(CurrentCarbs, GoalCarbs);
            AngleFat = CalculateAngle(CurrentFat, GoalFat);
            AngleFiber = CalculateAngle(CurrentFiber, GoalFiber);

            string warningColor = "#D32F2F";

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
    }

    // helper view model for calendar days
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