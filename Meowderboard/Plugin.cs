using System.Linq;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using JetBrains.Annotations;
using Meowderboard.Configuration;
using SiraUtil.Zenject;
using Meowderboard.Installers;
using Meowderboard.Objects;
using IPALogger = IPA.Logging.Logger;

namespace Meowderboard
{
    [Plugin(RuntimeOptions.DynamicInit), NoEnableDisable]
    [UsedImplicitly]
    public class Plugin
    {
        internal static Plugin Instance { [UsedImplicitly] get; private set; }
        internal static IPALogger Log { get; private set; }

        [Init]
        public void Init(Zenjector zenjector, Config config, IPALogger logger)
        {
            Instance = this;
            Log = logger;

            PluginConfig c = config.Generated<PluginConfig>();
            PluginConfig.Instance = c;
            if (!PluginConfig.Instance.Groups.Any())
            {
                PluginConfig.Instance.Groups.Add(new BlueskyGroup());
            }

            zenjector.UseLogger(logger);
            zenjector.UseMetadataBinder<Plugin>();

            zenjector.Install<MenuInstaller>(Location.Menu);
        }
    }
}