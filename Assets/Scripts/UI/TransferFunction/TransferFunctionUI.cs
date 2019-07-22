using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.ColorPicker;

public class TransferFunctionUI : MonoBehaviour, IPointerClickHandler {
    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private VolumeRenderer m_VolumeRenderer;
    [SerializeField] private Color m_ControlPointStartColor;
    [SerializeField] private TransferFunctionControlPointUI m_ControlPointPrefab;
    [SerializeField] private ColorPickerControl m_ColorPicker;
    [SerializeField] private UILineRenderer m_LineRenderer;
    [SerializeField] private RawImage m_HistogramTexture;

    private RectTransform m_RectTransform;
    private Bounds m_BoxBounds;
    private Vector2 m_ControlPointSize;
    private bool m_Initialized;

    private List<TransferFunctionControlPointUI> m_ControlPoints;
    private TransferFunctionControlPointUI m_SelectedPoint;

    private void Start() {
        m_DataManager.OnDataAssetChanged += asset => RedrawHistogram();
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

        Redraw();
    }

    public void Redraw() {
        if (!m_Initialized) {
            Initialize();
        }

        // We first need to correctly order the lines
        var s = m_ControlPoints.OrderBy(p => p.transform.localPosition.x);
        m_ControlPoints = s.ToList();

        RedrawLines();
        RedrawHistogram();
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
        if (m_DataManager.m_CurrentAsset != null) {
            if (eventData.button == PointerEventData.InputButton.Right) {
                CreatePoint(LimitPositionToPointInBox(eventData.position), m_ControlPointStartColor, true);
            } else {
                DeselectPoint();
            }
        }
    }

    private void Initialize() {
        m_Initialized = true;

        m_ControlPoints = new List<TransferFunctionControlPointUI>();
        m_RectTransform = GetComponent<RectTransform>();
        m_ControlPointSize = m_ControlPointPrefab.GetComponent<RectTransform>().sizeDelta;
        m_BoxBounds = CalculateBoxBounds(m_ControlPointSize);

        m_ColorPicker.onValueChanged.AddListener(OnColorPickerChanged);
        m_ColorPicker.gameObject.SetActive(false);

        m_LineRenderer.gameObject.SetActive(true);

        GenerateStartingPoints();
    }

    private void DeselectPoint() {
        if (m_SelectedPoint) {
            m_SelectedPoint.Deselect();

            m_SelectedPoint = null;

            m_ColorPicker.gameObject.SetActive(false);
        }
    }

    private void RedrawLines() {
        // Because of line renderer weirdness we need to disable it when we do not want to draw anything
        m_LineRenderer.gameObject.SetActive(m_ControlPoints.Count != 0);

        if (m_ControlPoints.Count == 0) {
            // If we have no control points, we do not want to draw anything
            m_LineRenderer.Points = new Vector2[0];
        } else {
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
    }

    private void RedrawHistogram() {
        if (m_DataManager.m_CurrentAsset != null) {
            var tex = GenerateTransferFunction().GetTexture();

            m_HistogramTexture.material.SetTexture("_HistTex", m_DataManager.m_CurrentAsset.HistogramTexture);
            m_HistogramTexture.material.SetTexture("_TFTex", tex);

            m_VolumeRenderer.SetTransferFunction(tex);
        }
    }

    private TransferFunction GenerateTransferFunction() {
        TransferFunction function = new TransferFunction();

        if (m_ControlPoints.Count > 0) {
            Vector2 size = m_RectTransform.sizeDelta;
            for (int i = 0; i < m_ControlPoints.Count; i++) {
                TransferFunctionControlPointUI controlPoint = m_ControlPoints[i];
                Transform transform = controlPoint.transform;

                // We need to normalize the x and y position
                float data = (transform.localPosition.x + size.x / 2) / size.x;
                float alpha = (transform.localPosition.y + size.y / 2) / size.y;

                function.AddControlPoint(new TFColourControlPoint(data, controlPoint.Color));
                function.AddControlPoint(new TFAlphaControlPoint(data, alpha));

                // We create additional edge points for the first and last control point
                if (i == 0) {
                    function.AddControlPoint(new TFColourControlPoint(0, controlPoint.Color));
                    function.AddControlPoint(new TFAlphaControlPoint(0, alpha));
                }
                if (i == m_ControlPoints.Count - 1) {
                    function.AddControlPoint(new TFColourControlPoint(1, controlPoint.Color));
                    function.AddControlPoint(new TFAlphaControlPoint(1, alpha));
                }
            }
        } else {
            function.AddControlPoint(new TFColourControlPoint(0, Color.white));
            function.AddControlPoint(new TFAlphaControlPoint(0, 1));
            function.AddControlPoint(new TFColourControlPoint(1, Color.white));
            function.AddControlPoint(new TFAlphaControlPoint(1, 1));
        }

        function.GenerateTexture();

        return function;
    }

    private void GenerateStartingPoints() {
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(m_RectTransform);

        // Coordinates are relativ to the middle center point of the parent rect transform

        float xMin = -bounds.extents.x + (m_ControlPointSize.x / 2f);
        float xMax =  bounds.extents.x + (m_ControlPointSize.x / 2f);
        float yMin = -bounds.extents.y + (m_ControlPointSize.y / 2f);
        float yMax =  bounds.extents.y + (m_ControlPointSize.y / 2f);

        float x, y;

        // Blue
        x = Mathf.Lerp(xMin, xMax, 0.125f);
        y = Mathf.Lerp(yMin, yMax, 0.25f);
        CreatePoint(new Vector2(x, y), Color.blue, false);

        // Green
        x = Mathf.Lerp(xMin, xMax, 0.375f);
        y = Mathf.Lerp(yMin, yMax, 0.5f);
        CreatePoint(new Vector2(x, y), Color.green, false);

        // Yellow
        x = Mathf.Lerp(xMin, xMax, 0.625f);
        y = Mathf.Lerp(yMin, yMax, 0.625f);
        CreatePoint(new Vector2(x, y), Color.yellow, false);

        // Red
        x = Mathf.Lerp(xMin, xMax, 0.875f);
        y = Mathf.Lerp(yMin, yMax, 0.75f);
        CreatePoint(new Vector2(x, y), Color.red, false);
    }

    private void OnColorPickerChanged(Color color) {
        // Bail out if no point is selected
        if (!m_SelectedPoint) {
            return;
        }

        m_SelectedPoint.Color = color;

        RedrawHistogram();
    }

    private void CreatePoint(Vector2 pointInBox, Color color, bool select) {
        TransferFunctionControlPointUI point = Instantiate(m_ControlPointPrefab, transform);
        point.GetComponent<RectTransform>().anchoredPosition = pointInBox;
        point.name = $"Transferfunction_Control_Point";
        point.Init(this, color, select);

        m_ControlPoints.Add(point);

        Redraw();

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

        // We need to include the size of the control points or otherwise their image goes outside the box
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
