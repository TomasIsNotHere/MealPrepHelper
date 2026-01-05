using ReactiveUI;
using System.Reactive;
using System.Linq;
using MealPrepHelper.Data;
using MealPrepHelper.Models;

namespace MealPrepHelper.ViewModels
{
    public class IngredientCheckViewModel : ViewModelBase
    {
        // data
        private readonly int _userId;
        private readonly int _ingredientId;
        public string Name { get; }
        public string Unit { get; }
        public double AmountNeeded { get; }
        public double AmountInPantry { get; }

        // state
        public bool HasEnough => AmountInPantry >= AmountNeeded;
        public string StatusColor => HasEnough ? "#4CAF50" : "#F44336";
        public string StatusIcon => HasEnough ? "✅" : "❌";
        public string StatusText => HasEnough
            ? $"Máte: {AmountInPantry} {Unit}"
            : $"Chybí (Máte jen {AmountInPantry} {Unit})";

        // shopping cart
        private string _cartIcon = "🛒+";
        public string CartIcon
        {
            get => _cartIcon;
            set => this.RaiseAndSetIfChanged(ref _cartIcon, value);
        }

        private bool _canAdd = true;
        public bool CanAdd
        {
            get => _canAdd;
            set => this.RaiseAndSetIfChanged(ref _canAdd, value);
        }
        public ReactiveCommand<Unit, Unit> AddToCartCommand { get; }

        public IngredientCheckViewModel(RecipeIngredient ri, double pantryAmount, int userId)
        {
            if (ri.Ingredient == null)
            {
                Name = "Neznámá surovina";
                Unit = "";
            }
            else
            {
                Name = ri.Ingredient.Name;
                Unit = ri.Ingredient.Unit;
            }

            AmountNeeded = ri.Amount;
            AmountInPantry = pantryAmount;

            _ingredientId = ri.IngredientId;
            _userId = userId;

            AddToCartCommand = ReactiveCommand.Create(AddToShoppingList);
        }

        // methods
        private void AddToShoppingList()
        {
            if (!CanAdd) return;

            using (var db = new AppDbContext())
            {
                var existingItem = db.ShoppingList
                    .FirstOrDefault(x => x.UserId == _userId && x.IngredientId == _ingredientId);

                double amountToBuy = AmountNeeded - AmountInPantry;

                if (amountToBuy <= 0) amountToBuy = AmountNeeded;

                if (existingItem != null)
                {
                    existingItem.Amount += amountToBuy;
                    existingItem.IsBought = false;
                }
                else
                {
                    db.ShoppingList.Add(new ShoppingListItem
                    {
                        UserId = _userId,
                        IngredientId = _ingredientId,
                        Amount = amountToBuy,
                        Unit = Unit,
                        IsBought = false
                    });
                }
                db.SaveChanges();
            }

            CartIcon = "✔️";
            CanAdd = false;
        }
    }
}