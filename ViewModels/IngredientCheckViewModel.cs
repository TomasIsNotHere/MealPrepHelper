using ReactiveUI;
using System.Reactive; // Pro Unit
using System.Linq;
using MealPrepHelper.Data;
using MealPrepHelper.Models;

namespace MealPrepHelper.ViewModels
{
    // ZMĚNA: Dědíme od ViewModelBase, abychom mohli používat ReactiveCommand
    public class IngredientCheckViewModel : ViewModelBase
    {
        private readonly int _userId;
        private readonly int _ingredientId;

        public string Name { get; set; } = "";
        public double AmountNeeded { get; set; }
        public string Unit { get; set; } = "";
        public double AmountInPantry { get; set; }
        public bool HasEnough => AmountInPantry >= AmountNeeded;

        // Barvy a texty
        public string StatusColor => HasEnough ? "#4CAF50" : "#F44336"; 
        public string StatusIcon => HasEnough ? "✅" : "❌";
        public string StatusText => HasEnough 
            ? $"Máte: {AmountInPantry} {Unit}" 
            : $"Chybí (Máte jen {AmountInPantry} {Unit})";

        // === NOVÉ: Logika pro přidání na nákupní seznam ===
        
        // Měnící se ikona tlačítka (Košík -> Fajfka)
        private string _cartIcon = "🛒+";
        public string CartIcon
        {
            get => _cartIcon;
            set => this.RaiseAndSetIfChanged(ref _cartIcon, value);
        }

        // Aby tlačítko nešlo zmáčknout 2x
        private bool _canAdd = true;
        public bool CanAdd
        {
            get => _canAdd;
            set => this.RaiseAndSetIfChanged(ref _canAdd, value);
        }

        public ReactiveCommand<Unit, Unit> AddToCartCommand { get; }

        // Konstruktor nyní přijímá i UserID a IngredientID
        public IngredientCheckViewModel(RecipeIngredient ri, double pantryAmount, int userId)
        {
            Name = ri.Ingredient.Name;
            AmountNeeded = ri.Amount;
            Unit = ri.Ingredient.Unit;
            AmountInPantry = pantryAmount;
            
            _ingredientId = ri.IngredientId;
            _userId = userId;

            AddToCartCommand = ReactiveCommand.Create(AddToShoppingList);
        }

        private void AddToShoppingList()
        {
            if (!CanAdd) return;

            using (var db = new AppDbContext())
            {
                // Zjistíme, jestli už to v seznamu není
                var existingItem = db.ShoppingList
                    .FirstOrDefault(x => x.UserId == _userId && x.IngredientId == _ingredientId);

                // Kolik potřebujeme dokoupit? (Rozdíl mezi tím co je potřeba a co máme)
                // Pokud máme 0, koupíme vše. Pokud nám chybí jen 50g, koupíme 50g.
                double amountToBuy = AmountNeeded - AmountInPantry;
                if (amountToBuy <= 0) amountToBuy = AmountNeeded; // Pojistka

                if (existingItem != null)
                {
                    existingItem.Amount += amountToBuy;
                    existingItem.IsBought = false; // Znovu odškrtnout, pokud už bylo koupeno
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

            // Vizuální zpětná vazba
            CartIcon = "✔️";
            CanAdd = false; // Deaktivovat tlačítko
        }
    }
}