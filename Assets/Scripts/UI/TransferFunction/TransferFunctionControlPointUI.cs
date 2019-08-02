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

    private void Awake() {
        m_RectTransform = GetComponent<RectTransform>();
        m_Image = GetComponent<Image>();
        m_NormalColor = m_Image.color;
    }

    public void Init(TransferFunctionUI _transferFunctionUI, Color _color, bool _selected) {
        m_TransferFunctionUI = _transferFunctionUI;
        Color = _color;
        if (_selected) {
            m_Selected = true;
            m_Image.color = m_SelectionColor;
        }
    }

    public void Deselect() {
        m_Selected = false;
        m_Image.color = DetermineFallbackColor(Input.mousePosition);
    }

    public void OnPointerEnter(PointerEventData _eventData) {
        if (!m_Selected) {
            m_Image.color = m_HighlightColor;
        }
    }

    public void OnPointerExit(PointerEventData _eventData) {
        if (!m_IsDragging && !m_Selected) {
            m_Image.color = m_NormalColor;
        }
    }

    // Works
    public void OnPointerDown(PointerEventData _eventData) {
        if (_eventData.button == PointerEventData.InputButton.Left) {
            // We do not want to select the same point twice
            if (!m_Selected) {
                m_Selected = true;
                m_Image.color = m_SelectionColor;
                m_RectTransform.SetAsLastSibling();

                m_TransferFunctionUI.SelectPoint(this);
            }
        } else if (_eventData.button == PointerEventData.InputButton.Middle) {
            m_TransferFunctionUI.DeletePoint(this);
        }
    }

    public void OnBeginDrag(PointerEventData _eventData) {
        m_IsDragging = true;
        m_RectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData _eventData) {
        m_RectTransform.anchoredPosition = m_TransferFunctionUI.LimitPositionToPointInBox(_eventData.position);
        m_TransferFunctionUI.Redraw();
    }

    public void OnEndDrag(PointerEventData eventData) {
        m_IsDragging = false;
    }

    private Color DetermineFallbackColor(Vector3 _position) {
        if (m_Selected) {
            return m_SelectionColor;
        } else if (RectTransformUtility.RectangleContainsScreenPoint(m_RectTransform, _position)) {
            return m_HighlightColor;
        } else {
            return m_NormalColor;
        }
    }
}
