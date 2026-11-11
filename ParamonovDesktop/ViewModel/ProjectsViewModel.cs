using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParamonovDesktop.Model;
using ParamonovDesktop.Services;
using ParamonovDesktop.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ParamonovDesktop.ViewModel
{
    public class ProjectsViewModel : ObservableObject
    {
        private readonly ProjectService _projectService;
        private readonly ProjectsWindow _window;

        private List<Project> _allProjects = new List<Project>();

        private ObservableCollection<Project> _projects = new ObservableCollection<Project>();
        public ObservableCollection<Project> Projects
        {
            get { return _projects; }
            set { SetProperty(ref _projects, value); }
        }

        private Project _selectedProject;
        public Project SelectedProject
        {
            get { return _selectedProject; }
            set
            {
                SetProperty(ref _selectedProject, value);
                OnPropertyChanged(nameof(CanOpenProject));

                // ⚠️ Пересчитать CanExecute для OpenProjectCommand
                var openCmd = OpenProjectCommand as IRelayCommand;
                if (openCmd != null) openCmd.NotifyCanExecuteChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        private string _selectedStatusFilter = "all";
        public string SelectedStatusFilter
        {
            get { return _selectedStatusFilter; }
            set
            {
                SetProperty(ref _selectedStatusFilter, value);
                ApplyFilter();
            }
        }

        public bool IsCustomer
        {
            get
            {
                return App.CurrentUser != null &&
                       !string.IsNullOrEmpty(App.CurrentUser.Role) &&
                       App.CurrentUser.Role.Equals("customer", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool CanOpenProject
        {
            get { return SelectedProject != null; }
        }

        // === Команды ===
        public IRelayCommand LoadProjectsCommand { get; private set; }
        public IRelayCommand OpenProjectCommand { get; private set; }
        public IRelayCommand CreateProjectCommand { get; private set; }
        public IRelayCommand<string> SetStatusFilterCommand { get; private set; }

        public ProjectsViewModel(ProjectsWindow window)
        {
            _window = window;
            _projectService = new ProjectService();

            LoadProjectsCommand = new AsyncRelayCommand(LoadProjects);
            OpenProjectCommand = new RelayCommand(OpenProject, () => SelectedProject != null);
            CreateProjectCommand = new AsyncRelayCommand(CreateProject);
            SetStatusFilterCommand = new RelayCommand<string>(SetFilter);

            var _ = LoadProjects();
        }

        private void SetFilter(string status)
        {
            SelectedStatusFilter = status ?? "all";
        }

        private async Task LoadProjects()
        {
            IsLoading = true;
            var list = await _projectService.GetMyProjects() ?? new List<Project>();
            _allProjects = list;
            ApplyFilter();
            IsLoading = false;
        }

        private void ApplyFilter()
        {
            IEnumerable<Project> filtered = _allProjects;

            if (SelectedStatusFilter != "all")
                filtered = filtered.Where(p =>
                    string.Equals(p.Status, SelectedStatusFilter,
                        StringComparison.OrdinalIgnoreCase));

            Projects = new ObservableCollection<Project>(filtered);
        }

        private void OpenProject()
        {
            if (SelectedProject == null)
            {
                MessageBox.Show("Выберите проект");
                return;
            }

            try
            {
                var detail = new ProjectDetailWindow(SelectedProject.IdProject);
                detail.Show();
                _window.Close();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка открытия проекта:\n" + ex.Message);
            }
        }

        private async Task CreateProject()
        {
            if (!IsCustomer)
            {
                CustomMessageBox.Show("Создавать проекты может только заказчик");
                return;
            }

            if (App.CurrentUser == null)
            {
                CustomMessageBox.Show("Пользователь не авторизован");
                return;
            }

            var newProject = new Project
            {
                Name = "Новый проект",
                Description = "Описание проекта",
                Status = "draft",
                IdCustomer = App.CurrentUser.IdUser
            };

            Project created = null;
            try
            {
                created = await _projectService.CreateProject(newProject);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка запроса:\n" + ex.Message);
                return;
            }

            if (created == null)
            {
                CustomMessageBox.Show("Не удалось создать проект (ответ API пустой или ошибка)");
                return;
            }

            try
            {
                var detail = new ProjectDetailWindow(created.IdProject);
                detail.Show();
                _window.Close();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Проект создан, но открыть карточку не удалось:\n" + ex.Message);
            }
        }
    }
}
