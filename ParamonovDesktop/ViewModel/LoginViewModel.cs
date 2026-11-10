using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParamonovDesktop.Model;
using ParamonovDesktop.Services;
using ParamonovDesktop.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ParamonovDesktop.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly LoginService _loginService;
        private readonly LoginWindow _window;

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

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public LoginViewModel(LoginWindow window)
        {
            _window = window;
            _loginService = new LoginService();
        }

        [RelayCommand]
        public async Task Auth()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                CustomMessageBox.Show("Введите логин и пароль");
                return;
            }

            IsBusy = true;
            var resp = await _loginService.SignIn(new LoginRequest
            {
                Login = Login,
                Password = Password
            });
            IsBusy = false;

            if (resp == null)
            {
                CustomMessageBox.Show("Неверный логин или пароль");
                return;
            }

            App.CurrentUser = resp;
            var main = new ProjectsWindow();
            main.Show();
            _window.Close();
        }

        [RelayCommand]
        public void OpenRegister()
        {
            var register = new RegisterWindow();
            register.Show();
            _window.Close();
        }
    }
}
