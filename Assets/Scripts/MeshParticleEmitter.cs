using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteAlways]
public class MeshParticleEmitter : MonoBehaviour
{
    public SkinnedMeshRenderer targetMesh;

    public VisualEffect effect;

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

    private Texture2D _positionMap = null;
    private Texture2D _normalMap = null;
    private Texture2D _uvMap = null;

    private int _targetMeshInstanceID;
    private int _colorTextureInstanceID;

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

    [ContextMenu("CreateMaps")]
    private void CreateMaps()
    {
        if (targetMesh == null) return; // targetMeshが入る前にOnValidateを呼んだときにエラーを吐かないようにする処置
        _positionMap = MeshToPositionMap(targetMesh.sharedMesh);
        _normalMap = MeshToNormalMap(targetMesh.sharedMesh);
        _uvMap = MeshToUVMap(targetMesh.sharedMesh);

        _targetMeshInstanceID = targetMesh.GetInstanceID();
        _colorTextureInstanceID = colorTexture.GetInstanceID();
    }

    private void GetInitialProperties()
    {
        rate = effect.GetUInt(RateID);
        width = effect.GetFloat(WidthID);
        alpha = effect.GetFloat(AlphaID);
        sizeDecayCurve = effect.GetAnimationCurve(SizeDecayCurveID);
        sizeMin = effect.GetFloat(SizeMinID);
        sizeMax = effect.GetFloat(SizeMaxID);
        lifeTimeMin = effect.GetFloat(LifeTimeMinID);
        lifeTimeMax = effect.GetFloat(LifeTimeMaxID);
        emissionIntensity = effect.GetFloat(EmissionIntensityID);
        rotateDegreeMin = effect.GetFloat(RotateDegreeMinID);
        rotateDegreeMax = effect.GetFloat(RotateDegreeMaxID);
        offset = effect.GetFloat(OffsetID);
        useTexture = effect.GetBool(UseTextureID);
        sparkleTexture = (Texture2D) effect.GetTexture(SparkleTextureID);
    }

    private void SetProperties()
    {
        effect.SetTexture("_PositionMap", _positionMap);
        effect.SetTexture("_ColorTexture", colorTexture);
        effect.SetTexture("_NormalMap", _normalMap);
        effect.SetTexture("_UVMap", _uvMap);

        effect.SetUInt(RateID, rate);
        effect.SetFloat(WidthID, width);
        effect.SetFloat(AlphaID, alpha);
        effect.SetAnimationCurve(SizeDecayCurveID, sizeDecayCurve);
        effect.SetFloat(SizeMinID, sizeMin);
        effect.SetFloat(SizeMaxID, sizeMax);
        effect.SetFloat(LifeTimeMinID, lifeTimeMin);
        effect.SetFloat(LifeTimeMaxID, lifeTimeMax);
        effect.SetFloat(EmissionIntensityID, emissionIntensity);
        effect.SetFloat(RotateDegreeMinID, rotateDegreeMin);
        effect.SetFloat(RotateDegreeMaxID, rotateDegreeMax);
        effect.SetFloat(OffsetID, offset);
        effect.SetBool(UseTextureID, useTexture);
        effect.SetTexture(SparkleTextureID, sparkleTexture);
    }

    private void Start()
    {
        CreateMaps();
        GetInitialProperties();
    }

    private void Update()
    {
        if (ResourcesHasChanged()) CreateMaps();
        SetProperties();
    }

    private bool ResourcesHasChanged()
    {
        return _targetMeshInstanceID != targetMesh.GetInstanceID() ||
               _colorTextureInstanceID != colorTexture.GetInstanceID();
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