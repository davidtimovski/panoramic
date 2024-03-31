using System;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Domain.Checklist;
using Panoramic.Services.Search;
using Panoramic.Services.Storage;
using Panoramic.ViewModels.Widgets.Checklist;

namespace Panoramic.Pages.Widgets.Checklist;

public sealed partial class ChecklistWidgetPage : Page
{
    private readonly IStorageService _storageService;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly ChecklistWidget _widget;

    public ChecklistWidgetPage(IServiceProvider serviceProvider, ChecklistWidget widget)
    {
        InitializeComponent();

        _storageService = serviceProvider.GetRequiredService<IStorageService>();
        _dispatcherQueue = serviceProvider.GetRequiredService<DispatcherQueue>();
        _widget = widget;

        var searchService = serviceProvider.GetRequiredService<ISearchService>();
        ViewModel = new ChecklistViewModel(searchService, _dispatcherQueue, this, widget);
    }

    public ChecklistViewModel ViewModel { get; }

    public void OpenNewTaskDialog()
    {
        var content = new NewTaskForm(_widget);
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "New task",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => AddTask(content.ViewModel)),
            IsPrimaryButtonEnabled = false
        };
       
        content.ViewModel.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };
        content.Submitted += (_, e) =>
        {
            AddTask(content.ViewModel);
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

        var data = _widget.GetData();
        var vm = new EditViewModel(_storageService, data);

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

        await dialog.ShowAsync();
    }

    private async void SettingsButton_Click(object _, RoutedEventArgs e)
    {
        ViewModel.Highlighted = true;

        var content = new EditWidgetDialog(_widget, _storageService);
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

        content.AttachSettingsValidationHandler((_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; });
        content.StepChanged += (_, e) => { dialog!.Title = e.DialogTitle; };

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
        => _widget.AddTask(viewModel.Title, viewModel.DueDate.HasValue ? DateOnly.FromDateTime(viewModel.DueDate.Value.Date) : null);
}
