using UnityEngine;
using UnityEngine.VFX;

namespace MeshSparklyEffect
{
    [ExecuteAlways]
    public class MeshSparklyEffect : MonoBehaviour
    {
        public SkinnedMeshRenderer targetMesh;
        public MeshFilter targetMeshFilter;
        public bool useMeshFilter;
        public Texture2D colorTexture;

        public bool isMapMode;

        public SparkleVFX sparkleVFX = new SparkleVFX();

        public Texture2D positionMap = null;
        public Texture2D normalMap = null;
        public Texture2D uvMap = null;

        private int _targetMeshInstanceID;
        private int _targetMeshFilterInstanceID;
        
        private Mesh GetMesh()
        {
            return useMeshFilter ? targetMeshFilter.sharedMesh : targetMesh.sharedMesh;
        }

        public void CreateMaps()
        {
            positionMap = MeshToMap.ToPositionMap(GetMesh());
            normalMap = MeshToMap.ToNormalMap(GetMesh());
            uvMap = MeshToMap.ToUVMap(GetMesh());

            _targetMeshInstanceID = targetMesh.GetInstanceID();
            _targetMeshFilterInstanceID = targetMeshFilter.GetInstanceID();
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

        private void Start()
        {
            AddVFX();
            sparkleVFX.GetInitialProperties();
        }

        private void Update()
        {
            if (sparkleVFX == null) return;

            if (ResourcesHasChanged()) CreateMaps();
            sparkleVFX.SetProperties(colorTexture, positionMap, normalMap, uvMap);
        }

        private bool ResourcesHasChanged()
        {
            return targetMesh != null && _targetMeshInstanceID != targetMesh.GetInstanceID() ||
                   targetMeshFilter != null && _targetMeshFilterInstanceID != targetMeshFilter.GetInstanceID();
        }
    }
}