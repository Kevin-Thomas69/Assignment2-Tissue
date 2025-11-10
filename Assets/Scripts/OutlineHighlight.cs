using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class OutlineHighlight : MonoBehaviour
{
    private Renderer rend;
    private MaterialPropertyBlock propBlock;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    
    
    
    
    public void Highlight(bool active)
    {
        rend.GetPropertyBlock(propBlock);
        
        propBlock.SetFloat("_Controller", active ? 150f : 0f);
        rend.SetPropertyBlock(propBlock);
    }
}
