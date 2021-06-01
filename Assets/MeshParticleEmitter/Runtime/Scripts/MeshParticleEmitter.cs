using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteAlways]
public class MeshParticleEmitter : MonoBehaviour
{
    public SkinnedMeshRenderer targetMesh;
    public MeshFilter targetMeshFilter;
    public bool useMeshFilter;

    public Texture2D colorTexture;

    public uint rate;
    [Range(0.0f, 1.0f)] public float width;
    public float alpha;
    public AnimationCurve sizeDecayCurve;
    public float sizeMin;
    public float sizeMax;
    public float lifeTimeMin;
    public float lifeTimeMax;
    public float emissionIntensity;
    public float rotateDegreeMin;
    public float rotateDegreeMax;
    public float offset;
    public bool useTexture;
    public Texture2D sparkleTexture;

    private VisualEffect _effect;

    private Texture2D _positionMap = null;
    private Texture2D _normalMap = null;
    private Texture2D _uvMap = null;

    private int _targetMeshInstanceID;
    private int _targetMeshFilterInstanceID;
    private bool _mapsCreated = false;

    private static readonly int RateID = Shader.PropertyToID("Rate");
    private static readonly int WidthID = Shader.PropertyToID("Width");
    private static readonly int AlphaID = Shader.PropertyToID("Alpha");
    private static readonly int SizeDecayCurveID = Shader.PropertyToID("SizeDecayCurve");
    private static readonly int SizeMinID = Shader.PropertyToID("SizeMin");
    private static readonly int SizeMaxID = Shader.PropertyToID("SizeMax");
    private static readonly int LifeTimeMinID = Shader.PropertyToID("LifeTimeMin");
    private static readonly int LifeTimeMaxID = Shader.PropertyToID("LifeTimeMax");
    private static readonly int EmissionIntensityID = Shader.PropertyToID("EmissionIntensity");
    private static readonly int RotateDegreeMinID = Shader.PropertyToID("RotateDegreeMin");
    private static readonly int RotateDegreeMaxID = Shader.PropertyToID("RotateDegreeMax");
    private static readonly int OffsetID = Shader.PropertyToID("Offset");
    private static readonly int UseTextureID = Shader.PropertyToID("UseTexture");
    private static readonly int SparkleTextureID = Shader.PropertyToID("SparkleTexture");

    private Mesh GetMesh()
    {
        return useMeshFilter ? targetMeshFilter.sharedMesh : targetMesh.sharedMesh;
    }

    public void CreateMaps()
    {
        _positionMap = MeshToPositionMap(GetMesh());
        _normalMap = MeshToNormalMap(GetMesh());
        _uvMap = MeshToUVMap(GetMesh());

        _targetMeshInstanceID = targetMesh.GetInstanceID();
        _targetMeshFilterInstanceID = targetMeshFilter.GetInstanceID();
        _mapsCreated = true;
    }

    private void AddVFX()
    {
        var visualEffect = transform.GetComponentInChildren<VisualEffect>();
        if (visualEffect != null)
        {
            _effect = visualEffect;
            return;
        }

        var vfx = Resources.Load<VisualEffectAsset>($"Sparkle");
        var vfxGameObject = new GameObject("VFX");
        visualEffect = vfxGameObject.AddComponent<VisualEffect>();
        visualEffect.visualEffectAsset = vfx;
        _effect = visualEffect;

        vfxGameObject.transform.SetParent(transform);
        vfxGameObject.transform.localPosition = Vector3.zero;
        vfxGameObject.transform.localRotation = Quaternion.identity;
        vfxGameObject.transform.localScale = Vector3.one;
    }

    private void GetInitialProperties()
    {
        rate = _effect.GetUInt(RateID);
        width = _effect.GetFloat(WidthID);
        alpha = _effect.GetFloat(AlphaID);
        sizeDecayCurve = _effect.GetAnimationCurve(SizeDecayCurveID);
        sizeMin = _effect.GetFloat(SizeMinID);
        sizeMax = _effect.GetFloat(SizeMaxID);
        lifeTimeMin = _effect.GetFloat(LifeTimeMinID);
        lifeTimeMax = _effect.GetFloat(LifeTimeMaxID);
        emissionIntensity = _effect.GetFloat(EmissionIntensityID);
        rotateDegreeMin = _effect.GetFloat(RotateDegreeMinID);
        rotateDegreeMax = _effect.GetFloat(RotateDegreeMaxID);
        offset = _effect.GetFloat(OffsetID);
        useTexture = _effect.GetBool(UseTextureID);
        sparkleTexture = (Texture2D) _effect.GetTexture(SparkleTextureID);
    }

    private void SetProperties()
    {
        if (_mapsCreated)
        {
            _effect.SetTexture("_PositionMap", _positionMap);
            if (colorTexture != null) _effect.SetTexture("_ColorTexture", colorTexture);
            _effect.SetTexture("_NormalMap", _normalMap);
            _effect.SetTexture("_UVMap", _uvMap);
        }

        _effect.SetUInt(RateID, rate);
        _effect.SetFloat(WidthID, width);
        _effect.SetFloat(AlphaID, alpha);
        _effect.SetAnimationCurve(SizeDecayCurveID, sizeDecayCurve);
        _effect.SetFloat(SizeMinID, sizeMin);
        _effect.SetFloat(SizeMaxID, sizeMax);
        _effect.SetFloat(LifeTimeMinID, lifeTimeMin);
        _effect.SetFloat(LifeTimeMaxID, lifeTimeMax);
        _effect.SetFloat(EmissionIntensityID, emissionIntensity);
        _effect.SetFloat(RotateDegreeMinID, rotateDegreeMin);
        _effect.SetFloat(RotateDegreeMaxID, rotateDegreeMax);
        _effect.SetFloat(OffsetID, offset);
        _effect.SetBool(UseTextureID, useTexture);
        _effect.SetTexture(SparkleTextureID, sparkleTexture);
    }

    private void Start()
    {
        AddVFX();
        GetInitialProperties();
    }

    private void Update()
    {
        if (_effect == null) return;

        if (ResourcesHasChanged()) CreateMaps();
        SetProperties();
    }

    private bool ResourcesHasChanged()
    {
        return targetMesh != null && _targetMeshInstanceID != targetMesh.GetInstanceID() ||
               targetMeshFilter != null && _targetMeshFilterInstanceID != targetMeshFilter.GetInstanceID();
    }

    private Texture2D MeshToUVMap(Mesh mesh)
    {
        var uvs = mesh.uv;
        var count = uvs.Count();

        var r = Mathf.Sqrt(count);
        var width = (int) Mathf.Ceil(r);

        var colors = new Color[width * width];
        for (var i = 0; i < width * width; i++)
        {
            var uv = uvs[i % count];
            colors[i] = new Color(uv.x, uv.y, 0.0f);
        }

        var tex = CreateMap(colors, width, width);

        return tex;
    }

    private Texture2D MeshToNormalMap(Mesh mesh)
    {
        var normals = mesh.normals;
        var count = normals.Count();

        var r = Mathf.Sqrt(count);
        var width = (int) Mathf.Ceil(r);

        var colors = new Color[width * width];
        for (var i = 0; i < width * width; i++)
        {
            var norm = normals[i % count];
            colors[i] = new Color(norm.x, norm.y, norm.z);
        }

        var tex = CreateMap(colors, width, width);

        return tex;
    }

    static Texture2D MeshToPositionMap(Mesh mesh)
    {
        var vertices = mesh.vertices;
        var count = vertices.Count();

        var r = Mathf.Sqrt(count);
        var width = (int) Mathf.Ceil(r);

        var colors = new Color[width * width];
        for (var i = 0; i < width * width; i++)
        {
            var vtx = vertices[i % count];
            colors[i] = new Color(vtx.x, vtx.y, vtx.z);
        }

        var tex = CreateMap(colors, width, width);

        return tex;
    }

    static Texture2D CreateMap(IEnumerable<Color> colors, int width, int height)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        var buf = new Color[width * height];

        var idx = 0;
        foreach (var color in colors)
        {
            buf[idx] = color;
            idx++;
        }

        tex.SetPixels(buf);
        tex.Apply();

        return tex;
    }
}