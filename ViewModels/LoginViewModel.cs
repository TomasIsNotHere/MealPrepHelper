using ReactiveUI;
using System.Reactive;
using System.Linq;
using MealPrepHelper.Data;
using MealPrepHelper.Models;
using System;
using MealPrepHelper.Services;
using System.Collections.Generic;

namespace MealPrepHelper.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        // Událost, která řekne hlavnímu oknu: "Jsem přihlášen, tady je můj uživatel"
        public event Action<User>? LoginSuccessful;

        private string _username = "";
        public string Username { get => _username; set => this.RaiseAndSetIfChanged(ref _username, value); }

        private string _password = "";
        public string Password { get => _password; set => this.RaiseAndSetIfChanged(ref _password, value); }

        // NOVÉ: Potvrzení hesla pro registraci
        private string _confirmPassword = "";
        public string ConfirmPassword { get => _confirmPassword; set => this.RaiseAndSetIfChanged(ref _confirmPassword, value); }

        // NOVÉ: Přepínač režimu (false = Login, true = Registrace)
        private bool _isRegistering;
        public bool IsRegistering 
        { 
            get => _isRegistering; 
            set 
            {
                this.RaiseAndSetIfChanged(ref _isRegistering, value);
                // Reset chybové hlášky při přepnutí
                ErrorMessage = "";
                // Aktualizace textů tlačítek (reaktivně se to projeví v UI)
                this.RaisePropertyChanged(nameof(MainButtonText));
                this.RaisePropertyChanged(nameof(SwitchModeText));
                this.RaisePropertyChanged(nameof(TitleText));
            }
        }

        private string _errorMessage = "";
        public string ErrorMessage { get => _errorMessage; set => this.RaiseAndSetIfChanged(ref _errorMessage, value); }

        private int? _regAge = null; // Výchozí hodnota
        public int? RegAge { get => _regAge; set => this.RaiseAndSetIfChanged(ref _regAge, value); }

        // Výška (cm)
        private int? _regHeight = null;
        public int? RegHeight { get => _regHeight; set => this.RaiseAndSetIfChanged(ref _regHeight, value); }

        // Váha (kg)
        private int? _regWeight = null;
        public int? RegWeight { get => _regWeight; set => this.RaiseAndSetIfChanged(ref _regWeight, value); }

        // Seznam pro Pohlaví (ComboBox)
        public List<string> Genders { get; } = new() { "Muž", "Žena" };
        
        private string _selectedGender;
        public string SelectedGender { get => _selectedGender; set => this.RaiseAndSetIfChanged(ref _selectedGender, value); }

        // Seznam pro Aktivitu (ComboBox)
        // Používáme anglické názvy, aby seděly s NutritionCalculator logika (nebo je musíme mapovat)
        public List<string> ActivityLevels { get; } = new() { "Sedavý", "Lehký", "Střední", "Aktivní", "Velmi aktivní" };
        
        private string _selectedActivity;
        public string SelectedActivity { get => _selectedActivity; set => this.RaiseAndSetIfChanged(ref _selectedActivity, value); }

        // Texty pro UI, které se mění podle režimu
        public string TitleText => IsRegistering ? "Vytvořit účet" : "Vítejte zpět";
        public string MainButtonText => IsRegistering ? "Zaregistrovat se" : "Přihlásit se";
        public string SwitchModeText => IsRegistering ? "Již máte účet? Přihlásit se" : "Nemáte účet? Zaregistrovat se";

        public ReactiveCommand<Unit, Unit> MainActionCommand { get; }
        public ReactiveCommand<Unit, Unit> SwitchModeCommand { get; }

        public LoginViewModel()
        {
            // Jedno tlačítko dělá buď Login nebo Register podle stavu
            MainActionCommand = ReactiveCommand.Create(() => 
            {
                if (IsRegistering) Register();
                else Login();
            });

            // Přepínání režimu
SwitchModeCommand = ReactiveCommand.Create(() => 
{ 
    IsRegistering = !IsRegistering; 
});        }

            private void Login()
        {
            using (var db = new AppDbContext())
            {
                // 1. Najdeme uživatele jen podle jména
                var user = db.Users.FirstOrDefault(u => u.Username == Username);
                
                // 2. Pokud existuje, ověříme heslo pomocí Helperu
                if (user != null && PasswordHelper.VerifyPassword(Password, user.PasswordHash))
                {
                    ErrorMessage = "";
                    LoginSuccessful?.Invoke(user); // Úspěch!
                }
                else
                {
                    // Buď uživatel neexistuje, nebo nesedí heslo
                    ErrorMessage = "❌ Špatné jméno nebo heslo.";
                }
            }
        }
private void Register()
        {
            // Validace
            if (RegAge == null || RegAge <= 0 || 
        RegHeight == null || RegHeight <= 0 || 
        RegWeight == null || RegWeight <= 0)
    {
        ErrorMessage = "⚠️ Zadejte platné fyzické údaje.";
        return;
    }
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "⚠️ Vyplňte jméno a heslo.";
                return;
            }
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "⚠️ Hesla se neshodují.";
                return;
            }
            if (RegAge <= 0 || RegWeight <= 0 || RegHeight <= 0)
            {
                ErrorMessage = "⚠️ Zadejte platné fyzické údaje.";
                return;
            }

            using (var db = new AppDbContext())
            {
                if (db.Users.Any(u => u.Username == Username))
                {
                    ErrorMessage = "⚠️ Uživatel již existuje.";
                    return;
                }

                // 1. Vytvoření instance uživatele
                var newUser = new User
                {
                    Username = Username,
                    PasswordHash = PasswordHelper.HashPassword(Password), // Hashování
                    
                    // Naplnění nových dat z formuláře
                    Age = RegAge.Value,
            HeightCm = RegHeight.Value,
            WeightKg = RegWeight.Value,
            
            Gender = SelectedGender,
            ActivityLevel = SelectedActivity ?? "Střední", // Pojistka pro aktivitu
            Settings = new UserSettings { DarkMode = false }
                };

                // 2. VÝPOČET KALORIÍ A MAKER (Automatika)
                // Tato metoda vezme váhu, výšku, věk atd. a vyplní DailyCalorieGoal, Protein, atd.
                NutritionCalculator.CalculateAndSetGoals(newUser);

                // 3. Uložení
                db.Users.Add(newUser);
                db.SaveChanges();

                // 4. Přihlášení
                LoginSuccessful?.Invoke(newUser);
            }
        }
    }
}