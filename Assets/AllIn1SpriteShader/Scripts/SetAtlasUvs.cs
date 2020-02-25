using UnityEngine;

[ExecuteInEditMode]
public class SetAtlasUvs : MonoBehaviour
{
    [SerializeField] private bool updateEveryFrame = false;
    private Renderer render;
    private SpriteRenderer spriteRender;

    void Start()
    {
        GetRendererReferencesIfNeeded();

        GetAndSetUVs();

        if (!updateEveryFrame && Application.isPlaying) this.enabled = false;
    }

    private void Update()
    {
        if (updateEveryFrame)
        {
            GetAndSetUVs();
        }       
    }

    public void GetAndSetUVs()
    {
        GetRendererReferencesIfNeeded();
        Rect r = spriteRender.sprite.rect;
        r.x /= spriteRender.sprite.texture.width;
        r.width /= spriteRender.sprite.texture.width;
        r.y /= spriteRender.sprite.texture.height;
        r.height /= spriteRender.sprite.texture.height;

        render.sharedMaterial.SetFloat("_MinXUV", r.xMin);
        render.sharedMaterial.SetFloat("_MaxXUV", r.xMax);
        render.sharedMaterial.SetFloat("_MinYUV", r.yMin);
        render.sharedMaterial.SetFloat("_MaxYUV", r.yMax);
    }

    public void ResetAtlasUvs()
    {
        GetRendererReferencesIfNeeded();
        render.sharedMaterial.SetFloat("_MinXUV", 0f);
        render.sharedMaterial.SetFloat("_MaxXUV", 1f);
        render.sharedMaterial.SetFloat("_MinYUV", 0f);
        render.sharedMaterial.SetFloat("_MaxYUV", 1f);
    }

    public void UpdateEveryFrame(bool everyFrame)
    {
        updateEveryFrame = everyFrame;
    }

    private void GetRendererReferencesIfNeeded()
    {
        if (spriteRender == null) spriteRender = GetComponent<SpriteRenderer>();
        if (render == null) render = GetComponent<Renderer>();
        if (render == null || spriteRender == null)
        {
            Debug.LogError("Looks like you are missing a Sprite Renderer on: " + gameObject.name +
                "\n SetAtlasUvs component will now get destroyed");
            Destroy(this);
        }
    }
}