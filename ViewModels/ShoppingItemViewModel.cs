using ReactiveUI;
using MealPrepHelper.Models;
using MealPrepHelper.Data;

namespace MealPrepHelper.ViewModels
{
    public class ShoppingItemViewModel : ViewModelBase
    {
        public ShoppingListItem Model { get; }

        public string Name => Model.Ingredient?.Name ?? "Neznámá";
        public string Unit => Model.Unit;

        // ZMĚNA: Amount je nyní editovatelná vlastnost
        public double Amount
        {
            get => Model.Amount;
            set
            {
                // Pokud se hodnota změnila, uložíme ji
                if (Model.Amount != value)
                {
                    Model.Amount = value;
                    this.RaisePropertyChanged(); // Upozorníme UI

                    // Uložíme nové množství do databáze
                    using (var db = new AppDbContext())
                    {
                        var item = db.ShoppingList.Find(Model.Id);
                        if (item != null)
                        {
                            item.Amount = value;
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        // Reakce na Checkbox (IsBought) - beze změn
        public bool IsBought
        {
            get => Model.IsBought;
            set
            {
                if (Model.IsBought != value)
                {
                    Model.IsBought = value;
                    this.RaisePropertyChanged();
                    
                    using (var db = new AppDbContext())
                    {
                        var item = db.ShoppingList.Find(Model.Id);
                        if (item != null)
                        {
                            item.IsBought = value;
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        public ShoppingItemViewModel(ShoppingListItem model)
        {
            Model = model;
        }
    }
}