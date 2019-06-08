using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class VolumeRenderer : MonoBehaviour {
    private MeshRenderer m_Renderer;

    private void Start() {
        m_Renderer = GetComponent<MeshRenderer>();
    }

    public void SetData(DataAsset data) {
        m_Renderer.material.SetTexture("_Data", data.DataTexture);
    }
}
