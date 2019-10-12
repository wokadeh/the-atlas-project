// ****************************** LOCATION ********************************
//
// [Camera] -> attached
//
// ************************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapshotHandler : MonoBehaviour
{
    [SerializeField] private Camera m_Camera;
    [SerializeField] private TimestampUI m_TimestampUI;

    [SerializeField] private bool m_CaptureAlpha;

    private bool m_IsTakeSnapshotNextFrame;

    private int m_PicWidth;
    private int m_PicHeight;

    private DataManager m_DataManager;

    private string m_DataName;

    private void Awake()
    {
        m_PicWidth = Screen.width;
        m_PicHeight = Screen.height;

        m_DataName = Globals.SNAPSHOT_NAME;
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

            m_DataName = m_TimestampUI.CurrentDate.Replace(":", "").Replace( "/", "_" );

            Log.Info( this, "Saving " + m_DataName );

            this.CheckForDuplicate();

            System.IO.File.WriteAllBytes(Globals.SAVE_SNAPSHOTS_PATH + "/" + m_DataName + ".png", byteArray);
            Log.Info(this, "Saved Snapshot");

            //cleanup
            RenderTexture.ReleaseTemporary(renderTexture);
            m_Camera.targetTexture = null;
        }
    }

    void CheckForDuplicate()
    {
        if (System.IO.File.Exists(Globals.SAVE_SNAPSHOTS_PATH + "/" + m_DataName + ".png"))
        {
            m_DataName += "_copy";
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