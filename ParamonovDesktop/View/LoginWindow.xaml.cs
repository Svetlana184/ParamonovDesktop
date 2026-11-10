
using ParamonovDesktop.ViewModel;
using System.Windows;
using System.Windows.Controls;


namespace ParamonovDesktop.View
{

    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _vm;

        public LoginWindow()
        {
            InitializeComponent();
            _vm = new LoginViewModel(this);
            DataContext = _vm;
        }

        private void PwdBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = sender as PasswordBox;
            if (pb != null)
                _vm.Password = pb.Password;
        }
    }
}
