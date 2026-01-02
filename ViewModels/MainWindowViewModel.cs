using ReactiveUI;
using System.Reactive;
using MealPrepHelper.Models;

namespace MealPrepHelper.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentPage;
    private User? _currentUser;
    private bool _isLoggedIn;

    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set => this.RaiseAndSetIfChanged(ref _isLoggedIn, value);
    }

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        private set 
        {
            this.RaiseAndSetIfChanged(ref _currentPage, value);
            // Upozorníme UI, že se změnila aktivní stránka (pro zvýraznění menu)
            this.RaisePropertyChanged(nameof(IsOverviewActive));
            this.RaisePropertyChanged(nameof(IsCalendarActive));
            this.RaisePropertyChanged(nameof(IsPantryActive));
            this.RaisePropertyChanged(nameof(IsShoppingListActive));
        }
    }

    // === Instance viewmodelů ===
    public OverviewViewModel? OverviewVM { get; private set; }
    public CalendarViewModel CalendarVM { get; private set; }
    
    // NOVÉ: Spižírna a Nákupní seznam
    public PantryViewModel? PantryVM { get; private set; }
    public ShoppingListViewModel? ShoppingListVM { get; private set; }

    // === Příkazy pro navigaci ===
    public ReactiveCommand<Unit, Unit> SwitchToOverviewCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToCalendarCommand { get; }
    // NOVÉ
    public ReactiveCommand<Unit, Unit> SwitchToPantryCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToShoppingListCommand { get; }

    // === Pomocné vlastnosti pro zvýraznění tlačítka v menu ===
    public bool IsOverviewActive => CurrentPage == OverviewVM;
    public bool IsCalendarActive => CurrentPage == CalendarVM;
    public bool IsPantryActive => CurrentPage == PantryVM;
    public bool IsShoppingListActive => CurrentPage == ShoppingListVM;


    public MainWindowViewModel()
    {
        // Kalendář inicializujeme hned (aby fungoval binding popupu i mimo přihlášení)
        CalendarVM = new CalendarViewModel();

        // Startujeme na přihlašovací obrazovce
        var loginVm = new LoginViewModel();
        loginVm.LoginSuccessful += OnLoginSuccess;
        _currentPage = loginVm;

        IsLoggedIn = false;

        // Definice příkazů
        SwitchToOverviewCommand = ReactiveCommand.Create(() => 
        { 
            if (OverviewVM != null) CurrentPage = OverviewVM; 
        });
        
        SwitchToCalendarCommand = ReactiveCommand.Create(() => 
        { 
            // Kalendář existuje vždy, není třeba null check
            CurrentPage = CalendarVM; 
        });

        // NOVÉ: Přepínání na Spižírnu
        SwitchToPantryCommand = ReactiveCommand.Create(() => 
        { 
            if (PantryVM != null) CurrentPage = PantryVM; 
        });

        // NOVÉ: Přepínání na Nákupní seznam
        SwitchToShoppingListCommand = ReactiveCommand.Create(() => 
        { 
            if (ShoppingListVM != null) CurrentPage = ShoppingListVM; 
        });
    }

    private void OnLoginSuccess(User user)
    {
        _currentUser = user;
        IsLoggedIn = true;
        
        // Inicializujeme stránky, které závisejí na uživateli
        OverviewVM = new OverviewViewModel(user.Id);
        
        // Zde vytvoříme i Pantry a ShoppingList
        // (V budoucnu sem asi taky pošlete user.Id, zatím stačí prázdný konstruktor)
        PantryVM = new PantryViewModel(user.Id);
        ShoppingListVM = new ShoppingListViewModel();

        // Po přihlášení přepneme na přehled
        CurrentPage = OverviewVM;
    }
}