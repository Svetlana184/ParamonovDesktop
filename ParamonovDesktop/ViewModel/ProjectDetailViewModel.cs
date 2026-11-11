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
        private readonly UserService _userService = new UserService();
        private readonly DirectoryService _directoryService = new DirectoryService();

        private readonly int _projectId;
        private readonly string _role;
        private readonly int _userId;

        public ProjectDetailViewModel(int projectId)
        {
            _projectId = projectId;
            _role = App.CurrentUser != null ? App.CurrentUser.Role : "customer";
            _userId = App.CurrentUser != null ? App.CurrentUser.IdUser : 0;

            LoadAllCommand = new AsyncRelayCommand(LoadAll);
            AddSpecItemCommand = new AsyncRelayCommand(AddSpecItem);
            DeleteSpecItemCommand = new AsyncRelayCommand(DeleteSpecItem);
            SaveSpecItemCommand = new AsyncRelayCommand(SaveSpecItem);
            AssignTeamCommand = new AsyncRelayCommand(AssignTeam);
            AddTaskCommand = new AsyncRelayCommand(AddTask);
            DeleteTaskCommand = new AsyncRelayCommand(DeleteTask);
            SaveTaskCommand = new AsyncRelayCommand(SaveTask);
            SendInvoiceForReviewCommand = new AsyncRelayCommand(SendInvoiceForReview);
            ApproveChangeCommand = new AsyncRelayCommand(ApproveChange);
            RejectChangeCommand = new AsyncRelayCommand(RejectChange);
            FinishApprovalCommand = new AsyncRelayCommand(FinishApproval);
            CloseProjectCommand = new AsyncRelayCommand(CloseProject);
            FreezeProjectCommand = new AsyncRelayCommand(FreezeProject);

            var _ = LoadAll();
        }

        // === Свойства ===
        private Project _project;
        public Project Project
        {
            get { return _project; }
            set
            {
                SetProperty(ref _project, value);
                RaiseAllVisibility();
            }
        }

        private ObservableCollection<SpecItem> _specItems = new ObservableCollection<SpecItem>();
        public ObservableCollection<SpecItem> SpecItems
        {
            get { return _specItems; }
            set { SetProperty(ref _specItems, value); }
        }

        private ObservableCollection<TaskItem> _tasks = new ObservableCollection<TaskItem>();
        public ObservableCollection<TaskItem> Tasks
        {
            get { return _tasks; }
            set { SetProperty(ref _tasks, value); }
        }

        private ObservableCollection<ChangeRequest> _changeRequests = new ObservableCollection<ChangeRequest>();
        public ObservableCollection<ChangeRequest> ChangeRequests
        {
            get { return _changeRequests; }
            set { SetProperty(ref _changeRequests, value); }
        }

        private ObservableCollection<CompanyItem> _companies = new ObservableCollection<CompanyItem>();
        public ObservableCollection<CompanyItem> Companies
        {
            get { return _companies; }
            set { SetProperty(ref _companies, value); }
        }

        private ObservableCollection<TeamItem> _teams = new ObservableCollection<TeamItem>();
        public ObservableCollection<TeamItem> Teams
        {
            get { return _teams; }
            set { SetProperty(ref _teams, value); }
        }

        private CompanyItem _selectedCompany;
        public CompanyItem SelectedCompany
        {
            get { return _selectedCompany; }
            set { SetProperty(ref _selectedCompany, value); }
        }

        private TeamItem _selectedTeam;
        public TeamItem SelectedTeam
        {
            get { return _selectedTeam; }
            set { SetProperty(ref _selectedTeam, value); }
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

        // === Информация о заказчике и команде ===
        private string _customerInfo;
        public string CustomerInfo
        {
            get { return _customerInfo; }
            set { SetProperty(ref _customerInfo, value); }
        }

        private string _teamInfo;
        public string TeamInfo
        {
            get { return _teamInfo; }
            set { SetProperty(ref _teamInfo, value); }
        }

        // === Роли ===
        public bool IsCustomer { get { return _role.Equals("customer", StringComparison.OrdinalIgnoreCase); } }
        public bool IsManager { get { return _role.Equals("manager", StringComparison.OrdinalIgnoreCase); } }
        public bool IsDeveloper { get { return _role.Equals("developer", StringComparison.OrdinalIgnoreCase); } }

        // === Видимость / активность ===
        public bool IsDraft { get { return Project != null && Project.Status == "draft"; } }
        public bool IsInProgress { get { return Project != null && Project.Status == "in_progress"; } }
        public bool IsPendingApproval { get { return Project != null && Project.Status == "pending_approval"; } }
        public bool IsFrozen { get { return Project != null && Project.Status == "frozen"; } }
        public bool IsEditable
        {
            get { return Project != null && Project.Status != "completed" && Project.Status != "closed"; }
        }

        // ТЗ: только заказчик в draft
        public bool CanEditSpec
        {
            get { return IsCustomer && IsDraft; }
        }

        // Назначить команду: только заказчик в draft
        public bool CanAssignTeam
        {
            get { return IsCustomer && IsDraft && SelectedTeam != null; }
        }

        // Редактировать счёт: manager в in_progress
        public bool CanEditInvoice
        {
            get { return IsManager && IsInProgress; }
        }

        // Редактировать задачи:
        // - developer в in_progress
        // - manager в in_progress
        // - developer в pending_approval может редактировать только ActualHours (это в XAML через IsReadOnly)
        public bool CanEditTasks
        {
            get
            {
                if (Project == null) return false;
                if (Project.Status == "in_progress" && (IsManager || IsDeveloper)) return true;
                if (Project.Status == "pending_approval" && IsDeveloper) return true;
                return false;
            }
        }

        // Кнопка «Сохранить и отправить» для менеджера (только in_progress)
        public bool CanSendInvoiceForReview
        {
            get { return IsManager && IsInProgress; }
        }

        // Принимать/отклонять изменения: customer или manager в pending_approval
        public bool CanApproveChanges
        {
            get { return (IsCustomer || IsManager) && IsPendingApproval; }
        }

        // «Сохранить изменения» заказчика (из pending_approval → in_progress)
        public bool CanFinishApproval
        {
            get { return IsCustomer && IsPendingApproval; }
        }

        // «Закрыть проект» — customer/manager, если не completed/closed
        public bool CanCloseProject
        {
            get { return (IsCustomer || IsManager) && IsEditable; }
        }

        // «Разморозить» — только customer/manager в frozen
        public bool CanUnfreezeProject
        {
            get { return (IsCustomer || IsManager) && IsFrozen; }
        }

        // === Команды ===
        public ICommand LoadAllCommand { get; private set; }
        public ICommand AddSpecItemCommand { get; private set; }
        public ICommand DeleteSpecItemCommand { get; private set; }
        public ICommand SaveSpecItemCommand { get; private set; }
        public ICommand AssignTeamCommand { get; private set; }
        public ICommand AddTaskCommand { get; private set; }
        public ICommand DeleteTaskCommand { get; private set; }
        public ICommand SaveTaskCommand { get; private set; }
        public ICommand SendInvoiceForReviewCommand { get; private set; }
        public ICommand ApproveChangeCommand { get; private set; }
        public ICommand RejectChangeCommand { get; private set; }
        public ICommand FinishApprovalCommand { get; private set; }
        public ICommand CloseProjectCommand { get; private set; }
        public ICommand FreezeProjectCommand { get; private set; }

        private void RaiseAllVisibility()
        {
            OnPropertyChanged(nameof(IsDraft));
            OnPropertyChanged(nameof(IsInProgress));
            OnPropertyChanged(nameof(IsPendingApproval));
            OnPropertyChanged(nameof(IsFrozen));
            OnPropertyChanged(nameof(IsEditable));
            OnPropertyChanged(nameof(CanEditSpec));
            OnPropertyChanged(nameof(CanAssignTeam));
            OnPropertyChanged(nameof(CanEditInvoice));
            OnPropertyChanged(nameof(CanEditTasks));
            OnPropertyChanged(nameof(CanSendInvoiceForReview));
            OnPropertyChanged(nameof(CanApproveChanges));
            OnPropertyChanged(nameof(CanFinishApproval));
            OnPropertyChanged(nameof(CanCloseProject));
            OnPropertyChanged(nameof(CanUnfreezeProject));
        }

        private async Task LoadAll()
        {
            Project = await _projectService.GetProject(_projectId);
            if (Project == null) return;

            // Заказчик
            var customer = await _userService.GetUser(Project.IdCustomer);
            if (customer != null)
            {
                string companyName = "-";
                if (customer.IdCompany != null)
                {
                    var comp = (await _directoryService.GetCompanies())
                        ?.FirstOrDefault(c => c.IdCompany == customer.IdCompany);
                    if (comp != null) companyName = comp.Name;
                }
                CustomerInfo = "Заказчик: " + customer.FirstName + " " + customer.LastName +
                               " (" + companyName + ")";
            }

            // Команда
            if (Project.IdTeam != null)
            {
                var teams = await _directoryService.GetTeams();
                var team = teams?.FirstOrDefault(t => t.IdTeam == Project.IdTeam);
                if (team != null)
                {
                    string teamCompanyName = "-";
                    var comp = (await _directoryService.GetCompanies())
                        ?.FirstOrDefault(c => c.IdCompany == team.IdCompany);
                    if (comp != null) teamCompanyName = comp.Name;

                    TeamInfo = "Команда: " + team.Name + " (" + teamCompanyName + ")";
                }
            }
            else
            {
                TeamInfo = "Команда: не назначена";
            }

            // ТЗ
            var items = await _specService.GetItems(_projectId);
            SpecItems = new ObservableCollection<SpecItem>(items ?? new List<SpecItem>());

            // Задачи
            var allTasks = await _taskService.GetAll() ?? new List<TaskItem>();
            if (IsDeveloper)
                Tasks = new ObservableCollection<TaskItem>(allTasks.Where(t => t.IdDeveloper == _userId));
            else
                Tasks = new ObservableCollection<TaskItem>(allTasks);

            // Изменения
            var changes = await _changeService.GetAll() ?? new List<ChangeRequest>();
            ChangeRequests = new ObservableCollection<ChangeRequest>(changes.Where(c => c.Status == 0));

            // Компании и команды для назначения
            Companies = new ObservableCollection<CompanyItem>(
                await _directoryService.GetCompanies() ?? new List<CompanyItem>());
            Teams = new ObservableCollection<TeamItem>(
                await _directoryService.GetTeams() ?? new List<TeamItem>());
        }

        // === ТЗ ===
        private async Task AddSpecItem()
        {
            if (!CanEditSpec) return;

            var newItem = new SpecItem
            {
                IdSpec = _projectId,
                Name = "Новый пункт",
                Description = "Описание",
                IsNew = true
            };

            var created = await _specService.CreateItem(newItem);
            if (created != null)
            {
                await LoadAll();
            }
        }

        private async Task DeleteSpecItem()
        {
            if (!CanEditSpec || SelectedSpecItem == null) return;
            await _specService.DeleteItem(SelectedSpecItem.IdItem);
            await LoadAll();
        }

        private async Task SaveSpecItem()
        {
            if (!CanEditSpec || SelectedSpecItem == null) return;
            await _specService.UpdateItem(SelectedSpecItem);
            MessageBox.Show("Пункт ТЗ сохранён");
        }

        // === Назначить команду ===
        private async Task AssignTeam()
        {
            if (!CanAssignTeam) return;

            Project.IdTeam = SelectedTeam.IdTeam;
            Project.Status = "in_progress";
            await _projectService.Update(Project);
            MessageBox.Show("Команда назначена. Проект переведён в статус «В работе»");
            await LoadAll();
        }

        // === Задачи ===
        private async Task AddTask()
        {
            if (!CanEditTasks) return;
            var newTask = new TaskItem
            {
                IdTaskList = 1, // ⚠️ упрощение; лучше брать TaskList проекта
                Description = "Новая задача",
                Status = 0,
                PlannedHours = 0,
                ActualHours = 0,
                Cost = 0,
                IdDeveloper = IsDeveloper ? _userId : (int?)null
            };
            await _taskService.Create(newTask);
            await LoadAll();
        }

        private async Task DeleteTask()
        {
            if (!CanEditTasks || SelectedTask == null) return;
            await _taskService.Delete(SelectedTask.IdTask);
            await LoadAll();
        }

        private async Task SaveTask()
        {
            if (!CanEditTasks || SelectedTask == null) return;
            await _taskService.Update(SelectedTask);
            MessageBox.Show("Задача сохранена");
        }

        // === Отправить счёт на проверку ===
        private async Task SendInvoiceForReview()
        {
            if (!CanSendInvoiceForReview) return;

            await _changeService.Create(new ChangeRequest
            {
                IdSpecItem = 1,
                IdTask = null,
                Name = "Проверка счёта проекта " + Project.Name,
                Description = "Менеджер отправил счёт на проверку",
                Status = 0,
                IdCreator = _userId
            });

            Project.Status = "pending_approval";
            await _projectService.Update(Project);
            MessageBox.Show("Счёт отправлен на проверку");
            await LoadAll();
        }

        // === Принять / отклонить изменение ===
        private async Task ApproveChange()
        {
            if (!CanApproveChanges || SelectedChange == null) return;
            SelectedChange.Status = 1;
            SelectedChange.IdApprover = _userId;
            await _changeService.Update(SelectedChange);
            MessageBox.Show("Изменение одобрено");
            await LoadAll();
        }

        private async Task RejectChange()
        {
            if (!CanApproveChanges || SelectedChange == null) return;
            SelectedChange.Status = 2;
            SelectedChange.IdApprover = _userId;
            await _changeService.Update(SelectedChange);
            MessageBox.Show("Изменение отклонено");
            await LoadAll();
        }

        // === Сохранить изменения заказчика ===
        private async Task FinishApproval()
        {
            if (!CanFinishApproval) return;

            Project.Status = "in_progress";
            await _projectService.Update(Project);
            MessageBox.Show("Изменения сохранены. Проект возвращён в работу");
            await LoadAll();
        }

        // === Закрыть / заморозить ===
        private async Task CloseProject()
        {
            if (!IsEditable) return;

            var result = MessageBox.Show(
                "Закрыть проект окончательно? (Да = Закрыт, Нет = Закончен, Отмена = отмена)",
                "Закрытие проекта",
                MessageBoxButton.YesNoCancel);

            if (result == MessageBoxResult.Cancel) return;

            Project.Status = result == MessageBoxResult.Yes ? "closed" : "completed";
            await _projectService.Update(Project);
            MessageBox.Show("Проект переведён в статус «" + Project.StatusText + "»");
            await LoadAll();
        }

        private async Task FreezeProject()
        {
            if (!IsEditable) return;

            Project.Status = "frozen";
            await _projectService.Update(Project);
            MessageBox.Show("Проект заморожен");
            await LoadAll();
        }
    }
}
