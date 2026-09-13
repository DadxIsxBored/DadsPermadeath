using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DadsPermadeath;

internal static class PermadeathWorldModifier
{
    internal const string GlobalKey = "dadspermadeath";

    private const string ButtonText = "DadsPermadeath";
    private const string TipText = "This mode with test your ability to stay alive. If you die, your world and character will be deleted and you will be sent back to the main menu.";
    private const string PopupText = "You have selected Permadeath, if you die, yourselected world and character will be deleted. Are you sure you want to proceed?";

    private static ServerOptionsGUI? _gui;
    private static World? _world;
    private static Button? _button;
    private static KeyButton? _keyButton;
    private static TMP_Text? _buttonLabel;
    private static ColorBlock _normalColors;
    private static bool _selected;

    internal static void CreateButton(ServerOptionsGUI gui)
    {
        if (!DadsPermadeathPlugin.ModEnabled.Value || _button != null)
        {
            return;
        }

        KeyButton? templateKeyButton = gui.m_presetsRoot
            .GetComponentsInChildren<KeyButton>(true)
            .Where(button => button.m_preset != WorldPresets.Normal)
            .OrderBy(button => button.transform.GetSiblingIndex())
            .LastOrDefault();
        Button? templateButton = templateKeyButton?.GetComponentInParent<Button>();
        if (templateKeyButton == null || templateButton == null)
        {
            DadsPermadeathPlugin.Log.LogError("DadsPermadeath could not locate a world preset button template.");
            return;
        }

        GameObject clone = UnityEngine.Object.Instantiate(
            templateButton.gameObject,
            templateButton.transform.parent,
            false);
        clone.name = "DadsPermadeathPreset";
        clone.transform.SetAsLastSibling();

        _gui = gui;
        _button = clone.GetComponent<Button>();
        _keyButton = clone.GetComponentInChildren<KeyButton>(true);
        _buttonLabel = clone.GetComponentInChildren<TMP_Text>(true);

        if (_button == null || _keyButton == null || _buttonLabel == null)
        {
            DadsPermadeathPlugin.Log.LogError("DadsPermadeath could not initialize its world modifier button.");
            clone.SetActive(false);
            return;
        }

        _buttonLabel.text = ButtonText;
        _keyButton.m_toolTip = TipText;
        _keyButton.m_toolTipLabel = gui.m_toolTipText;
        _keyButton.m_preset = WorldPresets.Custom;
        _keyButton.m_keys.Clear();
        _button.onClick = new Button.ButtonClickedEvent();
        _button.onClick.AddListener(ShowConfirmation);
        _normalColors = _button.colors;

        PlaceBesideResetButton(gui, clone.GetComponent<RectTransform>(), templateButton);
        UpdateButtonVisual();
    }

    internal static void ReadWorld(ServerOptionsGUI gui, World world)
    {
        _gui = gui;
        _world = world;
        _selected = ContainsKey(world.m_startingGlobalKeys);
        UpdateButtonVisual();
    }

    internal static void WriteWorld(World world)
    {
        RemoveKey(world.m_startingGlobalKeys);
        if (_selected)
        {
            world.m_startingGlobalKeys.Add(GlobalKey);
        }

        world.m_startingKeysChanged = true;
    }

    internal static void ClearSelection()
    {
        _selected = false;
        UpdateButtonVisual();
    }

    internal static bool IsPermadeathButton(KeyButton button)
    {
        return button == _keyButton;
    }

    internal static bool IsActiveInCurrentWorld()
    {
        ZoneSystem? zoneSystem = ZoneSystem.instance;
        if (zoneSystem != null && zoneSystem.GetGlobalKey(GlobalKey))
        {
            return true;
        }

        World? world = ZNet.instance?.GetWorld();
        return world != null && ContainsKey(world.m_startingGlobalKeys);
    }

    private static void ShowConfirmation()
    {
        if (_gui == null || _world == null || !UnifiedPopup.IsAvailable())
        {
            DadsPermadeathPlugin.Log.LogError("DadsPermadeath could not open its confirmation popup.");
            return;
        }

        _gui.m_toolTipText.text = TipText;
        UnifiedPopup.Push(new YesNoPopup(
            ButtonText,
            PopupText,
            ConfirmSelection,
            RejectSelection,
            false,
            true));
    }

    private static void ConfirmSelection()
    {
        UnifiedPopup.Pop();
        if (_gui == null || _world == null)
        {
            return;
        }

        _gui.SetPreset(_world, WorldPresets.Normal);
        _selected = true;
        _gui.m_toolTipText.text = TipText;
        UpdateButtonVisual();
    }

    private static void RejectSelection()
    {
        UnifiedPopup.Pop();
        _selected = false;

        if (_gui != null && _world != null)
        {
            _gui.SetPreset(_world, WorldPresets.Normal);
            SetNormalTooltip(_gui);
        }

        UpdateButtonVisual();
    }

    private static void UpdateButtonVisual()
    {
        if (_button == null)
        {
            return;
        }

        ColorBlock colors = _normalColors;
        if (_selected)
        {
            colors.normalColor = new Color(0.55f, 0.14f, 0.14f, 1f);
            colors.highlightedColor = new Color(0.68f, 0.2f, 0.2f, 1f);
            colors.selectedColor = colors.highlightedColor;
        }

        _button.colors = colors;
    }

    private static void SetNormalTooltip(ServerOptionsGUI gui)
    {
        KeyButton? normalButton = gui.m_presetsRoot
            .GetComponentsInChildren<KeyButton>(true)
            .FirstOrDefault(button => button != _keyButton && button.m_preset == WorldPresets.Normal);

        gui.m_toolTipText.text = normalButton == null
            ? string.Empty
            : Localization.instance.Localize(normalButton.m_toolTip);
    }

    private static void PlaceBesideResetButton(
        ServerOptionsGUI gui,
        RectTransform customRect,
        Button templateButton)
    {
        Button? resetButton = gui.GetComponentsInChildren<Button>(true)
            .Where(button => button != _button && button != templateButton)
            .FirstOrDefault(IsResetButton);

        if (resetButton != null)
        {
            RectTransform resetRect = resetButton.GetComponent<RectTransform>();
            float width = templateButton.GetComponent<RectTransform>().rect.width;
            float spacing = 12f;
            Vector2 center = resetRect.anchoredPosition;

            customRect.SetParent(resetRect.parent, false);
            customRect.SetSiblingIndex(resetRect.GetSiblingIndex());
            customRect.anchorMin = resetRect.anchorMin;
            customRect.anchorMax = resetRect.anchorMax;
            customRect.pivot = resetRect.pivot;
            customRect.sizeDelta = new Vector2(width, customRect.sizeDelta.y);
            customRect.anchoredPosition = new Vector2(center.x - (width + spacing) * 0.5f, center.y);

            resetRect.sizeDelta = new Vector2(width, resetRect.sizeDelta.y);
            resetRect.anchoredPosition = new Vector2(center.x + (width + spacing) * 0.5f, center.y);
            return;
        }

        RectTransform[] presetRects = gui.m_presetsRoot
            .GetComponentsInChildren<KeyButton>(true)
            .Where(button => button != _keyButton)
            .Select(button => button.GetComponentInParent<Button>()?.GetComponent<RectTransform>())
            .Where(rect => rect != null)
            .Cast<RectTransform>()
            .Distinct()
            .OrderBy(rect => rect.GetSiblingIndex())
            .ToArray();

        if (presetRects.Length >= 4)
        {
            Vector2 rowStep = presetRects[3].anchoredPosition - presetRects[0].anchoredPosition;
            customRect.anchoredPosition = presetRects[3].anchoredPosition + rowStep;
        }
    }

    private static bool IsResetButton(Button button)
    {
        TMP_Text? label = button.GetComponentInChildren<TMP_Text>(true);
        return button.name.IndexOf("reset", StringComparison.OrdinalIgnoreCase) >= 0 ||
               (label != null && label.text.IndexOf("reset", StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static bool ContainsKey(IEnumerable<string> keys)
    {
        return keys.Any(key => string.Equals(key, GlobalKey, StringComparison.OrdinalIgnoreCase));
    }

    private static void RemoveKey(List<string> keys)
    {
        keys.RemoveAll(key => string.Equals(key, GlobalKey, StringComparison.OrdinalIgnoreCase));
    }
}
