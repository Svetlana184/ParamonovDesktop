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
        private readonly ProjectService _projectService;
        private readonly ProjectsWindow _window;

        private List<Project> _allProjects = new();

        private ObservableCollection<Project> _projects = new();
        public ObservableCollection<Project> Projects
        {
            get => _projects;
            set
            {
                _projects = value;
                OnPropertyChanged(nameof(Projects));
            }
        }

        private Project _selectedProject;
        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                OnPropertyChanged(nameof(SelectedProject));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private string _selectedStatusFilter = "all";
        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                _selectedStatusFilter = value;
                OnPropertyChanged(nameof(SelectedStatusFilter));
                ApplyFilter();
            }
        }

        public bool IsCustomer =>
            App.CurrentUser != null &&
            !string.IsNullOrEmpty(App.CurrentUser.Role) &&
            App.CurrentUser.Role.Equals("customer", StringComparison.OrdinalIgnoreCase);

        public bool CanOpenProject => SelectedProject != null;

        public ProjectsViewModel(ProjectsWindow window)
        {
            _window = window;
            _projectService = new ProjectService();

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                await LoadProjects();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        [RelayCommand]
        private void SetStatusFilter(string status)
        {
            SelectedStatusFilter = status ?? "all";
        }

        [RelayCommand]
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
                    string.Equals(p.Status, SelectedStatusFilter, StringComparison.OrdinalIgnoreCase));

            Projects = new ObservableCollection<Project>(filtered);
        }

        [RelayCommand]
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

        [RelayCommand]
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
