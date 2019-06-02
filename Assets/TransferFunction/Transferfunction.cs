using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transferfunction : MonoBehaviour
{

    public List<TFColourControlPoint> colourControlPoints = new List<TFColourControlPoint>();
    public List<TFAlphaControlPoint> alphaControlPoints = new List<TFAlphaControlPoint>();

    private Texture2D texture = null;
    Color[] tfCols;

    private const int TEXTURE_WIDTH = 512;
    private const int TEXTURE_HEIGHT = 2;

    public Transferfunction()
    {
        texture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, TextureFormat.RGBAFloat, false);
        tfCols = new Color[TEXTURE_WIDTH * TEXTURE_HEIGHT];
    }
    // Start is called before the first frame update
    void Start()
    {

        Transferfunction function = new Transferfunction();
        function.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.6f, 0.6f, 0.1f, 0.1f)));
        function.AddControlPoint(new TFColourControlPoint(0.25f, new Color(0.6f, 0.4f, 0.13f, 0.4f)));
        function.AddControlPoint(new TFColourControlPoint(0.5f, new Color(0.3f, 0.3f, 0.0f, 0.3f)));
        function.AddControlPoint(new TFColourControlPoint(0.8f, new Color(0.3f, 0.5f, 0.0f, 0.1f)));
        function.AddControlPoint(new TFColourControlPoint(1.0f, new Color(0.3f, 0.1f, 0.0f, 0.1f)));

        function.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.5f));
        function.AddControlPoint(new TFAlphaControlPoint(0.5f, 0.155f));
        function.AddControlPoint(new TFAlphaControlPoint(1.0f, 0.4f));

        // function.GenerateTexture();
        Texture2D transferTexture = function.GetTexture();

        GetComponent<Renderer>().material.SetTexture("_TFTex", transferTexture);
        Debug.Log(transferTexture);
    }

    public void AddControlPoint(TFColourControlPoint ctrlPoint)
    {
        colourControlPoints.Add(ctrlPoint);
    }

    public void AddControlPoint(TFAlphaControlPoint ctrlPoint)
    {
        alphaControlPoints.Add(ctrlPoint);
    }

    public Texture2D GetTexture()
    {
        if (texture == null)
            GenerateTexture();

        return texture;
    }

    public void GenerateTexture()
    {
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

        //Add alpha points at beginning and end
        if (alphas.Count == 0 || alphas[alphas.Count - 1].dataValue < 1.0f)
            alphas.Add(new TFAlphaControlPoint(1.0f, 1.0f));
        if (alphas[0].dataValue > 0.0f)
            alphas.Insert(0, new TFAlphaControlPoint(0.0f, 0.0f));
        // Anzahl colours
        int numColours = cols.Count;
        //Anzahl der transparenten
        int numAlphas = alphas.Count;
        int iCurrColour = 0;
        int iCurrAlpha = 0;


        for (int iX = 0; iX < TEXTURE_WIDTH; iX++)
        {
            //position innerhalb der samples
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


            // Farbwerte u. transparenzwerte
         //   Color pixCol = rightCol.colourValue * tCol + leftCol.colourValue * (1.0f - tCol);
           // pixCol.a = rightAlpha.alphaValue * tCol + leftAlpha.alphaValue * (1.0f - tCol);
            Color pixCol = Color.blue;
            pixCol.a =  0.7f;

            for (int iY = 0; iY < TEXTURE_HEIGHT; iY++)
            {
                // Jedem pixel ein Farbwert zugeteilt
                tfCols[iX + iY * TEXTURE_WIDTH] = pixCol;
            }
        }

        // in Texture einbetten und anwenden
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(tfCols);
        texture.Apply();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
