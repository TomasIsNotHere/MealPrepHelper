using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using MealPrepHelper.Data;
using MealPrepHelper.Models;
using System.Globalization;

namespace MealPrepHelper.ViewModels
{
    public class CalendarViewModel : ViewModelBase
    {
        // set user
        private int _userId;
        public void SetUser(int userId)
        {
            _userId = userId;
            ReloadCalendar();
        }

        // calendar
        private DateTime _currentMonth;
        private string _monthTitle = string.Empty;
        public string MonthTitle
        {
            get => _monthTitle;
            set => this.RaiseAndSetIfChanged(ref _monthTitle, value);
        }
        public ObservableCollection<DayViewModel> Days { get; } = new();

        // popup state
        private bool _isPopupVisible;
        public bool IsPopupVisible
        {
            get => _isPopupVisible;
            set => this.RaiseAndSetIfChanged(ref _isPopupVisible, value);
        }
        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => this.RaiseAndSetIfChanged(ref _selectedDate, value);
        }

        // ui collections
        public ObservableCollection<Recipe> AvailableRecipes { get; } = new();
        public ObservableCollection<PlanItem> SelectedDayMeals { get; } = new();
        public ObservableCollection<string> MealTypes { get; } = new() { "Snídaně", "Svačina", "Oběd", "Večeře" };

        // form state
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => this.RaiseAndSetIfChanged(ref _isEditing, value);
        }

        private int? _editingItemId = null;
        private string _saveButtonText = "Přidat jídlo";
        public string SaveButtonText
        {
            get => _saveButtonText;
            set => this.RaiseAndSetIfChanged(ref _saveButtonText, value);
        }
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        // form data
        private Recipe? _formRecipe;
        public Recipe? FormRecipe
        {
            get => _formRecipe;
            set => this.RaiseAndSetIfChanged(ref _formRecipe, value);
        }
        private string _formType = "Snídaně";
        public string FormType
        {
            get => _formType;
            set => this.RaiseAndSetIfChanged(ref _formType, value);
        }
        private TimeSpan _formTime = TimeSpan.FromHours(12);
        public TimeSpan FormTime
        {
            get => _formTime;
            set => this.RaiseAndSetIfChanged(ref _formTime, value);
        }

        // commands
        // calendar navigation
        public ReactiveCommand<Unit, Unit> NextMonthCommand { get; }
        public ReactiveCommand<Unit, Unit> PrevMonthCommand { get; }

        // popup control
        public ReactiveCommand<DayViewModel, Unit> OpenPopupCommand { get; }
        public ReactiveCommand<Unit, Unit> ClosePopupCommand { get; }

        // meal manipulation
        public ReactiveCommand<Unit, Unit> SaveMealCommand { get; }
        public ReactiveCommand<PlanItem, Unit> DeleteMealCommand { get; }
        public ReactiveCommand<PlanItem, Unit> EditMealCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelEditCommand { get; }

        public CalendarViewModel()
        {
            _currentMonth = DateTime.Today;

            NextMonthCommand = ReactiveCommand.Create(() => ChangeMonth(1));
            PrevMonthCommand = ReactiveCommand.Create(() => ChangeMonth(-1));

            OpenPopupCommand = ReactiveCommand.Create<DayViewModel>(day =>
            {
                SelectedDate = day.Date;
                IsPopupVisible = true;
                ResetForm();
                LoadPopupData();
            });

            ClosePopupCommand = ReactiveCommand.Create(() => { IsPopupVisible = false; });

            SaveMealCommand = ReactiveCommand.Create(SaveMeal);
            DeleteMealCommand = ReactiveCommand.Create<PlanItem>(DeleteMeal);
            EditMealCommand = ReactiveCommand.Create<PlanItem>(PrepareEdit);

            CancelEditCommand = ReactiveCommand.Create(() =>
            {
                ResetForm();
            });

            ReloadCalendar();
        }

        // calendar logic
        private void ChangeMonth(int add)
        {
            _currentMonth = _currentMonth.AddMonths(add);
            ReloadCalendar();
        }

        public void ReloadCalendar()
        {
            Days.Clear();

            var czechCulture = new CultureInfo("cs-CZ");
            MonthTitle = _currentMonth.ToString("MMMM yyyy", czechCulture);

            MonthTitle = char.ToUpper(MonthTitle[0]) + MonthTitle.Substring(1);

            var firstDay = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
            int offset = ((int)firstDay.DayOfWeek == 0) ? 6 : (int)firstDay.DayOfWeek - 1;
            var startDate = firstDay.AddDays(-offset);
            var endDate = startDate.AddDays(42);

            using (var db = new AppDbContext())
            {
                var busyDates = db.PlanItems
                    .Include(p => p.MealPlan)
                    .Where(p => p.MealPlan.UserId == _userId)
                    .Where(p => p.ScheduledFor >= startDate && p.ScheduledFor < endDate)
                    .Select(p => p.ScheduledFor.Date)
                    .Distinct().ToList();

                for (int i = 0; i < 42; i++)
                {
                    var d = startDate.AddDays(i);
                    Days.Add(new DayViewModel
                    {
                        Date = d,
                        IsToday = d.Date == DateTime.Today,
                        IsCurrentMonth = d.Month == _currentMonth.Month,
                        HasPlan = busyDates.Contains(d.Date)
                    });
                }
            }
        }

        // popup logic
        private void LoadPopupData()
        {
            AvailableRecipes.Clear();
            SelectedDayMeals.Clear();

            using (var db = new AppDbContext())
            {
                foreach (var r in db.Recipes) AvailableRecipes.Add(r);

                var meals = db.PlanItems
                    .Include(p => p.Recipe)
                    .Include(p => p.MealPlan)
                    .Where(p => p.MealPlan.UserId == _userId)
                    .Where(p => p.ScheduledFor.Date == SelectedDate.Date)
                    .OrderBy(p => p.ScheduledFor)
                    .ToList();

                foreach (var m in meals) SelectedDayMeals.Add(m);
            }
        }

        private void SaveMeal()
        {
            if (FormRecipe == null || string.IsNullOrEmpty(FormType)) return;

            if (_userId <= 0)
            {
                System.Diagnostics.Debug.WriteLine("CHYBA: Pokus o uložení jídla bez nastaveného ID uživatele!");
                return;
            }

            using (var db = new AppDbContext())
            {
                var finalDateTime = SelectedDate.Date + FormTime;

                var mealPlan = db.MealPlans.FirstOrDefault(mp => mp.UserId == _userId);

                if (mealPlan == null)
                {
                    mealPlan = new MealPlan { UserId = _userId, Name = "Můj Plán" };
                    db.MealPlans.Add(mealPlan);
                    db.SaveChanges();
                }

                if (_editingItemId == null)
                {
                    var newItem = new PlanItem
                    {
                        MealPlanId = mealPlan.Id,
                        RecipeId = FormRecipe.Id,
                        MealType = FormType,
                        ScheduledFor = finalDateTime,
                        IsEaten = false
                    };
                    db.PlanItems.Add(newItem);
                }
                else
                {
                    var item = db.PlanItems.Find(_editingItemId);
                    if (item != null)
                    {
                        item.RecipeId = FormRecipe.Id;
                        item.MealType = FormType;
                        item.ScheduledFor = finalDateTime;
                    }
                }
                db.SaveChanges();
            }

            LoadPopupData();
            ReloadCalendar();
            ResetForm();
        }

        private void DeleteMeal(PlanItem item)
        {
            using (var db = new AppDbContext())
            {
                var dbItem = db.PlanItems.Find(item.Id);
                if (dbItem != null)
                {
                    db.PlanItems.Remove(dbItem);
                    db.SaveChanges();
                }
            }
            LoadPopupData();
            ReloadCalendar();

            if (_editingItemId == item.Id) ResetForm();
        }

        private void PrepareEdit(PlanItem item)
        {
            var recipe = AvailableRecipes.FirstOrDefault(r => r.Id == item.RecipeId);
            FormRecipe = recipe;

            SearchText = recipe?.Name ?? string.Empty;

            FormType = item.MealType;
            FormTime = item.ScheduledFor.TimeOfDay;

            _editingItemId = item.Id;
            SaveButtonText = "Uložit změny";
            IsEditing = true;
        }

        private void ResetForm()
        {
            FormRecipe = null;
            SearchText = string.Empty;

            FormType = "";
            FormTime = TimeSpan.FromHours(12);
            _editingItemId = null;
            SaveButtonText = "Přidat jídlo";
            IsEditing = false;
        }
    }

    // day representation
    public class DayViewModel : ViewModelBase
    {
        public DateTime Date { get; set; }
        public string DayNumber => Date.Day.ToString();

        private bool _hasPlan;
        public bool HasPlan { get => _hasPlan; set => this.RaiseAndSetIfChanged(ref _hasPlan, value); }

        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }

        public string TextColor => IsCurrentMonth ? "Black" : "#CCCCCC";
        public string DotColor => "#4CAF50";
    }
}