using ReactiveUI;
using MealPrepHelper.Models;
using MealPrepHelper.Data;

namespace MealPrepHelper.ViewModels
{
    public class ShoppingItemViewModel : ViewModelBase
    {
        // data
        public ShoppingListItem Model { get; }

        public string Name => Model.Ingredient?.Name ?? "Neznámá";
        public string Unit => Model.Unit;

        // editable properties
        public double? Amount
        {
            get => Model.Amount;
            set
            {
                var newValue = value ?? 0;

                if (Model.Amount != newValue)
                {
                    Model.Amount = newValue;
                    this.RaisePropertyChanged();

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