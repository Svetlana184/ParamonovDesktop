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
            DataContext = new ProjectDetailViewModel(projectId);
        }
    }
}
