using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Panoramic.Data;
using Panoramic.Services;

namespace Panoramic.ViewModels.LinkDrawers;

public sealed partial class ViewDrawerViewModel : ObservableObject
{
    private readonly IEventHub _eventHub;
    private readonly LinkDrawerData _data;

    public ViewDrawerViewModel(IEventHub eventHub, LinkDrawerData data)
    {
        _eventHub = eventHub;
        _data = data;

        Name = _data.Name;
        foreach (var link in _data.Links)
        {
            var searchTermsString = string.Join(", ", link.SearchTerms);
            var vm = new LinkViewModel(_eventHub, _data.Name, link.Title, link.Uri);
            vm.Clicked += (object? _, EventArgs e) => { LinkClicked?.Invoke(this, EventArgs.Empty); };

            Links.Add(vm);
        }
    }

    public event EventHandler<EventArgs>? LinkClicked;

    public string Name { get; }

    // TODO: Make links searchable
    [ObservableProperty]
    private string searchText = string.Empty;

    public ObservableCollection<LinkViewModel> Links { get; } = [];
}
