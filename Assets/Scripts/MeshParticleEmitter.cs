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
    private Texture2D _colorMap = null;

    [ContextMenu("CreateMaps")]
    private void CreateMaps()
    {
        _positionMap = MeshToPositionMap(targetMesh.sharedMesh);
        _normalMap = MeshToNormalMap(targetMesh.sharedMesh);
        _colorMap = GenerateColorMap(targetMesh.sharedMesh);
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

        if (!_positionMap || !_colorMap)
        {
            //Debug.LogError("テクスチャが生成されていません。CreateMapsを実行してください。");
            return;
        }

        effect.SetTexture("_PositionMap", _positionMap);
        effect.SetTexture("_ColorMap", _colorMap);
        effect.SetTexture("_NormalMap", _normalMap);
    }

    private Texture2D MeshToNormalMap(Mesh mesh)
    {
        var normals = mesh.normals;
        var count = normals.Count();

        var r = Mathf.Sqrt(count);
        var width = (int) Mathf.Ceil(r);

        var normalColors = normals.Select(norm => new Color(norm.x, norm.y, norm.z));

        var tex = CreateMap(normalColors, width, width);

        return tex;
    }

    private Texture2D GenerateColorMap(Mesh mesh)
    {
        var uvs = mesh.uv;
        var count = uvs.Length;

        var r = Mathf.Sqrt(count);
        var width = (int) Mathf.Ceil(r);

        var pixels = colorTexture.GetPixels();

        var colors = uvs.Select(uv => pixels[(int) (uv.x * colorTexture.width) + (int) (uv.y * colorTexture.width)]);

        return CreateMap(colors, width, width);
    }

    static Texture2D MeshToPositionMap(Mesh mesh)
    {
        var vertices = mesh.vertices;
        var count = vertices.Count();

        var r = Mathf.Sqrt(count);
        var width = (int) Mathf.Ceil(r);

        var positions = vertices.Select(vtx => new Color(vtx.x, vtx.y, vtx.z));

        var tex = CreateMap(positions, width, width);

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