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
        // Změna z 'double' na 'double?' (povolen null)
public double? Amount
{
    get => Model.Amount;
    set
    {
        // Pokud je hodnota null (prázdné pole), použijeme 0
        var newValue = value ?? 0;

        // Porovnáváme s newValue (což je teď bezpečné číslo)
        if (Model.Amount != newValue)
        {
            Model.Amount = newValue;
            this.RaisePropertyChanged(); 

            // Uložení do databáze
            using (var db = new AppDbContext())
            {
                var item = db.ShoppingList.Find(Model.Id);
                if (item != null)
                {
                    item.Amount = newValue;
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