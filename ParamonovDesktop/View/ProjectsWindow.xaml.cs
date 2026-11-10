using ParamonovDesktop.ViewModel;
using System.Windows;


namespace ParamonovDesktop.View
{
    /// <summary>
    /// Логика взаимодействия для ProjectsWindow.xaml
    /// </summary>
    public partial class ProjectsWindow : Window
    {
        public ProjectsWindow()
        {
            InitializeComponent();
            DataContext = new ProjectsViewModel(this);
        }
    }
}
