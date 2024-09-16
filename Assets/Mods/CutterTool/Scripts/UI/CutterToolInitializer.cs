using System.Collections.Generic;
using Timberborn.Beavers;
using Timberborn.CoreUI;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cordial.Mods.CutterTool.Scripts.UI
{
    public class CutterToolInitializer : ILoadableSingleton 
    {

        private readonly UILayout _uiLayout;
        private readonly VisualElementLoader _visualElementLoader;
        private readonly EventBus _eventBus;


        readonly CutterToolConfigFragment _cutterToolConfigFragment;

        private VisualElement _root;
        private VisualElement _entityroot;

        public CutterToolInitializer( UILayout uiLayout, 
                                            VisualElementLoader visualElementLoader,
                                            CutterToolConfigFragment cutterToolConfigFragment,
                                            EventBus eventBus)
        {
            this._uiLayout = uiLayout;
            this._visualElementLoader = visualElementLoader;
            this._cutterToolConfigFragment = cutterToolConfigFragment;
            this._eventBus = eventBus;
        }

        public void Load()
        {
            this._entityroot = this._visualElementLoader.LoadVisualElement("Common/EntityPanel/EntityPanel");

            //Rect rect = this._entityroot.layout;

            //Debug.Log("CTI: LEP: x:" + rect.size.x + " y:" + rect.size.y + " p1:" + rect.position.x + " p2:" + rect.position.y + " c:" + this._entityroot.childCount );

            var elements = this._entityroot.Children();
            List<VisualElement> elementList = new();

            foreach (var element in elements)
            {
                Debug.Log("CTI: LEP: E: " + element.name + " EC: " + element.childCount);


                switch (element.name)
                {
                    case "EntityDescription":
                    case "EntityDescriptionHider":
                    case "SideFragments":
                    case "Fragments":
                        elementList.Add(element);
                        break;
                    default:
                        break;
                }

                foreach (var child in element.Children())
                {
                    Debug.Log("CTI: LEP:  C: " + child.name + " CC: " + child.childCount);

                    if (child.name == "Header")
                    {
                        elementList.Add(child);
                    }

                    foreach (var kid in child.Children())
                    {
                        Debug.Log("CTI: LEP:   K: " + kid.name + " KC: " + kid.childCount);

                        foreach (var newt in kid.Children())
                        {
                            Debug.Log("CTI: LEP:    N: " + newt.name + " NC: " + newt.childCount);

                            foreach (var sal in newt.Children())
                            {
                                Debug.Log("CTI: LEP:     S: " + sal.name + " SC: " + sal.childCount);

                            }
                        }
                    }
                }
            }

            //for (int index = 0; index < elementList.Count; ++index)
            //{
            //    this._entityroot.Remove(elementList[index]);
            //}
            //elementList.Clear();

            this._entityroot.Clear();

            for (int index = 0; index < elementList.Count; ++index)
            {
                this._entityroot.Add(elementList[index]);
            }


            this._root = this._cutterToolConfigFragment.InitializeFragment();

            this._entityroot.Add(this._root);

            this._uiLayout.AddAbsoluteItem(this._entityroot);
            //this._uiLayout.AddBottomRight(this._root, 10);
            this._eventBus.Register((object)this);

            this._entityroot.ToggleDisplayStyle(false);
            //this._root.ToggleDisplayStyle(false);

            Debug.Log("CTI: Load");
        }

        public void SetVisualState(bool setActive)
        {
            this._root.ToggleDisplayStyle(setActive);
            Debug.Log("CTI: SVS: " + setActive);
        }

        [OnEvent]
        public void OnCutterToolSelectedEvent( CutterToolSelectedEvent cutterToolSelectedEvent)
        {
            if (null == cutterToolSelectedEvent)
                return;

            Debug.Log("CTI: SE");
            this.SetVisualState(true);

            this._entityroot.ToggleDisplayStyle(true);

            Rect rect = this._entityroot.layout;

            Debug.Log("CTI: SEP: c" + this._entityroot.childCount );

        }

        [OnEvent]
        public void OnCutterToolUnselectedEvent(CutterToolUnselectedEvent cutterToolUnselectedEvent )
        {
            if (null == cutterToolUnselectedEvent)
                return;
            Debug.Log("CTI: USE");
            this.SetVisualState(false);
            this._entityroot.ToggleDisplayStyle(false);
        }
    }
}