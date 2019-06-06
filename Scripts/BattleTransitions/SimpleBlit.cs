using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SimpleBlit : MonoBehaviour
{
    public Material TransitionMaterial;
    public float _cutoff = 0;

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (TransitionMaterial != null){
            TransitionMaterial.SetFloat("_Cutoff", _cutoff);
            Graphics.Blit(src, dst, TransitionMaterial);
        }
    }
}
