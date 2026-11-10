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

namespace ParamonovDesktop.ViewModel
{
    public partial class ProjectsViewModel : ObservableObject
    {
        private readonly ProjectService _projectService;
        private readonly ProjectsWindow _window;

        private ObservableCollection<Project> _projects;
        public ObservableCollection<Project> Projects
        {
            get { return _projects; }
            set { SetProperty(ref _projects, value); }
        }

        private Project _selectedProject;
        public Project SelectedProject
        {
            get { return _selectedProject; }
            set { SetProperty(ref _selectedProject, value); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        public ProjectsViewModel(ProjectsWindow window)
        {
            _window = window;
            _projectService = new ProjectService();
            Projects = new ObservableCollection<Project>();
            LoadProjectsAsync();
        }

        private async void LoadProjectsAsync()
        {
            await LoadProjects();
        }

        [RelayCommand]
        private async Task LoadProjects()
        {
            IsLoading = true;
            var list = await _projectService.GetMyProjects();
            Projects = new ObservableCollection<Project>(
                list ?? new List<Project>());
            IsLoading = false;
        }

        [RelayCommand]
        public void OpenProject()
        {
            if (SelectedProject == null)
            {
                MessageBox.Show("Выберите проект");
                return;
            }
            var detail = new ProjectDetailWindow(SelectedProject.IdProject);
            detail.Show();
            _window.Close();
        }
    }
}
