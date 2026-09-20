using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NukeLib.UI;

/// <summary>
/// Button that has handler for pressing instead of releasing
/// </summary>
public class ClickHandler : MonoBehaviour, IPointerDownHandler {
    /// <summary>
    /// Event triggered when LMB is down on this object
    /// </summary>
    public event Action? OnPress;

    /// <summary>
    /// Event triggered when MMB is down on this object
    /// </summary>
    public event Action? OnMiddlePress;

    /// <summary>
    /// Event triggered when RMB is down on this object
    /// </summary>
    public event Action? OnRightPress;

    /// <summary>
    /// Event triggered when mouse button is clicked the second time in quick succession on this object
    /// </summary>
    public event Action? OnDoubleClick;

    /// <summary>
    /// Max delay between two clicks to be considered a double click
    /// </summary>
    public float DoubleClickInterval = 0.4f;

    private float _lastClickTime = -1f;

    /// <summary>
    /// Handles mouse button down event
    /// </summary>
    /// <param name="eventData">The mouse event data</param>
    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Middle) OnMiddlePress?.Invoke();
        if (eventData.button == PointerEventData.InputButton.Right) OnRightPress?.Invoke();
        if (eventData.button != PointerEventData.InputButton.Left) return;
        OnPress?.Invoke();

        float currentTime = Time.unscaledTime;
        if (currentTime - _lastClickTime <= DoubleClickInterval) {
            OnDoubleClick?.Invoke();
            _lastClickTime = -1f;
        } else {
            _lastClickTime = currentTime;
        }
    }
}
