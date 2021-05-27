using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteAlways]
public class MeshParticleEmitter : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer targetMesh;

    [SerializeField] private VisualEffect effect;

    [SerializeField] private Texture2D colorTexture;

    private Texture2D _positionMap = null;
    private Texture2D _normalMap = null;
    private Texture2D _uvMap = null;

    private int _targetMeshInstanceID;
    private int _colorTextureInstanceID;

    [ContextMenu("CreateMaps")]
    private void CreateMaps()
    {
        _positionMap = MeshToPositionMap(targetMesh.sharedMesh);
        _normalMap = MeshToNormalMap(targetMesh.sharedMesh);
        _uvMap = MeshToUVMap(targetMesh.sharedMesh);

        _targetMeshInstanceID = targetMesh.GetInstanceID();
        _colorTextureInstanceID = colorTexture.GetInstanceID();
    }

    private void Start()
    {
        CreateMaps();
    }

    private void Update()
    {
        if (!targetMesh || !effect)
        {
            Debug.LogError("いずれかのSerializeFieldがセットされていません。");
            return;
        }

        if (ResourcesHasChanged()) CreateMaps();

        effect.SetTexture("_PositionMap", _positionMap);
        effect.SetTexture("_ColorTexture", colorTexture);
        effect.SetTexture("_NormalMap", _normalMap);
        effect.SetTexture("_UVMap", _uvMap);
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