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
using System.Windows.Documents;
using System.Windows.Input;

namespace ParamonovDesktop.ViewModel
{
    public partial class ProjectDetailViewModel : ObservableObject
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


        private string headerTeamVisibility;
        public string HeaderTeamVisibility
        {
            get { return headerTeamVisibility; }
            set
            {
                headerTeamVisibility = value;
                OnPropertyChanged(nameof(HeaderTeamVisibility));
            }
        }

        private string headerSelectTeamVisibility;
        public string HeaderSelectTeamVisibility
        {
            get { return headerSelectTeamVisibility; }
            set
            {
                headerSelectTeamVisibility = value;
                OnPropertyChanged(nameof(HeaderSelectTeamVisibility));
            }
        }

        private string changeRequestsVisibility;
        public string ChangeRequestsVisibility
        {
            get { return changeRequestsVisibility; }
            set
            {
                changeRequestsVisibility = value;
                OnPropertyChanged(nameof(ChangeRequestsVisibility));
            }
        }

        private string closeButtonsVisibility;
        public string CloseButtonsVisibility
        {
            get { return closeButtonsVisibility; }
            set
            {
                closeButtonsVisibility = value;
                OnPropertyChanged(nameof(CloseButtonsVisibility));
            }
        }

        private string refrozeButtonVisibility;
        public string RefrozeButtonVisibility
        {
            get { return refrozeButtonVisibility; }
            set
            {
                refrozeButtonVisibility = value;
                OnPropertyChanged(nameof(RefrozeButtonVisibility));
            }
        }

        private bool headerTextEditable;
        public bool HeaderTextEditable
        {
            get { return headerTextEditable; }
            set
            {
                headerTextEditable = value;
                OnPropertyChanged(nameof(HeaderTextEditable));
            }
        }

        private bool headerSelectTeamEditable;
        public bool HeaderSelectTeamEditable
        {
            get { return headerSelectTeamEditable; }
            set
            {
                headerSelectTeamEditable = value;
                OnPropertyChanged(nameof(HeaderSelectTeamEditable));
            }
        }

        private bool technicalSpecificationEditable;
        public bool TechnicalSpecificationEditable
        {
            get { return technicalSpecificationEditable; }
            set
            {
                technicalSpecificationEditable = value;
                OnPropertyChanged(nameof(TechnicalSpecificationEditable));
            }
        }

        private bool invoiceEditable;
        public bool InvoiceEditable
        {
            get { return invoiceEditable; }
            set
            {
                invoiceEditable = value;
                OnPropertyChanged(nameof(InvoiceEditable));
            }
        }

        private bool closeButtonsEditable;
        public bool CloseButtonsEditable
        {
            get { return closeButtonsEditable; }
            set
            {
                closeButtonsEditable = value;
                OnPropertyChanged(nameof(CloseButtonsEditable));
            }
        }
       

        public ProjectDetailViewModel(int projectId)
        {
            _projectId = projectId;
            _role = App.CurrentUser != null ? App.CurrentUser.Role : "customer";
            _userId = App.CurrentUser != null ? App.CurrentUser.IdUser : 0;
            LoadData();
            AnalyzeStatus();
        }

        private async void LoadData()
        {
            try
            {
                await LoadAll();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        

        // === Свойства ===
        private Project _project;
        public Project Project
        {
            get => _project;
            set
            {
                SetProperty(ref _project, value);
            }
        }

        private ObservableCollection<SpecItem> _specItems = new();
        public ObservableCollection<SpecItem> SpecItems
        {
            get => _specItems;
            set => SetProperty(ref _specItems, value);
        }

        private ObservableCollection<TaskItem> _tasks = new();
        public ObservableCollection<TaskItem> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        private ObservableCollection<ChangeRequest> _changeRequests = new();
        public ObservableCollection<ChangeRequest> ChangeRequests
        {
            get => _changeRequests;
            set => SetProperty(ref _changeRequests, value);
        }

        private ObservableCollection<CompanyItem> _companies = new();
        public ObservableCollection<CompanyItem> Companies
        {
            get => _companies;
            set => SetProperty(ref _companies, value);
        }

        private ObservableCollection<TeamItem> _teams = new();
        public ObservableCollection<TeamItem> Teams
        {
            get => _teams;
            set => SetProperty(ref _teams, value);
        }

        private CompanyItem _selectedCompany;
        public CompanyItem SelectedCompany
        {
            get => _selectedCompany;
            set => SetProperty(ref _selectedCompany, value);
        }

        private TeamItem _selectedTeam;
        public TeamItem SelectedTeam
        {
            get => _selectedTeam;
            set
            {
                SetProperty(ref _selectedTeam, value);
            }
        }

        private SpecItem _selectedSpecItem;
        public SpecItem SelectedSpecItem
        {
            get => _selectedSpecItem;
            set
            {
                SetProperty(ref _selectedSpecItem, value);
            }
        }

        private TaskItem _selectedTask;
        public TaskItem SelectedTask
        {
            get => _selectedTask;
            set
            {
                SetProperty(ref _selectedTask, value);
            }
        }

        private ChangeRequest _selectedChange;
        public ChangeRequest SelectedChange
        {
            get => _selectedChange;
            set
            {
                SetProperty(ref _selectedChange, value);
            }
        }

        // === Информация о заказчике и команде ===
        private string _customerInfo;
        public string CustomerInfo
        {
            get => _customerInfo;
            set => SetProperty(ref _customerInfo, value);
        }

        private string _teamInfo;
        public string TeamInfo
        {
            get => _teamInfo;
            set => SetProperty(ref _teamInfo, value);
        }

        // === Роли ===
        public bool IsCustomer => _role.Equals("customer", StringComparison.OrdinalIgnoreCase);
        public bool IsManager => _role.Equals("manager", StringComparison.OrdinalIgnoreCase);
        public bool IsDeveloper => _role.Equals("developer", StringComparison.OrdinalIgnoreCase);

        // === Статусы проекта ===
        public bool IsDraft => Project != null && Project.Status == "draft";
        public bool IsInProgress => Project != null && Project.Status == "in_progress";
        public bool IsPendingApproval => Project != null && Project.Status == "pending_approval";
        public bool IsFrozen => Project != null && Project.Status == "frozen";
        public bool IsEditable => Project != null && Project.Status != "completed" && Project.Status != "closed";


        private void AnalyzeStatus()
        {
            HeaderTextEditable = false;
            HeaderTeamVisibility = "Visible";
            HeaderSelectTeamEditable = false;
            HeaderSelectTeamVisibility = "Hidden";
            ChangeRequestsVisibility = "Visible";
            TechnicalSpecificationEditable = false;
            InvoiceEditable = false;
            CloseButtonsVisibility = "Visible";
            RefrozeButtonVisibility = "Hidden";
            if (IsDraft)
            {
                ChangeRequestsVisibility = "Hidden";
                if (_role == "customer")
                {
                    TechnicalSpecificationEditable = true;
                    HeaderTextEditable = true;
                    HeaderSelectTeamEditable = true;
                    HeaderSelectTeamVisibility = "Visible";
                    
                }
            }
            else if (IsInProgress)
            {
                if (_role == "customer")
                {
                    ChangeRequestsVisibility = "Hidden";
                }
                else if (_role == "manager" && _role == "developer")
                {
                    InvoiceEditable = true;
                }
            }
            else if (IsPendingApproval)
            {
                if (_role == "customer")
                {
                    TechnicalSpecificationEditable = true;
                    HeaderTextEditable = true;
                    HeaderSelectTeamEditable = true;
                    HeaderSelectTeamVisibility = "Visible";
                }
            }
            else if (IsFrozen)
            {
                RefrozeButtonVisibility = "Visible";
                CloseButtonsVisibility = "Hidden";
            }
            else
            {
                CloseButtonsVisibility = "Hidden";
            }

        }


            // === Загрузка данных ===
            [RelayCommand]
        private async Task LoadAll()
        {
            Project = await _projectService.GetProject(_projectId);
            if (Project == null)
            {
                CustomMessageBox.Show("Проект не найден");
            }

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

            var items = await _specService.GetItems(_projectId);
            SpecItems = new ObservableCollection<SpecItem>(items ?? new List<SpecItem>());

            var allTasks = await _taskService.GetAll() ?? new List<TaskItem>();
            if (IsDeveloper)
                Tasks = new ObservableCollection<TaskItem>(allTasks.Where(t => t.IdDeveloper == _userId));
            else
                Tasks = new ObservableCollection<TaskItem>(allTasks);

            var changes = await _changeService.GetAll() ?? new List<ChangeRequest>();
            ChangeRequests = new ObservableCollection<ChangeRequest>(
                changes.Where(c => c.Status == "created"));

            Companies = new ObservableCollection<CompanyItem>(
                await _directoryService.GetCompanies() ?? new List<CompanyItem>());
            Teams = new ObservableCollection<TeamItem>(
                await _directoryService.GetTeams() ?? new List<TeamItem>());
        }

        // === ТЗ ===
        [RelayCommand]
        private async Task AddSpecItem()
        {
            var newItem = new SpecItem
            {
                IdSpec = _projectId,
                Name = "Новый пункт",
                Description = "Описание",
                IsNew = true
            };

            var created = await _specService.CreateItem(newItem);
            if (created != null)
                await LoadAll();
        }

        [RelayCommand]
        private async Task DeleteSpecItem()
        {
            if (SelectedSpecItem == null) return;
            await _specService.DeleteItem(SelectedSpecItem.IdItem);
            await LoadAll();
        }
        private bool CanDeleteSpecItem() => SelectedSpecItem != null;

        [RelayCommand(CanExecute = nameof(CanSaveSpecItem))]
        private async Task SaveSpecItem()
        {
            if (SelectedSpecItem == null) return;
            await _specService.UpdateItem(SelectedSpecItem);
            CustomMessageBox.Show("Пункт ТЗ сохранён");
        }
        private bool CanSaveSpecItem() => SelectedSpecItem != null;

        // === Назначить команду ===
        [RelayCommand]
        private async Task AssignTeam()
        {
            if (SelectedTeam == null) return;
            Project.IdTeam = SelectedTeam.IdTeam;
            Project.Status = "in_progress";
            await _projectService.Update(Project);
            CustomMessageBox.Show("Команда назначена. Проект переведён в статус «В работе»");
            await LoadAll();
        }

        // === Задачи ===
        [RelayCommand]
        private async Task AddTask()
        {
            var newTask = new TaskItem
            {
                IdTaskList = 1,
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

        [RelayCommand]
        private async Task DeleteTask()
        {
            if (SelectedTask == null) return;
            await _taskService.Delete(SelectedTask.IdTask);
            await LoadAll();
        }
        private bool CanDeleteTask() => SelectedTask != null;

        [RelayCommand]
        private async Task SaveTask()
        {
            if (SelectedTask == null) return;
            await _taskService.Update(SelectedTask);
            CustomMessageBox.Show("Задача сохранена");
        }

        // Команда «Сохранить часы» (только факт. часы), доступна разработчику в pending_approval
        [RelayCommand]
        private async Task SaveHours()
        {
            if (SelectedTask == null)
            {
                CustomMessageBox.Show("Выберите задачу");
                return;
            }
            await _taskService.Update(SelectedTask);
            CustomMessageBox.Show("Часы сохранены");
        }

        // === Отправить счёт на проверку ===
        [RelayCommand]
        private async Task SendInvoiceForReview()
        {
            var firstSpecItem = SpecItems.FirstOrDefault();

            await _changeService.Create(new ChangeRequest
            {
                IdSpecItem = firstSpecItem?.IdItem,
                IdTask = null,
                Name = "Проверка счёта проекта " + Project.Name,
                Description = "Менеджер отправил счёт на проверку",
                Status = "created",
                IdCreator = _userId
            });

            Project.Status = "pending_approval";
            await _projectService.Update(Project);
            CustomMessageBox.Show("Счёт отправлен на проверку");
            await LoadAll();
        }

        // === Принять / отклонить изменение ===
        [RelayCommand]
        private async Task ApproveChange()
        {
            if (SelectedChange == null) return;
            SelectedChange.Status = "approved";
            SelectedChange.IdApprover = _userId;
            await _changeService.Update(SelectedChange);
            CustomMessageBox.Show("Изменение одобрено");
            await LoadAll();
        }
        private bool CanApproveChange() => SelectedChange != null;

        [RelayCommand]
        private async Task RejectChange()
        {
            if (SelectedChange == null) return;
            SelectedChange.Status = "revoked";
            SelectedChange.IdApprover = _userId;
            await _changeService.Update(SelectedChange);
            CustomMessageBox.Show("Изменение отклонено");
            await LoadAll();
        }
        private bool CanRejectChange() => SelectedChange != null;

        // === Сохранить изменения заказчика ===
        [RelayCommand]
        private async Task FinishApproval()
        {
            Project.Status = "in_progress";
            await _projectService.Update(Project);
            CustomMessageBox.Show("Изменения сохранены. Проект возвращён в работу");
            await LoadAll();
        }

        // === Закрыть / заморозить / разморозить ===
        [RelayCommand]
        private async Task CloseProject()
        {
            var result = MessageBox.Show(
                "Закрыть проект окончательно? (Да = Закрыт, Нет = Закончен, Отмена = отмена)",
                "Закрытие проекта",
                MessageBoxButton.YesNoCancel);

            if (result == MessageBoxResult.Cancel) return;

            Project.Status = result == MessageBoxResult.Yes ? "closed" : "completed";
            await _projectService.Update(Project);
            CustomMessageBox.Show("Проект переведён в статус «" + Project.StatusText + "»");
            await LoadAll();
        }

        [RelayCommand(CanExecute = nameof(IsEditable))]
        private async Task FreezeProject()
        {
            Project.Status = "frozen";
            await _projectService.Update(Project);
            CustomMessageBox.Show("Проект заморожен");
            await LoadAll();
        }

        [RelayCommand]
        private async Task UnfreezeProject()
        {
            Project.Status = "in_progress";
            await _projectService.Update(Project);
            CustomMessageBox.Show("Проект разморожен и возвращён в работу");
            await LoadAll();
        }
    }
}