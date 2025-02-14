using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using UnityEngine.UI;

namespace Meowderboard.UI {
    [ViewDefinition("Meowderboard.UI.BSML.TopPanel.bsml")]
    [HotReload(RelativePathToLayout = "BSML.TopPanel.bsml")]
    public class TopPanelViewController : BSMLAutomaticViewController
    {
        internal static TopPanelViewController Instance;

        public TopPanelViewController()
        {
            Instance = this;
        }
        
        [UIComponent("refreshButton")]
        private readonly Button _refreshButton = null!;

        [UIAction("getCat")]
        internal void GetCat()
        {
            _ = CatNeedsToSleep();
            MeowderboardViewController.Instance.GetCat();
        }

        internal async Task CatNeedsToSleep()
        {
            _refreshButton.interactable = false;
            await Task.Delay(3000);
            _refreshButton.interactable = true;
        }
    }
}