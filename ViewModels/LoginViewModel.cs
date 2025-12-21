using ReactiveUI;
using System.Reactive;
using MealPrepHelper.Data;
using System.Linq;
using MealPrepHelper.Models;
using MealPrepHelper.Services;
using System;

namespace MealPrepHelper.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;

        // Událost, která řekne hlavnímu oknu: "Přihlášení se povedlo, tady je uživatel"
        public event Action<User>? LoginSuccessful;

        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }

        public LoginViewModel()
        {
            // Tlačítko Přihlásit bude aktivní jen když je vyplněné jméno
            var canLogin = this.WhenAnyValue(
                x => x.Username,
                (name) => !string.IsNullOrWhiteSpace(name));

            LoginCommand = ReactiveCommand.Create(Login, canLogin);
        }

        private void Login()
        {
            ErrorMessage = string.Empty;
        
            // Kontrola prázdných polí (pro jistotu, i když to řeší tlačítko)
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Zadejte jméno a heslo.";
                return;
            }
        
            using (var db = new AppDbContext())
            {
                // 1. Najdeme uživatele podle jména
                var user = db.Users.FirstOrDefault(u => u.Username == Username);
        
                if (user == null)
                {
                    ErrorMessage = "Uživatel neexistuje.";
                    return;
                }
        
                // 2. Ověříme heslo pomocí našeho Helperu
                // Porovnáváme hash zadaného hesla s hashem v databázi
                bool isPasswordValid = PasswordHelper.VerifyPassword(Password, user.PasswordHash);
        
                if (isPasswordValid)
                {
                    // Úspěch!
                    LoginSuccessful?.Invoke(user);
                }
                else
                {
                    // Chyba hesla
                    ErrorMessage = "Chybné heslo.";
                }
            }
        }
    }
}