using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.AssetList
{
    public abstract class BaseTextFilter : IFilter
    {
        class Choice
        {
            public Radio Radio;
            public object Value;
        }

        readonly List<Choice> m_Choices = new();

        public string Label { get; set; }
        public bool IsUsed { get; private set; }

        protected BaseTextFilter(string label)
        {
            Label = label;
        }

        public void FillContent(VisualElement container, IEnumerable<IAsset> assets, AssetSearchFilter searchFilter)
        {
            m_Choices.Clear();
            var options = new List<object>();

            foreach (var asset in assets)
            {
                var filteredValue = GetAssetFilteredValue(asset);
                if (options.Contains(filteredValue))
                    continue;
                options.Add(filteredValue);
            }

            var radioGroup = new RadioGroup();
            container.Add(radioGroup);
            radioGroup.RegisterCallback<ChangeEvent<bool>>(OnItemChosen);

            foreach (var option in options)
            {
                var isSelected = searchFilter.AllCriteria.Any(c => !c.IsEmpty() && c.IsMatch(option));

                var radio = new Radio();
                radio.emphasized = true;
                radio.label = GetStringValue(option);
                radio.value = isSelected;
                m_Choices.Add(new Choice { Radio = radio, Value = option });
                radioGroup?.Add(radio);
            }
        }

        public void ApplyFilter(AssetSearchFilter searchFilter, Chip filterChip)
        {
            IsUsed = true;

            filterChip.label = Label;
            foreach (var choice in m_Choices)
            {
                if (!choice.Radio.value)
                    continue;

                filterChip.label = string.Empty;
                var label = filterChip.Q(Chip.labelUssClassName);
                label.Clear();
                label.style.flexDirection = FlexDirection.Row;
                label.Add(new Text(Label));
                label.Add(new Text(": "));
                label.Add(new Text(GetStringValue(choice.Value)));

                ApplySpecificFilter(searchFilter, choice.Value);
            }
        }

        public abstract void ClearFilter(AssetSearchFilter searchFilter);
        public event Action ItemChosen;

        protected abstract object GetAssetFilteredValue(IAsset asset);
        protected abstract void ApplySpecificFilter(AssetSearchFilter searchFilter, object value);
        protected abstract string GetStringValue(object value);

        void OnItemChosen(ChangeEvent<bool> evt)
        {
            ItemChosen?.Invoke();
        }
    }
}
