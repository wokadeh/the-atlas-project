using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.ColorPicker;

public class TransferFunctionUI : MonoBehaviour, IPointerClickHandler {
    [SerializeField] private Color m_ControlPointStartColor;
    [SerializeField] private TransferFunctionControlPointUI m_ControlPointPrefab;
    [SerializeField] private ColorPickerControl m_ColorPicker;
    [SerializeField] private UILineRenderer m_LineRenderer;

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

        m_LineRenderer.gameObject.SetActive(true);

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

        RedrawLines();
    }

    public void RedrawLines() {
        m_ControlPoints = m_ControlPoints.OrderBy(p => p.transform.localPosition.x).ToList();

        // We add two extra points for the edges
        int pointCount = m_ControlPoints.Count + 2;
        Vector2[] points = new Vector2[pointCount];
        for (int i = 0; i < m_ControlPoints.Count; i++) {
            points[i + 1] = m_ControlPoints[i].transform.localPosition;
        }

        // First and last point get derived from second and second last point respectively.
        // We have to take into account that the bounds include the size of a control point
        // which we do not want
        Vector2 second = points[1];
        second.x = m_BoxBounds.min.x - (m_ControlPointSize.x / 2f);
        points[0] = second;

        Vector2 secondLast = points[pointCount - 2];
        secondLast.x = m_BoxBounds.max.x + (m_ControlPointSize.x / 2f);
        points[pointCount - 1] = secondLast;

        m_LineRenderer.Points = points;
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
            CreatePoint(LimitPositionToPointInBox(eventData.position), m_ControlPointStartColor, true);
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
        float x = xStep / 2f;
        for (int i = 0; i < NUMBER_OF_POINTS; i++) {
            float y = Random.Range(0, bounds.size.y - m_ControlPointSize.y);

            float xPos = x - bounds.extents.x + m_ControlPointSize.x / 2f;
            float yPos = y - bounds.extents.y + m_ControlPointSize.y / 2f;

            CreatePoint(new Vector2(xPos, yPos), Random.ColorHSV(), false);

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

    private void CreatePoint(Vector2 pointInBox, Color color, bool select) {
        TransferFunctionControlPointUI point = Instantiate(m_ControlPointPrefab, transform);
        point.GetComponent<RectTransform>().anchoredPosition = pointInBox;
        point.name = $"Transferfunction_Control_Point";
        point.Init(this, color, select);

        m_ControlPoints.Add(point);

        RedrawLines();

        if (select) {
            SelectPoint(point);
        }
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
