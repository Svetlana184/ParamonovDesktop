using ParamonovDesktop.ViewModel;

using System.Windows;


namespace ParamonovDesktop.View
{
    /// <summary>
    /// Логика взаимодействия для ProjectDetailWindow.xaml
    /// </summary>
    public partial class ProjectDetailWindow : Window
    {
        public ProjectDetailWindow(int projectId)
        {
            InitializeComponent();
            var vm = new ProjectDetailViewModel(projectId);
            DataContext = vm;

            // Вызываем публичный метод напрямую
            Loaded += async (s, e) => await vm.LoadAllAsync();
        }
    }
}
