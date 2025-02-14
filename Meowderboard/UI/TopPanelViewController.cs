using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HarmonyLib;
using Meowderboard.Configuration;
using Meowderboard.Objects;
using UnityEngine.UI;

namespace Meowderboard.UI {
    [ViewDefinition("Meowderboard.UI.BSML.TopPanel.bsml")]
    [HotReload(RelativePathToLayout = "BSML.TopPanel.bsml")]
    public class TopPanelViewController : BSMLAutomaticViewController
    {
        internal static TopPanelViewController Instance;

        [UIComponent("refreshButtonContainer")]
        internal static readonly LayoutGroup RefreshButtonContainer;
        
        [UIValue("groups")]
        internal static List<BlueskyGroup> BlueskyGroups => PluginConfig.Instance.Groups;
        
        public TopPanelViewController()
        {
            Instance = this;
        }

        internal static async Task CooldownButtons()
        {
            Plugin.Log.Info("Triggering button cooldown");
            try
            {
                Button[] buttons = RefreshButtonContainer.gameObject.GetComponentsInChildren<Button>();
                Plugin.Log.Info($"Found {buttons.Length} buttons");
            
                buttons.Do(button => button.interactable = false);
                await Task.Delay(3000);
                buttons.Do(button => button.interactable = true);
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
            }
        }
    }
}