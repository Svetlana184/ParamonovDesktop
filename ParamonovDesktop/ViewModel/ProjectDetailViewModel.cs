using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParamonovDesktop.Model;
using ParamonovDesktop.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ParamonovDesktop.ViewModel
{
    public partial class ProjectDetailViewModel : ObservableObject
    {
        private readonly ProjectService _projectService = new ProjectService();
        private readonly SpecService _specService = new SpecService();
        private readonly TaskService _taskService = new TaskService();
        private readonly ChangeService _changeService = new ChangeService();

        private readonly int _projectId;
        private readonly string _role;
        private readonly int _userId;

        [ObservableProperty]
        private Project project;

        [ObservableProperty]
        private ObservableCollection<SpecItem> specItems = new ObservableCollection<SpecItem>();

        [ObservableProperty]
        private ObservableCollection<TaskItem> tasks = new ObservableCollection<TaskItem>();

        [ObservableProperty]
        private ObservableCollection<ChangeRequest> changeRequests = new ObservableCollection<ChangeRequest>();

        [ObservableProperty]
        private SpecItem selectedSpecItem;

        [ObservableProperty]
        private TaskItem selectedTask;

        [ObservableProperty]
        private ChangeRequest selectedChange;

        // === Роли ===
        public bool IsCustomer { get { return _role == "Customer"; } }
        public bool IsManager { get { return _role == "Manager"; } }
        public bool IsDeveloper { get { return _role == "Developer"; } }

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

        // === Уведомления для пересчёта Can*Enabled ===
        partial void OnSelectedSpecItemChanged(SpecItem value)
        {
            OnPropertyChanged(nameof(CanSaveSpecAndSendEnabled));
        }

        partial void OnSelectedTaskChanged(TaskItem value)
        {
            OnPropertyChanged(nameof(CanSaveHoursEnabled));
            OnPropertyChanged(nameof(CanSaveTaskAndSendEnabled));
        }

        partial void OnSelectedChangeChanged(ChangeRequest value)
        {
            OnPropertyChanged(nameof(CanApproveChangeEnabled));
        }

        public ProjectDetailViewModel(int projectId)
        {
            _projectId = projectId;
            _role = App.CurrentUser != null ? App.CurrentUser.Role : "Customer";
            _userId = App.CurrentUser != null ? App.CurrentUser.IdUser : 0;
            var _ = LoadAll();
        }

        [RelayCommand]
        public async Task LoadAll()
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

        [RelayCommand]
        public async Task SaveSpecAndSend()
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

        [RelayCommand]
        public async Task SaveTaskAndSend()
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
