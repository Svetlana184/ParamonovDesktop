using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParamonovDesktop.Model;
using ParamonovDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ParamonovDesktop.ViewModel
{
    public class ProjectDetailViewModel : ObservableObject
    {
        private readonly ProjectService _projectService = new ProjectService();
        private readonly SpecService _specService = new SpecService();
        private readonly TaskService _taskService = new TaskService();
        private readonly ChangeService _changeService = new ChangeService();

        private readonly int _projectId;
        private readonly string _role;
        private readonly int _userId;

        // === Свойства ===
        private Project _project;
        public Project Project
        {
            get { return _project; }
            set { SetProperty(ref _project, value); }
        }

        private ObservableCollection<SpecItem> _specItems =
            new ObservableCollection<SpecItem>();
        public ObservableCollection<SpecItem> SpecItems
        {
            get { return _specItems; }
            set { SetProperty(ref _specItems, value); }
        }

        private ObservableCollection<TaskItem> _tasks =
            new ObservableCollection<TaskItem>();
        public ObservableCollection<TaskItem> Tasks
        {
            get { return _tasks; }
            set { SetProperty(ref _tasks, value); }
        }

        private ObservableCollection<ChangeRequest> _changeRequests =
            new ObservableCollection<ChangeRequest>();
        public ObservableCollection<ChangeRequest> ChangeRequests
        {
            get { return _changeRequests; }
            set { SetProperty(ref _changeRequests, value); }
        }

        private SpecItem _selectedSpecItem;
        public SpecItem SelectedSpecItem
        {
            get { return _selectedSpecItem; }
            set
            {
                SetProperty(ref _selectedSpecItem, value);
                OnPropertyChanged(nameof(CanSaveSpecAndSendEnabled));
            }
        }

        private TaskItem _selectedTask;
        public TaskItem SelectedTask
        {
            get { return _selectedTask; }
            set
            {
                SetProperty(ref _selectedTask, value);
                OnPropertyChanged(nameof(CanSaveHoursEnabled));
                OnPropertyChanged(nameof(CanSaveTaskAndSendEnabled));
            }
        }

        private ChangeRequest _selectedChange;
        public ChangeRequest SelectedChange
        {
            get { return _selectedChange; }
            set
            {
                SetProperty(ref _selectedChange, value);
                OnPropertyChanged(nameof(CanApproveChangeEnabled));
            }
        }

        // === Роли (строки в нижнем регистре, без учёта регистра) ===
        public bool IsCustomer
        {
            get
            {
                return !string.IsNullOrEmpty(_role) &&
                       _role.Equals("customer", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool IsManager
        {
            get
            {
                return !string.IsNullOrEmpty(_role) &&
                       _role.Equals("manager", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool IsDeveloper
        {
            get
            {
                return !string.IsNullOrEmpty(_role) &&
                       _role.Equals("developer", StringComparison.OrdinalIgnoreCase);
            }
        }

        // === Видимость кнопок ===
        public bool CanEditSpecVisible { get { return IsCustomer; } }
        public bool CanEditTasksVisible { get { return IsManager; } }
        public bool CanEditHoursVisible { get { return IsDeveloper; } }
        public bool CanApproveChangesVisible { get { return IsCustomer || IsManager; } }

        // === Активность кнопок ===
        public bool CanSaveSpecAndSendEnabled
        {
            get { return IsCustomer && SelectedSpecItem != null; }
        }

        public bool CanSaveHoursEnabled
        {
            get { return IsDeveloper && SelectedTask != null; }
        }

        public bool CanSaveTaskAndSendEnabled
        {
            get { return IsManager && SelectedTask != null; }
        }

        public bool CanApproveChangeEnabled
        {
            get { return (IsCustomer || IsManager) && SelectedChange != null; }
        }

        // === Команды (ручная регистрация вместо [RelayCommand]) ===
        public ICommand LoadAllCommand { get; private set; }
        public ICommand SaveSpecAndSendCommand { get; private set; }
        public ICommand SaveTaskAndSendCommand { get; private set; }
        public ICommand SaveHoursCommand { get; private set; }
        public ICommand ApproveChangeCommand { get; private set; }
        public ICommand RejectChangeCommand { get; private set; }

        public ProjectDetailViewModel(int projectId)
        {
            _projectId = projectId;
            _role = App.CurrentUser != null ? App.CurrentUser.Role : "customer";
            _userId = App.CurrentUser != null ? App.CurrentUser.IdUser : 0;

            LoadAllCommand = new AsyncRelayCommand(LoadAll);
            SaveSpecAndSendCommand = new AsyncRelayCommand(SaveSpecAndSend);
            SaveTaskAndSendCommand = new AsyncRelayCommand(SaveTaskAndSend);
            SaveHoursCommand = new AsyncRelayCommand(SaveHours);
            ApproveChangeCommand = new AsyncRelayCommand(ApproveChange);
            RejectChangeCommand = new AsyncRelayCommand(RejectChange);

            var _ = LoadAll();
        }

        private async Task LoadAll()
        {
            Project = await _projectService.GetProject(_projectId);

            var items = await _specService.GetItems(_projectId);
            SpecItems = new ObservableCollection<SpecItem>(items ?? new List<SpecItem>());

            var allTasks = await _taskService.GetAll() ?? new List<TaskItem>();
            if (IsDeveloper)
                Tasks = new ObservableCollection<TaskItem>(
                    allTasks.Where(t => t.IdDeveloper == _userId));
            else
                Tasks = new ObservableCollection<TaskItem>(allTasks);

            var changes = await _changeService.GetAll() ?? new List<ChangeRequest>();
            ChangeRequests = new ObservableCollection<ChangeRequest>(
                changes.Where(c => c.Status == 0));
        }

        private async Task SaveSpecAndSend()
        {
            if (SelectedSpecItem == null)
            {
                MessageBox.Show("Выберите пункт ТЗ");
                return;
            }

            await _specService.UpdateItem(SelectedSpecItem);

            await _changeService.Create(new ChangeRequest
            {
                IdSpecItem = SelectedSpecItem.IdItem,
                Name = "Изменение ТЗ: " + SelectedSpecItem.Name,
                Description = SelectedSpecItem.Description,
                Status = 0,
                IdCreator = _userId
            });

            MessageBox.Show("Изменение ТЗ отправлено менеджеру");
            await LoadAll();
        }

        private async Task SaveTaskAndSend()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Выберите задачу");
                return;
            }

            await _taskService.Update(SelectedTask);

            await _changeService.Create(new ChangeRequest
            {
                IdSpecItem = 1,
                IdTask = SelectedTask.IdTask,
                Name = "Изменение задачи: " + SelectedTask.Description,
                Description = "Новая стоимость: " + SelectedTask.Cost,
                Status = 0,
                IdCreator = _userId
            });

            MessageBox.Show("Изменения счёта отправлены заказчику");
            await LoadAll();
        }

        private async Task SaveHours()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Выберите задачу");
                return;
            }

            await _taskService.Update(SelectedTask);
            MessageBox.Show("Фактические часы сохранены");
            await LoadAll();
        }

        private async Task ApproveChange()
        {
            if (SelectedChange == null)
            {
                MessageBox.Show("Выберите запрос");
                return;
            }

            SelectedChange.Status = 1;
            SelectedChange.IdApprover = _userId;
            await _changeService.Update(SelectedChange);
            MessageBox.Show("Изменение одобрено");
            await LoadAll();
        }

        private async Task RejectChange()
        {
            if (SelectedChange == null)
            {
                MessageBox.Show("Выберите запрос");
                return;
            }

            SelectedChange.Status = 2;
            SelectedChange.IdApprover = _userId;
            await _changeService.Update(SelectedChange);
            MessageBox.Show("Изменение отклонено");
            await LoadAll();
        }
    }
}
