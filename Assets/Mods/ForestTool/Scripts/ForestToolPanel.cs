using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Timberborn.CoreUI;
using Timberborn.SingletonSystem;
using Timberborn.InputSystem;
using UnityEngine.UIElements;
using UnityEngine;


namespace Mods.ForestTool.Scripts
{
    public class ForestToolPanel  : IPanelController, ILoadableSingleton, IPriorityInputProcessor, IInputProcessor
    {
        public static readonly string ShortcutKey = "Cordial.ForestTool.KeyBinding.ForestToolConfigShortcut";
    
        private readonly InputService _inputService;        // to check keybinding, to open UI
        private readonly PanelStack _panelStack;
        private readonly ForestToolInitializer _forestToolInitializer;      // go get UIs

        private static List<bool> _LocalCheckbox = new List<bool> { false };
        private static bool _IsEnabled = false;

       

        public ForestToolPanel( InputService inputService,
                                PanelStack panelStack,
                                ForestToolInitializer forestToolInitializer )
        {
            this._inputService = inputService;
            this._panelStack = panelStack;
            this._forestToolInitializer = forestToolInitializer;
        }

        public void Load()
        {
            // reset complete panel to default from config at setup
            // activate input handling service
            _inputService.AddInputProcessor((IPriorityInputProcessor)this);
        }

        public bool GetUIEnabledByKey()
        {
            return _IsEnabled;
        }
        public VisualElement GetPanel()
        {
            if (_IsEnabled)
            {
                Debug.Log("ForestTool Config Enabled");
                return GetPanelConfigUi();
            }
            else
            {
                Debug.Log("ForestTool Config Disabled");
                return GetPanelErrorUi();
            }
        }
        public bool OnUIConfirmed()
        {
            //this._panelStack.HideAndPush((IPanelController)this);
            this._panelStack.HideAndPushOverlay((IPanelController)this);
            return false;
        }
        public void OnUICancelled()
        {
            this.Close();
        }

        private void Close()
        {
            this._panelStack.Pop((IPanelController)this);
        }

        public VisualElement GetPanelErrorUi()
        {
            return _forestToolInitializer.GetErrorUiElement();
        }
        public VisualElement GetPanelConfigUi()
        {
            return _forestToolInitializer.GetConfigUiElement();
        }

        void IPriorityInputProcessor.ProcessInput()
        {
            _inputService.AddInputProcessor((IInputProcessor)this);

            if (true == _inputService.IsKeyHeld(ShortcutKey))
            {
                _IsEnabled = true;
            }
            else
            {
                _IsEnabled = false;
            }
        }

        bool IInputProcessor.ProcessInput()
        {
            return false;
        }


    }



}
