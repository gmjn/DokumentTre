using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DokumentTre.View.Controls;

public class ContextMenuSelectButton : Button
{
    public static readonly DependencyProperty DropDownProperty = DependencyProperty.Register("DropDown", typeof(ContextMenu), typeof(ContextMenuSelectButton), new UIPropertyMetadata(null));

    public ContextMenuSelectButton()
    {
        FlowDirection = FlowDirection.RightToLeft;
    }

    public ContextMenu DropDown
    {
        get { return (ContextMenu)GetValue(DropDownProperty); }
        set { SetValue(DropDownProperty, value); }
    }

    protected override void OnClick()
    {
        if (DropDown != null)
        {
            DropDown.PlacementTarget = this;
            DropDown.Placement = PlacementMode.Bottom;
            DropDown.FlowDirection = FlowDirection.LeftToRight;

            DropDown.IsOpen = true;
        }
    }
}