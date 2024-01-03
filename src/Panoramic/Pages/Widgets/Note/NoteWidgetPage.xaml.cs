using System;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Models.Domain.Note;
using Panoramic.Services;
using Panoramic.ViewModels.Widgets.Note;

namespace Panoramic.Pages.Widgets.Note;

public sealed partial class NoteWidgetPage : Page
{
    private readonly IStorageService _storageService;
    private readonly IMarkdownService _markdownService;
    private readonly NoteWidget _widget;

    public NoteWidgetPage(IServiceProvider serviceProvider, NoteWidget widget)
    {
        InitializeComponent();

        _storageService = serviceProvider.GetRequiredService<IStorageService>();
        _markdownService = serviceProvider.GetRequiredService<IMarkdownService>();
        _widget = widget;

        ViewModel = new NoteViewModel(widget);

        SetPresenterContent();
    }

    private void Editor_TextChanged(object _, TextChangedEventArgs e)
    {
        _widget.Text = ViewModel.Text;
        _storageService.EnqueueWidgetWrite(_widget.Id);
    }

    public NoteViewModel ViewModel { get; }

    private async void SettingsButton_Click(object _, RoutedEventArgs e)
    {
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

        content.StepChanged += (_, e) => { dialog!.Title = e.DialogTitle; };
        content.Validated += (_, e) => { dialog!.IsPrimaryButtonEnabled = e.Valid; };

        ViewModel.Highlighted = true;

        await dialog.ShowAsync();
    }

    private async void RemoveButton_Click(object _, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Remove widget",
            Content = $"Are you sure want to remove {_widget.Title}?\n\nAny data that it holds will also be deleted permanently.",
            PrimaryButtonText = "Yes, remove",
            CloseButtonText = "Cancel",
            PrimaryButtonCommand = new RelayCommand(() => { _storageService.DeleteWidget(_widget); })
        };
        await dialog.ShowAsync();
    }

    private void SetPresenterContent()
    {
        var paragraphs = _markdownService.TextToMarkdownParagraphs(ViewModel.Text);
        Presenter.Blocks.Clear();

        foreach (var paragraph in paragraphs)
        {
            Presenter.Blocks.Add(paragraph);
        }
    }

    private void EditButton_Click(object _, RoutedEventArgs e)
    {
        if (ViewModel.Editing)
        {
            Editor.Focus(FocusState.Programmatic);
        }
        else
        {
            SetPresenterContent();
        }
    }
}
