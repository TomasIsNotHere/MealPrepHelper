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
    public UserProfileViewModel? UserProfileVM { get; private set; }

// 2. Přidejte příkaz
public ReactiveCommand<Unit, Unit> SwitchToProfileCommand { get; }
    
    // NOVÉ: Spižírna a Nákupní seznam
    public PantryViewModel? PantryVM { get; private set; }
    public ShoppingListViewModel? ShoppingListVM { get; private set; }

    // === Příkazy pro navigaci ===
    public ReactiveCommand<Unit, Unit> SwitchToOverviewCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToCalendarCommand { get; }
    // NOVÉ
    public ReactiveCommand<Unit, Unit> SwitchToPantryCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToShoppingListCommand { get; }

    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

    // === Pomocné vlastnosti pro zvýraznění tlačítka v menu ===
    public bool IsOverviewActive => CurrentPage == OverviewVM;
    public bool IsCalendarActive => CurrentPage == CalendarVM;
    public bool IsPantryActive => CurrentPage == PantryVM;
    public bool IsShoppingListActive => CurrentPage == ShoppingListVM;


    public MainWindowViewModel()
    {
        // Kalendář inicializujeme hned (aby fungoval binding popupu i mimo přihlášení)
        CalendarVM = new CalendarViewModel();

        LogoutCommand = ReactiveCommand.Create(Logout);


        // Definice příkazů
        SwitchToOverviewCommand = ReactiveCommand.Create(() => 
        { 
            if (OverviewVM != null) {
                OverviewVM.LoadData();
                CurrentPage = OverviewVM; 
                }
        });
        SwitchToProfileCommand = ReactiveCommand.Create(() => 
{ 
    if (UserProfileVM != null) 
    {
        UserProfileVM.LoadData(); // Vždy načíst čerstvá data
        CurrentPage = UserProfileVM; 
    }
});
        
        SwitchToCalendarCommand = ReactiveCommand.Create(() => 
        { 
            // Kalendář existuje vždy, není třeba null check
            CurrentPage = CalendarVM; 
        });

        // NOVÉ: Přepínání na Spižírnu
        SwitchToPantryCommand = ReactiveCommand.Create(() => 
        { 
            if (PantryVM != null){
        PantryVM.LoadData(); // <--- PŘIDAT: Tohle je klíčové!
        CurrentPage = PantryVM; 
    }
        });

        // NOVÉ: Přepínání na Nákupní seznam
        SwitchToShoppingListCommand = ReactiveCommand.Create(() => 
        { 
            if (ShoppingListVM != null) {
        ShoppingListVM.LoadData(); // <--- PŘIDAT
        CurrentPage = ShoppingListVM; 
    }
        });

                // Startujeme na přihlašovací obrazovce
        var loginVm = new LoginViewModel();
        loginVm.LoginSuccessful += OnLoginSuccess;
        _currentPage = loginVm;

        IsLoggedIn = false;
    }

    private void OnLoginSuccess(User user)
{
    _currentUser = user;
    IsLoggedIn = true;
    
    // Inicializujeme stránky
    OverviewVM = new OverviewViewModel(user.Id);
    UserProfileVM = new UserProfileViewModel(user.Id);
    PantryVM = new PantryViewModel(user.Id);
    ShoppingListVM = new ShoppingListViewModel(user.Id);

    // !!! OPRAVA: Předáme ID uživatele do existujícího CalendarVM !!!
    CalendarVM.SetUser(user.Id); 

    CurrentPage = OverviewVM;
}
    private void Logout()
{
    // Vymazat uživatele
    _currentUser = null;
    IsLoggedIn = false;

    // Vyčistit ViewModely (aby se při příštím přihlášení jiného uživatele načetla správná data)
    OverviewVM = null;
    UserProfileVM = null;
    PantryVM = null;
    ShoppingListVM = null;

    // Vytvořit novou přihlašovací obrazovku
    var loginVm = new LoginViewModel();
    loginVm.LoginSuccessful += OnLoginSuccess;
    
    // Přepnout na login
    CurrentPage = loginVm;
}
}