using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Panoramic.Models.Domain.Note;

public class ExplorerItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate FolderTemplate { get; set; }
    public DataTemplate FileTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        var explorerItem = (ExplorerItem)item;
        return explorerItem.Type == FileType.Folder ? FolderTemplate : FileTemplate;
    }
}
