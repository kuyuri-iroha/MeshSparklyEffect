using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

internal class MinMaxSliderWithValue : VisualElement
{
    public new class UxmlFactory : UxmlFactory<MinMaxSliderWithValue, UxmlTraits>
    {
    }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlStringAttributeDescription titleText = new UxmlStringAttributeDescription
        {
            name = "title",
            defaultValue = "Label"
        };

        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }

        public override void Init(VisualElement visualElement, IUxmlAttributes bag, CreationContext creationContext)
        {
            base.Init(visualElement, bag, creationContext);
            var title = titleText.GetValueFromBag(bag, creationContext);
            (visualElement as MinMaxSliderWithValue).Initialize(title);
        }
    }

    public string title { get; set; }

    private MinMaxSlider _slider;
    private FloatField _lowLimit;
    private FloatField _highLimit;
    private FloatField _minValue;
    private FloatField _maxValue;

    public MinMaxSliderWithValue()
    {
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/MeshSparklyEffect/Editor/MinMaxSliderWithValue.uxml");
        VisualElement root = uxml.CloneTree();
        root.name = "min-max-slider-with-value";

        _slider = root.Q<MinMaxSlider>("min-max-slider");
        _lowLimit = root.Q<FloatField>("low-limit");
        _highLimit = root.Q<FloatField>("high-limit");
        _minValue = root.Q<FloatField>("min-value");
        _maxValue = root.Q<FloatField>("max-value");

        _slider.RegisterValueChangedCallback(changeEvent =>
        {
            _minValue.value = changeEvent.newValue.x;
            _maxValue.value = changeEvent.newValue.y;
        });

        _lowLimit.RegisterValueChangedCallback(changeEvent =>
        {
            var value = changeEvent.newValue < _slider.highLimit ? changeEvent.newValue : _slider.highLimit;
            _slider.lowLimit = value;
        });
        _highLimit.RegisterValueChangedCallback(changeEvent =>
        {
            var value = _slider.lowLimit < changeEvent.newValue ? changeEvent.newValue : _slider.lowLimit;
            _slider.highLimit = value;
        });

        _minValue.RegisterValueChangedCallback(changeEvent =>
        {
            var value = _slider.lowLimit < changeEvent.newValue
                ? changeEvent.newValue < _slider.maxValue
                    ? changeEvent.newValue
                    : _slider.maxValue
                : _slider.lowLimit;

            _slider.minValue = value;
            _minValue.value = value;
        });
        _maxValue.RegisterValueChangedCallback(changeEvent =>
        {
            var value = _slider.minValue < changeEvent.newValue
                ? changeEvent.newValue < _slider.highLimit
                    ? changeEvent.newValue
                    : _slider.highLimit
                : _slider.minValue;

            _slider.maxValue = value;
            _maxValue.value = value;
        });

        hierarchy.Add(root);
    }

    public void Initialize(string title)
    {
    }
}