using System.Collections.Generic;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using IPA.Utilities.Async;
using JetBrains.Annotations;
using Meowderboard.UI;

namespace Meowderboard.Objects
{
    public class BlueskyGroup
    {
        [UIValue("groupName")]
        [NotNull] public string GroupName { get; set; } = "Cats";
        [UIValue("buttonText")]
        [NotNull] public string ButtonText { get; set; } = "Meow";
        
        [UseConverter(typeof(ListConverter<string>))]
        [NotNull] public List<string> BlueskyHandles { get; set; } = new List<string> {
            "bodegacats.bsky.social",
            "catworkers.bsky.social",
            "fartycheddarcat.bsky.social",
            "gonzo.bsky.social",
            "ricky.baby",
            "parkinkatt.bsky.social",
            "karaisntactive.bsky.social",
            "billythecat.bsky.social",
            "harveyandpetey.bsky.social",
            "baxtercat.bsky.social",
            "cheesecakethecat.bsky.social",
            "cats.blue",
            "jumpingcat.org",
            "weirdlilguys.bsky.social",
            "catbraincell.bsky.social",
            "shouldhavecat.bsky.social"
        };

        [UIAction("fetchImage")]
        public void FetchImage()
        {
            Plugin.Log.Info($"Refresh for group {GroupName} called");
            MeowderboardViewController.Instance.GetCat(this);

            UnityMainThreadTaskScheduler.Factory.StartNew(async () =>
            {
                await TopPanelViewController.CooldownButtons();
            });
        }
    }
}