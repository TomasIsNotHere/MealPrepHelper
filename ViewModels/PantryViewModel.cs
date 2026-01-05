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

        // data
        public ObservableCollection<PantryItemViewModel> PantryItems { get; } = new();
        public ObservableCollection<Ingredient> AllIngredients { get; } = new();

        // form for adding new item
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

        // commands
        public ReactiveCommand<Unit, Unit> AddCommand { get; }
        public ReactiveCommand<PantryItemViewModel, Unit> RemoveCommand { get; }


        public PantryViewModel(int userId)
        {
            _userId = userId;

            AddCommand = ReactiveCommand.Create(AddItem);
            RemoveCommand = ReactiveCommand.Create<PantryItemViewModel>(RemoveItem);

            LoadData();
        }

        // logic
        public void LoadData()
        {
            using (var db = new AppDbContext())
            {
                AllIngredients.Clear();
                var ingredients = db.Ingredients.OrderBy(x => x.Name).ToList();
                foreach (var ing in ingredients) AllIngredients.Add(ing);

                PantryItems.Clear();
                var items = db.Pantry
                    .Include(p => p.Ingredient)
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

            if (SelectedIngredient == null || amountToAdd <= 0) return;

            using (var db = new AppDbContext())
            {
                var existing = db.Pantry
                    .FirstOrDefault(p => p.UserId == _userId && p.IngredientId == SelectedIngredient.Id);

                if (existing != null)
                {
                    existing.Amount += amountToAdd;
                }
                else
                {
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

            AddAmount = null;
            SelectedIngredient = null;

            LoadData();
        }

        private void RemoveItem(PantryItemViewModel vm)
        {
            if (vm == null) return;

            double amountToRemove = vm.AmountToRemove ?? 0;
            if (amountToRemove <= 0) return;

            using (var db = new AppDbContext())
            {
                var dbItem = db.Pantry.Find(vm.Model.Id);
                if (dbItem != null)
                {
                    dbItem.Amount -= amountToRemove;

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