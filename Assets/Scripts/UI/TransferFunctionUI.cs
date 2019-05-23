using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TransferFunctionUI : MonoBehaviour, IPointerClickHandler {
    [SerializeField] private TransferFunctionControlPointUI m_ControlPointPrefab;

    private List<TransferFunctionControlPointUI> m_ControlPoints;
    private RectTransform m_RectTransform;
    private TransferFunctionControlPointUI m_SelectedPoint;

    private void Start() {
        m_ControlPoints = new List<TransferFunctionControlPointUI>();
        m_RectTransform = GetComponent<RectTransform>();

        GenerateRandomControlPoints();
    }

    public void SelectPoint(TransferFunctionControlPointUI point) {
        m_SelectedPoint = point;
    }

    public void DeletePoint(TransferFunctionControlPointUI point) {
        Destroy(point.gameObject);
        m_ControlPoints.Remove(point);
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
        const int CONTROL_POINT_SIZE = 16;

        float xStep = (bounds.size.x - CONTROL_POINT_SIZE) / NUMBER_OF_POINTS;
        float x = 0;
        for (int i = 0; i < NUMBER_OF_POINTS; i++) {
            float y = Random.Range(0, bounds.size.y - CONTROL_POINT_SIZE);

            CreatePoint(new Vector2(x - bounds.extents.x, y - bounds.extents.y));

            x += xStep;
        }
    }

    private void CreatePoint(Vector2 position) {
        TransferFunctionControlPointUI point = Instantiate(m_ControlPointPrefab, transform);
        point.GetComponent<RectTransform>().anchoredPosition = position;
        point.name = $"Transferfunction_Control_Point";

        point.Init(this);

        m_ControlPoints.Add(point);
    }

    private void ClearPoints() {
        foreach (var point in m_ControlPoints) {
            Destroy(point.gameObject);
        }
        m_ControlPoints.Clear();
    }
}
