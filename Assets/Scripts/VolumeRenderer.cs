using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class VolumeRenderer : MonoBehaviour {
    [SerializeField] private Material m_CartesianMaterial;
    [SerializeField] private Material m_SphericalMaterial;

    [SerializeField] private MeshRenderer m_Groundplane;
    [SerializeField] private MeshRenderer m_EarthSphere;

    public VolumeRendererMode Mode { get; private set; }
    
    private MeshRenderer m_Renderer;

    private void Start() {
        m_Renderer = GetComponent<MeshRenderer>();

        // We always start of in cartesian mode
        SetMode(VolumeRendererMode.Cartesian);
    }

    public void SetMode(VolumeRendererMode mode) {
        Mode = mode;

        if (mode == VolumeRendererMode.Cartesian) {
            m_Groundplane.enabled = true;
            m_EarthSphere.enabled = false;
            m_Renderer.material = m_CartesianMaterial;
            transform.localScale = new Vector3(1f, 0.2f, -0.75f);
            transform.rotation = Quaternion.Euler(180, 0, 0);
        } else if (mode == VolumeRendererMode.Spherical) {
            m_Groundplane.enabled = false;
            m_EarthSphere.enabled = true;
            m_Renderer.material = m_SphericalMaterial;
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.rotation = Quaternion.Euler(-90, 0, 0);
        }
    }

    public void SetData(EarthDataFrame data) {
        m_CartesianMaterial.SetTexture("_Data", data.DataTexture);
        m_SphericalMaterial.SetTexture("_Data", data.DataTexture);
    }

    public void SetTransferFunction(Texture2D transferFunction) {
        m_CartesianMaterial.SetTexture("_TFTex", transferFunction);
        m_SphericalMaterial.SetTexture("_TFTex", transferFunction);
    }
}
