using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MealPrepHelper.Views;
using MealPrepHelper.ViewModels;
using MealPrepHelper.Data;

namespace MealPrepHelper;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            using (var context = new AppDbContext())
                {
                    DbInitializer.Initialize(context);
                }

            desktop.MainWindow = new MainWindow(){
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}