using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(MeshSparklyEffect))]
public class MeshSparklyEffectInspector : Editor
{
    private const string NullReferenceErrorMessage = " is missing.";

    private static readonly string NotReadableErrorMessage =
        " is not readable. Please make this asset readable in the import settings.";

    private const string UnknownErrorMessage = "Error: ";

    private const string ModeButtonTextOnProceduralMode = "Switch Texture Mode";
    private const string ModeButtonTextOnTextureMode = "Switch Procedural Mode";

    private const string ModeButtonTextOnMeshFilterMode = "Switch SkinnedMeshRenderer Mode";
    private const string ModeButtonTextOnSkinnedMeshRendererMode = "Switch MeshFilter Mode";

    private const float Margin = 10.0f;

    private VisualElement _root;
    private HelpBox _errorMessageBox;
    private VisualElement _parametersRoot;
    private VisualElement _sharedParameters;
    private Button _modeSwitchButton;
    private VisualElement _proceduralModeParameter;
    private VisualElement _textureModeParameter;

    public override VisualElement CreateInspectorGUI()
    {
        var meshParticleEmitter = target as MeshSparklyEffect;

        _root = new VisualElement();
        _root.Bind(serializedObject);

        _errorMessageBox = new HelpBox("", HelpBoxMessageType.Error);
        _errorMessageBox.style.marginTop = Margin;
        _errorMessageBox.style.marginBottom = Margin;

        var meshParameters = CreateMeshParametersUI();

        _parametersRoot = new VisualElement();
        _sharedParameters = CreateSharedParametersUI();
        _modeSwitchButton = new Button(OnClickedModeButton)
            {text = meshParticleEmitter.useTexture ? ModeButtonTextOnTextureMode : ModeButtonTextOnProceduralMode};
        _modeSwitchButton.style.marginTop = Margin;
        _modeSwitchButton.style.marginBottom = Margin;
        _proceduralModeParameter = CreateProceduralModeUI();
        _proceduralModeParameter.style.display = meshParticleEmitter.useTexture ? DisplayStyle.None : DisplayStyle.Flex;
        _textureModeParameter = CreateTextureModeUI();
        _textureModeParameter.style.display = meshParticleEmitter.useTexture ? DisplayStyle.Flex : DisplayStyle.None;

        _parametersRoot.Add(_sharedParameters);
        _parametersRoot.Add(_modeSwitchButton);
        _parametersRoot.Add(_proceduralModeParameter);
        _parametersRoot.Add(_textureModeParameter);

        _root.Add(meshParameters);
        _root.Add(_parametersRoot);

        return _root;
    }

    private VisualElement CreateMeshParametersUI()
    {
        var meshParticleEmitter = target as MeshSparklyEffect;

        var meshParameters = new VisualElement();

        var targetMeshProp =
            new PropertyField(serializedObject.FindProperty("targetMesh"), "Target SkinnedMeshRenderer");
        targetMeshProp.RegisterValueChangeCallback(OnChangedTargetMesh);
        targetMeshProp.style.marginTop = new StyleLength(Margin);
        targetMeshProp.style.marginBottom = new StyleLength(Margin);
        targetMeshProp.style.display = meshParticleEmitter.useMeshFilter ? DisplayStyle.None : DisplayStyle.Flex;

        var targetMeshFilterProp =
            new PropertyField(serializedObject.FindProperty("targetMeshFilter"), "Target MeshFilter");
        targetMeshFilterProp.RegisterValueChangeCallback(OnChangedTargetMesh);
        targetMeshFilterProp.style.marginTop = new StyleLength(Margin);
        targetMeshFilterProp.style.marginBottom = new StyleLength(Margin);
        targetMeshFilterProp.style.display = meshParticleEmitter.useMeshFilter ? DisplayStyle.Flex : DisplayStyle.None;

        var modeButton = new Button
        {
            text = meshParticleEmitter.useMeshFilter
                ? ModeButtonTextOnMeshFilterMode
                : ModeButtonTextOnSkinnedMeshRendererMode
        };
        modeButton.RegisterCallback<ClickEvent>(_ =>
        {
            if (modeButton.text.Equals(ModeButtonTextOnSkinnedMeshRendererMode))
            {
                meshParticleEmitter.useMeshFilter = true;
                OnChangedTargetMesh(null);
                targetMeshProp.style.display = DisplayStyle.None;
                targetMeshFilterProp.style.display = DisplayStyle.Flex;
                modeButton.text = ModeButtonTextOnMeshFilterMode;
            }
            else
            {
                meshParticleEmitter.useMeshFilter = false;
                OnChangedTargetMesh(null);
                targetMeshProp.style.display = DisplayStyle.Flex;
                targetMeshFilterProp.style.display = DisplayStyle.None;
                modeButton.text = ModeButtonTextOnSkinnedMeshRendererMode;
            }
        });

        meshParameters.Add(modeButton);
        meshParameters.Add(targetMeshProp);
        meshParameters.Add(targetMeshFilterProp);

        return meshParameters;
    }

    private void OnChangedTargetMesh(SerializedPropertyChangeEvent changeEvent)
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

    private VisualElement CreateSharedParametersUI()
    {
        var meshParticleEmitter = target as MeshSparklyEffect;

        var sharedParameters = new VisualElement();

        var colorTextureProp = new PropertyField(serializedObject.FindProperty("colorTexture"), "Color Texture");
        var rateProp = new PropertyField(serializedObject.FindProperty("rate"), "Rate");
        var alphaProp = new PropertyField(serializedObject.FindProperty("alpha"), "Alpha");
        var sizeDecayCurveProp = new PropertyField(serializedObject.FindProperty("sizeDecayCurve"), "Size Decay Curve");
        var sizeMinProp = new PropertyField(serializedObject.FindProperty("sizeMin"), "Size Min");
        var sizeMaxProp = new PropertyField(serializedObject.FindProperty("sizeMax"), "Size Max");
        sizeMinProp.RegisterValueChangeCallback(_ =>
        {
            meshParticleEmitter.sizeMin =
                Mathf.Clamp(meshParticleEmitter.sizeMin, 0.0f, meshParticleEmitter.sizeMax);
        });
        sizeMaxProp.RegisterValueChangeCallback(_ =>
        {
            meshParticleEmitter.sizeMax = Mathf.Max(meshParticleEmitter.sizeMax, meshParticleEmitter.sizeMin);
        });
        var lifeTimeMinProp = new PropertyField(serializedObject.FindProperty("lifeTimeMin"), "Life Time Min");
        var lifeTimeMaxProp = new PropertyField(serializedObject.FindProperty("lifeTimeMax"), "Life Time Max");
        lifeTimeMinProp.RegisterValueChangeCallback(_ =>
        {
            meshParticleEmitter.lifeTimeMin =
                Mathf.Clamp(meshParticleEmitter.lifeTimeMin, 0.0f, meshParticleEmitter.lifeTimeMax);
        });
        lifeTimeMaxProp.RegisterValueChangeCallback(_ =>
        {
            meshParticleEmitter.lifeTimeMax =
                Mathf.Max(meshParticleEmitter.lifeTimeMax, meshParticleEmitter.lifeTimeMin);
        });
        var emissionIntensityProp =
            new PropertyField(serializedObject.FindProperty("emissionIntensity"), "Emission Intensity");
        var rotateDegreeProp =
            new PropertyField(serializedObject.FindProperty("rotateDegree"), "Rotate Degree");
        var offsetProp = new PropertyField(serializedObject.FindProperty("offset"), "Offset");

        sharedParameters.Add(colorTextureProp);
        sharedParameters.Add(rateProp);
        sharedParameters.Add(alphaProp);
        sharedParameters.Add(sizeDecayCurveProp);
        sharedParameters.Add(sizeMinProp);
        sharedParameters.Add(sizeMaxProp);
        sharedParameters.Add(lifeTimeMinProp);
        sharedParameters.Add(lifeTimeMaxProp);
        sharedParameters.Add(emissionIntensityProp);
        sharedParameters.Add(rotateDegreeProp);
        sharedParameters.Add(offsetProp);

        sharedParameters.style.marginTop = Margin;
        sharedParameters.style.marginBottom = Margin;

        return sharedParameters;
    }

    private VisualElement CreateTextureModeUI()
    {
        var textureModeParameters = new VisualElement();

        var sparkleTextureProp = new PropertyField(serializedObject.FindProperty("sparkleTexture"), "Sparkle Texture");

        textureModeParameters.Add(sparkleTextureProp);

        textureModeParameters.style.marginTop = Margin * 0.5f;

        return textureModeParameters;
    }

    private VisualElement CreateProceduralModeUI()
    {
        var proceduralModeParameters = new VisualElement();

        var widthProp = new PropertyField(serializedObject.FindProperty("width"), "Spike Width");

        proceduralModeParameters.Add(widthProp);

        proceduralModeParameters.style.marginTop = Margin * 0.5f;

        return proceduralModeParameters;
    }

    private void OnClickedModeButton()
    {
        var meshParticleEmitter = target as MeshSparklyEffect;

        if (_modeSwitchButton.text.Equals(ModeButtonTextOnProceduralMode))
        {
            meshParticleEmitter.useTexture = true;
            _proceduralModeParameter.style.display = DisplayStyle.None;
            _textureModeParameter.style.display = DisplayStyle.Flex;
            _modeSwitchButton.text = ModeButtonTextOnTextureMode;
        }
        else
        {
            meshParticleEmitter.useTexture = false;
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