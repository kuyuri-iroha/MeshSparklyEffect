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

        public float lowLimit
        {
            get => _lowLimit.value;
            set
            {
                _lowLimit.value = value;
                _slider.lowLimit = value;
            }
        }

        public float highLimit
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

        private Action<Vector2> _onValueChanged;

        public MinMaxSliderWithValue() : this(null)
        {
        }

        public MinMaxSliderWithValue(string label)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/MeshSparklyEffect/Editor/MinMaxSliderWithValue.uxml");
            _root = uxml.CloneTree();
            _root.name = "min-max-slider-with-value";

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
            _slider.RegisterValueChangedCallback(changeEvent => { _onValueChanged?.Invoke(changeEvent.newValue); });
            _minValue.RegisterValueChangedCallback(changeEvent =>
            {
                _onValueChanged?.Invoke(new Vector2(changeEvent.newValue, _maxValue.value));
            });
            _maxValue.RegisterValueChangedCallback(changeEvent =>
            {
                _onValueChanged?.Invoke(new Vector2(_minValue.value, changeEvent.newValue));
            });

            hierarchy.Add(_root);
        }

        public void Initialize(string newTitle, float newLowLimit, float newHighLimit, float newMinValue,
            float newMaxValue)
        {
            title = newTitle;
            lowLimit = newLowLimit;
            highLimit = newHighLimit;
            minValue = newMinValue;
            maxValue = newMaxValue;
        }

        public void RegisterValueChangedCallback(Action<Vector2> callback)
        {
            _onValueChanged += callback;
        }

        public void UnRegisterValueChangedCallback(Action<Vector2> callback)
        {
            _onValueChanged -= callback;
        }
    }
}