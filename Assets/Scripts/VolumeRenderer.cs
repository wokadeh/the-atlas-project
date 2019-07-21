using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class VolumeRenderer : MonoBehaviour {
    public VolumeRendererMode Mode { get; private set; }
    
    private MeshRenderer m_Renderer;

    private void Start() {
        m_Renderer = GetComponent<MeshRenderer>();
    }

    public void SetMode(VolumeRendererMode mode) {
        Mode = mode;
        // TODO: Implement accordingly
    }

    public void SetData(EarthDataFrame data) {
        m_Renderer.material.SetTexture("_Data", data.DataTexture);
    }

    public void SetTransferFunction(Texture2D transferFunction) {
        m_Renderer.material.SetTexture("_TFTex", transferFunction);
    }
}
