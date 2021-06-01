using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(MeshParticleEmitter))]
public class MeshParticleEmitterInspector : Editor
{
    private const string NullReferenceErrorMessage = " is missing.";

    private static readonly string NotReadableErrorMessage =
        " is not readable. Please make this asset readable in the import settings.";

    private const string UnknownErrorMessage = "Error: ";

    private const string ModeButtonTextOnProceduralMode = "Switch Texture Mode";
    private const string ModeButtonTextOnTextureMode = "Switch Procedural Mode";

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
        _root = new VisualElement();
        _root.Bind(serializedObject);

        _errorMessageBox = new HelpBox("", HelpBoxMessageType.Error);
        _errorMessageBox.style.marginTop = Margin;
        _errorMessageBox.style.marginBottom = Margin;

        _parametersRoot = new VisualElement();
        _sharedParameters = CreateSharedParametersUI();
        _modeSwitchButton = new Button(OnClickedModeButton) {text = ModeButtonTextOnProceduralMode};
        _modeSwitchButton.style.marginTop = Margin;
        _modeSwitchButton.style.marginBottom = Margin;
        _proceduralModeParameter = CreateProceduralModeUI();
        _textureModeParameter = CreateTextureModeUI();
        _textureModeParameter.style.display = DisplayStyle.None;

        _parametersRoot.Add(_sharedParameters);
        _parametersRoot.Add(_modeSwitchButton);
        _parametersRoot.Add(_proceduralModeParameter);
        _parametersRoot.Add(_textureModeParameter);

        var targetMeshProp = new PropertyField(serializedObject.FindProperty("targetMesh"), "Target Mesh");
        targetMeshProp.RegisterValueChangeCallback(OnChangedTargetMesh);
        targetMeshProp.style.marginTop = new StyleLength(Margin);
        targetMeshProp.style.marginBottom = new StyleLength(Margin);

        _root.Add(targetMeshProp);
        _root.Add(_parametersRoot);

        return _root;
    }

    private void OnChangedTargetMesh(SerializedPropertyChangeEvent changeEvent)
    {
        RemoveWhenContains(_root, _errorMessageBox);

        if (!VerifySkinnedMeshRenderer((target as MeshParticleEmitter).targetMesh, out var message))
        {
            _parametersRoot.style.display = DisplayStyle.None;

            _errorMessageBox.text = message;
            _root.Insert(0, _errorMessageBox);

            (target as MeshParticleEmitter).enabled = false;
        }
        else
        {
            _parametersRoot.style.display = DisplayStyle.Flex;

            (target as MeshParticleEmitter).enabled = true;
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

    private VisualElement CreateSharedParametersUI()
    {
        var meshParticleEmitter = target as MeshParticleEmitter;

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
        var rotateDegreeMinProp =
            new PropertyField(serializedObject.FindProperty("rotateDegreeMin"), "Rotate Degree Min");
        var rotateDegreeMaxProp =
            new PropertyField(serializedObject.FindProperty("rotateDegreeMax"), "Rotate Degree Max");
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
        sharedParameters.Add(rotateDegreeMinProp);
        sharedParameters.Add(rotateDegreeMaxProp);
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
        var meshParticleEmitter = target as MeshParticleEmitter;

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