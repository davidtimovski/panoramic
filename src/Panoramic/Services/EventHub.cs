using System;

namespace Panoramic.Services;

public interface IEventHub
{
    event EventHandler<HyperlinkClickedEventArgs>? HyperlinkClicked;

    void RaiseHyperlinkClicked(string title, Uri uri, DateTime clicked);
}

public class EventHub : IEventHub
{
    public event EventHandler<HyperlinkClickedEventArgs>? HyperlinkClicked;

    public void RaiseHyperlinkClicked(string title, Uri uri, DateTime clicked)
    {
        HyperlinkClicked?.Invoke(this, new HyperlinkClickedEventArgs(title, uri, clicked));
    }
}

public class HyperlinkClickedEventArgs(string title, Uri uri, DateTime clicked) : EventArgs
{
    public string Title { get; } = title;
    public Uri Uri { get; } = uri;
    public DateTime Clicked { get; } = clicked;
}
