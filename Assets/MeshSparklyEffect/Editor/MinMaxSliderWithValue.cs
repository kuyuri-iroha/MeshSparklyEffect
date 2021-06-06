using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkitExtensions
{
    public class MinMaxSliderWithValue : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<MinMaxSliderWithValue, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription titleText = new UxmlStringAttributeDescription
            {
                name = "title",
                defaultValue = "Label"
            };

            private UxmlFloatAttributeDescription lowLimitAttr = new UxmlFloatAttributeDescription
            {
                name = "low-limit",
                defaultValue = 0.0f
            };

            private UxmlFloatAttributeDescription highLimitAttr = new UxmlFloatAttributeDescription
            {
                name = "high-limit",
                defaultValue = 100.0f
            };

            private UxmlFloatAttributeDescription minValueAttr = new UxmlFloatAttributeDescription
            {
                name = "min-value",
                defaultValue = 25.0f
            };

            private UxmlFloatAttributeDescription maxValueAttr = new UxmlFloatAttributeDescription
            {
                name = "max-value",
                defaultValue = 75.0f
            };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement visualElement, IUxmlAttributes bag, CreationContext creationContext)
            {
                base.Init(visualElement, bag, creationContext);

                var newTitle = titleText.GetValueFromBag(bag, creationContext);
                var newLowLimit = lowLimitAttr.GetValueFromBag(bag, creationContext);
                var newHighLimit = highLimitAttr.GetValueFromBag(bag, creationContext);
                var newMinValue = minValueAttr.GetValueFromBag(bag, creationContext);
                var newMaxValue = maxValueAttr.GetValueFromBag(bag, creationContext);

                (visualElement as MinMaxSliderWithValue).Initialize(newTitle, newLowLimit, newHighLimit, newMinValue,
                    newMaxValue);
            }
        }

        public string title
        {
            get => _title.text;
            set => _title.text = value;
        }

        public float lowLimitValue
        {
            get => _lowLimit.value;
            set
            {
                _lowLimit.value = value;
                _slider.lowLimit = value;
            }
        }

        public float highLimitValue
        {
            get => _highLimit.value;
            set
            {
                _highLimit.value = value;
                _slider.highLimit = value;
            }
        }

        public float minValue
        {
            get => _minValue.value;
            set
            {
                _minValue.value = value;
                _slider.minValue = value;
            }
        }

        public float maxValue
        {
            get => _maxValue.value;
            set
            {
                _maxValue.value = value;
                _slider.maxValue = value;
            }
        }

        private VisualElement _root;
        private Label _title;
        private MinMaxSlider _slider;
        private FloatField _lowLimit;
        private FloatField _highLimit;
        private FloatField _minValue;
        private FloatField _maxValue;

        private Action<Vector2, Vector2> _onValueChanged;

        public MinMaxSliderWithValue() : this(null)
        {
        }

        public MinMaxSliderWithValue(string label)
        {
            _root = CreateStructure();

            _title = _root.Q<Label>("title");
            _slider = _root.Q<MinMaxSlider>("min-max-slider");
            _lowLimit = _root.Q<FloatField>("low-limit");
            _highLimit = _root.Q<FloatField>("high-limit");
            _minValue = _root.Q<FloatField>("min-value");
            _maxValue = _root.Q<FloatField>("max-value");

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

            // ユーザースクリプト側のイベント登録
            _slider.RegisterValueChangedCallback(changeEvent =>
            {
                _onValueChanged?.Invoke(changeEvent.newValue, new Vector2(_lowLimit.value, _highLimit.value));
            });
            _minValue.RegisterValueChangedCallback(changeEvent =>
            {
                _onValueChanged?.Invoke(new Vector2(changeEvent.newValue, _maxValue.value),
                    new Vector2(_lowLimit.value, _highLimit.value));
            });
            _maxValue.RegisterValueChangedCallback(changeEvent =>
            {
                _onValueChanged?.Invoke(new Vector2(_minValue.value, changeEvent.newValue),
                    new Vector2(_lowLimit.value, _highLimit.value));
            });
            _lowLimit.RegisterValueChangedCallback(changeEvent =>
            {
                _onValueChanged?.Invoke(new Vector2(_minValue.value, _maxValue.value),
                    new Vector2(changeEvent.newValue, _highLimit.value));
            });
            _highLimit.RegisterValueChangedCallback(changeEvent =>
            {
                _onValueChanged?.Invoke(new Vector2(_minValue.value, _maxValue.value),
                    new Vector2(_lowLimit.value, changeEvent.newValue));
            });

            hierarchy.Add(_root);
        }

        private VisualElement CreateStructure()
        {
            const float initialMin = 25;
            const float initialMax = 75;
            const float initialLowLimit = 0;
            const float initialHighLimit = 100;

            var root = new VisualElement {name = "min-max-slider-with-value"};

            var titleArea = new VisualElement {name = "title-area"};
            titleArea.style.width = Length.Percent(100.0f);
            root.Add(titleArea);

            var title = new Label {name = "title", text = "Label", displayTooltipWhenElided = true};
            title.style.fontSize = 14;
            titleArea.Add(title);

            var limitArea = new VisualElement {name = "limit-area"};
            limitArea.style.flexDirection = FlexDirection.Row;
            limitArea.style.justifyContent = Justify.SpaceBetween;
            limitArea.style.width = Length.Percent(100.0f);
            root.Add(limitArea);

            var lowLimitArea = new VisualElement {name = "low-limit-area"};
            lowLimitArea.style.flexDirection = FlexDirection.Row;
            lowLimitArea.style.alignItems = Align.Stretch;
            lowLimitArea.style.unityTextAlign = TextAnchor.MiddleLeft;
            limitArea.Add(lowLimitArea);

            var lowLimitLabel = new Label
                {name = "low-limit-label", text = "Low Limit", displayTooltipWhenElided = true};
            lowLimitArea.Add(lowLimitLabel);

            var lowLimitFloatField = new FloatField {name = "low-limit", value = initialLowLimit};
            lowLimitArea.Add(lowLimitFloatField);

            var highLimitArea = new VisualElement {name = "high-limit-area"};
            highLimitArea.style.flexDirection = FlexDirection.Row;
            highLimitArea.style.alignItems = Align.Stretch;
            highLimitArea.style.unityTextAlign = TextAnchor.MiddleLeft;
            limitArea.Add(highLimitArea);

            var highLimitLabel = new Label
                {name = "high-limit-label", text = "High Limit", displayTooltipWhenElided = true};
            highLimitArea.Add(highLimitLabel);

            var highLimitFloatField = new FloatField {name = "high-limit", value = initialHighLimit};
            highLimitArea.Add(highLimitFloatField);

            var minMaxSlider = new MinMaxSlider
                {name = "min-max-slider", minValue = initialMin, maxValue = initialMax, lowLimit = 0, highLimit = 100};
            root.Add(minMaxSlider);

            var valueArea = new VisualElement {name = "value-area"};
            valueArea.style.flexDirection = FlexDirection.Row;
            valueArea.style.justifyContent = Justify.SpaceBetween;
            valueArea.style.width = Length.Percent(100.0f);
            root.Add(valueArea);

            var minValueArea = new VisualElement {name = "min-value-area"};
            minValueArea.style.flexDirection = FlexDirection.Row;
            minValueArea.style.alignItems = Align.Stretch;
            minValueArea.style.unityTextAlign = TextAnchor.MiddleLeft;
            valueArea.Add(minValueArea);

            var minValueLabel = new Label {name = "min-value-label", text = "Min", displayTooltipWhenElided = true};
            minValueArea.Add(minValueLabel);

            var minValueFloatField = new FloatField {name = "min-value", value = initialMin};
            minValueArea.Add(minValueFloatField);

            var maxValueArea = new VisualElement {name = "max-value-area"};
            maxValueArea.style.flexDirection = FlexDirection.Row;
            maxValueArea.style.alignItems = Align.Stretch;
            maxValueArea.style.unityTextAlign = TextAnchor.MiddleLeft;
            valueArea.Add(maxValueArea);

            var maxValueLabel = new Label {name = "max-value-label", text = "Max", displayTooltipWhenElided = true};
            maxValueArea.Add(maxValueLabel);

            var maxValueFloatField = new FloatField {name = "max-value", value = initialMax};
            maxValueArea.Add(maxValueFloatField);

            return root;
        }

        public void Initialize(string newTitle, float newLowLimit, float newHighLimit, float newMinValue,
            float newMaxValue)
        {
            title = newTitle;
            lowLimitValue = newLowLimit;
            highLimitValue = newHighLimit;
            minValue = newMinValue;
            maxValue = newMaxValue;
        }

        public void RegisterValueChangedCallback(Action<Vector2, Vector2> callback)
        {
            _onValueChanged += callback;
        }

        public void UnRegisterAllValueChangedCallback()
        {
            _onValueChanged = null;
        }

        public void ApplyMinMaxValue(float min, float max, float lowLimit, float highLimit)
        {
            _minValue.value = min;
            _maxValue.value = max;
            _slider.minValue = min;
            _slider.maxValue = max;
            _lowLimit.value = lowLimit;
            _highLimit.value = highLimit;
            _slider.lowLimit = lowLimit;
            _slider.highLimit = highLimit;
        }
    }
}