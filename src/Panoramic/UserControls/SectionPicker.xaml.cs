using System;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Panoramic.Services.Storage;

namespace Panoramic.UserControls;

public sealed partial class SectionPicker : UserControl
{
    private readonly IStorageService _storageService;

    public SectionPicker(IStorageService storageService)
    {
        InitializeComponent();

        _storageService = storageService;

        if (_storageService.Sections.ContainsKey("A1"))
        {
            A1Button.IsEnabled = false;
        }

        if (_storageService.Sections.ContainsKey("A2"))
        {
            A2Button.IsEnabled = false;
        }

        if (_storageService.Sections.ContainsKey("A3"))
        {
            A3Button.IsEnabled = false;
        }

        if (_storageService.Sections.ContainsKey("B1"))
        {
            B1Button.IsEnabled = false;
        }

        if (_storageService.Sections.ContainsKey("B2"))
        {
            B2Button.IsEnabled = false;
        }

        if (_storageService.Sections.ContainsKey("B3"))
        {
            B3Button.IsEnabled = false;
        }

        if (_storageService.Sections.ContainsKey("C1"))
        {
            C1Button.IsEnabled = false;
        }

        if (_storageService.Sections.ContainsKey("C2"))
        {
            C2Button.IsEnabled = false;
        }

        if (_storageService.Sections.ContainsKey("C3"))
        {
            C3Button.IsEnabled = false;
        }

        if (_storageService.Sections.ContainsKey("D1"))
        {
            D1Button.IsEnabled = false;
        }

        if (_storageService.Sections.ContainsKey("D2"))
        {
            D2Button.IsEnabled = false;
        }

        SectionClicked = new RelayCommand<string>((section) => { SectionPicked?.Invoke(this, new SectionPickedEventArgs(section!)); });
    }

    public event EventHandler<SectionPickedEventArgs>? SectionPicked;
    public RelayCommand<string> SectionClicked { get; }
}

public class SectionPickedEventArgs : EventArgs
{
    public SectionPickedEventArgs(string section)
    {
        Section = section;
    }

    public string Section { get; }
}
