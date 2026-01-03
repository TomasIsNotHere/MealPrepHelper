using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using MealPrepHelper.Data;
using MealPrepHelper.Models;

namespace MealPrepHelper.ViewModels
{
    public class PantryViewModel : ViewModelBase
    {
        private readonly int _userId;

        // Seznam věcí ve spižírně
        public ObservableCollection<PantryItemViewModel> PantryItems { get; } = new();
        
        // Seznam pro dropdown (výběr suroviny k přidání)
        public ObservableCollection<Ingredient> AllIngredients { get; } = new();

        // === FORMULÁŘ PŘIDÁNÍ ===
        private Ingredient? _selectedIngredient;
        public Ingredient? SelectedIngredient
        {
            get => _selectedIngredient;
            set 
            {
                this.RaiseAndSetIfChanged(ref _selectedIngredient, value);
                if (value != null) AddUnit = value.Unit; 
            }
        }

        private double? _addAmount;
        public double? AddAmount
        {
            get => _addAmount;
            set => this.RaiseAndSetIfChanged(ref _addAmount, value);
        }

        private string _addUnit = "";
        public string AddUnit
        {
            get => _addUnit;
            set => this.RaiseAndSetIfChanged(ref _addUnit, value);
        }

        // === PŘÍKAZY ===
        public ReactiveCommand<Unit, Unit> AddCommand { get; }
        public ReactiveCommand<PantryItemViewModel, Unit> RemoveCommand { get; }

        // Konstruktor nyní přijímá ID uživatele
        public PantryViewModel(int userId)
        {
            _userId = userId;

            AddCommand = ReactiveCommand.Create(AddItem);
            RemoveCommand = ReactiveCommand.Create<PantryItemViewModel>(RemoveItem);

            LoadData();
        }
        
        // Bezparametrický konstruktor pro Designer (aby to nepadalo v náhledu)
        public PantryViewModel() { _userId = 1; }

        public void LoadData()
        {
            using (var db = new AppDbContext())
            {
                // 1. Načíst katalog surovin
                AllIngredients.Clear();
                var ingredients = db.Ingredients.OrderBy(x => x.Name).ToList();
                foreach (var ing in ingredients) AllIngredients.Add(ing);

                // 2. Načíst spižírnu uživatele
                PantryItems.Clear();
                var items = db.Pantry
                    .Include(p => p.Ingredient) // DŮLEŽITÉ: Načíst i název suroviny
                    .Where(p => p.UserId == _userId)
                    .ToList();

                foreach (var item in items)
                {
                    PantryItems.Add(new PantryItemViewModel(item));
                }
            }
        }

        private void AddItem()
        {
            double amountToAdd = AddAmount ?? 0;

            if (SelectedIngredient == null || AddAmount <= 0) return;

            using (var db = new AppDbContext())
            {
                // Zkontrolujeme, jestli už to ve spižírně máme
                var existing = db.Pantry
                    .FirstOrDefault(p => p.UserId == _userId && p.IngredientId == SelectedIngredient.Id);

                if (existing != null)
                {
                    existing.Amount += amountToAdd; // Jen přičteme
                }
                else
                {
                    // Vytvoříme nový záznam
                    var newItem = new PantryItem
                    {
                        UserId = _userId,
                        IngredientId = SelectedIngredient.Id,
                        Amount = amountToAdd,
                        Unit = SelectedIngredient.Unit
                    };
                    db.Pantry.Add(newItem);
                }
                db.SaveChanges();
            }

            // Reset formuláře
            AddAmount = 0;
            SelectedIngredient = null;
            LoadData();
        }

        private void RemoveItem(PantryItemViewModel vm)
        {

            double amountToRemove = vm.AmountToRemove ?? 0;

            if (vm == null || vm.AmountToRemove <= 0) return;

            using (var db = new AppDbContext())
            {
                var dbItem = db.Pantry.Find(vm.Model.Id);
                if (dbItem != null)
                {
                    dbItem.Amount -= amountToRemove;

                    // Pokud jsme odebrali všechno (nebo víc), smažeme řádek úplně
                    if (dbItem.Amount <= 0.001)
                    {
                        db.Pantry.Remove(dbItem);
                    }
                    
                    db.SaveChanges();
                }
            }
            LoadData();
        }
    }
}