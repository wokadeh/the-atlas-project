using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class TransferFunctionControlPointUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [SerializeField] private Color m_HighlightColor;
    [SerializeField] private Color m_SelectionColor;

    public Color Color { get; set; }

    private TransferFunctionUI m_TransferFunctionUI;
    private RectTransform m_RectTransform;
    private Image m_Image;
    private Color m_NormalColor;
    private bool m_IsDragging;
    private bool m_Selected;

    private void Start() {
        m_RectTransform = GetComponent<RectTransform>();
        m_Image = GetComponent<Image>();
        m_NormalColor = m_Image.color;
    }

    public void Init(TransferFunctionUI transferFunctionUI, Color color) {
        m_TransferFunctionUI = transferFunctionUI;
        Color = color;
    }

    public void Deselect() {
        m_Selected = false;
        m_Image.color = DetermineFallbackColor(Input.mousePosition);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!m_Selected) {
            m_Image.color = m_HighlightColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!m_IsDragging && !m_Selected) {
            m_Image.color = m_NormalColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            m_Selected = true;
            m_Image.color = m_SelectionColor;
            m_RectTransform.SetAsLastSibling();

            m_TransferFunctionUI.SelectPoint(this);
        } else if (eventData.button == PointerEventData.InputButton.Right) {
            m_TransferFunctionUI.DeletePoint(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        m_IsDragging = true;
        m_RectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData) {
        m_RectTransform.anchoredPosition = m_TransferFunctionUI.LimitPositionToPointInBox(eventData.position);
        m_TransferFunctionUI.RedrawLines();
    }

    public void OnEndDrag(PointerEventData eventData) {
        m_IsDragging = false;
    }

    private Color DetermineFallbackColor(Vector3 position) {
        if (m_Selected) {
            return m_SelectionColor;
        } else if (RectTransformUtility.RectangleContainsScreenPoint(m_RectTransform, position)) {
            return m_HighlightColor;
        } else {
            return m_NormalColor;
        }
    }
}
