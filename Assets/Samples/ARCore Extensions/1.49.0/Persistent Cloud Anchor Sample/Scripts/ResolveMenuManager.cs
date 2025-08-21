//-----------------------------------------------------------------------
// <copyright file="ResolveMenuManager.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    /// <summary>
    /// A manager component that helps to populate and handle the options of resolving anchors,
    /// allowing manual entry and dropdown selection simultaneously.
    /// </summary>
    public class ResolveMenuManager : MonoBehaviour
    {
        /// <summary>
        /// The main controller for Persistent Cloud Anchors sample.
        /// </summary>
        public PersistentCloudAnchorsController Controller;

        /// <summary>
        /// A multiselection dropdown component that contains all available resolving options.
        /// </summary>
        public MultiselectionDropdown Multiselection;

        /// <summary>
        /// An input field for manually typing Cloud Anchor Id(s), separated by commas.
        /// </summary>
        public InputField InputField;

        /// <summary>
        /// The warning text that appears when invalid characters are filled in.
        /// </summary>
        public GameObject InvalidInputWarning;

        /// <summary>
        /// The resolve button which leads to AR view screen.
        /// </summary>
        public Button ResolveButton;

        /// <summary>
        /// Cached Cloud Anchor history data used to fetch the Cloud Anchor Id using
        /// the index given by multi-selection dropdown.
        /// </summary>
        private CloudAnchorHistoryCollection _history;

        /// <summary>
        /// Cached active color for interactable buttons.
        /// </summary>
        private Color _activeColor;

        private void Awake()
        {
            _activeColor = ResolveButton.GetComponent<Image>().color;
        }

        private void OnEnable()
        {
            // Initialize UI state
            InvalidInputWarning.SetActive(false);
            InputField.text = string.Empty;
            SetButtonActive(ResolveButton, false);

            // Load history and populate dropdown
            _history = Controller.LoadCloudAnchorHistory();
            var options = new List<MultiselectionDropdown.OptionData>();
            foreach (var data in _history.Collection)
            {
                options.Add(new MultiselectionDropdown.OptionData(
                    data.Name, FormatDateTime(data.CreatedTime)));
            }
            Multiselection.Options = options;

            // Attach callbacks
            InputField.onValueChanged.AddListener(OnInputFieldValueChanged);
            InputField.onEndEdit.AddListener(OnInputFieldEndEdit);
            Multiselection.OnValueChanged += OnResolvingSelectionChanged;
        }

        private void OnDisable()
        {
            // Detach callbacks
            InputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
            InputField.onEndEdit.RemoveListener(OnInputFieldEndEdit);
            Multiselection.OnValueChanged -= OnResolvingSelectionChanged;

            // Clear dropdown and history
            Multiselection.Deselect();
            Multiselection.Options.Clear();
            _history.Collection.Clear();
        }

        /// <summary>
        /// Callback handling the validation of the input field.
        /// </summary>
        public void OnInputFieldValueChanged(string inputString)
        {
            var regex = new Regex("^[a-zA-Z0-9-_,]*$");
            InvalidInputWarning.SetActive(!regex.IsMatch(inputString));
            RefreshResolveButton();
        }

        /// <summary>
        /// Callback handling the end edit event of the input field.
        /// </summary>
        public void OnInputFieldEndEdit(string inputString)
        {
            if (!InvalidInputWarning.activeSelf)
            {
                RefreshResolveButton();
            }
        }

        /// <summary>
        /// Callback handling the selection values changed in multiselection dropdown.
        /// </summary>
        public void OnResolvingSelectionChanged()
        {
            RefreshResolveButton();
        }

        /// <summary>
        /// Updates the resolving set and toggles the resolve button state.
        /// Enabled if either dropdown has selections or input field has valid entries.
        /// </summary>
        private void RefreshResolveButton()
        {
            Controller.ResolvingSet.Clear();

            // Add from dropdown selections
            var selected = Multiselection.SelectedValues;
            foreach (var idx in selected)
            {
                Controller.ResolvingSet.Add(_history.Collection[idx].Id);
            }

            // Add from manual input
            if (!InvalidInputWarning.activeSelf)
            {
                var text = InputField.text.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    var parts = text.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var id in parts)
                    {
                        Controller.ResolvingSet.Add(id.Trim());
                    }
                }
            }

            SetButtonActive(ResolveButton, Controller.ResolvingSet.Count > 0);
        }

        private string FormatDateTime(DateTime time)
        {
            var span = DateTime.Now.Subtract(time);
            if (span.Hours == 0)
            {
                return span.Minutes == 0 ? "Just now" : string.Format("{0}m ago", span.Minutes);
            }
            return string.Format("{0}h ago", span.Hours);
        }

        private void SetButtonActive(Button button, bool active)
        {
            var img = button.GetComponent<Image>();
            img.color = active ? _activeColor : Color.grey;
            button.enabled = active;
        }
    }
}
