using System;

namespace Panoramic.Models.Domain.Checklist;

public sealed class TaskCompletedEventArgs : EventArgs
{
    public required string Title { get; init; }
}
