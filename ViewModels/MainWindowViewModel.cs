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
            this.RaisePropertyChanged(nameof(IsOverviewActive));
            this.RaisePropertyChanged(nameof(IsCalendarActive));
        }
    }

    // Instance viewmodelů
    // OverviewVM může být null před přihlášením, přidáme '?'
    public OverviewViewModel? OverviewVM { get; private set; }
    
    // CalendarVM bude existovat vždy, aby fungoval popup binding
    public CalendarViewModel CalendarVM { get; private set; }

    public ReactiveCommand<Unit, Unit> SwitchToOverviewCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToCalendarCommand { get; }

    public bool IsOverviewActive => CurrentPage == OverviewVM;
    public bool IsCalendarActive => CurrentPage == CalendarVM;


    public MainWindowViewModel()
    {
        // 1. OPRAVA: Kalendář inicializujeme HNED ZDE
        // Tím zajistíme, že 'CalendarVM.IsPopupVisible' vrátí false, místo chyby bindingu
        CalendarVM = new CalendarViewModel();

        // Startujeme na přihlašovací obrazovce
        var loginVm = new LoginViewModel();
        loginVm.LoginSuccessful += OnLoginSuccess;
        _currentPage = loginVm;

        IsLoggedIn = false;

        SwitchToOverviewCommand = ReactiveCommand.Create(() => 
        { 
            if (OverviewVM != null) CurrentPage = OverviewVM; 
        });
        
        SwitchToCalendarCommand = ReactiveCommand.Create(() => 
        { 
            CurrentPage = CalendarVM; 
        });
    }

    private void OnLoginSuccess(User user)
    {
        _currentUser = user;
        IsLoggedIn = true;
        
        // 2. Overview potřebuje uživatele, takže ho vytvoříme až tady
        OverviewVM = new OverviewViewModel(user.Id);
        
        // CalendarVM už máme z konstruktoru, takže ho tu nevytváříme znovu!
        // (Pokud byste potřeboval předat user.Id i do kalendáře, 
        //  musel byste tam dodělat metodu např. CalendarVM.SetUser(user.Id))

        // 3. Přepneme na přehled
        CurrentPage = OverviewVM;
    }
}