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
        
        // === DATA PRO KALENDÁŘ ===
        private DateTime _currentDate;
    public DateTime CurrentDate 
    {
        get => _currentDate;
        set 
        {
            // 1. Změníme hodnotu
            this.RaiseAndSetIfChanged(ref _currentDate, value);
            
            // 2. Automaticky obnovíme data a týdenní lištu
            // (voláme to jen pokud se datum opravdu změnilo)
            LoadData();
        }
    }

        private string _weekRangeText = string.Empty;
        public string WeekRangeText 
        {
            get => _weekRangeText;
            set => this.RaiseAndSetIfChanged(ref _weekRangeText, value);
        }

        // Seznam 7 dnů v horní liště
        public ObservableCollection<CalendarDayViewModel> WeekDays { get; } = new();

        // === DATA JÍDEL ===
        public ObservableCollection<PlanItem> TodayMeals { get; } = new();

        // === GRAFY (Hodnoty a Úhly) ===
        // Hodnoty
        private int _currentCalories; public int CurrentCalories { get => _currentCalories; set => this.RaiseAndSetIfChanged(ref _currentCalories, value); }
        private int _goalCalories; public int GoalCalories { get => _goalCalories; set => this.RaiseAndSetIfChanged(ref _goalCalories, value); }
        // ... (Zde si nechte ostatní makra: Protein, Carbs, Fat, Fiber - zkrátil jsem to pro přehlednost) ...
        
        private double _currentProtein; public double CurrentProtein { get => _currentProtein; set => this.RaiseAndSetIfChanged(ref _currentProtein, value); }
        private int _goalProtein; public int GoalProtein { get => _goalProtein; set => this.RaiseAndSetIfChanged(ref _goalProtein, value); }

        private double _currentCarbs; public double CurrentCarbs { get => _currentCarbs; set => this.RaiseAndSetIfChanged(ref _currentCarbs, value); }
        private int _goalCarbs; public int GoalCarbs { get => _goalCarbs; set => this.RaiseAndSetIfChanged(ref _goalCarbs, value); }

        private double _currentFat; public double CurrentFat { get => _currentFat; set => this.RaiseAndSetIfChanged(ref _currentFat, value); }
        private int _goalFat; public int GoalFat { get => _goalFat; set => this.RaiseAndSetIfChanged(ref _goalFat, value); }

        private double _currentFiber; public double CurrentFiber { get => _currentFiber; set => this.RaiseAndSetIfChanged(ref _currentFiber, value); }
        private int _goalFiber; public int GoalFiber { get => _goalFiber; set => this.RaiseAndSetIfChanged(ref _goalFiber, value); }

        // Úhly
        private double _angleCalories; public double AngleCalories { get => _angleCalories; set => this.RaiseAndSetIfChanged(ref _angleCalories, value); }
        private double _angleProtein; public double AngleProtein { get => _angleProtein; set => this.RaiseAndSetIfChanged(ref _angleProtein, value); }
        private double _angleCarbs; public double AngleCarbs { get => _angleCarbs; set => this.RaiseAndSetIfChanged(ref _angleCarbs, value); }
        private double _angleFat; public double AngleFat { get => _angleFat; set => this.RaiseAndSetIfChanged(ref _angleFat, value); }
        private double _angleFiber; public double AngleFiber { get => _angleFiber; set => this.RaiseAndSetIfChanged(ref _angleFiber, value); }

        // === PŘÍKAZY ===
        public ReactiveCommand<Unit, Unit> NextWeekCommand { get; }
        public ReactiveCommand<Unit, Unit> PrevWeekCommand { get; }
        public ReactiveCommand<CalendarDayViewModel, Unit> SelectDayCommand { get; }


        public OverviewViewModel(int userId)
        {
            _currentUserId = userId;
            
            // Startujeme na dnešku
            _currentDate = DateTime.Today;

            // Inicializace příkazů
            NextWeekCommand = ReactiveCommand.Create(() => ChangeDate(7));
            PrevWeekCommand = ReactiveCommand.Create(() => ChangeDate(-7));
            SelectDayCommand = ReactiveCommand.Create<CalendarDayViewModel>(day => 
            {
                ChangeDate((day.Date - CurrentDate).Days);
            });

            LoadData(); // Načte data a vygeneruje kalendář
        }
        public OverviewViewModel() { }

        // Změna data o X dní (posun týdne nebo kliknutí na den)
        private void ChangeDate(int daysToAdd)
        {
            CurrentDate = CurrentDate.AddDays(daysToAdd);
            LoadData();
        }

        public void LoadData()
        {
            // 1. Vygenerovat horní lištu týdne podle CurrentDate
            GenerateWeek();

            TodayMeals.Clear();
            using (var db = new AppDbContext())
            {
                // Načtení cílů uživatele
                var user = db.Users.Find(_currentUserId);
                if (user != null)
                {
                    GoalCalories = user.DailyCalorieGoal;
                    GoalProtein = user.Protein;
                    GoalCarbs = user.Carbs;
                    GoalFat = user.Fat;
                    GoalFiber = user.DietaryFiber;
                }

                // Načtení jídel PRO VYBRANÉ DATUM (CurrentDate)
                var meals = db.PlanItems
                    .Include(p => p.Recipe)
                    .Where(p => p.MealPlan.UserId == _currentUserId 
                             && p.ScheduledFor.Date == CurrentDate.Date) // <--- ZDE JE ZMĚNA
                    .OrderBy(p => p.ScheduledFor)
                    .ToList();

                foreach (var meal in meals) TodayMeals.Add(meal);

                RecalculateEaten();
            }
        }

        private void GenerateWeek()
        {
            WeekDays.Clear();
            
            // Najdeme pondělí aktuálního týdne
            // (DayOfWeek.Monday je 1, Sunday je 0 -> musíme to ošetřit pro české týdny)
            int diff = (7 + (CurrentDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            var monday = CurrentDate.Date.AddDays(-diff);
            var sunday = monday.AddDays(6);

            // Nastavení textu rozsahu (např. "15.12. - 21.12.")
            WeekRangeText = $"{monday:d.M.} - {sunday:d.M. yyyy}";

            var czechCulture = new CultureInfo("cs-CZ");

            // Vygenerujeme 7 dní
            for (int i = 0; i < 7; i++)
            {
                var dayDate = monday.AddDays(i);
                bool isSelected = dayDate == CurrentDate.Date;
                bool isToday = dayDate == DateTime.Today;

                var dayVm = new CalendarDayViewModel
                {
                    Date = dayDate,
                    DayName = czechCulture.DateTimeFormat.GetAbbreviatedDayName(dayDate.DayOfWeek), // Po, Út...
                    DayNumber = dayDate.Day.ToString(),
                    
                    // Styl podle toho, jestli je vybráno
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
    public class CalendarDayViewModel : ViewModelBase
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = string.Empty; // "Po", "Út"...
        public string DayNumber { get; set; } = string.Empty; // "15"
        
        // Barvy pro UI
        private string _background = "White";
        public string Background { get => _background; set => this.RaiseAndSetIfChanged(ref _background, value); }
        
        private string _foreground = "Black";
        public string Foreground { get => _foreground; set => this.RaiseAndSetIfChanged(ref _foreground, value); }

        private string _fontWeight = "Normal";
        public string FontWeight { get => _fontWeight; set => this.RaiseAndSetIfChanged(ref _fontWeight, value); }
    }
}