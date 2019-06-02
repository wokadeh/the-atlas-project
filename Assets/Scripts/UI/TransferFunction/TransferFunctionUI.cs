using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions.ColorPicker;

public class TransferFunctionUI : MonoBehaviour, IPointerClickHandler {
    [SerializeField] private TransferFunctionControlPointUI m_ControlPointPrefab;
    [SerializeField] private ColorPickerControl m_ColorPicker;

    private RectTransform m_RectTransform;
    private Bounds m_BoxBounds;
    private Vector2 m_ControlPointSize;

    private List<TransferFunctionControlPointUI> m_ControlPoints;
    private TransferFunctionControlPointUI m_SelectedPoint;

    private void Start() {
        m_ControlPoints = new List<TransferFunctionControlPointUI>();
        m_RectTransform = GetComponent<RectTransform>();
        m_ControlPointSize = m_ControlPointPrefab.GetComponent<RectTransform>().sizeDelta;
        m_BoxBounds = CalculateBoxBounds(m_ControlPointSize);

        m_ColorPicker.onValueChanged.AddListener(OnColorPickerChanged);
        m_ColorPicker.gameObject.SetActive(false);

        GenerateRandomControlPoints();
    }

    public void SelectPoint(TransferFunctionControlPointUI point) {
        DeselectPoint();

        m_SelectedPoint = point;

        m_ColorPicker.gameObject.SetActive(true);
        m_ColorPicker.CurrentColor = m_SelectedPoint.Color;
    }

    public void DeletePoint(TransferFunctionControlPointUI point) {
        // If we are deleting the currently selected point, deselect it first
        if (m_SelectedPoint == point) {
            DeselectPoint();
        }

        Destroy(point.gameObject);
        m_ControlPoints.Remove(point);
    }

    public Vector2 LimitPositionToPointInBox(Vector2 position) {
        Vector2 point = ConvertToLocalPointInBox(position);
        Vector3 min = m_BoxBounds.min;
        Vector3 max = m_BoxBounds.max;

        if (point.x < min.x) {
            point.x = min.x;
        } else if (point.x > max.x) {
            point.x = max.x;
        }

        if (point.y < min.y) {
            point.y = min.y;
        } else if (point.y > max.y) {
            point.y = max.y;
        }

        return point;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            CreatePoint(LimitPositionToPointInBox(eventData.position));
        } else {
            DeselectPoint();
        }
    }

    private void DeselectPoint() {
        if (m_SelectedPoint) {
            m_SelectedPoint.Deselect();

            m_SelectedPoint = null;

            m_ColorPicker.gameObject.SetActive(false);
        }
    }

    private void GenerateRandomControlPoints() {
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(m_RectTransform);

        const int NUMBER_OF_POINTS = 10;

        float xStep = (bounds.size.x - m_ControlPointSize.x) / NUMBER_OF_POINTS;
        float x = 0;
        for (int i = 0; i < NUMBER_OF_POINTS; i++) {
            float y = Random.Range(0, bounds.size.y - m_ControlPointSize.y);

            float xPos = x - bounds.extents.x + m_ControlPointSize.x / 2f;
            float yPos = y - bounds.extents.y + m_ControlPointSize.y / 2f;

            CreatePoint(new Vector2(xPos, yPos));

            x += xStep;
        }
    }

    private void OnColorPickerChanged(Color color) {
        // Bail out if no point is selected
        if (!m_SelectedPoint) {
            return;
        }

        m_SelectedPoint.Color = color;
    }

    private void CreatePoint(Vector2 pointInBox) {
        TransferFunctionControlPointUI point = Instantiate(m_ControlPointPrefab, transform);
        point.GetComponent<RectTransform>().anchoredPosition = pointInBox;
        point.name = $"Transferfunction_Control_Point";

        point.Init(this, Color.white);

        m_ControlPoints.Add(point);
    }

    private void ClearPoints() {
        // Destroy all points and clear out the list
        foreach (var point in m_ControlPoints) {
            Destroy(point.gameObject);
        }
        m_ControlPoints.Clear();
    }
    
    private Bounds CalculateBoxBounds(Vector2 controlPointSize) {
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(m_RectTransform);

        float halfControlPointSizeX = controlPointSize.x / 2.0f;
        float halfControlPointSizeY = controlPointSize.y / 2.0f;

        Vector3 min = bounds.min;
        min.x += halfControlPointSizeX;
        min.y += halfControlPointSizeY;

        Vector3 max = bounds.max;
        max.x -= halfControlPointSizeX;
        max.y -= halfControlPointSizeY;

        bounds.SetMinMax(min, max);

        return bounds;
    }

    private Vector2 ConvertToLocalPointInBox(Vector3 position) {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectTransform, position, null, out Vector2 localPoint);
        return localPoint;
    }
}
