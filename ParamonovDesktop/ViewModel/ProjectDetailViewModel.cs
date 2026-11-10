using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParamonovDesktop.Model;
using ParamonovDesktop.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace ParamonovDesktop.ViewModel
{
    public class ProjectDetailViewModel : ObservableObject
    {
        private readonly ProjectService _projectService;
        private readonly SpecService _specService;
        private readonly TaskService _taskService;
        private readonly ChangeService _changeService;

        private readonly int _projectId;
        private readonly string _role;
        private readonly int _userId;

        private Project _project;
        public Project Project
        {
            get { return _project; }
            set { SetProperty(ref _project, value); }
        }

        private ObservableCollection<SpecItem> _specItems;
        public ObservableCollection<SpecItem> SpecItems
        {
            get { return _specItems; }
            set { SetProperty(ref _specItems, value); }
        }

        private ObservableCollection<TaskItem> _tasks;
        public ObservableCollection<TaskItem> Tasks
        {
            get { return _tasks; }
            set { SetProperty(ref _tasks, value); }
        }

        private ObservableCollection<ChangeRequest> _changeRequests;
        public ObservableCollection<ChangeRequest> ChangeRequests
        {
            get { return _changeRequests; }
            set { SetProperty(ref _changeRequests, value); }
        }

        private SpecItem _selectedSpecItem;
        public SpecItem SelectedSpecItem
        {
            get { return _selectedSpecItem; }
            set { SetProperty(ref _selectedSpecItem, value); }
        }

        private TaskItem _selectedTask;
        public TaskItem SelectedTask
        {
            get { return _selectedTask; }
            set { SetProperty(ref _selectedTask, value); }
        }

        private ChangeRequest _selectedChange;
        public ChangeRequest SelectedChange
        {
            get { return _selectedChange; }
            set { SetProperty(ref _selectedChange, value); }
        }

        public bool CanEditSpec
        {
            get { return _role == "Customer"; }
        }

        public bool CanEditTasks
        {
            get { return _role == "Manager"; }
        }

        public bool CanEditHours
        {
            get { return _role == "Developer"; }
        }

        public bool CanApproveChanges
        {
            get { return _role == "Manager" || _role == "Customer"; }
        }

        public bool CanReadOnlyTasks
        {
            get { return !CanEditTasks; }
        }

        public bool CanReadOnlyHours
        {
            get { return !CanEditHours; }
        }

        public ProjectDetailViewModel(int projectId)
        {
            _projectId = projectId;
            _role = App.CurrentUser != null ? App.CurrentUser.Role : "Customer";
            _userId = App.CurrentUser != null ? App.CurrentUser.IdUser : 0;

            _projectService = new ProjectService();
            _specService = new SpecService();
            _taskService = new TaskService();
            _changeService = new ChangeService();

            SpecItems = new ObservableCollection<SpecItem>();
            Tasks = new ObservableCollection<TaskItem>();
            ChangeRequests = new ObservableCollection<ChangeRequest>();

            LoadAllAsync();
        }

        private async void LoadAllAsync()
        {
            await LoadAll();
        }

        [RelayCommand]
        public async Task LoadAll()
        {
            Project = await _projectService.GetProject(_projectId);

            var items = await _specService.GetItems(_projectId);
            SpecItems = new ObservableCollection<SpecItem>(
                items != null ? items : new List<SpecItem>());

            var allTasks = await _taskService.GetAll();
            if (allTasks == null)
                allTasks = new List<TaskItem>();

            var filtered = new List<TaskItem>();
            foreach (var t in allTasks)
            {
                if (CanEditHours)
                {
                    if (t.IdDeveloper == _userId)
                        filtered.Add(t);
                }
                else
                {
                    filtered.Add(t);
                }
            }
            Tasks = new ObservableCollection<TaskItem>(filtered);

            var changes = await _changeService.GetAll();
            if (changes == null)
                changes = new List<ChangeRequest>();

            var pending = new List<ChangeRequest>();
            foreach (var c in changes)
            {
                if (c.Status == 0)
                    pending.Add(c);
            }
            ChangeRequests = new ObservableCollection<ChangeRequest>(pending);
        }

        [RelayCommand]
        public async Task SaveSpecAndSend()
        {
            if (SelectedSpecItem == null)
            {
                MessageBox.Show("Выберите пункт ТЗ");
                return;
            }

            await _specService.UpdateItem(SelectedSpecItem);

            var change = new ChangeRequest
            {
                IdSpecItem = SelectedSpecItem.IdItem,
                Name = "Изменение ТЗ: " + SelectedSpecItem.Name,
                Description = SelectedSpecItem.Description,
                Status = 0,
                IdCreator = _userId
            };
            await _changeService.Create(change);

            MessageBox.Show("Изменение ТЗ отправлено менеджеру");
            await LoadAll();
        }

        [RelayCommand]
        public async Task SaveTaskAndSend()
        {
            if (SelectedTask == null)
            {
                MessageBox.Show("Выберите задачу");
                return;
            }

            await _taskService.Update(SelectedTask);

            var change = new ChangeRequest
            {
                IdSpecItem = 1,
                IdTask = SelectedTask.IdTask,
                Name = "Изменение задачи: " + SelectedTask.Description,
                Description = "Новая стоимость: " + SelectedTask.Cost,
                Status = 0,
                IdCreator = _userId
            };
            await _changeService.Create(change);

            MessageBox.Show("Изменения счёта отправлены заказчику");
            await LoadAll();
        }

        [RelayCommand]
        public async Task SaveHours()
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

        [RelayCommand]
        public async Task ApproveChange()
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

        [RelayCommand]
        public async Task RejectChange()
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
