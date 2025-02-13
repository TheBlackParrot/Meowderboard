using IPA;
using JetBrains.Annotations;
using SiraUtil.Zenject;
using Meowderboard.Installers;
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
        public void Init(Zenjector zenjector, IPALogger logger)
        {
            Instance = this;
            Log = logger;

            zenjector.UseLogger(logger);
            zenjector.UseMetadataBinder<Plugin>();

            zenjector.Install<MenuInstaller>(Location.Menu);
        }
    }
}