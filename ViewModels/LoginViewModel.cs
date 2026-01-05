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
        public event Action<User>? LoginSuccessful;
        // login data
        private string _username = "";
        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        private string _password = "";
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        private string _confirmPassword = "";
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => this.RaiseAndSetIfChanged(ref _confirmPassword, value);
        }

        // ui state
        private bool _isRegistering;
        public bool IsRegistering
        {
            get => _isRegistering;
            set
            {
                this.RaiseAndSetIfChanged(ref _isRegistering, value);
                ErrorMessage = "";
                this.RaisePropertyChanged(nameof(MainButtonText));
                this.RaisePropertyChanged(nameof(SwitchModeText));
                this.RaisePropertyChanged(nameof(TitleText));
            }
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public string TitleText => IsRegistering ? "Vytvořit účet" : "Vítejte zpět";
        public string MainButtonText => IsRegistering ? "Zaregistrovat se" : "Přihlásit se";
        public string SwitchModeText => IsRegistering ? "Již máte účet? Přihlásit se" : "Nemáte účet? Zaregistrovat se";

        // registration data
        private int? _regAge = null;
        public int? RegAge
        {
            get => _regAge;
            set => this.RaiseAndSetIfChanged(ref _regAge, value);
        }

        private int? _regHeight = null;
        public int? RegHeight
        {
            get => _regHeight;
            set => this.RaiseAndSetIfChanged(ref _regHeight, value);
        }

        private int? _regWeight = null;
        public int? RegWeight
        {
            get => _regWeight;
            set => this.RaiseAndSetIfChanged(ref _regWeight, value);
        }

        public List<string> Genders { get; } = new() { "Muž", "Žena" };
        private string _selectedGender = "Muž";
        public string SelectedGender
        {
            get => _selectedGender;
            set => this.RaiseAndSetIfChanged(ref _selectedGender, value);
        }

        public List<string> ActivityLevels { get; } = new() { "Sedavý", "Lehký", "Střední", "Aktivní", "Velmi aktivní" };
        private string _selectedActivity = "Střední";
        public string SelectedActivity
        {
            get => _selectedActivity;
            set => this.RaiseAndSetIfChanged(ref _selectedActivity, value);
        }

        public List<string> AvailableGoals { get; } = new() { "Hubnutí", "Udržení váhy", "Nabírání svalů" };
        private string _selectedGoal = "Udržení váhy";
        public string SelectedGoal
        {
            get => _selectedGoal;
            set => this.RaiseAndSetIfChanged(ref _selectedGoal, value);
        }

        // commands
        public ReactiveCommand<Unit, Unit> MainActionCommand { get; }
        public ReactiveCommand<Unit, Unit> SwitchModeCommand { get; }


        public LoginViewModel()
        {
            MainActionCommand = ReactiveCommand.Create(() =>
            {
                if (IsRegistering) Register();
                else Login();
            });

            SwitchModeCommand = ReactiveCommand.Create(() =>
            {
                IsRegistering = !IsRegistering;
            });
        }

        // login/registration logic
        private void Login()
        {
            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Username == Username);

                if (user != null && PasswordHelper.VerifyPassword(Password, user.PasswordHash))
                {
                    ErrorMessage = "";
                    LoginSuccessful?.Invoke(user);
                }
                else
                {
                    ErrorMessage = "❌ Špatné jméno nebo heslo.";
                }
            }
        }

        private void Register()
        {
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

            using (var db = new AppDbContext())
            {
                if (db.Users.Any(u => u.Username == Username))
                {
                    ErrorMessage = "⚠️ Uživatel již existuje.";
                    return;
                }

                var newUser = new User
                {
                    Username = Username,
                    PasswordHash = PasswordHelper.HashPassword(Password),
                    Age = RegAge.Value,
                    HeightCm = RegHeight.Value,
                    WeightKg = RegWeight.Value,
                    Gender = SelectedGender,
                    ActivityLevel = SelectedActivity,
                    Goal = SelectedGoal
                };

                NutritionCalculator.CalculateAndSetGoals(newUser);

                db.Users.Add(newUser);
                db.SaveChanges();

                LoginSuccessful?.Invoke(newUser);
            }
        }
    }
}