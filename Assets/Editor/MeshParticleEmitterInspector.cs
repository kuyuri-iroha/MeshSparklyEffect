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

        _parametersRoot = new VisualElement();
        _sharedParameters = CreateSharedParametersUI();
        _modeSwitchButton = new Button(OnClickedModeButton) {text = ModeButtonTextOnProceduralMode};
        _proceduralModeParameter = CreateProceduralModeUI();
        _textureModeParameter = CreateTextureModeUI();
        _textureModeParameter.style.display = DisplayStyle.None;

        _parametersRoot.Add(_sharedParameters);
        _parametersRoot.Add(_modeSwitchButton);
        _parametersRoot.Add(_proceduralModeParameter);
        _parametersRoot.Add(_textureModeParameter);

        var targetMeshProp = new PropertyField(serializedObject.FindProperty("targetMesh"), "Target Mesh");
        targetMeshProp.RegisterValueChangeCallback(OnChangedTargetMesh);

        _root.Add(targetMeshProp);
        _root.Add(_parametersRoot);

        /*

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/MeshParticleEmitterEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/MeshParticleEmitterEditor.uss");
        VisualElement labelWithStyle = new Label("Hello World! With Style");
        labelWithStyle.styleSheets.Add(styleSheet);
        root.Add(labelWithStyle);
        */

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
        }
        else
        {
            _parametersRoot.style.display = DisplayStyle.Flex;
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
        var sharedParameters = new VisualElement();

        var rateProp = new PropertyField(serializedObject.FindProperty("rate"), "Rate");
        var alphaProp = new PropertyField(serializedObject.FindProperty("alpha"), "Alpha");
        var sizeDecayCurveProp = new PropertyField(serializedObject.FindProperty("sizeDecayCurve"), "Size Decay Curve");
        var sizeMinProp = new PropertyField(serializedObject.FindProperty("sizeMin"), "Size Min");
        var sizeMaxProp = new PropertyField(serializedObject.FindProperty("sizeMax"), "Size Max");
        var lifeTimeMinProp = new PropertyField(serializedObject.FindProperty("lifeTimeMin"), "Life Time Min");
        var lifeTimeMaxProp = new PropertyField(serializedObject.FindProperty("lifeTimeMax"), "Life Time Max");
        var emissionIntensityProp =
            new PropertyField(serializedObject.FindProperty("emissionIntensity"), "Emission Intensity");
        var rotateDegreeMinProp =
            new PropertyField(serializedObject.FindProperty("rotateDegreeMin"), "Rotate Degree Min");
        var rotateDegreeMaxProp =
            new PropertyField(serializedObject.FindProperty("rotateDegreeMax"), "Rotate Degree Max");
        var offsetProp = new PropertyField(serializedObject.FindProperty("offset"), "Offset");

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

        return sharedParameters;
    }

    private VisualElement CreateTextureModeUI()
    {
        var textureModeParameters = new VisualElement();

        var sparkleTextureProp = new PropertyField(serializedObject.FindProperty("sparkleTexture"), "Sparkle Texture");

        textureModeParameters.Add(sparkleTextureProp);

        return textureModeParameters;
    }

    private VisualElement CreateProceduralModeUI()
    {
        var proceduralModeParameters = new VisualElement();

        var widthProp = new PropertyField(serializedObject.FindProperty("width"), "Spike Width");

        proceduralModeParameters.Add(widthProp);

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