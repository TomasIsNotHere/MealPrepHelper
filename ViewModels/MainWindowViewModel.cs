using ReactiveUI;
using System.Reactive;
using MealPrepHelper.Models;

namespace MealPrepHelper.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    // 1. Proměnná, která drží aktuální stránku (Přehled nebo Kalendář)
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
        private set {
            this.RaiseAndSetIfChanged(ref _currentPage, value);
            this.RaisePropertyChanged(nameof(IsOverviewActive));
            this.RaisePropertyChanged(nameof(IsCalendarActive));
        }
    }
    // Instance viewmodelů
    public OverviewViewModel OverviewVM { get; private set; }
    public CalendarViewModel CalendarVM { get; private set; }
    public ReactiveCommand<Unit, Unit> SwitchToOverviewCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToCalendarCommand { get; }
    public bool IsOverviewActive => CurrentPage == OverviewVM;
    public bool IsCalendarActive => CurrentPage == CalendarVM;

    public MainWindowViewModel()
    {
        // 1. Startujeme na přihlašovací obrazovce
        var loginVm = new LoginViewModel();
        loginVm.LoginSuccessful += OnLoginSuccess; // Odebíráme událost úspěchu
        _currentPage = loginVm;

        IsLoggedIn = false; // Menu bude skryté
        // Inicializace příkazů (zatím prázdné akce, naplníme až po loginu)
        SwitchToOverviewCommand = ReactiveCommand.Create(() => { CurrentPage = OverviewVM; });
        SwitchToCalendarCommand = ReactiveCommand.Create(() => { CurrentPage = CalendarVM; });
    }
    private void OnLoginSuccess(User user)
    {
        _currentUser = user;
        IsLoggedIn = true; // Zobrazíme menu
        // 2. Inicializujeme hlavní obrazovky s konkrétním uživatelem
        // (Zde bychom ideálně předali ID uživatele do konstruktoru, aby věděli, co načíst)
        OverviewVM = new OverviewViewModel(user.Id);
        CalendarVM = new CalendarViewModel();
        // 3. Přepneme na přehled
        CurrentPage = OverviewVM;
    }
}