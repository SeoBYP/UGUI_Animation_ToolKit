using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIAnimationToolkit
{
    [AddComponentMenu("UI/Advanced/UGUI Toggle Group", 31)]
    public class UGUIToggleGroup : UIBehaviour
    {
        [SerializeField] private bool m_AllowSwitchOff = false;

        public bool allowSwitchOff
        {
            get => m_AllowSwitchOff;
            set => m_AllowSwitchOff = value;
        }

        private readonly List<UGUIToggle> m_Toggles = new List<UGUIToggle>();

        protected UGUIToggleGroup()
        {
        }

        public void RegisterToggle(UGUIToggle toggle)
        {
            if (!m_Toggles.Contains(toggle))
                m_Toggles.Add(toggle);
        }

        public void UnregisterToggle(UGUIToggle toggle)
        {
            if (m_Toggles.Contains(toggle))
                m_Toggles.Remove(toggle);
        }

        public void NotifyToggleOn(UGUIToggle toggle, bool sendCallback = true)
        {
            if (!allowSwitchOff && !AnyTogglesOn())
            {
                toggle.isOn = true;
                return;
            }

            foreach (var t in m_Toggles)
            {
                if (t == toggle) continue;

                if (sendCallback)
                    t.isOn = false;
                else
                    t.SetIsOnWithoutNotify(false);
            }
        }

        public bool AnyTogglesOn() => m_Toggles.Any(x => x.isOn);
    }
}