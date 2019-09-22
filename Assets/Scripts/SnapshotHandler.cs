using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapshotHandler : MonoBehaviour
{
    private Camera m_Camera;
    private bool m_IsTakeSnapshotNextFrame;

    public int m_PicWidth;
    public int m_PicHeight;

    public string m_DataName = "camerasnapshot";
    public string m_SnapshotPath;

    public bool m_CaptureAlpha = true;

    public DataManager m_DataManager;

    public TimestampUI m_Timestamp;

    private void Awake()
    {
        m_Camera = gameObject.GetComponent<Camera>();

        m_PicWidth = Screen.width;
        m_PicHeight = Screen.height;

        m_SnapshotPath = Application.dataPath + "/Snapshots";
    }

    private void OnPostRender()
    {
        if (m_IsTakeSnapshotNextFrame)
        {
            m_IsTakeSnapshotNextFrame = false;

            //grab Texture from camera
            RenderTexture renderTexture = m_Camera.targetTexture;
            Texture2D renderResult;

            if (m_CaptureAlpha)
            {
                renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            }
            else
            {
                renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            }

            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);

            //store as PNG
            byte[] byteArray = renderResult.EncodeToPNG();

            m_DataName = m_Timestamp.m_CurrentDate.Replace(":", "");

            this.CheckForDuplicate();

            System.IO.File.WriteAllBytes(m_SnapshotPath + "/" + m_DataName + ".png", byteArray);
            Log.Info(this, "Saved Snapshot");

            //cleanup
            RenderTexture.ReleaseTemporary(renderTexture);
            m_Camera.targetTexture = null;
        }
    }

    void CheckForDuplicate()
    {
        if (System.IO.File.Exists(m_SnapshotPath + "/" + m_DataName + ".png"))
        {
            m_DataName += " Kopie";
            this.CheckForDuplicate();
        }
    }

    //triggered by snapshotButton in UI
    public void TakeSnapshot()
    {
        m_Camera.targetTexture = RenderTexture.GetTemporary(m_PicWidth, m_PicHeight, 16);

        m_IsTakeSnapshotNextFrame = true;
    }
}