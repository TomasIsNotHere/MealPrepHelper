using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using MealPrepHelper.Data;
using MealPrepHelper.Models;

namespace MealPrepHelper.ViewModels
{
    public class CalendarViewModel : ViewModelBase
    {
        // 1. Přidáme privátní proměnnou pro ID uživatele
    private int _userId;

    // 2. Přidáme metodu pro nastavení uživatele (zavoláme ji po přihlášení)
    public void SetUser(int userId)
    {
        _userId = userId;
        ReloadCalendar(); // Přenačíst kalendář pro konkrétního uživatele
    }
        // --- KALENDÁŘ DATA ---
        private DateTime _currentMonth;
        private string _monthTitle = string.Empty;
        public string MonthTitle { get => _monthTitle; set => this.RaiseAndSetIfChanged(ref _monthTitle, value); }
        public ObservableCollection<DayViewModel> Days { get; } = new();

        public ReactiveCommand<Unit, Unit> NextMonthCommand { get; }
        public ReactiveCommand<Unit, Unit> PrevMonthCommand { get; }

        // --- POPUP DATA ---
        private bool _isPopupVisible;
        public bool IsPopupVisible { get => _isPopupVisible; set => this.RaiseAndSetIfChanged(ref _isPopupVisible, value); }

        private DateTime _selectedDate;
        public DateTime SelectedDate { get => _selectedDate; set => this.RaiseAndSetIfChanged(ref _selectedDate, value); }

        private bool _isEditing;
public bool IsEditing 
{ 
    get => _isEditing; 
    set => this.RaiseAndSetIfChanged(ref _isEditing, value); 
}
public ReactiveCommand<Unit, Unit> CancelEditCommand { get; } // <--- NOVÝ PŘÍKAZ

private string _searchText = string.Empty;
public string SearchText 
{ 
    get => _searchText; 
    set => this.RaiseAndSetIfChanged(ref _searchText, value); 
}

        // Seznamy pro UI (ComboBoxy a Karty)
        public ObservableCollection<Recipe> AvailableRecipes { get; } = new();
        public ObservableCollection<PlanItem> SelectedDayMeals { get; } = new();
        public ObservableCollection<string> MealTypes { get; } = new() { "Snídaně", "Svačina", "Oběd", "Večeře" };

        // --- FORMULÁŘ (Data binding) ---
        private Recipe? _formRecipe;
        public Recipe? FormRecipe { get => _formRecipe; set => this.RaiseAndSetIfChanged(ref _formRecipe, value); }

        private string _formType = "Snídaně";
        public string FormType { get => _formType; set => this.RaiseAndSetIfChanged(ref _formType, value); }

        private TimeSpan _formTime = TimeSpan.FromHours(12);
        public TimeSpan FormTime { get => _formTime; set => this.RaiseAndSetIfChanged(ref _formTime, value); }

        // Pokud editujeme, zde je ID. Pokud přidáváme, je to null.
        private int? _editingItemId = null;
        
        // Text na tlačítku (mění se: Přidat vs Uložit změny)
        private string _saveButtonText = "Přidat jídlo";
        public string SaveButtonText { get => _saveButtonText; set => this.RaiseAndSetIfChanged(ref _saveButtonText, value); }


        // --- PŘÍKAZY ---
        public ReactiveCommand<DayViewModel, Unit> OpenPopupCommand { get; }
        public ReactiveCommand<Unit, Unit> ClosePopupCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveMealCommand { get; }
        public ReactiveCommand<PlanItem, Unit> DeleteMealCommand { get; }
        public ReactiveCommand<PlanItem, Unit> EditMealCommand { get; }


        public CalendarViewModel()
        {
            _currentMonth = DateTime.Today;
            NextMonthCommand = ReactiveCommand.Create(() => ChangeMonth(1));
            PrevMonthCommand = ReactiveCommand.Create(() => ChangeMonth(-1));

            // Otevření popupu
            OpenPopupCommand = ReactiveCommand.Create<DayViewModel>(day =>
            {
                SelectedDate = day.Date;
                IsPopupVisible = true;
                ResetForm();      // Vyčistit formulář
                LoadPopupData();  // Načíst data pro tento den
            });

            // !!! OPRAVA CHYBY Z OBRÁZKU !!! 
            // Použití složených závorek { } zajistí, že se nevrací bool, ale void.
            ClosePopupCommand = ReactiveCommand.Create(() => { IsPopupVisible = false; });

            SaveMealCommand = ReactiveCommand.Create(SaveMeal);
            DeleteMealCommand = ReactiveCommand.Create<PlanItem>(DeleteMeal);
            EditMealCommand = ReactiveCommand.Create<PlanItem>(PrepareEdit);
            CancelEditCommand = ReactiveCommand.Create(() => 
            {
                ResetForm(); // Zavolá reset, který vyčistí data a zruší editaci
            });

            ReloadCalendar();
        }

        private void ChangeMonth(int add)
        {
            _currentMonth = _currentMonth.AddMonths(add);
            ReloadCalendar();
        }

        public void ReloadCalendar()
        {
            Days.Clear();
            MonthTitle = _currentMonth.ToString("MMMM yyyy");

            var firstDay = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
            int offset = ((int)firstDay.DayOfWeek == 0) ? 6 : (int)firstDay.DayOfWeek - 1;
            var startDate = firstDay.AddDays(-offset);
            var endDate = startDate.AddDays(42);

            using (var db = new AppDbContext())
            {
                var busyDates = db.PlanItems
                .Include(p => p.MealPlan) // Nutný Include pro přístup k UserId
                .Where(p => p.MealPlan.UserId == _userId) // <--- ZDE JE FILTR
                .Where(p => p.ScheduledFor >= startDate && p.ScheduledFor < endDate)
                .Select(p => p.ScheduledFor.Date)
                .Distinct().ToList();

                for (int i = 0; i < 42; i++)
                {
                    var d = startDate.AddDays(i);
                    Days.Add(new DayViewModel 
                    { 
                        Date = d, 
                        IsCurrentMonth = d.Month == _currentMonth.Month,
                        HasPlan = busyDates.Contains(d.Date)
                    });
                }
            }
        }

        // --- POPUP LOGIKA (Načítání, Ukládání, Mazání) ---

        private void LoadPopupData()
        {
            AvailableRecipes.Clear();
            SelectedDayMeals.Clear();

            using (var db = new AppDbContext())
            {
                // 1. Načíst recepty
                foreach (var r in db.Recipes) AvailableRecipes.Add(r);

                // 2. Načíst jídla pro tento den
                var meals = db.PlanItems
                .Include(p => p.Recipe)
                .Include(p => p.MealPlan) // Nutný Include
                .Where(p => p.MealPlan.UserId == _userId) // <--- ZDE JE FILTR
                .Where(p => p.ScheduledFor.Date == SelectedDate.Date)
                .OrderBy(p => p.ScheduledFor)
                .ToList();

                foreach (var m in meals) SelectedDayMeals.Add(m);
            }
        }

        private void SaveMeal()
{
    // 1. Validace formuláře
    if (FormRecipe == null || string.IsNullOrEmpty(FormType)) return;

    // 2. BEZPEČNOSTNÍ KONTROLA: Máme přihlášeného uživatele?
    if (_userId <= 0)
    {
        // Pokud se toto stane, znamená to, že jste v MainWindowViewModel nezavolal CalendarVM.SetUser(user.Id)
        System.Diagnostics.Debug.WriteLine("CHYBA: Pokus o uložení jídla bez nastaveného ID uživatele!");
        return;
    }

    using (var db = new AppDbContext())
    {
        var finalDateTime = SelectedDate.Date + FormTime;

        // A. Získáme nebo vytvoříme MealPlan PŘÍMO PRO _userId
        // (Už nehledáme "prvního uživatele", ale toho našeho)
        var mealPlan = db.MealPlans.FirstOrDefault(mp => mp.UserId == _userId);
        
        if (mealPlan == null)
        {
            // Pokud plán neexistuje, vytvoříme ho
            mealPlan = new MealPlan { UserId = _userId, Name = "Můj Plán" };
            db.MealPlans.Add(mealPlan);
            db.SaveChanges(); // Uložíme hned, abychom získali ID plánu
        }

        if (_editingItemId == null)
        {
            // --- PŘIDÁVÁNÍ NOVÉHO ---
            var newItem = new PlanItem
            {
                MealPlanId = mealPlan.Id, // Vazba na plán aktuálního uživatele
                RecipeId = FormRecipe.Id,
                MealType = FormType,
                ScheduledFor = finalDateTime,
                IsEaten = false
            };
            db.PlanItems.Add(newItem);
        }
        else
        {
            // --- ÚPRAVA EXISTUJÍCÍHO ---
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
    
    // Musíme ručně nastavit i text, aby uživatel viděl název receptu
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
    SearchText = string.Empty; // <--- TOTO SMAŽE "asd"
    
    FormType = null;
    FormTime = TimeSpan.FromHours(12);
    _editingItemId = null;
    SaveButtonText = "Přidat jídlo";
    IsEditing = false;
}
    }

    public class DayViewModel : ViewModelBase
    {
        public DateTime Date { get; set; }
        public string DayNumber => Date.Day.ToString();
        private bool _hasPlan;
        public bool HasPlan { get => _hasPlan; set => this.RaiseAndSetIfChanged(ref _hasPlan, value); }
        public bool IsCurrentMonth { get; set; }
        public string TextColor => IsCurrentMonth ? "Black" : "#CCCCCC";
        public string DotColor => "#4CAF50";
    }
}