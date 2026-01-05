using ReactiveUI;
using System.Reactive;
using MealPrepHelper.Models;

namespace MealPrepHelper.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    // app state
    private User? _currentUser;
    private bool _isLoggedIn;

    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set => this.RaiseAndSetIfChanged(ref _isLoggedIn, value);
    }

    private ViewModelBase _currentPage;
    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        private set
        {
            this.RaiseAndSetIfChanged(ref _currentPage, value);

            this.RaisePropertyChanged(nameof(IsOverviewActive));
            this.RaisePropertyChanged(nameof(IsCalendarActive));
            this.RaisePropertyChanged(nameof(IsPantryActive));
            this.RaisePropertyChanged(nameof(IsShoppingListActive));
        }
    }

    // views
    public OverviewViewModel? OverviewVM { get; private set; }
    public UserProfileViewModel? UserProfileVM { get; private set; }
    public PantryViewModel? PantryVM { get; private set; }
    public ShoppingListViewModel? ShoppingListVM { get; private set; }
    public CalendarViewModel CalendarVM { get; private set; }

    // menu state
    public bool IsOverviewActive => CurrentPage == OverviewVM;
    public bool IsCalendarActive => CurrentPage == CalendarVM;
    public bool IsPantryActive => CurrentPage == PantryVM;
    public bool IsShoppingListActive => CurrentPage == ShoppingListVM;

    // commands
    public ReactiveCommand<Unit, Unit> SwitchToOverviewCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToCalendarCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToPantryCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToShoppingListCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToProfileCommand { get; }
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

    public MainWindowViewModel()
    {
        CalendarVM = new CalendarViewModel();

        SwitchToOverviewCommand = ReactiveCommand.Create(() =>
        {
            if (OverviewVM != null)
            {
                OverviewVM.LoadData();
                CurrentPage = OverviewVM;
            }
        });

        SwitchToCalendarCommand = ReactiveCommand.Create(() =>
        {
            CurrentPage = CalendarVM;
        });

        SwitchToPantryCommand = ReactiveCommand.Create(() =>
        {
            if (PantryVM != null)
            {
                PantryVM.LoadData();
                CurrentPage = PantryVM;
            }
        });

        SwitchToShoppingListCommand = ReactiveCommand.Create(() =>
        {
            if (ShoppingListVM != null)
            {
                ShoppingListVM.LoadData();
                CurrentPage = ShoppingListVM;
            }
        });

        SwitchToProfileCommand = ReactiveCommand.Create(() =>
        {
            if (UserProfileVM != null)
            {
                UserProfileVM.LoadData();
                CurrentPage = UserProfileVM;
            }
        });

        LogoutCommand = ReactiveCommand.Create(Logout);

        var loginVm = new LoginViewModel();
        loginVm.LoginSuccessful += OnLoginSuccess;
        _currentPage = loginVm;

        IsLoggedIn = false;
    }

    // methods
    private void OnLoginSuccess(User user)
    {
        _currentUser = user;
        IsLoggedIn = true;

        OverviewVM = new OverviewViewModel(user.Id);
        UserProfileVM = new UserProfileViewModel(user.Id);
        PantryVM = new PantryViewModel(user.Id);
        ShoppingListVM = new ShoppingListViewModel(user.Id);

        CalendarVM.SetUser(user.Id);

        CurrentPage = OverviewVM;
    }

    private void Logout()
    {
        _currentUser = null;
        IsLoggedIn = false;

        OverviewVM = null;
        UserProfileVM = null;
        PantryVM = null;
        ShoppingListVM = null;

        var loginVm = new LoginViewModel();
        loginVm.LoginSuccessful += OnLoginSuccess;

        CurrentPage = loginVm;
    }
}