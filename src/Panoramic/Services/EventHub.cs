using System;

namespace Panoramic.Services;

public interface IEventHub
{
    event EventHandler<HyperlinkClickedEventArgs>? HyperlinkClicked;

    void RaiseHyperlinkClicked(string id, string title, Uri uri);
}

public class EventHub : IEventHub
{
    public event EventHandler<HyperlinkClickedEventArgs>? HyperlinkClicked;

    public void RaiseHyperlinkClicked(string id, string title, Uri uri)
    {
        HyperlinkClicked?.Invoke(this, new HyperlinkClickedEventArgs(id, title, uri));
    }
}

public class HyperlinkClickedEventArgs : EventArgs
{
    public HyperlinkClickedEventArgs(string id, string title, Uri uri)
    {
        Id = id;
        Title = title;
        Uri = uri;
    }

    public string Id { get; set; }
    public string Title { get; }
    public Uri Uri { get; }
}
