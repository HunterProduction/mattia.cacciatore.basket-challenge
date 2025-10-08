using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class RelativePositionFitter : MonoBehaviour
{
    [Tooltip("If null, the immediate parent RectTransform is used.")]
    [SerializeField] private RectTransform parentRect;

    [Header("Relative Position")]
    [Tooltip("Normalized position of this element inside its parent (X = horizontal, Y = vertical).")]
    [SerializeField] private Vector2 percent = new Vector2(0.5f, 0.5f);

    [Header("Layout")]
    [Tooltip("Optional padding inside the parent rect (pixels).")]
    [SerializeField] private Vector2 padding = Vector2.zero;

    private RectTransform _rectTransform;

    public Vector2 Percent
    {
        get => percent;
        set
        {
            percent = new Vector2(
                Mathf.Clamp01(value.x),
                Mathf.Clamp01(value.y)
            );
            UpdatePosition();
        }
    }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (!parentRect)
            parentRect = transform.parent as RectTransform;
    }

    private void Start() => UpdatePosition();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!_rectTransform) _rectTransform = GetComponent<RectTransform>();
        if (!parentRect) parentRect = transform.parent as RectTransform;
        percent.x = Mathf.Clamp01(percent.x);
        percent.y = Mathf.Clamp01(percent.y);
        UpdatePosition();
    }
#endif

    /// <summary>
    /// Updates the element's world position based on its normalized Percent inside the parent.
    /// Keeps the element fully inside the parent bounds, respecting pivot and padding.
    /// </summary>
    public void UpdatePosition()
    {
        if (!_rectTransform || !parentRect)
            return;

        Rect parentBounds = parentRect.rect;
        Rect selfRect = _rectTransform.rect;

        // Apply padding
        parentBounds.xMin += padding.x;
        parentBounds.xMax -= padding.x;
        parentBounds.yMin += padding.y;
        parentBounds.yMax -= padding.y;

        // Compute allowed pivot positions
        float left = parentBounds.xMin + _rectTransform.pivot.x * selfRect.width;
        float right = parentBounds.xMax - (1f - _rectTransform.pivot.x) * selfRect.width;
        float bottom = parentBounds.yMin + _rectTransform.pivot.y * selfRect.height;
        float top = parentBounds.yMax - (1f - _rectTransform.pivot.y) * selfRect.height;

        // Interpolate position in parent's local space
        float localX = Mathf.Lerp(left, right, percent.x);
        float localY = Mathf.Lerp(bottom, top, percent.y);

        Vector3 worldPos = parentRect.TransformPoint(new Vector3(localX, localY, 0f));
        _rectTransform.position = worldPos;
    }
}
