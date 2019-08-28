using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapshotHandler : MonoBehaviour
{
    private static SnapshotHandler instance;
    private Camera myCam;
    private bool takeSnapshotNextFrame;

    public int picWidth;
    public int picHeight;

    public string dataName = "camerasnapshot";
    public string snapshotPath;

    public bool captureAlpha = true;

    public DataManager dataManager;

    public TimestampUI timestamp;

    private void Awake()
    {
        instance = this;
        myCam = gameObject.GetComponent<Camera>();

        picWidth = Screen.width;
        picHeight = Screen.height;

        snapshotPath = Application.dataPath + "/Snapshots";
    }

    private void OnPostRender()
    {
        if (takeSnapshotNextFrame)
        {
            takeSnapshotNextFrame = false;


            //grab Texture from camera
            RenderTexture renderTexture = myCam.targetTexture;
            Texture2D renderResult;


            if (captureAlpha)
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

            dataName = timestamp.m_CurrentDate.Replace(":", "");


            checkForDuplicate();

            System.IO.File.WriteAllBytes(snapshotPath + "/" + dataName + ".png", byteArray);
            Debug.Log("Saved Snapshot");

            //cleanup
            RenderTexture.ReleaseTemporary(renderTexture);
            myCam.targetTexture = null;

        }
    }

    void checkForDuplicate()
    {
        if (System.IO.File.Exists(snapshotPath + "/" + dataName + ".png"))
        {
            dataName += " Kopie";
            checkForDuplicate();
        }
    }

    //triggered by snapshotButton in UI
    public void TakeSnapshot()
    {
        myCam.targetTexture = RenderTexture.GetTemporary(picWidth, picHeight, 16);

        takeSnapshotNextFrame = true;
    }
}