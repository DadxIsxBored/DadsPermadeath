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
    private const string PopupText = "You have selected Permadeath, if you die, your selected world and character will be deleted. Are you sure you want to proceed?";

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
            .Where(button => button.m_preset != WorldPresets.Default &&
                             button.m_preset != WorldPresets.Normal &&
                             button.m_preset != WorldPresets.Custom)
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

        IntegratePresetRows(gui, clone.GetComponent<RectTransform>(), templateButton);
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

    internal static bool RemovePermadeathKeyForSummary(ref IEnumerable<string> keys)
    {
        List<string> filteredKeys = keys.ToList();
        bool containedPermadeath = ContainsKey(filteredKeys);
        if (containedPermadeath)
        {
            RemoveKey(filteredKeys);
            keys = filteredKeys;
        }

        return containedPermadeath;
    }

    internal static void AddPermadeathToSummary(
        bool compact,
        string separator,
        bool containedPermadeath,
        ref string summary)
    {
        if (!containedPermadeath)
        {
            return;
        }

        if (string.IsNullOrEmpty(summary))
        {
            summary = "Permadeath";
            return;
        }

        if (compact)
        {
            summary = "Permadeath+";
            return;
        }

        summary = "Permadeath" + separator + summary;
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

    private static void IntegratePresetRows(
        ServerOptionsGUI gui,
        RectTransform customRect,
        Button templateButton)
    {
        KeyButton? resetKeyButton = gui.m_presetsRoot
            .GetComponentsInChildren<KeyButton>(true)
            .FirstOrDefault(button => button != _keyButton &&
                                      (button.m_preset == WorldPresets.Default ||
                                       button.m_preset == WorldPresets.Normal));
        Button? resetButton = resetKeyButton?.GetComponentInParent<Button>();

        RectTransform[] presetRects = gui.m_presetsRoot
            .GetComponentsInChildren<KeyButton>(true)
            .Where(button => button != _keyButton &&
                             button.m_preset != WorldPresets.Default &&
                             button.m_preset != WorldPresets.Normal &&
                             button.m_preset != WorldPresets.Custom)
            .Select(button => button.GetComponentInParent<Button>()?.GetComponent<RectTransform>())
            .Where(rect => rect != null)
            .Cast<RectTransform>()
            .Distinct()
            .OrderBy(rect => rect.GetSiblingIndex())
            .ToArray();

        if (resetButton != null && presetRects.Length >= 4)
        {
            Canvas.ForceUpdateCanvases();

            RectTransform resetRect = resetButton.GetComponent<RectTransform>();
            float width = templateButton.GetComponent<RectTransform>().rect.width;
            float height = templateButton.GetComponent<RectTransform>().rect.height;
            RectTransform[] leftColumn = presetRects
                .OrderBy(rect => rect.position.x)
                .Take(2)
                .OrderByDescending(rect => rect.position.y)
                .ToArray();
            if (leftColumn.Length != 2)
            {
                return;
            }

            RectTransform firstRowLeft = leftColumn[0];
            RectTransform secondRowLeft = leftColumn[1];
            float signedRowShift = secondRowLeft.anchoredPosition.y - firstRowLeft.anchoredPosition.y;
            float rowHeight = Mathf.Abs(signedRowShift);
            if (rowHeight <= 0f)
            {
                rowHeight = GetPresetRowHeight(presetRects);
                signedRowShift = -rowHeight;
            }

            RectTransform? panelRect = gui.transform.Find("panel") as RectTransform;
            if (panelRect == null)
            {
                panelRect = gui.GetComponent<RectTransform>();
            }
            Vector2 permadeathPosition = new(
                secondRowLeft.anchoredPosition.x,
                secondRowLeft.anchoredPosition.y + signedRowShift);
            Vector2 resetPosition = new(
                resetRect.anchoredPosition.x,
                resetRect.anchoredPosition.y + signedRowShift);

            customRect.SetParent(resetRect.parent, false);
            customRect.SetSiblingIndex(resetRect.GetSiblingIndex());
            customRect.anchorMin = secondRowLeft.anchorMin;
            customRect.anchorMax = secondRowLeft.anchorMax;
            customRect.pivot = secondRowLeft.pivot;
            customRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            customRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            PermadeathLayoutController controller =
                gui.gameObject.GetComponent<PermadeathLayoutController>() ??
                gui.gameObject.AddComponent<PermadeathLayoutController>();
            controller.Configure(
                panelRect,
                customRect,
                resetRect,
                permadeathPosition,
                resetPosition,
                rowHeight,
                width,
                height);
            DadsPermadeathPlugin.Log.LogInfo(
                $"Four-row preset layout applied using reset button '{resetButton.name}'.");
            return;
        }

        DadsPermadeathPlugin.Log.LogError(
            "DadsPermadeath could not locate Valheim's Default/Normal reset preset; four-row layout was not applied.");
    }

    private static float GetPresetRowHeight(IReadOnlyList<RectTransform> presetRects)
    {
        float closestColumnDistance = float.MaxValue;
        float rowHeight = 0f;

        for (int first = 0; first < presetRects.Count; first++)
        {
            for (int second = first + 1; second < presetRects.Count; second++)
            {
                Vector3 delta = presetRects[second].position - presetRects[first].position;
                float verticalDistance = Mathf.Abs(delta.y);
                if (verticalDistance < 1f)
                {
                    continue;
                }

                float columnDistance = Mathf.Abs(delta.x);
                if (columnDistance < closestColumnDistance ||
                    (Mathf.Approximately(columnDistance, closestColumnDistance) &&
                     (rowHeight <= 0f || verticalDistance < rowHeight)))
                {
                    closestColumnDistance = columnDistance;
                    rowHeight = verticalDistance;
                }
            }
        }

        return rowHeight;
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

internal sealed class PermadeathLayoutController : MonoBehaviour
{
    private RectTransform? _panel;
    private RectTransform? _permadeath;
    private RectTransform? _reset;
    private Vector2 _permadeathPosition;
    private Vector2 _resetPosition;
    private Vector2 _panelPosition;
    private float _panelHeight;
    private float _rowHeight;
    private float _buttonWidth;
    private float _buttonHeight;

    internal void Configure(
        RectTransform? panel,
        RectTransform permadeath,
        RectTransform reset,
        Vector2 permadeathPosition,
        Vector2 resetPosition,
        float rowHeight,
        float buttonWidth,
        float buttonHeight)
    {
        _panel = panel;
        _permadeath = permadeath;
        _reset = reset;
        _permadeathPosition = permadeathPosition;
        _resetPosition = resetPosition;
        _panelHeight = panel == null ? 0f : panel.rect.height;
        _panelPosition = panel == null ? Vector2.zero : panel.anchoredPosition;
        _rowHeight = rowHeight;
        _buttonWidth = buttonWidth;
        _buttonHeight = buttonHeight;

        ApplyLayout();
    }

    private void LateUpdate()
    {
        ApplyLayout();
    }

    private void ApplyLayout()
    {
        if (_permadeath == null || _reset == null || _rowHeight <= 0f)
        {
            return;
        }

        if (_panel != null)
        {
            _panel.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                _panelHeight + _rowHeight);
            _panel.anchoredPosition = _panelPosition +
                                      Vector2.down * (_rowHeight * (1f - _panel.pivot.y));
        }

        _permadeath.anchoredPosition = _permadeathPosition;
        _permadeath.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _buttonWidth);
        _permadeath.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _buttonHeight);
        _reset.anchoredPosition = _resetPosition;
    }
}
