using System;
using System.IO;
using Panoramic.Services.Storage;
using Windows.Storage;

namespace Panoramic.Utils;

internal static class FileLogger
{
    private const string ErrorLogFileName = "error log.txt";

    private static readonly string LogFilePath;

    static FileLogger()
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        object? storagePathValue = localSettings.Values[nameof(IStorageService.StoragePath)];

        string storagePath;
        if (storagePathValue is null)
        {
            var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), StorageService.DefaultDirectoryName);
            if (!Directory.Exists(defaultPath))
            {
                Directory.CreateDirectory(defaultPath);
            }

            storagePath = defaultPath;
        }
        else
        {
            storagePath = (string)storagePathValue;
        }

        LogFilePath = Path.Combine(storagePath, StorageService.SystemDirectoryName, ErrorLogFileName);
    }

    /// <summary>
    /// Writes out the exception in a text file in the system folder.
    /// </summary>
    internal static void WriteException(Exception ex)
    {
        var exceptionType = ex.GetType().Name;
        var message = ex.Message;

        using var writer = File.AppendText(LogFilePath);
        writer.WriteLine($"{DateTime.Now} {exceptionType}");
        writer.WriteLine($"Message: {message}");
        writer.WriteLine("Stack trace:");
        writer.WriteLine(ex.InnerException!.StackTrace);
        writer.WriteLine();
    }
}
