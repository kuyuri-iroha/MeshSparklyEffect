using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

namespace MeshSparklyEffect
{
    [ExecuteAlways]
    public class SkinnedMeshSparklyEffect : MonoBehaviour
    {
        public SkinnedMeshRenderer targetMesh;
        public Texture2D colorTexture;

        public bool isMapMode;

        public SparkleVFX sparkleVFX = new SparkleVFX();

        public RenderTexture positionMap;
        public Texture2D normalMap;
        public Texture2D uvMap;

        private int _targetMeshInstanceID;

        private ComputeShader _compute;
        private ComputeBuffer _positionBuffer;
        private Mesh _tmpMesh;

        public void CreateMaps()
        {
            normalMap = MeshToMap.ToNormalMap(targetMesh.sharedMesh);
            uvMap = MeshToMap.ToNormalMap(targetMesh.sharedMesh);

            _targetMeshInstanceID = targetMesh.GetInstanceID();

            // Position map
            var vertexCount = targetMesh.sharedMesh.vertexCount;
            _positionBuffer = new ComputeBuffer(vertexCount, Marshal.SizeOf<Vector3>());
            var r = Mathf.Sqrt(vertexCount);
            var width = (int) Mathf.Ceil(r);
            width = width / 8 * 8;
            positionMap = new RenderTexture(width, width, 0, RenderTextureFormat.ARGBFloat)
            {
                hideFlags = HideFlags.DontSave,
                enableRandomWrite = true
            };
            positionMap.Create();
        }

        public void Initialize()
        {
            _compute = Resources.Load<ComputeShader>("BakeToRenderTexture");

            _tmpMesh = new Mesh();
            _tmpMesh.hideFlags = HideFlags.DontSave;
        }

        private void AddVFX()
        {
            var visualEffect = transform.GetComponentInChildren<VisualEffect>();
            if (visualEffect != null)
            {
                sparkleVFX.SetSparklyVFX(visualEffect);
                return;
            }

            var vfx = Resources.Load<VisualEffectAsset>($"Sparkle");
            var vfxGameObject = new GameObject("VFX");
            visualEffect = vfxGameObject.AddComponent<VisualEffect>();
            visualEffect.visualEffectAsset = vfx;
            sparkleVFX.SetSparklyVFX(visualEffect);

            vfxGameObject.transform.SetParent(transform);
            vfxGameObject.transform.localPosition = Vector3.zero;
            vfxGameObject.transform.localRotation = Quaternion.identity;
            vfxGameObject.transform.localScale = Vector3.one;
        }

        private void UpdatePositionMap()
        {
            targetMesh.BakeMesh(_tmpMesh);

            using var dataArray = Mesh.AcquireReadOnlyMeshData(_tmpMesh);
            var data = dataArray[0];
            MeshToMap.ToPositionMapWithGPU(data, _compute, _positionBuffer, positionMap);
        }

        private void Start()
        {
            Initialize();
            AddVFX();
            sparkleVFX.GetInitialProperties();
        }

        private void LateUpdate()
        {
            if (sparkleVFX == null || targetMesh == null) return;

            UpdatePositionMap();

            if (ResourcesHasChanged()) CreateMaps();
            sparkleVFX.SetProperties(colorTexture, positionMap, normalMap, uvMap);
        }

        private bool ResourcesHasChanged()
        {
            return targetMesh != null && _targetMeshInstanceID != targetMesh.GetInstanceID();
        }
    }
}