using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NukeLib.UI;

/// <summary>
/// A component that allows resizing a RectTransform by dragging its edges or corners.
/// It also changes the cursor to indicate the resize direction when hovering.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ResizeController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
    IDragHandler, IPointerUpHandler {
    /// <summary>
    /// Thickness of the invisible edge area that can be grabbed
    /// </summary>
    [Header("Settings")] [Tooltip("The thickness of the invisible edge area that can be grabbed.")]
    public float grabAreaThickness = 10f;

    /// <summary>
    /// Min width of item
    /// </summary>
    [Tooltip("Minimum width the UI element can be resized to.")]
    public float minWidth = 50f;

    /// <summary>
    /// Min height of item
    /// </summary>
    [Tooltip("Minimum height the UI element can be resized to.")]
    public float minHeight = 50f;

    private RectTransform? _rectTransform;
    private bool _isPointerOver = false;
    private bool _isDragging = false;

    private enum ResizeDirection {
        None,
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    private ResizeDirection _currentHoverDirection = ResizeDirection.None;
    private ResizeDirection _activeDragDirection = ResizeDirection.None;

    // For draggin
    private Vector2 _originalSize;
    private Vector2 _originalLocalPosition;
    private Vector2 _dragStartPointerPosition;

    private void Start() {
        _rectTransform = GetComponent<RectTransform>();
    }

    private static readonly string CursorBundlePath =
        Path.Combine(Plugin.workingDir, "assets", "nukelib_resizecursors.bundle");

    private static readonly string[] IconNames = [
        "move_horizontal_cursor",
        "move_vertical_cursor",
        "move_maindiagonal_cursor",
        "move_subdiagonal_cursor"
    ];

    private static Dictionary<string, Texture2D> Cursors = [];
    private static bool LoadedCursors = false;

    private static void LoadCursors() {
        AssetBundle bundle = AssetBundle.LoadFromFile(CursorBundlePath);
        for (int i = 0; i < IconNames.Length; i++) {
            var iconName = IconNames[i];
            var iconSprite = bundle.LoadAsset<Texture2D>(iconName);
            Cursors.Add(iconName, iconSprite);
        }

        bundle.Unload(false);
    }

    private Texture2D GetTexture(string name) {
        if (!LoadedCursors) {
            LoadedCursors = true;
            LoadCursors();
        }

        return Cursors[name];
    }

    private void Update() {
        if (_isPointerOver && !_isDragging) {
            UpdateHoverState();
            UpdateCursor();
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        _isPointerOver = false;
        if (!_isDragging) {
            ResetCursor();
            _currentHoverDirection = ResizeDirection.None;
        }
    }

    private void UpdateHoverState() {
        if (_rectTransform == null) return;
        Vector2 localMousePos;
        // Convert screen mouse position to local coordinates within the RectTransform
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, Input.mousePosition, null,
            out localMousePos);

        var rect = _rectTransform.rect;

        // Outer
        bool onTop = localMousePos.y >= rect.yMax && localMousePos.y <= rect.yMax + grabAreaThickness;
        bool onBottom = localMousePos.y <= rect.yMin && localMousePos.y >= rect.yMin - grabAreaThickness;
        bool onLeft = localMousePos.x <= rect.xMin && localMousePos.x >= rect.xMin - grabAreaThickness;
        bool onRight = localMousePos.x >= rect.xMax && localMousePos.x <= rect.xMax + grabAreaThickness;
        bool nearTop = localMousePos.y >= rect.yMax - rect.height / 3f;
        bool nearBottom = localMousePos.y <= rect.yMin + rect.height / 3f;
        bool nearLeft = localMousePos.x <= rect.xMin + rect.width / 3f;
        bool nearRight = localMousePos.x >= rect.xMax - rect.width / 3f;

        // We consider the mouse to be at the corner when it's on the first 1/3 of the edge.
        // Having such a huge corner area might seem weird to non-tiling window manager users,
        // but this gives much more control
        if (onTop && onLeft) _currentHoverDirection = ResizeDirection.TopLeft;
        else if (onTop && onRight) _currentHoverDirection = ResizeDirection.TopRight;
        else if (onBottom && onLeft) _currentHoverDirection = ResizeDirection.BottomLeft;
        else if (onBottom && onRight) _currentHoverDirection = ResizeDirection.BottomRight;
        else if (onTop && nearLeft) _currentHoverDirection = ResizeDirection.TopLeft;
        else if (onLeft && nearTop) _currentHoverDirection = ResizeDirection.TopLeft;
        else if (onTop && nearRight) _currentHoverDirection = ResizeDirection.TopRight;
        else if (onRight && nearTop) _currentHoverDirection = ResizeDirection.TopRight;
        else if (onBottom && nearLeft) _currentHoverDirection = ResizeDirection.BottomLeft;
        else if (onLeft && nearBottom) _currentHoverDirection = ResizeDirection.BottomLeft;
        else if (onBottom && nearRight) _currentHoverDirection = ResizeDirection.BottomRight;
        else if (onRight && nearBottom) _currentHoverDirection = ResizeDirection.BottomRight;
        else if (onTop) _currentHoverDirection = ResizeDirection.Top;
        else if (onBottom) _currentHoverDirection = ResizeDirection.Bottom;
        else if (onLeft) _currentHoverDirection = ResizeDirection.Left;
        else if (onRight) _currentHoverDirection = ResizeDirection.Right;
        else _currentHoverDirection = ResizeDirection.None;
    }

    private void UpdateCursor() {
        Texture2D targetCursor = null!;

        switch (_currentHoverDirection) {
            case ResizeDirection.Left:
            case ResizeDirection.Right:
                targetCursor = GetTexture("move_horizontal_cursor");
                break;
            case ResizeDirection.Top:
            case ResizeDirection.Bottom:
                targetCursor = GetTexture("move_vertical_cursor");
                break;
            case ResizeDirection.TopLeft:
            case ResizeDirection.BottomRight:
                targetCursor = GetTexture("move_maindiagonal_cursor");
                break;
            case ResizeDirection.TopRight:
            case ResizeDirection.BottomLeft:
                targetCursor = GetTexture("move_subdiagonal_cursor");
                break;
            case ResizeDirection.None:
                ResetCursor();
                return;
        }

        if (targetCursor == null) return;

        var hotspot = new Vector2(targetCursor.width / 2f, targetCursor.height / 2f);
        Cursor.SetCursor(targetCursor, hotspot, CursorMode.Auto);
    }

    private void ResetCursor() {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (_rectTransform == null) return;
        if (_currentHoverDirection != ResizeDirection.None) {
            _isDragging = true;
            _activeDragDirection = _currentHoverDirection;

            // Remember state before drag
            _originalSize = _rectTransform.rect.size;
            _originalLocalPosition = _rectTransform.localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_rectTransform.parent,
                eventData.position,
                eventData.pressEventCamera,
                out _dragStartPointerPosition);
        }
    }

    public void OnDrag(PointerEventData eventData) {
        if (!_isDragging || _activeDragDirection == ResizeDirection.None)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_rectTransform.parent,
                eventData.position,
                eventData.pressEventCamera,
                out var currentPointerPosition)) {
            return;
        }

        var pointerDelta = currentPointerPosition - _dragStartPointerPosition;
        Vector2 newSize = _originalSize;
        Vector2 positionOffset = Vector2.zero;
        // Horizontal
        if (_activeDragDirection == ResizeDirection.Right ||
            _activeDragDirection == ResizeDirection.TopRight ||
            _activeDragDirection == ResizeDirection.BottomRight) {
            newSize.x = Mathf.Max(minWidth, _originalSize.x + pointerDelta.x);
            // Move to keep left edge from moving
            positionOffset.x = (newSize.x - _originalSize.x) * _rectTransform.pivot.x;
        } else if (_activeDragDirection == ResizeDirection.Left ||
                   _activeDragDirection == ResizeDirection.TopLeft ||
                   _activeDragDirection == ResizeDirection.BottomLeft) {
            newSize.x = Mathf.Max(minWidth, _originalSize.x - pointerDelta.x);
            // Move to keep right edge from moving
            positionOffset.x = -(newSize.x - _originalSize.x) * (1f - _rectTransform.pivot.x);
        }

        // Vertical
        if (_activeDragDirection == ResizeDirection.Top ||
            _activeDragDirection == ResizeDirection.TopLeft ||
            _activeDragDirection == ResizeDirection.TopRight) {
            newSize.y = Mathf.Max(minHeight, _originalSize.y + pointerDelta.y);
            // Prevent bottom edge from moving
            positionOffset.y = (newSize.y - _originalSize.y) * _rectTransform.pivot.y;
        } else if (_activeDragDirection == ResizeDirection.Bottom ||
                   _activeDragDirection == ResizeDirection.BottomLeft ||
                   _activeDragDirection == ResizeDirection.BottomRight) {
            newSize.y = Mathf.Max(minHeight, _originalSize.y - pointerDelta.y);
            // Prevent top edge from moving
            positionOffset.y = -(newSize.y - _originalSize.y) * (1f - _rectTransform.pivot.y);
        }

        // Apply
        _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
        _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);
        _rectTransform.localPosition =
            (Vector3)_originalLocalPosition + new Vector3(positionOffset.x, positionOffset.y, 0f);
    }

    public void OnPointerUp(PointerEventData eventData) {
        _isDragging = false;
        _activeDragDirection = ResizeDirection.None;

        UpdateHoverState();
        if (_currentHoverDirection == ResizeDirection.None && !_isPointerOver) {
            ResetCursor();
        }
    }

    private void OnDisable() {
        ResetCursor();
        _isDragging = false;
        _isPointerOver = false;
        _currentHoverDirection = ResizeDirection.None;
    }
}
