using JetBrains.Annotations;
using Meowderboard.UI;
using Zenject;

namespace Meowderboard.Installers
{
    [UsedImplicitly]
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MeowderboardViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<TopPanelViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesTo<MeowderboardManager>().AsSingle();
        }
    }
}