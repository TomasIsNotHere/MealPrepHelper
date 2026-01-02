using ReactiveUI;
using MealPrepHelper.Models;

namespace MealPrepHelper.ViewModels
{
    public class PantryItemViewModel : ViewModelBase
    {
        // Odkaz na váš databázový model
        public PantryItem Model { get; }

        // Pomocná vlastnost: Kolik chce uživatel zrovna odebrat
        private double? _amountToRemove;
        public double? AmountToRemove
        {
            get => _amountToRemove;
            set{ 
                this.RaiseAndSetIfChanged(ref _amountToRemove, value);
                if (_amountToRemove != CurrentAmount && RemoveAll)
                {
                    RemoveAll = false;
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
                    // Pokud zaškrtnuto -> nastavíme maximum
                    AmountToRemove = CurrentAmount;
                }
            }
        }

        // Vlastnosti pro zobrazení (Binding)
        // Používáme ?. operátor pro jistotu, kdyby Ingredient náhodou nebyl načten
        public string Name => Model.Ingredient?.Name ?? "Neznámá surovina";
        public double CurrentAmount => Model.Amount;
        public string Unit => Model.Unit;

        public PantryItemViewModel(PantryItem model)
        {
            Model = model;
            AmountToRemove = 0; // Výchozí hodnota
        }
    }
}