using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using MealPrepHelper.Data;
using MealPrepHelper.Models;

namespace MealPrepHelper.ViewModels
{
    public class ShoppingListViewModel : ViewModelBase
    {
        private readonly int _userId;

        // data
        public ObservableCollection<ShoppingItemViewModel> Items { get; } = new();
        public ObservableCollection<Ingredient> AllIngredients { get; } = new();

        // data for add form
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
        public ReactiveCommand<ShoppingItemViewModel, Unit> DeleteCommand { get; }
        public ReactiveCommand<Unit, Unit> FinishShoppingCommand { get; }


        public ShoppingListViewModel(int userId)
        {
            _userId = userId;

            AddCommand = ReactiveCommand.Create(AddItem);
            DeleteCommand = ReactiveCommand.Create<ShoppingItemViewModel>(DeleteItem);
            FinishShoppingCommand = ReactiveCommand.Create(FinishShopping);

            LoadData();
        }

        public ShoppingListViewModel()
        {
            _userId = 1;
            AddCommand = ReactiveCommand.Create(() => {});
            DeleteCommand = ReactiveCommand.Create<ShoppingItemViewModel>(_ => {});
            FinishShoppingCommand = ReactiveCommand.Create(() => {});
        }

        // logic
        public void LoadData()
        {
            using (var db = new AppDbContext())
            {
                AllIngredients.Clear();
                var ingredients = db.Ingredients.OrderBy(x => x.Name).ToList();
                foreach (var ing in ingredients) AllIngredients.Add(ing);

                Items.Clear();
                var dbItems = db.ShoppingList
                    .Include(x => x.Ingredient)
                    .Where(x => x.UserId == _userId)
                    .ToList();

                foreach (var item in dbItems)
                {
                    Items.Add(new ShoppingItemViewModel(item));
                }
            }
        }

        private void AddItem()
        {
            double amount = AddAmount ?? 0;

            if (SelectedIngredient == null || amount <= 0) return;

            using (var db = new AppDbContext())
            {
                var existing = db.ShoppingList
                    .FirstOrDefault(x => x.UserId == _userId && x.IngredientId == SelectedIngredient.Id);

                if (existing != null)
                {
                    existing.Amount += amount;
                    existing.IsBought = false;
                }
                else
                {
                    db.ShoppingList.Add(new ShoppingListItem
                    {
                        UserId = _userId,
                        IngredientId = SelectedIngredient.Id,
                        Amount = amount,
                        Unit = SelectedIngredient.Unit,
                        IsBought = false
                    });
                }
                db.SaveChanges();
            }

            AddAmount = null;
            SelectedIngredient = null;
            LoadData();
        }

        private void DeleteItem(ShoppingItemViewModel vm)
        {
            if (vm == null) return;

            using (var db = new AppDbContext())
            {
                var item = db.ShoppingList.Find(vm.Model.Id);
                if (item != null)
                {
                    db.ShoppingList.Remove(item);
                    db.SaveChanges();
                }
            }
            LoadData();
        }

        // delete shopping list items that are marked as bought and add them to pantry
        private void FinishShopping()
        {
            using (var db = new AppDbContext())
            {
                var boughtItems = db.ShoppingList
                    .Where(x => x.UserId == _userId && x.IsBought)
                    .ToList();

                if (!boughtItems.Any()) return;

                foreach (var item in boughtItems)
                {
                    var pantryItem = db.Pantry
                        .FirstOrDefault(p => p.UserId == _userId && p.IngredientId == item.IngredientId);

                    if (pantryItem != null)
                    {
                        pantryItem.Amount += item.Amount;
                    }
                    else
                    {
                        db.Pantry.Add(new PantryItem
                        {
                            UserId = _userId,
                            IngredientId = item.IngredientId,
                            Amount = item.Amount,
                            Unit = item.Unit
                        });
                    }

                    db.ShoppingList.Remove(item);
                }
                db.SaveChanges();
            }

            LoadData();
        }
    }
}