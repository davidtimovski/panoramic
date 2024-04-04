using System.Globalization;

namespace Panoramic.Data;

public sealed class Area
{
    public Area(string name)
    {
        Name = name;

        short y1 = short.Parse(name[0].ToString(), CultureInfo.InvariantCulture);
        short x1 = short.Parse(name[1].ToString(), CultureInfo.InvariantCulture);
        short y2 = short.Parse(name[3].ToString(), CultureInfo.InvariantCulture);
        short x2 = short.Parse(name[4].ToString(), CultureInfo.InvariantCulture);

        Row = y1;
        Column = x1;
        RowSpan = y2 - y1 + 1;
        ColumnSpan = x2 - x1 + 1;
    }

    public string Name { get; }
    public int Row { get; }
    public int Column { get; }
    public int RowSpan { get; }
    public int ColumnSpan { get; }

    public override string ToString() => Name;
}
