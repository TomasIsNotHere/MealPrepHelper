using ReactiveUI;
using System.Reactive;

namespace MealPrepHelper.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    // 1. Proměnná, která drží aktuální stránku (Přehled nebo Kalendář)
    private ViewModelBase _currentPage;
    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        private set {
            this.RaiseAndSetIfChanged(ref _currentPage, value);
            this.RaisePropertyChanged(nameof(IsOverviewActive));
            this.RaisePropertyChanged(nameof(IsCalendarActive));
        }
    }

    // 2. Instance podstránek (aby se nenačítaly znovu a znovu)
    public OverviewViewModel OverviewVM { get; } = new OverviewViewModel();
    public CalendarViewModel CalendarVM { get; } = new CalendarViewModel();

    // 3. PŘÍKAZY - Toto vám pravděpodobně chybělo
    public ReactiveCommand<Unit, Unit> SwitchToOverviewCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToCalendarCommand { get; }

    public MainWindowViewModel()
    {
        // Výchozí stránka po startu
        _currentPage = OverviewVM;

        // Definice akcí pro tlačítka
        SwitchToOverviewCommand = ReactiveCommand.Create(() => { CurrentPage = OverviewVM; });
        SwitchToCalendarCommand = ReactiveCommand.Create(() => { CurrentPage = CalendarVM; });
    }
    public bool IsOverviewActive => CurrentPage == OverviewVM;
    public bool IsCalendarActive => CurrentPage == CalendarVM;
}