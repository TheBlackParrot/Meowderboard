using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using HMUI;
using Meowderboard.Objects;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Random = System.Random;

namespace Meowderboard.Utils
{
    internal static class FetchImage
    {
        private static readonly HttpClient HttpClient = new HttpClient
        {
            BaseAddress = new Uri("https://public.api.bsky.app")
        };
        private static readonly HttpClient ImageHttpClient = new HttpClient
        {
            BaseAddress = new Uri("https://cdn.bsky.app")
        };
        
        internal static bool IsFetching;
        private static readonly Dictionary<string, float> CachedResponseTime = new Dictionary<string, float>();
        private static readonly Dictionary<string, JObject> CachedResponse = new Dictionary<string, JObject>();
        internal static string SourceAccount;
        internal static string SourceLink;
        internal static string SourceText;
        internal static DateTime SourceTime;

        private static async Task<byte[]> Fetch(BlueskyGroup blueskyGroup)
        {
            ASCIIEncoding ascii = new ASCIIEncoding();
            List<string> blueskyHandles = blueskyGroup.BlueskyHandles;
            
            start:
                Plugin.Log.Info("Getting an image...");
                IsFetching = true;
                
                string wantedHandle = blueskyHandles[new Random().Next(0, blueskyHandles.Count)];
                
                CachedResponse.TryGetValue(wantedHandle, out JObject cachedResponse);
                CachedResponseTime.TryGetValue(wantedHandle, out float cachedResponseTime);

                if (Time.time - cachedResponseTime > 7200f || cachedResponse == null)
                {
                    HttpResponseMessage response = await HttpClient.GetAsync($"xrpc/app.bsky.feed.getAuthorFeed?actor={wantedHandle}&filter=posts_with_media&limit=100");
                    if (!response.IsSuccessStatusCode)
                    {
                        Plugin.Log.Warn($"Failed to fetch image from {wantedHandle}, Bluesky JSON response was bad :(");
                        IsFetching = false;
                        return null;
                    }
                    
                    CachedResponse[wantedHandle] = JObject.Parse(await response.Content.ReadAsStringAsync());
                    CachedResponseTime[wantedHandle] = Time.time;
                }

                JToken post;
                try
                {
                    int max = Math.Min(CachedResponse[wantedHandle]["feed"].Count(), 100);
                    post = CachedResponse[wantedHandle]["feed"]?[new Random().Next(0, max)]?["post"];
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e);
                    goto start;
                }
                
            SourceAccount = wantedHandle;
            SourceLink = $"https://bsky.app/profile/{wantedHandle}/post/{post["uri"]?.ToString().Split('/').Last()}";
            
            if (post?["record"]?["embed"]?["$type"]?.ToString() != "app.bsky.embed.images")
            {
                Plugin.Log.Info($"(post {SourceLink} doesn't have an embedded image)");
                goto start;
            }
            
            string imageURL = post["embed"]?["images"]?[0]?["thumb"]?.ToString();

            if (imageURL == null)
            {
                Plugin.Log.Info($"(post {SourceLink} doesn't have an embedded thumbnail (????))");
                goto start;
            }

            Uri uri = new Uri(imageURL);
            HttpResponseMessage imageResponse = await ImageHttpClient.GetAsync(uri.PathAndQuery.Substring(1));
            if (!imageResponse.IsSuccessStatusCode)
            {
                Plugin.Log.Warn($"Failed to fetch image from {SourceLink}, image response was bad :(");
                IsFetching = false;
                return null;
            }

            string rawText = post["record"]?["text"]?.ToString();
            if (rawText != null)
            {
                Byte[] encodedBytes = ascii.GetBytes(rawText);
                SourceText = ascii.GetString(encodedBytes).Replace("\r", "").Replace("\n", " ");
            }
            else
            {
                SourceText = null;
            }
            
            SourceTime = DateTime.Parse(post["record"]?["createdAt"]?.ToString());
            

            Plugin.Log.Info("Fetched image. :D");
            return await imageResponse.Content.ReadAsByteArrayAsync();
        }

        internal static async Task Fetch(ImageView imageView, BlueskyGroup blueskyGroup)
        {
            byte[] catBytes = await Fetch(blueskyGroup);
            if (catBytes != null)
            {
                imageView.sprite = await Utilities.LoadSpriteAsync(catBytes);
            }
            IsFetching = false;
        }
    }
}