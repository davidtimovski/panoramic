using System;

namespace Panoramic.Services;

public interface IEventHub
{
    event EventHandler<HyperlinkClickedEventArgs>? HyperlinkClicked;

    void RaiseHyperlinkClicked(string id, string title, Uri uri, DateTime clicked);
}

public class EventHub : IEventHub
{
    public event EventHandler<HyperlinkClickedEventArgs>? HyperlinkClicked;

    public void RaiseHyperlinkClicked(string id, string title, Uri uri, DateTime clicked)
    {
        HyperlinkClicked?.Invoke(this, new HyperlinkClickedEventArgs(id, title, uri, clicked));
    }
}

public class HyperlinkClickedEventArgs : EventArgs
{
    public HyperlinkClickedEventArgs(string id, string title, Uri uri, DateTime clicked)
    {
        Id = id;
        Title = title;
        Uri = uri;
        Clicked = clicked;
    }

    public string Id { get; }
    public string Title { get; }
    public Uri Uri { get; }
    public DateTime Clicked { get; }
}
