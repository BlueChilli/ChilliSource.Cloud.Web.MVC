
namespace ChilliSource.Cloud.Web.MVC;

/// <summary>
/// Displays a help text after the input field.
/// </summary>
public class HelpTextAttribute : Attribute
{
    /// <summary>
    /// Help text to be displayed
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Specifies whether the help text should be displayed under the label or afterwards as tooltip with help icon.
    /// </summary>
    public bool DisplayAsTooltip { get; set; }

    public HelpTextAttribute(string value, bool displayAsTooltip = false)
    {
        Value = value;
        DisplayAsTooltip = displayAsTooltip;
    }
}