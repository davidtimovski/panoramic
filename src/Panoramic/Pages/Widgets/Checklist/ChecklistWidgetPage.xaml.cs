using System;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Domain.Checklist;
using Panoramic.Services;
using Panoramic.Services.Notes;
using Panoramic.Services.Search;
using Panoramic.Services.Storage;
using Panoramic.ViewModels.Widgets.Checklist;

namespace Panoramic.Pages.Widgets.Checklist;

public sealed partial class ChecklistWidgetPage : Page
{
    private readonly IStorageService _storageService;
    private readonly INotesOrchestrator _notesOrchestrator;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly ChecklistWidget _widget;

    public ChecklistWidgetPage(IServiceProvider serviceProvider, ChecklistWidget widget)
    {
        InitializeComponent();

        _storageService = serviceProvider.GetRequiredService<IStorageService>();
        _notesOrchestrator = serviceProvider.GetRequiredService<INotesOrchestrator>();
        _dispatcherQueue = serviceProvider.GetRequiredService<DispatcherQueue>();
        _widget = widget;

        var eventHub = serviceProvider.GetRequiredService<IEventHub>();
        var searchService = serviceProvider.GetRequiredService<ISearchService>();
        ViewModel = new ChecklistViewModel(eventHub, searchService, _dispatcherQueue, widget);
    }

    public ChecklistViewModel ViewModel { get; }

    public void OpenNewTaskDialog()
    {
        var content = new NewTaskForm(_widget);
        void addTask() => AddTask(content.ViewModel);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "New task",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(addTask),
            IsPrimaryButtonEnabled = false
        };

        content.ViewModel.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };
        content.Submitted += (_, e) =>
        {
            addTask();
            dialog.Hide();
        };

        _dispatcherQueue.TryEnqueue(async () =>
        {
            await dialog.ShowAsync();
        });
    }

    private void AddTask_Click(object _, RoutedEventArgs e) => OpenNewTaskDialog();

    private async void EditButton_Click(object _, RoutedEventArgs e)
    {
        ViewModel.Highlighted = true;

        var vm = new EditViewModel(_storageService, _widget);

        var content = new EditDialog(vm);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Edit checklist",
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new AsyncRelayCommand(vm.SaveAsync),
            CloseButtonCommand = new RelayCommand(() => { ViewModel.Highlighted = false; })
        };

        vm.Validated += (_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; };

        await dialog.ShowAsync();
    }

    private async void SettingsButton_Click(object _, RoutedEventArgs e)
    {
        ViewModel.Highlighted = true;

        var content = new EditWidgetDialog(_widget, _storageService, _notesOrchestrator);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = content.EditSettingsTitle,
            Content = content,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new AsyncRelayCommand(content.SubmitAsync),
            CloseButtonCommand = new RelayCommand(() => { ViewModel.Highlighted = false; })
        };

        content.AttachSettingsValidationHandler((_, e) => { dialog.IsPrimaryButtonEnabled = e.Valid; });
        content.StepChanged += (_, e) => { dialog.Title = e.DialogTitle; };

        await dialog.ShowAsync();
    }

    private async void DeleteButton_Click(object _, RoutedEventArgs e)
    {
        ViewModel.Highlighted = true;

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Delete widget",
            Content = $"Are you sure want to delete {_widget.Title}?\n\nAny data that it holds will also be deleted permanently.",
            PrimaryButtonText = "Yes, delete",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => { _storageService.DeleteWidget(_widget); }),
            CloseButtonCommand = new RelayCommand(() => { ViewModel.Highlighted = false; })
        };

        await dialog.ShowAsync();
    }

    private void AddTask(NewTaskViewModel viewModel)
    {
        var dueDate = viewModel.DueDate.HasValue ? DateOnly.FromDateTime(viewModel.DueDate.Value.Date) : (DateOnly?)null;
        var uri = viewModel.Url.Trim().Length > 0 ? new Uri(viewModel.Url.Trim()) : null;
        _widget.AddTask(viewModel.Title, dueDate, uri);
    }
}
