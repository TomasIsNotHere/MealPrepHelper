using ReactiveUI;
using MealPrepHelper.Models;

namespace MealPrepHelper.ViewModels
{
    public class PantryItemViewModel : ViewModelBase
    {
        // data
        public PantryItem Model { get; }

        public string Name => Model.Ingredient?.Name ?? "Neznámá surovina";
        public double CurrentAmount => Model.Amount;
        public string Unit => Model.Unit;

        // interactive properties
        private double? _amountToRemove;
        public double? AmountToRemove
        {
            get => _amountToRemove;
            set
            {
                this.RaiseAndSetIfChanged(ref _amountToRemove, value);

                if (RemoveAll && _amountToRemove != CurrentAmount)
                {
                    _removeAll = false;
                    this.RaisePropertyChanged(nameof(RemoveAll));
                }
            }
        }

        private bool _removeAll;
        public bool RemoveAll
        {
            get => _removeAll;
            set
            {
                this.RaiseAndSetIfChanged(ref _removeAll, value);
                if (value)
                {
                    AmountToRemove = CurrentAmount;
                }
                else if (AmountToRemove == CurrentAmount)
                {
                }
            }
        }


        public PantryItemViewModel(PantryItem model)
        {
            Model = model;
            AmountToRemove = 0;
        }
    }
}