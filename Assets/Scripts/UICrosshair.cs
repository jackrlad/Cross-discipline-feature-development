using UnityEngine;
using UnityEngine.UI;

public class UICrosshair : MonoBehaviour
{
    private RectTransform _rectTransform;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        // Crosshair follows mouse position directly in screen space
        _rectTransform.position = Input.mousePosition;
    }
}