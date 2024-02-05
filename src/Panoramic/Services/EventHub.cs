using System;
using Panoramic.Models.Domain.Note;

namespace Panoramic.Services;

public interface IEventHub
{
    event EventHandler<HyperlinkClickedEventArgs>? HyperlinkClicked;
    event EventHandler<FileCreatedEventArgs>? FileCreated;
    event EventHandler<EventArgs>? FileRenamed;
    event EventHandler<FileDeletedEventArgs>? FileDeleted;

    void RaiseHyperlinkClicked(string title, Uri uri, DateTime clicked);

    void RaiseFileCreated(string name, string directory, FileType type);

    void RaiseFileRenamed();

    void RaiseFileDeleted(string path);
}

public class EventHub : IEventHub
{
    public event EventHandler<HyperlinkClickedEventArgs>? HyperlinkClicked;
    public event EventHandler<FileCreatedEventArgs>? FileCreated;
    public event EventHandler<EventArgs>? FileRenamed;
    public event EventHandler<FileDeletedEventArgs>? FileDeleted;

    public void RaiseHyperlinkClicked(string title, Uri uri, DateTime clicked)
        => HyperlinkClicked?.Invoke(this, new HyperlinkClickedEventArgs(title, uri, clicked));

    public void RaiseFileCreated(string name, string directory, FileType type)
        => FileCreated?.Invoke(this, new FileCreatedEventArgs(name, directory, type));

    public void RaiseFileRenamed()
      => FileRenamed?.Invoke(this, new EventArgs());

    public void RaiseFileDeleted(string path)
        => FileDeleted?.Invoke(this, new FileDeletedEventArgs(path));
}

public class HyperlinkClickedEventArgs(string title, Uri uri, DateTime clicked) : EventArgs
{
    public string Title { get; } = title;
    public Uri Uri { get; } = uri;
    public DateTime Clicked { get; } = clicked;
}

public class FileCreatedEventArgs(string name, string directory, FileType type) : EventArgs
{
    public string Name { get; } = name;
    public string Directory { get; } = directory;
    public FileType Type { get; } = type;
}

public class FileDeletedEventArgs(string path) : EventArgs
{
    public string Path { get; } = path;
}
