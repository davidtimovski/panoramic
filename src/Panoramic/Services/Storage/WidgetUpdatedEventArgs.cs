﻿using System;

namespace Panoramic.Services.Storage;

public sealed class WidgetUpdatedEventArgs : EventArgs
{
    public required Guid Id { get; init; }
}
