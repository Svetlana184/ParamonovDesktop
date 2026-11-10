using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParamonovDesktop.Model;
using ParamonovDesktop.Services;
using ParamonovDesktop.View;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ParamonovDesktop.ViewModel
{
        public class RegisterViewModel : ObservableObject
        {
            private readonly LoginService _loginService;
            private readonly DirectoryService _directoryService;
            private readonly RegisterWindow _window;

            // === Поля ===
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

            // === Компании и команды ===
            private ObservableCollection<CompanyItem> _companies =
                new ObservableCollection<CompanyItem>();
            public ObservableCollection<CompanyItem> Companies
            {
                get { return _companies; }
                set { SetProperty(ref _companies, value); }
            }

            private CompanyItem _selectedCompany;
            public CompanyItem SelectedCompany
            {
                get { return _selectedCompany; }
                set
                {
                    SetProperty(ref _selectedCompany, value);
                    FilterTeams();
                }
            }

            private ObservableCollection<TeamItem> _teams =
                new ObservableCollection<TeamItem>();
            public ObservableCollection<TeamItem> Teams
            {
                get { return _teams; }
                set { SetProperty(ref _teams, value); }
            }

            private TeamItem _selectedTeam;
            public TeamItem SelectedTeam
            {
                get { return _selectedTeam; }
                set { SetProperty(ref _selectedTeam, value); }
            }

            private ObservableCollection<TeamItem> _allTeams =
                new ObservableCollection<TeamItem>();

            private bool _isBusy;
            public bool IsBusy
            {
                get { return _isBusy; }
                set { SetProperty(ref _isBusy, value); }
            }

            // === Команды ===
            public ICommand RegisterCommand { get; private set; }
            public ICommand BackToLoginCommand { get; private set; }

            public RegisterViewModel(RegisterWindow window)
            {
                _window = window;
                _loginService = new LoginService();
                _directoryService = new DirectoryService();

                RegisterCommand = new AsyncRelayCommand(Register, CanRegister);
                BackToLoginCommand = new RelayCommand(BackToLogin);

                var _ = LoadDirectories();
            }

            private bool CanRegister()
            {
                return !IsBusy;
            }

            private async Task LoadDirectories()
            {
                var companies = await _directoryService.GetCompanies();
                if (companies != null)
                    Companies = new ObservableCollection<CompanyItem>(companies);

                var teams = await _directoryService.GetTeams();
                if (teams != null)
                {
                    _allTeams = new ObservableCollection<TeamItem>(teams);
                    Teams = new ObservableCollection<TeamItem>(_allTeams);
                }
            }

            private void FilterTeams()
            {
                if (SelectedCompany == null)
                {
                    Teams = new ObservableCollection<TeamItem>(_allTeams);
                }
                else
                {
                    var filtered = _allTeams
                        .Where(t => t.IdCompany == SelectedCompany.IdCompany)
                        .ToList();
                    Teams = new ObservableCollection<TeamItem>(filtered);
                }

                // Сброс выбранной команды, если её нет в новом списке
                if (SelectedTeam != null && !Teams.Contains(SelectedTeam))
                    SelectedTeam = null;
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

                int? companyId = SelectedCompany != null
                    ? (int?)SelectedCompany.IdCompany
                    : null;

                int? teamId = SelectedTeam != null
                    ? (int?)SelectedTeam.IdTeam
                    : null;

                var request = new RegisterRequest
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Login = Login.Trim(),
                    Password = Password,
                    Role = RoleName,
                    IdCompany = companyId,
                    IdTeam = teamId,
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
