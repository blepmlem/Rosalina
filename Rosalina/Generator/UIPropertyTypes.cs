using System.Collections.Generic;

namespace Rosalina;

internal static class UIPropertyTypes
{
    private static readonly IReadOnlyDictionary<string, string> _nativeUITypes = new Dictionary<string, string>()
    {
        // Containers
        { "VisualElement", "UnityEngine.UIElements.VisualElement" },
        { "ScrollView", "UnityEngine.UIElements.ScrollView" },
        { "ListView", "UnityEngine.UIElements.ListView" },
        { "IMGUIContainer", "UnityEngine.UIElements.IMGUIContainer" },
        { "GroupBox", "UnityEngine.UIElements.GroupBox" },
        // Controls
        { "Label", "UnityEngine.UIElements.Label" },
        { "Button", "UnityEngine.UIElements.Button" },
        { "Toggle", "UnityEngine.UIElements.Toggle" },
        { "Scroller", "UnityEngine.UIElements.Scroller" },
        { "TextField", "UnityEngine.UIElements.TextField" },
        { "Foldout", "UnityEngine.UIElements.Foldout" },
        { "Slider", "UnityEngine.UIElements.Slider" },
        { "SliderInt", "UnityEngine.UIElements.SliderInt" },
        { "MinMaxSlider", "UnityEngine.UIElements.MinMaxSlider" },
        { "ProgressBar", "UnityEngine.UIElements.ProgressBar" },
        { "DropdownField", "UnityEngine.UIElements.DropdownField" },
        { "RadioButton", "UnityEngine.UIElements.RadioButton" },
        { "RadioButtonGroup", "UnityEngine.UIElements.RadioButtonGroup" },
        { "Image", "UnityEngine.UIElements.Image" },
    };

    public static string GetUIElementType(string uiElementName)
    {
        return _nativeUITypes.TryGetValue(uiElementName, out string type) ? type : null;
    }
}
