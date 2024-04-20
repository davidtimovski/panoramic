using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Panoramic.ViewModels.Widgets.Checklist;

public sealed class TaskTemplateSelector : DataTemplateSelector
{
    public DataTemplate? TaskTemplate { get; set; }
    public DataTemplate? UriTaskTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        var task = (TaskViewModel)item;
        return task.Uri is null ? TaskTemplate : UriTaskTemplate;
    }
}
