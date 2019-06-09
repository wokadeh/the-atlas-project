using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.ColorPicker;

public class TransferFunctionUI : MonoBehaviour, IPointerClickHandler {
    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private Color m_ControlPointStartColor;
    [SerializeField] private TransferFunctionControlPointUI m_ControlPointPrefab;
    [SerializeField] private ColorPickerControl m_ColorPicker;
    [SerializeField] private UILineRenderer m_LineRenderer;
    [SerializeField] private RawImage m_HistogramTexture;

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

        Redraw();
    }

    public void Redraw() {
        // We first need to correctly order the lines
        m_ControlPoints = m_ControlPoints.OrderBy(p => p.transform.localPosition.x).ToList();

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

    private void RedrawLines() {
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

    private void RedrawHistogram() {
        if (m_DataManager.CurrentAsset != null) {
            m_HistogramTexture.material.SetTexture("_HistTex", m_DataManager.CurrentAsset.HistogramTexture);
            m_HistogramTexture.material.SetTexture("_TFTex", GenerateTransferFunction().GetTexture());
        }
    }

    private TransferFunction GenerateTransferFunction() {
        TransferFunction function = new TransferFunction();

        // NOTE: For the size we should need to take into account the size of the control points
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
            } else if (i == m_ControlPoints.Count - 1) {
                function.AddControlPoint(new TFColourControlPoint(1, controlPoint.Color));
                function.AddControlPoint(new TFAlphaControlPoint(1, alpha));
            }
        }

        function.GenerateTexture();

        return function;
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

    public class TransferFunction {
        public List<TFColourControlPoint> colourControlPoints = new List<TFColourControlPoint>();
        public List<TFAlphaControlPoint> alphaControlPoints = new List<TFAlphaControlPoint>();

        public Texture2D histogramTexture = null;

        private Texture2D texture = null;
        Color[] tfCols;

        private const int TEXTURE_WIDTH = 512;
        private const int TEXTURE_HEIGHT = 2;

        public TransferFunction() {
            texture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, TextureFormat.RGBAFloat, false);
            tfCols = new Color[TEXTURE_WIDTH * TEXTURE_HEIGHT];
        }

        public void AddControlPoint(TFColourControlPoint ctrlPoint) {
            colourControlPoints.Add(ctrlPoint);
        }

        public void AddControlPoint(TFAlphaControlPoint ctrlPoint) {
            alphaControlPoints.Add(ctrlPoint);
        }

        public Texture2D GetTexture() {
            if (texture == null)
                GenerateTexture();

            return texture;
        }

        public void GenerateTexture() {
            List<TFColourControlPoint> cols = new List<TFColourControlPoint>(colourControlPoints);
            List<TFAlphaControlPoint> alphas = new List<TFAlphaControlPoint>(alphaControlPoints);

            // Sort lists of control points
            cols.Sort((a, b) => (a.dataValue.CompareTo(b.dataValue)));
            alphas.Sort((a, b) => (a.dataValue.CompareTo(b.dataValue)));

            // Add colour points at beginning and end
            if (cols.Count == 0 || cols[cols.Count - 1].dataValue < 1.0f)
                cols.Add(new TFColourControlPoint(1.0f, Color.white));
            if (cols[0].dataValue > 0.0f)
                cols.Insert(0, new TFColourControlPoint(0.0f, Color.white));

            // Add alpha points at beginning and end
            if (alphas.Count == 0 || alphas[alphas.Count - 1].dataValue < 1.0f)
                alphas.Add(new TFAlphaControlPoint(1.0f, 1.0f));
            if (alphas[0].dataValue > 0.0f)
                alphas.Insert(0, new TFAlphaControlPoint(0.0f, 0.0f));

            int numColours = cols.Count;
            int numAlphas = alphas.Count;
            int iCurrColour = 0;
            int iCurrAlpha = 0;

            for (int iX = 0; iX < TEXTURE_WIDTH; iX++) {
                float t = iX / (float)(TEXTURE_WIDTH - 1);
                while (iCurrColour < numColours - 2 && cols[iCurrColour + 1].dataValue < t)
                    iCurrColour++;
                while (iCurrAlpha < numAlphas - 2 && alphas[iCurrAlpha + 1].dataValue < t)
                    iCurrAlpha++;

                TFColourControlPoint leftCol = cols[iCurrColour];
                TFColourControlPoint rightCol = cols[iCurrColour + 1];
                TFAlphaControlPoint leftAlpha = alphas[iCurrAlpha];
                TFAlphaControlPoint rightAlpha = alphas[iCurrAlpha + 1];

                float tCol = (Mathf.Clamp(t, leftCol.dataValue, rightCol.dataValue) - leftCol.dataValue) / (rightCol.dataValue - leftCol.dataValue);
                float tAlpha = (Mathf.Clamp(t, leftAlpha.dataValue, rightAlpha.dataValue) - leftAlpha.dataValue) / (rightAlpha.dataValue - leftAlpha.dataValue);

                Color pixCol = rightCol.colourValue * tCol + leftCol.colourValue * (1.0f - tCol);
                pixCol.a = rightAlpha.alphaValue * tAlpha + leftAlpha.alphaValue * (1.0f - tAlpha);

                for (int iY = 0; iY < TEXTURE_HEIGHT; iY++) {
                    tfCols[iX + iY * TEXTURE_WIDTH] = pixCol;
                }
            }

            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(tfCols);
            texture.Apply();
        }
    }
}
