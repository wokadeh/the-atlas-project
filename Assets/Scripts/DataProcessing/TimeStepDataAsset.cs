using UnityEngine;

[System.Serializable]
public class TimeStepDataAsset {
    public Vector3 Dimensions { get; set; }
    public Texture3D DataTexture3D { get; set; }
    public Texture2D DataTexture2D { get; set; }
    public Texture2D HistogramTexture { get; set; }
    public double DateTimeDouble { get; set; }
}
