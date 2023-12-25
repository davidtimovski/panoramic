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

public class HyperlinkClickedEventArgs : EventArgs
{
    public HyperlinkClickedEventArgs(string title, Uri uri, DateTime clicked)
    {
        Title = title;
        Uri = uri;
        Clicked = clicked;
    }

    public string Title { get; }
    public Uri Uri { get; }
    public DateTime Clicked { get; }
}
