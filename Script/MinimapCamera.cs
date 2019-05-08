using UnityEngine;

[RequireComponent(typeof (Camera))]
public class MinimapCamera : MonoBehaviour {

    [SerializeField] bool enableFog = false;

    bool previousFogState;

    private void OnPreRender()
    {
        previousFogState = RenderSettings.fog;
        RenderSettings.fog = enableFog;
    }

    private void OnPostRender()
    {
        RenderSettings.fog = previousFogState;
    }
}
