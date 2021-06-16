using System;
using System.IO;
using UIToolkitExtensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MeshSparklyEffect
{
    [CustomEditor(typeof(SkinnedMeshSparklyEffect))]
    public class SkinnedMeshSparklyEffectInspector : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset skinnedMeshSparklyEffectInspectorUXML;

        private const string NullReferenceErrorMessage = " is missing.";

        private static readonly string NotReadableErrorMessage =
            " is not readable. Please make this asset readable in the import settings.";

        private const string UnknownErrorMessage = "Error: ";

        private const string ModeButtonTextOnProceduralMode = "Switch Texture Mode";
        private const string ModeButtonTextOnTextureMode = "Switch Procedural Mode";

        private const string UndoRecordName = "Changed SkinnedMeshSparklyEffect convert mode";

        private const float Margin = 10.0f;

        private VisualElement _root;
        private HelpBox _errorMessageBox;
        private VisualElement _parametersRoot;
        private VisualElement _sharedParameters;
        private Button _modeSwitchButton;
        private VisualElement _proceduralModeParameter;
        private VisualElement _textureModeParameter;
        private MinMaxSliderWithValue _sizeMinMaxProp;
        private MinMaxSliderWithValue _lifeTimeMinMaxProp;
        private Button _convertButton;

        public override VisualElement CreateInspectorGUI()
        {
            if (!skinnedMeshSparklyEffectInspectorUXML) return new VisualElement();

            var meshSparklyEffect = target as SkinnedMeshSparklyEffect;
            var sparkleVFX = meshSparklyEffect.sparkleVFX;

            _root = skinnedMeshSparklyEffectInspectorUXML.CloneTree();
            _root.name = "mesh-sparkly-effect";
            _root.Bind(serializedObject);

            // Error message box
            _errorMessageBox = new HelpBox("", HelpBoxMessageType.Error);
            _errorMessageBox.style.marginTop = Margin;
            _errorMessageBox.style.marginBottom = Margin;

            // Mesh parameters
            var targetMeshProp = _root.Q<ObjectField>("skinned-mesh-renderer");
            targetMeshProp.RegisterValueChangedCallback(_ => OnChangedTargetMesh());
            targetMeshProp.RegisterCallback<DragExitedEvent>(_ => OnChangedTargetMesh());

            // Parameters root
            _parametersRoot = _root.Q<VisualElement>("parameters-root");
            _sizeMinMaxProp = _root.Q<MinMaxSliderWithValue>("size-min-max");
            _sizeMinMaxProp.RegisterValueChangedCallback((range, limit) =>
            {
                Undo.RecordObject(meshSparklyEffect, "Changed MeshSparklyEffect Slider");

                sparkleVFX.sizeMin = range.x;
                sparkleVFX.sizeMax = range.y;
                sparkleVFX.sizeLowLimit = limit.x;
                sparkleVFX.sizeHighLimit = limit.y;
            });

            _lifeTimeMinMaxProp = _root.Q<MinMaxSliderWithValue>("life-time-min-max");
            _lifeTimeMinMaxProp.RegisterValueChangedCallback((range, limit) =>
            {
                Undo.RecordObject(meshSparklyEffect, UndoRecordName);

                sparkleVFX.lifeTimeMin = range.x;
                sparkleVFX.lifeTimeMax = range.y;
                sparkleVFX.lifeTimeLowLimit = limit.x;
                sparkleVFX.lifeTimeHighLimit = limit.y;
            });

            ApplyMinMaxValues();
            Undo.undoRedoPerformed += ApplyMinMaxValues;

            // Sparkle parameters
            _modeSwitchButton = _root.Q<Button>("sparkle-mode-button");
            _modeSwitchButton.clicked += OnClickedModeButton;
            _modeSwitchButton.text =
                sparkleVFX.useTexture ? ModeButtonTextOnTextureMode : ModeButtonTextOnProceduralMode;

            _proceduralModeParameter = _root.Q<VisualElement>("procedural-mode-parameters");
            _proceduralModeParameter.style.display =
                sparkleVFX.useTexture ? DisplayStyle.None : DisplayStyle.Flex;
            _textureModeParameter = _root.Q<VisualElement>("texture-mode-parameters");
            _textureModeParameter.style.display = sparkleVFX.useTexture ? DisplayStyle.Flex : DisplayStyle.None;

            return _root;
        }

        private void OnDestroy()
        {
            _lifeTimeMinMaxProp?.UnRegisterAllValueChangedCallback();
            Undo.undoRedoPerformed -= ApplyMinMaxValues;
        }

        private void ApplyMinMaxValues()
        {
            var sparkleVFX = (target as SkinnedMeshSparklyEffect).sparkleVFX;
            _sizeMinMaxProp?.ApplyMinMaxValue(sparkleVFX.sizeMin, sparkleVFX.sizeMax,
                sparkleVFX.sizeLowLimit, sparkleVFX.sizeHighLimit);
            _lifeTimeMinMaxProp?.ApplyMinMaxValue(sparkleVFX.lifeTimeMin, sparkleVFX.lifeTimeMax,
                sparkleVFX.lifeTimeLowLimit, sparkleVFX.lifeTimeHighLimit);
        }

        private void OnChangedTargetMesh()
        {
            RemoveWhenContains(_root, _errorMessageBox);

            var meshSparklyEffect = target as SkinnedMeshSparklyEffect;

            var message = "";
            var ableToRun = VerifySkinnedMeshRenderer(meshSparklyEffect.targetMesh, out message);

            if (ableToRun)
            {
                _parametersRoot.style.display = DisplayStyle.Flex;

                meshSparklyEffect.enabled = true;

                meshSparklyEffect.CreateMaps();
            }
            else
            {
                _parametersRoot.style.display = DisplayStyle.None;

                _errorMessageBox.text = message;
                _root.Insert(0, _errorMessageBox);

                meshSparklyEffect.enabled = false;
            }
        }

        private static bool VerifySkinnedMeshRenderer(SkinnedMeshRenderer renderer, out string errorMessage)
        {
            try
            {
                if (renderer == null)
                {
                    errorMessage = $"The SkinnedMeshRenderer {NullReferenceErrorMessage}";
                    return false;
                }

                if (renderer.sharedMesh.isReadable == false)
                {
                    errorMessage = $"The Mesh in SkinnedMeshRenderer{NotReadableErrorMessage}";
                    return false;
                }

                var readTest = renderer.sharedMesh.vertices;
                if (readTest == null)
                {
                    errorMessage = $"The vertices of Mesh in SkinnedMeshRenderer {NullReferenceErrorMessage}";
                }

                errorMessage = "";
            }
            catch (Exception e)
            {
                errorMessage = $"{UnknownErrorMessage} {e.Message}";
                return false;
            }

            return true;
        }

        private void OnClickedModeButton()
        {
            var sparkleVFX = (target as SkinnedMeshSparklyEffect).sparkleVFX;

            if (_modeSwitchButton.text.Equals(ModeButtonTextOnProceduralMode))
            {
                sparkleVFX.useTexture = true;
                _proceduralModeParameter.style.display = DisplayStyle.None;
                _textureModeParameter.style.display = DisplayStyle.Flex;
                _modeSwitchButton.text = ModeButtonTextOnTextureMode;
            }
            else
            {
                sparkleVFX.useTexture = false;
                _proceduralModeParameter.style.display = DisplayStyle.Flex;
                _textureModeParameter.style.display = DisplayStyle.None;
                _modeSwitchButton.text = ModeButtonTextOnProceduralMode;
            }
        }

        private static void RemoveWhenContains(VisualElement parent, VisualElement element)
        {
            if (parent.Contains(element))
            {
                parent.Remove(element);
            }
        }
    }
}