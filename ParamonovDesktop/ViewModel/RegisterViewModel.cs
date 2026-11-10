using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParamonovDesktop.Model;
using ParamonovDesktop.Services;
using ParamonovDesktop.View;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ParamonovDesktop.ViewModel
{
    public class RegisterViewModel : ObservableObject
    {
        private readonly LoginService _loginService;
        private readonly RegisterWindow _window;

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set { SetProperty(ref _firstName, value); }
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set { SetProperty(ref _lastName, value); }
        }

        private string _login;
        public string Login
        {
            get { return _login; }
            set { SetProperty(ref _login, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        private string _passwordConfirm;
        public string PasswordConfirm
        {
            get { return _passwordConfirm; }
            set { SetProperty(ref _passwordConfirm, value); }
        }

        private int _selectedRoleIndex;
        public int SelectedRoleIndex
        {
            get { return _selectedRoleIndex; }
            set { SetProperty(ref _selectedRoleIndex, value); }
        }

        private string _idCompanyText;
        public string IdCompanyText
        {
            get { return _idCompanyText; }
            set { SetProperty(ref _idCompanyText, value); }
        }

        private string _idTeamText;
        public string IdTeamText
        {
            get { return _idTeamText; }
            set { SetProperty(ref _idTeamText, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public ICommand RegisterCommand { get; private set; }
        public ICommand BackToLoginCommand { get; private set; }

        public RegisterViewModel(RegisterWindow window)
        {
            _window = window;
            _loginService = new LoginService();

            RegisterCommand = new AsyncRelayCommand(Register, CanRegister);
            BackToLoginCommand = new RelayCommand(BackToLogin);
        }

        private bool CanRegister()
        {
            return !IsBusy;
        }

        private string RoleName
        {
            get
            {
                switch (SelectedRoleIndex)
                {
                    case 0: return "customer";
                    case 1: return "manager";
                    case 2: return "developer";
                    default: return "customer";
                }
            }
        }

        private async Task Register()
        {
            if (string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Login) ||
                string.IsNullOrWhiteSpace(Password))
            {
                CustomMessageBox.Show("Заполните обязательные поля");
                return;
            }

            if (Password != PasswordConfirm)
            {
                CustomMessageBox.Show("Пароли не совпадают");
                return;
            }

            if (Password.Length < 4)
            {
                CustomMessageBox.Show("Пароль должен содержать минимум 4 символа");
                return;
            }

            IsBusy = true;

            int companyValue;
            int? company = null;
            if (int.TryParse(IdCompanyText, out companyValue))
                company = companyValue;

            int teamValue;
            int? team = null;
            if (int.TryParse(IdTeamText, out teamValue))
                team = teamValue;

            var request = new RegisterRequest
            {
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Login = Login.Trim(),
                Password = Password,
                Role = RoleName,             //"customer" / "manager" / "developer"
                IdCompany = company,
                IdTeam = team,
                ShortDescription = "Роль: " + RoleName
            };

            string error = await _loginService.Register(request);

            if (error != null)
            {
                IsBusy = false;
                CustomMessageBox.Show("Не удалось зарегистрироваться:\n" + error);
                return;
            }

            var resp = await _loginService.SignIn(new LoginRequest
            {
                Login = Login.Trim(),
                Password = Password
            });

            IsBusy = false;

            if (resp == null)
            {
                CustomMessageBox.Show("Регистрация успешна, но войти автоматически не удалось. Войдите вручную.");
                BackToLogin();
                return;
            }

            App.CurrentUser = resp;
            var main = new ProjectsWindow();
            main.Show();
            _window.Close();
        }

        private void BackToLogin()
        {
            var login = new LoginWindow();
            login.Show();
            _window.Close();
        }
    }
}
