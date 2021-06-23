using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace MeshSparklyEffect
{
    public static class MeshToMap
    {
        public static Texture2D ToPositionMap(Mesh mesh)
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

        public static void ToPositionMapWithGPU(Mesh.MeshData meshData, ComputeShader compute,
            ComputeBuffer positionBuffer, RenderTexture positionMap)
        {
            var vertexCount = meshData.vertexCount;

            using var positionArray =
                new NativeArray<Vector3>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            meshData.GetVertices(positionArray);
            positionBuffer.SetData(positionArray, 0, 0, vertexCount);

            var bakeKernelID = compute.FindKernel("BakeToRenderTexture");
            compute.SetInt("VertexCount", vertexCount);
            compute.SetBuffer(bakeKernelID, "PositionBuffer", positionBuffer);
            compute.SetTexture(bakeKernelID, "PositionMap", positionMap);

            var width = positionMap.width;
            var height = positionMap.height;
            compute.Dispatch(bakeKernelID, width / 8, height / 8, 1);
        }

        public static Texture2D ToUVMap(Mesh mesh)
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

        public static Texture2D ToNormalMap(Mesh mesh)
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

        private static Texture2D CreateMap(IEnumerable<Color> colors, int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBAFloat, false)
            {
                filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp
            };

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
}