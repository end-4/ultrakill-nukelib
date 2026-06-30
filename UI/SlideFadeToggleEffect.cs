using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NukeLib.UI;

/// <summary>
/// Component for slide+fade effect when toggling a GameObject's visibility.
/// </summary>
public class SlideFadeToggleEffect : MonoBehaviour {
    private enum State {
        Idle,
        Entering,
        Exiting
    }

    private State currentState = State.Idle;

    private RectTransform rect;
    private CanvasGroup? canvasGroup;

    private float originalAlpha;
    private Vector2 originalPos;

    /// <summary>
    /// The alpha value when completely hidden
    /// </summary>
    public float hiddenAlpha = 0;

    /// <summary>
    /// The speed of the animation
    /// </summary>
    public float speed = 18f;

    /// <summary>
    /// The offset when the object is hidden
    /// </summary>
    public Vector2 hiddenOffset = Vector2.zero;

    /// <summary>
    /// Emitted when the exiting animation is finished. You can hide/destroy the element after this.
    /// </summary>
    public event Action OnExitComplete;

    private Vector2 hiddenPos => originalPos + hiddenOffset;

    private void Set2DLocalPosition(Vector2 newPos) {
        rect.localPosition = new Vector3(newPos.x, newPos.y, rect.localPosition.z);
    }

    private void Initialize() {
        if ((Object)(object)rect == (Object)null) rect = ((Component)this).GetComponent<RectTransform>();
        if ((Object?)(object?)canvasGroup == (Object?)null) canvasGroup = ((Component)this).GetComponent<CanvasGroup>();

        originalAlpha = canvasGroup?.alpha ?? 1;
        originalPos = rect.localPosition;
    }

    private void OnEnable() {
        Set2DLocalPosition(hiddenPos);
        if (canvasGroup != null) canvasGroup.alpha = hiddenAlpha;
        currentState = State.Entering;
    }

    /// <summary>
    /// Starts exiting animation
    /// </summary>
    public void StartExit() {
        currentState = State.Exiting;
    }

    private void Awake() {
        Initialize();
    }

    private float currentExitSpeed = 0f;

    private void Update() {
        if (currentState == State.Idle) return;

        bool entering = currentState == State.Entering;
        Vector2 targetPos = entering ? originalPos : hiddenPos;
        float targetAlpha = entering ? originalAlpha : hiddenAlpha;

        Vector2 currentPos = rect.localPosition;
        float currentAlpha = canvasGroup?.alpha ?? 1;

        float moveStep;
        float alphaStep;

        if (entering) {
            // Decelerating
            float speedMultiplier = Time.unscaledDeltaTime * speed;
            moveStep = (Vector2.Distance(currentPos, targetPos) + 0.1f) * speedMultiplier;
            alphaStep = (Mathf.Abs(targetAlpha - currentAlpha) + 0.1f) * speedMultiplier;
            currentExitSpeed = 0f; // Reset exit speed
        } else {
            // Accelerating
            currentExitSpeed += speed * 7f * Time.unscaledDeltaTime;
            moveStep = currentExitSpeed;
            alphaStep = currentExitSpeed * 0.01f;
        }

        // Apply movement
        Set2DLocalPosition(Vector2.MoveTowards(currentPos, targetPos, moveStep));
        if (canvasGroup != null) {
            canvasGroup.alpha = Mathf.MoveTowards(currentAlpha, targetAlpha, alphaStep);
        }

        // Check Completion
        if ((Vector2)rect.localPosition == targetPos &&
            (canvasGroup == null || Mathf.Approximately(canvasGroup.alpha, targetAlpha))) {
            State finishedState = currentState;
            currentState = State.Idle;
            currentExitSpeed = 0f;
            if (finishedState == State.Exiting) OnExitComplete?.Invoke();
        }
    }
}

/// <summary>
/// Extensions to work with SlideFadeToggleEffect more easily
/// </summary>
public static class SlideFadeToggleEffectExtensions {
    /// <summary>
    /// Sets object's activeness and animate if available
    /// </summary>
    /// <param name="gameObject">The target game object</param>
    /// <param name="value">Whether to show or hide</param>
    /// <param name="hiddenOffset">How much to shift when the GameObject is hidden</param>
    /// <param name="speed">The speed of the hiding/showing animation</param>
    public static void SetActiveAnimated(this GameObject gameObject, bool value, Vector2 hiddenOffset, float speed = 25f) {
        SlideFadeToggleEffect toggleEffect = gameObject.GetComponent<SlideFadeToggleEffect>();
        if (toggleEffect == null) {
            toggleEffect = gameObject.AddComponent<SlideFadeToggleEffect>();
            toggleEffect.hiddenOffset = hiddenOffset;
            toggleEffect.speed = speed;
            toggleEffect.OnExitComplete += () => { gameObject.SetActive(false); };
        }

        if (!gameObject.activeSelf) {
            gameObject.SetActive(true);
        } else if (toggleEffect != null) {
            toggleEffect.StartExit();
        } else {
            gameObject.SetActive(false);
        }
    }
}
