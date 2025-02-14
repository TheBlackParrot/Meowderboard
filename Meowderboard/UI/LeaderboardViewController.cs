using System;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities.Async;
using JetBrains.Annotations;
using LeaderboardCore.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using Zenject;

namespace Meowderboard.UI
{
    [ViewDefinition("Meowderboard.UI.BSML.MainPanel.bsml")]
    [HotReload(RelativePathToLayout = "BSML.MainPanel.bsml")]
    internal class MeowderboardViewController : BSMLAutomaticViewController, INotifyLeaderboardSet
    {
        internal static MeowderboardViewController Instance;
        
        #pragma warning disable CS0649
        [Inject] private readonly TopPanelViewController _panelView;
        [Inject] private readonly PlatformLeaderboardViewController _platformLeaderboardViewController;
        #pragma warning restore CS0649
        
        [UIComponent("catImage")]
        private readonly ImageView _catImage = null!;
        
        [UIComponent("sourceAccount")]
        private readonly TextMeshProUGUI _sourceAccount = null!;
        
        [UIComponent("postText")]
        private readonly TextMeshProUGUI _postText = null!;

        private static float _lastFetch;
        
        // something ridiculous to signal it hasn't been set
        private static Color _originalHeaderColor;
        
        private static CancellationTokenSource _currentTokenSource;

        internal MeowderboardViewController()
        {
            Instance = this;
        }
        
        [UIAction("openPostSource")]
        [UsedImplicitly]
        private void OpenPostSource()
        {
            Application.OpenURL(Utils.Cats.SourceLink);
        }

        private async Task FadeIn()
        {
            _currentTokenSource?.Cancel();
            _currentTokenSource?.Dispose();

            _currentTokenSource = new CancellationTokenSource();

            await Animate(time => _catImage.color = Color.Lerp(Color.clear, Color.white, time), _currentTokenSource.Token, 0.75f);
        }
        
        private async Task FadeOut()
        {
            _currentTokenSource?.Cancel();
            _currentTokenSource?.Dispose();

            _currentTokenSource = new CancellationTokenSource();

            await Animate(time => _catImage.color = Color.Lerp(Color.white, Color.clear, time), _currentTokenSource.Token, 0.15f);
        }
        
        private static async Task Animate(Action<float> transition, CancellationToken cancellationToken, float duration)
        {
            float elapsedTime = 0.0f;
            while (elapsedTime <= duration)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                float value = Easing.InOutSine(elapsedTime / duration);
                transition?.Invoke(value);
                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }

            transition?.Invoke(1f);
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            
            if (!isActiveAndEnabled || !_platformLeaderboardViewController || !_panelView.isActiveAndEnabled)
            {
                return;
            }
            
            if (GameObject.Find("HeaderPanel").transform.Find("BG").TryGetComponent(out ImageView imageView))
            {
                _originalHeaderColor = imageView.color;
                
                imageView.color0 = Color.clear;
                imageView.color1 = Color.clear;
                imageView.color = Color.clear;
            }
            _platformLeaderboardViewController.GetComponentInChildren<TextMeshProUGUI>().color = Color.clear;

            if (firstActivation)
            {
                _postText.maxVisibleLines = 2;
                _postText.lineSpacing = -45;
                
                _catImage.color = Color.clear;
                
                _ = TopPanelViewController.Instance.CatNeedsToSleep();
                GetCat();
            }
        }

        internal void GetCat()
        {
            _sourceAccount.text = "";
            _postText.text = "";
            _sourceAccount.SetAllDirty();
            _postText.SetAllDirty();
            
            _lastFetch = Time.time;
            
            UnityMainThreadTaskScheduler.Factory.StartNew(async () =>
            {
                if (_catImage.color.a > 0f)
                {
                    await FadeOut();
                }
                
                await Utils.Cats.Fetch(_catImage);
                _sourceAccount.text = $"<b>@{Utils.Cats.SourceAccount}</b>  <alpha=#44>—  <size=85%><alpha=#88>{Utils.Cats.SourceTime:g}";
                _postText.text = "<size=95%>" + (string.IsNullOrEmpty(Utils.Cats.SourceText) ? "<alpha=#88><i>(no caption provided)</i>" : $"<alpha=#CC>{Utils.Cats.SourceText}");
                _sourceAccount.SetAllDirty();
                _postText.SetAllDirty();
                
                await FadeIn();
            });
        }
        
        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            
            if (!_platformLeaderboardViewController)
            {
                return;
            }

            if (GameObject.Find("HeaderPanel").transform.Find("BG").TryGetComponent(out ImageView imageView))
            {
                imageView.color = _originalHeaderColor;
            }
            _platformLeaderboardViewController.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        }

        public void OnLeaderboardSet(BeatmapKey beatmapKey)
        {
        }
    }
}