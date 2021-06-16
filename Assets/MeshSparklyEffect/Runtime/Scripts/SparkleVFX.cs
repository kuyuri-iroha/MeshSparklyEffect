using System;
using UnityEngine;
using UnityEngine.VFX;

namespace MeshSparklyEffect
{
    [Serializable]
    public class SparkleVFX
    {
        private VisualEffect _effect;

        public uint rate;
        [Range(0.0f, 1.0f)] public float width;
        [Range(0.0f, 1.0f)] public float alpha;
        public AnimationCurve sizeDecayCurve;
        public float sizeMin;
        public float sizeMax;
        public float sizeLowLimit = 0.0f;
        public float sizeHighLimit = 10.0f;
        public float lifeTimeMin;
        public float lifeTimeMax;
        public float lifeTimeLowLimit = 0.0f;
        public float lifeTimeHighLimit = 10.0f;
        public float emissionIntensity;
        public float rotateDegree;
        public float offset;
        public bool useTexture;
        public Texture2D sparkleTexture;

        private static readonly int RateID = Shader.PropertyToID("Rate");
        private static readonly int WidthID = Shader.PropertyToID("Width");
        private static readonly int AlphaID = Shader.PropertyToID("Alpha");
        private static readonly int SizeDecayCurveID = Shader.PropertyToID("SizeDecayCurve");
        private static readonly int SizeMinID = Shader.PropertyToID("SizeMin");
        private static readonly int SizeMaxID = Shader.PropertyToID("SizeMax");
        private static readonly int LifeTimeMinID = Shader.PropertyToID("LifeTimeMin");
        private static readonly int LifeTimeMaxID = Shader.PropertyToID("LifeTimeMax");
        private static readonly int EmissionIntensityID = Shader.PropertyToID("EmissionIntensity");
        private static readonly int RotateDegreeID = Shader.PropertyToID("RotateDegree");
        private static readonly int OffsetID = Shader.PropertyToID("Offset");
        private static readonly int UseTextureID = Shader.PropertyToID("UseTexture");
        private static readonly int SparkleTextureID = Shader.PropertyToID("SparkleTexture");

        public void SetSparklyVFX(VisualEffect sparklyEffect)
        {
            _effect = sparklyEffect;
        }

        public void GetInitialProperties()
        {
            if (!_effect) return;

            rate = _effect.GetUInt(RateID);
            width = _effect.GetFloat(WidthID);
            alpha = _effect.GetFloat(AlphaID);
            sizeDecayCurve = _effect.GetAnimationCurve(SizeDecayCurveID);
            sizeMin = _effect.GetFloat(SizeMinID);
            sizeMax = _effect.GetFloat(SizeMaxID);
            lifeTimeMin = _effect.GetFloat(LifeTimeMinID);
            lifeTimeMax = _effect.GetFloat(LifeTimeMaxID);
            emissionIntensity = _effect.GetFloat(EmissionIntensityID);
            rotateDegree = _effect.GetFloat(RotateDegreeID);
            offset = _effect.GetFloat(OffsetID);
            useTexture = _effect.GetBool(UseTextureID);
            sparkleTexture = _effect.GetTexture(SparkleTextureID) as Texture2D;
        }

        public void SetProperties(Texture colorTexture, Texture positionMap, Texture normalMap, Texture uvMap)
        {
            if (!_effect) return;

            if (normalMap)
            {
                _effect.SetTexture("_PositionMap", positionMap);
                if (colorTexture != null) _effect.SetTexture("_ColorTexture", colorTexture);
                _effect.SetTexture("_NormalMap", normalMap);
                _effect.SetTexture("_UVMap", uvMap);
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
            _effect.SetFloat(RotateDegreeID, rotateDegree);
            _effect.SetFloat(OffsetID, offset);
            _effect.SetBool(UseTextureID, useTexture);
            _effect.SetTexture(SparkleTextureID, sparkleTexture);
        }
    }
}