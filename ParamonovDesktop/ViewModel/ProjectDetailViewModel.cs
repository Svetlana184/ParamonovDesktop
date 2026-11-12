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
        private readonly InvoiceService _invoiceService = new InvoiceService();
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
        }

        // ===== Свойства (без [ObservableProperty]) =====

        private Project _project;
        public Project Project
        {
            get { return _project; }
            set
            {
                _project = value;
                OnPropertyChanged(nameof(Project));
                RaiseAllVisibility();
                UpdateTotalCost();
            }
        }

        private Invoice _invoice;
        public Invoice Invoice
        {
            get { return _invoice; }
            set
            {
                _invoice = value;
                OnPropertyChanged(nameof(Invoice));
                UpdateTotalCost();
            }
        }

        private TechnicalSpecification _specification;
        public TechnicalSpecification Specification
        {
            get { return _specification; }
            set
            {
                _specification = value;
                OnPropertyChanged(nameof(Specification));
                UpdateTotalCost();
            }
        }


        private ObservableCollection<SpecItem> _specItems = new ObservableCollection<SpecItem>();
        public ObservableCollection<SpecItem> SpecItems
        {
            get { return _specItems; }
            set
            {
                _specItems = value;
                OnPropertyChanged(nameof(SpecItems));
            }
        }

        private ObservableCollection<TaskItem> _tasks = new ObservableCollection<TaskItem>();
        public ObservableCollection<TaskItem> Tasks
        {
            get { return _tasks; }
            set
            {
                _tasks = value;
                OnPropertyChanged(nameof(Tasks));
            }
        }

        private ObservableCollection<ChangeRequest> _changeRequests = new ObservableCollection<ChangeRequest>();
        public ObservableCollection<ChangeRequest> ChangeRequests
        {
            get { return _changeRequests; }
            set
            {
                _changeRequests = value;
                OnPropertyChanged(nameof(ChangeRequests));
            }
        }

        private ObservableCollection<TeamItem> _teams = new ObservableCollection<TeamItem>();
        public ObservableCollection<TeamItem> Teams
        {
            get { return _teams; }
            set
            {
                _teams = value;
                OnPropertyChanged(nameof(Teams));
            }
        }

        private TeamItem _selectedTeam;
        public TeamItem SelectedTeam
        {
            get { return _selectedTeam; }
            set
            {
                _selectedTeam = value;
                OnPropertyChanged(nameof(SelectedTeam));
                OnPropertyChanged(nameof(CanAssignTeam));
                SaveProjectAndSubmitCommand.NotifyCanExecuteChanged();
            }
        }

        private SpecItem _selectedSpecItem;
        public SpecItem SelectedSpecItem
        {
            get { return _selectedSpecItem; }
            set
            {
                _selectedSpecItem = value;
                OnPropertyChanged(nameof(SelectedSpecItem));
                DeleteSpecItemCommand.NotifyCanExecuteChanged();
            }
        }

        private TaskItem _selectedTask;
        public TaskItem SelectedTask
        {
            get { return _selectedTask; }
            set
            {
                _selectedTask = value;
                OnPropertyChanged(nameof(SelectedTask));
                SaveHoursCommand.NotifyCanExecuteChanged();
                DeleteTaskCommand.NotifyCanExecuteChanged();
            }
        }

        private ChangeRequest _selectedChange;
        public ChangeRequest SelectedChange
        {
            get { return _selectedChange; }
            set
            {
                _selectedChange = value;
                OnPropertyChanged(nameof(SelectedChange));
                ApproveChangeCommand.NotifyCanExecuteChanged();
                RejectChangeCommand.NotifyCanExecuteChanged();
            }
        }

        private string _customerInfo;
        public string CustomerInfo
        {
            get { return _customerInfo; }
            set
            {
                _customerInfo = value;
                OnPropertyChanged(nameof(CustomerInfo));
            }
        }

        private string _teamInfo;
        public string TeamInfo
        {
            get { return _teamInfo; }
            set
            {
                _teamInfo = value;
                OnPropertyChanged(nameof(TeamInfo));
            }
        }

        private string _totalCostText;
        public string TotalCostText
        {
            get { return _totalCostText; }
            set
            {
                _totalCostText = value;
                OnPropertyChanged(nameof(TotalCostText));
            }
        }

        private void UpdateTotalCost()
        {
            decimal total = 0;
            if (Invoice != null) total = Invoice.TotalCost;
            else if (Project != null) total = Project.TotalCost;
            TotalCostText = "Общая стоимость: " + total.ToString("N2") + " ₽";
        }

        // ===== Роли =====
        public bool IsCustomer { get { return _role.Equals("customer", StringComparison.OrdinalIgnoreCase); } }
        public bool IsManager { get { return _role.Equals("manager", StringComparison.OrdinalIgnoreCase); } }
        public bool IsDeveloper { get { return _role.Equals("developer", StringComparison.OrdinalIgnoreCase); } }

        // ===== Статусы =====
        public bool IsDraft { get { return Project != null && Project.Status == "draft"; } }
        public bool IsInProgress { get { return Project != null && Project.Status == "in_progress"; } }
        public bool IsPendingInvoiceApproval { get { return Project != null && Project.Status == "pending_invoice_approval"; } }
        public bool IsPendingSpecApproval { get { return Project != null && Project.Status == "pending_spec_approval"; } }
        public bool IsFrozen { get { return Project != null && Project.Status == "frozen"; } }
        public bool IsEditable { get { return Project != null && Project.Status != "completed" && Project.Status != "closed"; } }

        // ===== Права =====
        public bool CanEditProjectHeader { get { return IsCustomer && IsDraft; } }
        public bool CanEditSpec { get { return IsCustomer && IsDraft; } }
        public bool CanAssignTeam { get { return IsCustomer && IsDraft && SelectedTeam != null; } }
        public bool CanEditInvoice { get { return IsManager && IsInProgress; } }
        public bool CanManageTasks { get { return (IsManager || IsDeveloper) && IsInProgress; } }
        public bool CanEditHours { get { return IsDeveloper && IsInProgress; } }
        public bool CanSendInvoice { get { return IsManager && IsInProgress; } }
        public bool CanApproveChanges { get { return IsCustomer && IsPendingInvoiceApproval && SelectedChange != null; } }
        public bool CanFinishApproval { get { return IsCustomer && IsPendingInvoiceApproval; } }
        public bool CanCloseProject { get { return (IsCustomer || IsManager) && IsEditable; } }
        public bool CanFreeze { get { return (IsCustomer || IsManager) && IsEditable; } }
        public bool CanUnfreeze { get { return (IsCustomer || IsManager) && IsFrozen; } }

        private bool CanSaveProjectAndSubmit() => CanEditProjectHeader;
        private bool CanSaveInvoiceCmd() => CanEditInvoice;
        private bool CanSaveHoursCmd() => CanEditHours;
        private bool CanSendInvoiceCmd() => CanSendInvoice;
        private bool CanApproveChangesCmd() => CanApproveChanges;
        private bool CanFinishApprovalCmd() => CanFinishApproval;
        private bool CanAddSpecItem() => CanEditSpec;
        private bool CanDeleteSpecItem() => CanEditSpec;
        private bool CanAddTask() => CanManageTasks;
        private bool CanDeleteTask() => CanManageTasks;
        private bool CanCloseProjectCmd() => CanCloseProject;
        private bool CanFreezeCmd() => CanFreeze;
        private bool CanUnfreezeCmd() => CanUnfreeze;

        private void RaiseAllVisibility()
        {
            OnPropertyChanged(nameof(IsDraft));
            OnPropertyChanged(nameof(IsInProgress));
            OnPropertyChanged(nameof(IsPendingInvoiceApproval));
            OnPropertyChanged(nameof(IsPendingSpecApproval));
            OnPropertyChanged(nameof(IsFrozen));
            OnPropertyChanged(nameof(IsEditable));
            OnPropertyChanged(nameof(CanEditProjectHeader));
            OnPropertyChanged(nameof(CanEditSpec));
            OnPropertyChanged(nameof(CanAssignTeam));
            OnPropertyChanged(nameof(CanEditInvoice));
            OnPropertyChanged(nameof(CanManageTasks));
            OnPropertyChanged(nameof(CanEditHours));
            OnPropertyChanged(nameof(CanSendInvoice));
            OnPropertyChanged(nameof(CanApproveChanges));
            OnPropertyChanged(nameof(CanFinishApproval));
            OnPropertyChanged(nameof(CanCloseProject));
            OnPropertyChanged(nameof(CanFreeze));
            OnPropertyChanged(nameof(CanUnfreeze));
        }

        // ===== Загрузка =====
        public async Task LoadAllAsync()
        {
            Project = await _projectService.GetProject(_projectId);
            if (Project == null) return;



            Invoice = Project.IdInvoice != null
                ? await _invoiceService.GetInvoice(Project.IdInvoice.Value)
                : null;

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
            else TeamInfo = "Команда: не назначена";

            // 1. Шапка ТЗ по проекту
            Specification = await _specService.GetSpecByProject(_projectId);

            // 2. Пункты ТЗ по IdSpec
            if (Specification != null)
            {
                var items = await _specService.GetItemsBySpec(Specification.IdSpec);
                SpecItems = new ObservableCollection<SpecItem>(items);
            }
            else
            {
                SpecItems = new ObservableCollection<SpecItem>();
            }

            var allTasks = await _taskService.GetAll() ?? new List<TaskItem>();
            if (IsDeveloper)
                Tasks = new ObservableCollection<TaskItem>(allTasks.Where(t => t.IdDeveloper == _userId));
            else
                Tasks = new ObservableCollection<TaskItem>(allTasks);

            var changes = await _changeService.GetAll() ?? new List<ChangeRequest>();
            ChangeRequests = new ObservableCollection<ChangeRequest>(
                changes.Where(c => c.Status == "created"));

            Teams = new ObservableCollection<TeamItem>(
                await _directoryService.GetTeams() ?? new List<TeamItem>());

            UpdateTotalCost();
        }

        // ===== Заказчик: сохранить и отправить =====
        [RelayCommand]
        private async Task SaveProjectAndSubmit()
        {
            if (MessageBox.Show(
                "Сохранить проект и отправить команде?",
                "Подтверждение",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            try
            {
                // 1. Назначаем команду ДО сохранения проекта — один PUT вместо двух
                if (SelectedTeam != null)
                    Project.IdTeam = SelectedTeam.IdTeam;

                // 2. Сохраняем шапку проекта
                if (!await _projectService.Update(Project))
                {
                    CustomMessageBox.Show("Не удалось сохранить проект");
                    return;
                }

                // 3. Сохраняем шапку ТЗ (если она есть)
                if (Specification != null)
                {
                    if (Specification.IdProject == 0)
                        Specification.IdProject = _projectId;

                    await _specService.UpdateSpec(Specification);
                }

                // 4. Синхронизируем пункты ТЗ: новые — create, изменённые — update
                foreach (var item in SpecItems)
                {
                    if (item.IsNew)
                    {
                        item.IdSpec = Specification?.IdSpec ?? item.IdSpec;

                        var created = await _specService.CreateItem(item);
                        if (created != null)
                        {
                            // подтягиваем присвоенный сервером Id и снимаем флаг
                            item.IdItem = created.IdItem;
                            item.IsNew = false;
                        }
                    }
                    else
                    {
                        await _specService.UpdateItem(item);
                    }
                }

                // 5. Меняем статус проекта (draft → in_progress и т.п.)
                await _projectService.SubmitProject(_projectId);

                CustomMessageBox.Show("Проект отправлен команде");
                await LoadAllAsync();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Ошибка при сохранении/отправке:\n" + ex.Message);
            }
        }

        // ===== Менеджер: сохранить счёт и задачи =====
        [RelayCommand]
        private async Task SaveInvoice()
        {
            try
            {
                if (Invoice != null)
                    await _invoiceService.Update(Invoice);

                foreach (var t in Tasks)
                    await _taskService.Update(t);

                MessageBox.Show("Счёт и задачи сохранены");
                await LoadAllAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // ===== Разработчик: сохранить часы =====
        [RelayCommand]
        private async Task SaveHours()
        {
            if (SelectedTask == null) return;
            try
            {
                await _taskService.Update(SelectedTask);
                MessageBox.Show("Часы сохранены");
                await LoadAllAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // ===== Менеджер: отправить =====
        [RelayCommand]
        private async Task SendInvoice()
        {
            try
            {
                await _projectService.SendInvoice(_projectId);
                MessageBox.Show("Счёт отправлен заказчику");
                await LoadAllAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // ===== Заказчик: принять/отклонить =====
        [RelayCommand]
        private async Task ApproveChange()
        {
            if (SelectedChange == null) return;
            try
            {
                SelectedChange.Status = "approved";
                SelectedChange.IdApprover = _userId;
                await _changeService.Update(SelectedChange);
                MessageBox.Show("Изменение одобрено");
                await LoadAllAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        [RelayCommand]
        private async Task RejectChange()
        {
            if (SelectedChange == null) return;
            try
            {
                SelectedChange.Status = "revoked";
                SelectedChange.IdApprover = _userId;
                await _changeService.Update(SelectedChange);
                MessageBox.Show("Изменение отклонено");
                await LoadAllAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // ===== Заказчик: сохранить изменения =====
        [RelayCommand]
        private async Task FinishApproval()
        {
            try
            {
                await _projectService.FinishApproval(_projectId);
                MessageBox.Show("Изменения сохранены, проект возвращён в работу");
                await LoadAllAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // ===== ТЗ =====
        [RelayCommand]
        private void AddSpecItem()
        {
            var item = new SpecItem
            {
                IdSpec = Specification.IdSpec,
                Name = "Новый пункт",
                Description = "Описание",
                IsNew = true
            };
            SpecItems.Add(item);
        }

        [RelayCommand]
        private async Task DeleteSpecItem()
        {
            if (SelectedSpecItem == null) return;
            try
            {
                if (!SelectedSpecItem.IsNew)
                    await _specService.DeleteItem(SelectedSpecItem.IdItem);
                SpecItems.Remove(SelectedSpecItem);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // ===== Задачи =====
        [RelayCommand]
        private async Task AddTask()
        {
            try
            {
                var t = new TaskItem
                {
                    IdTaskList = 1,
                    Description = "Новая задача",
                    Status = 0
                };
                await _taskService.Create(t);
                await LoadAllAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        [RelayCommand]
        private async Task DeleteTask()
        {
            if (SelectedTask == null) return;
            try
            {
                await _taskService.Delete(SelectedTask.IdTask);
                await LoadAllAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // ===== Закрыть / заморозить / разморозить =====
        [RelayCommand]
        private async Task CloseProject()
        {
            var r = MessageBox.Show(
                "Закрыть окончательно? (Да = Закрыт, Нет = Закончен)",
                "Закрытие", MessageBoxButton.YesNoCancel);
            if (r == MessageBoxResult.Cancel) return;

            try
            {
                await _projectService.Close(_projectId, r == MessageBoxResult.Yes);
                await LoadAllAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        [RelayCommand]
        private async Task FreezeProject()
        {
            try
            {
                await _projectService.Freeze(_projectId);
                await LoadAllAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        [RelayCommand]
        private async Task UnfreezeProject()
        {
            try
            {
                await _projectService.Unfreeze(_projectId);
                await LoadAllAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}