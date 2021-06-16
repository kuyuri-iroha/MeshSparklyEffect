using System;
using System.IO;
using UIToolkitExtensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MeshSparklyEffect
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MeshSparklyEffect))]
    public class MeshSparklyEffectInspector : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset meshSparklyEffectInspectorUXML;

        private const string NullReferenceErrorMessage = " is missing.";

        private static readonly string NotReadableErrorMessage =
            " is not readable. Please make this asset readable in the import settings.";

        private const string UnknownErrorMessage = "Error: ";

        private const string ModeButtonTextOnProceduralMode = "Switch Texture Mode";
        private const string ModeButtonTextOnTextureMode = "Switch Procedural Mode";

        private const string ModeButtonTextOnMeshFilterMode = "Switch SkinnedMeshRenderer Mode";
        private const string ModeButtonTextOnSkinnedMeshRendererMode = "Switch MeshFilter Mode";

        private const string ConvertButtonTextOnMapMode = "Convert to Mesh";
        private const string ConvertButtonTextOnMeshMode = "Convert to Map";

        private const string UndoRecordName = "Changed MeshSparklyEffect convert mode";

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
            var meshSparklyEffect = target as MeshSparklyEffect;
            var sparkleVFX = meshSparklyEffect.sparkleVFX;

            _root = Resources.Load<VisualTreeAsset>("MeshSparklyEffectInspector").CloneTree();
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
            targetMeshProp.style.display = meshSparklyEffect.useMeshFilter ? DisplayStyle.None : DisplayStyle.Flex;

            var targetMeshFilterProp = _root.Q<ObjectField>("mesh-filter");
            targetMeshFilterProp.RegisterValueChangedCallback(_ => OnChangedTargetMesh());
            targetMeshFilterProp.RegisterCallback<DragExitedEvent>(_ => OnChangedTargetMesh());
            targetMeshFilterProp.style.display =
                meshSparklyEffect.useMeshFilter ? DisplayStyle.Flex : DisplayStyle.None;

            var modeButton = _root.Q<Button>("mesh-mode-button");
            modeButton.text = meshSparklyEffect.useMeshFilter
                ? ModeButtonTextOnMeshFilterMode
                : ModeButtonTextOnSkinnedMeshRendererMode;
            modeButton.RegisterCallback<ClickEvent>(_ =>
            {
                if (modeButton.text.Equals(ModeButtonTextOnSkinnedMeshRendererMode))
                {
                    meshSparklyEffect.useMeshFilter = true;
                    OnChangedTargetMesh();
                    targetMeshProp.style.display = DisplayStyle.None;
                    targetMeshFilterProp.style.display = DisplayStyle.Flex;
                    modeButton.text = ModeButtonTextOnMeshFilterMode;
                }
                else
                {
                    meshSparklyEffect.useMeshFilter = false;
                    OnChangedTargetMesh();
                    targetMeshProp.style.display = DisplayStyle.Flex;
                    targetMeshFilterProp.style.display = DisplayStyle.None;
                    modeButton.text = ModeButtonTextOnSkinnedMeshRendererMode;
                }
            });

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

            // Convert button
            _convertButton = _root.Q<Button>("convert-button");
            _convertButton.RegisterCallback<ClickEvent>(_ =>
            {
                Undo.RecordObject(meshSparklyEffect, UndoRecordName);

                meshSparklyEffect.isMapMode = !meshSparklyEffect.isMapMode;
                ApplyConvertMode();
            });
            ApplyConvertMode();
            Undo.undoRedoPerformed += ApplyConvertMode;

            // Bake button
            var bakeButton = _root.Q<Button>("bake-button");
            bakeButton.RegisterCallback<ClickEvent>(_ =>
            {
                RemoveWhenContains(_root, _errorMessageBox);

                var path = EditorUtility.SaveFolderPanel("Save texture as EXR", "", "");
                if (path.Length != 0)
                {
                    var meshName = meshSparklyEffect.useMeshFilter
                        ? meshSparklyEffect.targetMeshFilter.name
                        : meshSparklyEffect.targetMesh.name;

                    var bytes = meshSparklyEffect.positionMap.EncodeToEXR();
                    File.WriteAllBytes($"{path}/{meshName}_PositionMap.exr", bytes);
                    bytes = meshSparklyEffect.normalMap.EncodeToEXR();
                    File.WriteAllBytes($"{path}/{meshName}_NormalMap.exr", bytes);
                    bytes = meshSparklyEffect.uvMap.EncodeToEXR();
                    File.WriteAllBytes($"{path}/{meshName}_UVMap.exr", bytes);

                    EditorUtility.DisplayDialog("Bake succeeded", $"Save to {path}", "OK");
                }

                AssetDatabase.Refresh();
            });

            return _root;
        }

        private void OnDestroy()
        {
            _lifeTimeMinMaxProp?.UnRegisterAllValueChangedCallback();
            Undo.undoRedoPerformed -= ApplyMinMaxValues;
            Undo.undoRedoPerformed -= ApplyConvertMode;
        }

        private void ApplyMinMaxValues()
        {
            var sparkleVFX = (target as MeshSparklyEffect).sparkleVFX;
            _sizeMinMaxProp?.ApplyMinMaxValue(sparkleVFX.sizeMin, sparkleVFX.sizeMax,
                sparkleVFX.sizeLowLimit, sparkleVFX.sizeHighLimit);
            _lifeTimeMinMaxProp?.ApplyMinMaxValue(sparkleVFX.lifeTimeMin, sparkleVFX.lifeTimeMax,
                sparkleVFX.lifeTimeLowLimit, sparkleVFX.lifeTimeHighLimit);
        }

        private void ApplyConvertMode()
        {
            var meshSparklyEffect = target as MeshSparklyEffect;
            var meshParameters = _root?.Q<VisualElement>("mesh-parameters");
            var convertedParameters = _root?.Q<VisualElement>("converted-parameters");

            if (meshSparklyEffect.isMapMode)
            {
                convertedParameters.style.display = DisplayStyle.Flex;
                meshParameters.style.display = DisplayStyle.None;
                _convertButton.text = ConvertButtonTextOnMapMode;
            }
            else
            {
                convertedParameters.style.display = DisplayStyle.None;
                meshParameters.style.display = DisplayStyle.Flex;
                _convertButton.text = ConvertButtonTextOnMeshMode;
            }
        }

        private void OnChangedTargetMesh()
        {
            RemoveWhenContains(_root, _errorMessageBox);

            var meshParticleEmitter = target as MeshSparklyEffect;

            var message = "";
            var ableToRun = meshParticleEmitter.useMeshFilter
                ? VerifyMeshFilterRenderer(meshParticleEmitter.targetMeshFilter, out message)
                : VerifySkinnedMeshRenderer(meshParticleEmitter.targetMesh, out message);

            if (ableToRun)
            {
                _parametersRoot.style.display = DisplayStyle.Flex;

                meshParticleEmitter.enabled = true;

                meshParticleEmitter.CreateMaps();
            }
            else
            {
                _parametersRoot.style.display = DisplayStyle.None;

                _errorMessageBox.text = message;
                _root.Insert(0, _errorMessageBox);

                meshParticleEmitter.enabled = false;
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

        private static bool VerifyMeshFilterRenderer(MeshFilter meshFilter, out string errorMessage)
        {
            try
            {
                if (meshFilter == null)
                {
                    errorMessage = $"The MeshFilter {NullReferenceErrorMessage}";
                    return false;
                }

                if (meshFilter.sharedMesh.isReadable == false)
                {
                    errorMessage = $"The Mesh in MeshFilter {NotReadableErrorMessage}";
                    return false;
                }

                var readTest = meshFilter.sharedMesh.vertices;
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
            var sparkleVFX = (target as MeshSparklyEffect).sparkleVFX;

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
#endif
}