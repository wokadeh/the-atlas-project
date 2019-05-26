using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TransferFunctionUI : MonoBehaviour, IPointerClickHandler {
    private const int CONTROL_POINT_SIZE = 16;

    [SerializeField] private TransferFunctionControlPointUI m_ControlPointPrefab;
    [SerializeField] private ColorPicker m_ColorPicker;

    private RectTransform m_RectTransform;
    private Bounds m_BoxBounds;
    private List<TransferFunctionControlPointUI> m_ControlPoints;
    private TransferFunctionControlPointUI m_SelectedPoint;

    private void Start() {
        m_ControlPoints = new List<TransferFunctionControlPointUI>();
        m_RectTransform = GetComponent<RectTransform>();

        m_BoxBounds = CalculateBoxBounds();

        m_ColorPicker.onValueChanged.AddListener(OnColorPickerChanged);

        GenerateRandomControlPoints();
    }

    public void SelectPoint(TransferFunctionControlPointUI point) {
        m_SelectedPoint = point;
        m_ColorPicker.CurrentColor = m_SelectedPoint.Color;
    }

    public void DeletePoint(TransferFunctionControlPointUI point) {
        Destroy(point.gameObject);
        m_ControlPoints.Remove(point);
    }

    public Vector2 GetPointInBox(Vector2 position) {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectTransform, position, null, out Vector2 localPoint);

        Vector2 point = localPoint;

        if (localPoint.x < m_BoxBounds.min.x) {
            point.x = m_BoxBounds.min.x;
        } else if (localPoint.x > m_BoxBounds.max.x) {
            point.x = m_BoxBounds.max.x;
        }

        if (localPoint.y < m_BoxBounds.min.y) {
            point.y = m_BoxBounds.min.y;
        } else if (localPoint.y > m_BoxBounds.max.y) {
            point.y = m_BoxBounds.max.y;
        }

        return point;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Left) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectTransform, eventData.position, null, out Vector2 localPoint);
            CreatePoint(localPoint);
        }
    }

    private void GenerateRandomControlPoints() {
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(m_RectTransform);

        // HACK: Hardcoded control point size
        const int NUMBER_OF_POINTS = 10;

        float xStep = (bounds.size.x - CONTROL_POINT_SIZE) / NUMBER_OF_POINTS;
        float x = 0;
        for (int i = 0; i < NUMBER_OF_POINTS; i++) {
            float y = Random.Range(0, bounds.size.y - CONTROL_POINT_SIZE) * 0;

            float xPos = x - bounds.extents.x + CONTROL_POINT_SIZE / 2f;
            float yPos = y - bounds.extents.y + CONTROL_POINT_SIZE / 2f;

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

    private void CreatePoint(Vector2 position) {
        TransferFunctionControlPointUI point = Instantiate(m_ControlPointPrefab, transform);
        point.GetComponent<RectTransform>().anchoredPosition = position;
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
    
    private Bounds CalculateBoxBounds() {
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(m_RectTransform);

        float halfControlPointSize = CONTROL_POINT_SIZE / 2.0f;

        Vector3 min = bounds.min;
        min.x += halfControlPointSize;
        min.y += halfControlPointSize;

        Vector3 max = bounds.max;
        max.x -= halfControlPointSize;
        max.y -= halfControlPointSize;

        bounds.SetMinMax(min, max);

        return bounds;
    }
}
