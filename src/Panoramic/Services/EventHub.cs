using System;

namespace Panoramic.Services;

public interface IEventHub
{
    event EventHandler<string>? SearchInvoked;
    event EventHandler<HyperlinkClickedEventArgs>? HyperlinkClicked;

    void RaiseSearchInvoked(string searchText);
    void RaiseHyperlinkClicked(string title, Uri uri, DateTime clicked);
}

public sealed class EventHub : IEventHub
{
    public event EventHandler<string>? SearchInvoked;
    public event EventHandler<HyperlinkClickedEventArgs>? HyperlinkClicked;

    public void RaiseSearchInvoked(string searchText) => SearchInvoked?.Invoke(this, searchText);

    public void RaiseHyperlinkClicked(string title, Uri uri, DateTime clicked)
        => HyperlinkClicked?.Invoke(this, new HyperlinkClickedEventArgs
        {
            Title = title,
            Uri = uri,
            Clicked = clicked
        });
}

public sealed class HyperlinkClickedEventArgs : EventArgs
{
    public required string Title { get; init; }
    public required Uri Uri { get; init; }
    public required DateTime Clicked { get; init; }
}
