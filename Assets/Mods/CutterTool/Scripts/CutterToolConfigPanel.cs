//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Timberborn.CoreUI;
//using Timberborn.InputSystem;
//using Timberborn.UILayoutSystem;
//using UnityEngine.UIElements;

//namespace Mods.CutterTool.Scripts
//{
//    public class CutterToolConfigPanel : IPanelController
//    {

//        public static readonly string ShortcutKey = "Cordial.TreeTool.KeyBinding.TreeToolConfigShortcut";
//        private readonly PanelStack _panelStack;
//        private List<string> _resourceNames = new();

//        private UILayout _uiLayout; 

//        private VisualElement _root;
//        private readonly InputService _inputService;        // to check keybinding, to open UI

//        private bool _boUiEnabled;

//        private static List<bool> _LocalCheckbox = new List<bool> { false };

//        public CutterToolConfigPanel( PanelStack panelStack,
//                                        UILayout uiLayout,
//                                        InputService inputService)
//        {
//            this._uiLayout = uiLayout;
//            this._panelStack = panelStack;
//            this._inputService = inputService;

//            this._root = new VisualElement();
//        }

//        public void Load()
//        {
//            // reset complete panel to default from config at setup
//            // activate input handling service
//            _inputService.AddInputProcessor((IPriorityInputProcessor)this);
//        }

//        public VisualElement GetPanel()
//        {
//            // get / initialise resourcenames list
//            UpdateResourceNamesList();

//            UIBoxBuilder menu = _uiBuilder.CreateBoxBuilder()
//                .SetHeight(new Length(400))
//                .SetWidth(new Length(300))
//                .ModifyBox(builder =>
//                {
//                    builder.SetMargin(new Margin(0, 0, 0, 0));
//                    builder.SetStyle(style => style.position = Position.Relative);
//                });


//            menu.AddPreset(factory => factory.Labels().DefaultText("Cordial.TreeTool.TreeToolPanel.PanelDescription", builder: builder => builder.SetStyle(style => { style.alignSelf = Align.Center; style.marginBottom = new Length(10); })));

//            foreach (string resourceName in this._resourceNames)
//            {
//                menu.AddPreset(factory => factory.Toggles().CheckmarkInverted(resourceName, text: resourceName, builder: builder => builder.SetMargin(new Margin(0, 0, new Length(8), 0))));
//            }

//            menu.AddPreset(factory => factory.Buttons().Button(name: "Reset", locKey: "Cordial.TreeTool.TreeToolPanel.ButtonReset", width: new Length(150)));
//            menu.AddPreset(factory => factory.Buttons().Button(name: "Confirm", locKey: "Cordial.TreeTool.TreeToolPanel.ButtonConfirm", width: new Length(150)));
//            menu.AddCloseButton("Close").SetBoxInCenter().AddHeader("Cordial.TreeTool.TreeToolPanel.Title");


//            _root = menu.BuildAndInitialize();


//            _root.Q<Button>("Confirm").clicked += ButtonConfirm;
//            _root.Q<Button>("Reset").clicked += ButtonReset;
//            _root.Q<Button>("Close").clicked += OnUICancelled;

//            foreach (string resourceName in this._resourceNames)
//            {
//                _root.Q<Toggle>(resourceName).RegisterValueChangedCallback(value => ToggleValueChange(resourceName, value.newValue));
//            }

//            //Mod.Log.LogWarning("Get Panel!");

//            UpdatePanelFromParam();

//            return _root;
//        }

//        public bool GetUIEnabledByKey()
//        {
//            return _boUiEnabled;
//        }
//        void IPriorityInputProcessor.ProcessInput()
//        {
//            _inputService.AddInputProcessor((IInputProcessor)this);
//            if (true == _inputService.IsKeyHeld(ShortcutKey))
//            {
//                _boUiEnabled = true;
//            }
//            else
//            {
//                _boUiEnabled = false;
//            }
//        }

//        bool IInputProcessor.ProcessInput()
//        {
//            return false;
//        }

//        public bool OnUIConfirmed()
//        {
//            //this._panelStack.HideAndPush((IPanelController)this);
//            this._panelStack.HideAndPushOverlay((IPanelController)this);
//            return false;
//        }

//        public void ButtonConfirm()
//        {
//            // store local config to parameters
//            for (int index = 0; index < _LocalCheckbox.Count; ++index)
//            {
//                TreeToolParam.SetResourceState(_resourceNames[index], _LocalCheckbox[index]);
//            }

//            this.Close();
//        }

//        public void ButtonReset()
//        {
//            // only call parameter init once
//            TreeToolParam.UpdateFromConfig();
//            UpdatePanelFromParam();
//        }

//        public void OnUICancelled()
//        {
//            this.Close();
//        }

//        private void Close()
//        {
//            this._panelStack.Pop((IPanelController)this);
//        }

//        private void ToggleValueChange(string resourceName, bool value)
//        {
//            // Do some action when toggle changed value
//            for (int index = 0; index < _resourceNames.Count; ++index)
//            {

//                if (resourceName == _resourceNames[index])
//                {
//                    _LocalCheckbox[index] = value;
//                }
//            }
//        }

//        private void UpdatePanelFromParam()
//        {
//            _LocalCheckbox.Clear();

//            foreach (string resourceName in this._resourceNames)
//            {
//                bool state = TreeToolParam.GetResourceState(resourceName);
//                SendToggleUpdateEvent(resourceName, state);
//                _LocalCheckbox.Add(state);
//            }
//        }

//        private void SendToggleUpdateEvent(string name, bool newValue)
//        {
//            bool oldValue = _root.Q<Toggle>(name).value;

//            if (oldValue != newValue)
//            {
//                _root.Q<Toggle>(name).value = newValue;
//                _root.Q<Toggle>(name).SendEvent(ChangeEvent<bool>.GetPooled(oldValue, newValue));
//            }
//        }

//        private void UpdateResourceNamesList()
//        {
//            this._resourceNames = TreeToolParam.GetResourceNames();
//        }


//    }
//}
