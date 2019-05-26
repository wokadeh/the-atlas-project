using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class TransferFunctionControlPointUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [SerializeField] private Color m_HighlightColor;

    public Color Color { get; set; }

    private TransferFunctionUI m_TransferFunctionUI;
    private RectTransform m_RectTransform;
    private Image m_Image;
    private Color m_NormalColor;
    private bool m_IsDragging;

    private void Start() {
        m_RectTransform = GetComponent<RectTransform>();
        m_Image = GetComponent<Image>();
        m_NormalColor = m_Image.color;
    }

    public void Init(TransferFunctionUI transferFunctionUI, Color color) {
        m_TransferFunctionUI = transferFunctionUI;
        Color = color;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        m_Image.color = m_HighlightColor;
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!m_IsDragging) {
            m_Image.color = m_NormalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            m_TransferFunctionUI.SelectPoint(this);
        } else if (eventData.button == PointerEventData.InputButton.Right) {
            m_TransferFunctionUI.DeletePoint(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        m_IsDragging = true;
        m_Image.color = m_HighlightColor;
    }

    public void OnDrag(PointerEventData eventData) {
        m_RectTransform.anchoredPosition = m_TransferFunctionUI.GetPointInBox(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData) {
        m_IsDragging = false;

        if (!RectTransformUtility.RectangleContainsScreenPoint(m_RectTransform, eventData.position)) {
            m_Image.color = m_NormalColor;
        }
    }
}
