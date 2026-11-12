using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParamonovDesktop.Model;
using ParamonovDesktop.Services;
using ParamonovDesktop.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ParamonovDesktop.ViewModel
{
    public partial class ProjectsViewModel : ObservableObject
    {
        private readonly ProjectService _projectService = new ProjectService();
        private readonly ProjectsWindow _window;

        private List<Project> _allProjects = new List<Project>();

        // === Projects ===
        private ObservableCollection<Project> _projects = new ObservableCollection<Project>();
        public ObservableCollection<Project> Projects
        {
            get { return _projects; }
            set
            {
                _projects = value;
                OnPropertyChanged(nameof(Projects));
            }
        }

        // === SelectedProject ===
        private Project _selectedProject;
        public Project SelectedProject
        {
            get { return _selectedProject; }
            set
            {
                _selectedProject = value;
                OnPropertyChanged(nameof(SelectedProject));
                OnPropertyChanged(nameof(CanOpenProject));
                OpenProjectCommand.NotifyCanExecuteChanged();
            }
        }

        // === IsLoading ===
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        // === SelectedStatusFilter ===
        private string _selectedStatusFilter = "all";
        public string SelectedStatusFilter
        {
            get { return _selectedStatusFilter; }
            set
            {
                _selectedStatusFilter = value;
                OnPropertyChanged(nameof(SelectedStatusFilter));
                ApplyFilter();
            }
        }

        // === IsCustomer ===
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

        public ProjectsViewModel(ProjectsWindow window)
        {
            _window = window;
            _projectService = new ProjectService();
            var _ = LoadProjectsAsync();
        }

        // === Загрузка ===
        [RelayCommand]
        private async Task LoadProjectsAsync()
        {
            IsLoading = true;
            var list = await _projectService.GetProjects() ?? new List<Project>();
            _allProjects = list;
            ApplyFilter();
            IsLoading = false;
        }

        private void ApplyFilter()
        {
            IEnumerable<Project> filtered = _allProjects;

            if (SelectedStatusFilter != "all")
                filtered = filtered.Where(p =>
                    string.Equals(p.Status, SelectedStatusFilter, StringComparison.OrdinalIgnoreCase));

            Projects = new ObservableCollection<Project>(filtered);
        }

        // === Фильтр ===
        [RelayCommand]
        private void SetStatusFilter(string status)
        {
            SelectedStatusFilter = status ?? "all";
        }

        // === Открыть проект ===
        [RelayCommand(CanExecute = nameof(CanOpenProject))]
        private void OpenProject()
        {
            if (SelectedProject == null)
            {
                MessageBox.Show("Выберите проект");
                return;
            }

            var detail = new ProjectDetailWindow(SelectedProject.IdProject);
            detail.Show();
            //_window.Close();
        }

        // === Создать проект ===
        [RelayCommand(CanExecute = nameof(IsCustomer))]
        private async Task CreateProject()
        {
            if (App.CurrentUser == null)
            {
                MessageBox.Show("Пользователь не авторизован");
                return;
            }

            Project created = null;
            try
            {
                created = await _projectService.CreateProjectDraft("Новый проект", "Описание проекта");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка запроса:\n" + ex.Message);
                return;
            }

            if (created == null)
            {
                MessageBox.Show("Не удалось создать проект");
                return;
            }

            var detail = new ProjectDetailWindow(created.IdProject);
            detail.Show();
            _window.Close();
        }
    }
}
