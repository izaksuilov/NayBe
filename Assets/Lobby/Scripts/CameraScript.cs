
using UnityEngine;
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
    [SerializeField] private bool uniform = true;
    [SerializeField] private bool autoSetUniform = false;
    private Camera Camera;
    private void Awake()
    {
        Camera = GetComponent<Camera>();
        Camera.orthographic = true;
        if (uniform)
            SetUniform();
    }
    private void LateUpdate()
    {
        if (autoSetUniform && uniform)
            SetUniform();
    }
    private void SetUniform()
    {
        float orthographicSize = Camera.pixelHeight / 2;
        if (orthographicSize != Camera.orthographicSize)
            Camera.orthographicSize = orthographicSize;
    }
}
