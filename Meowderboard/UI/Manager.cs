using System;
using HMUI;
using JetBrains.Annotations;
using LeaderboardCore.Managers;
using LeaderboardCore.Models;

namespace Meowderboard.UI
{
    [UsedImplicitly]
    public class MeowderboardManager : CustomLeaderboard, IDisposable
    {
        private readonly CustomLeaderboardManager _manager;
        private readonly MeowderboardViewController _meowderboardViewController;

        protected override string leaderboardId => "Meowderboard";
        protected override ViewController panelViewController { get; }
        protected override ViewController leaderboardViewController => _meowderboardViewController;
        
        internal MeowderboardManager(CustomLeaderboardManager customLeaderboardManager, TopPanelViewController topPanelView, MeowderboardViewController meowderboardViewController)
        {
            _meowderboardViewController = meowderboardViewController;
            this.panelViewController = topPanelView;
            _manager = customLeaderboardManager;
            _manager.Register(this);
        }

        public void Dispose()
        {
            _manager.Unregister(this);
        }
    }
}