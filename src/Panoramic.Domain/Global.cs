using System.Globalization;

namespace Panoramic.Data;

public static class Global
{
    public static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
    public const string StoredDateOnlyFormat = "yyyy-MM-dd";
    public const string StoredDateTimeFormat = "yyyy-MM-dd HH:mm:ss.ff";
}
