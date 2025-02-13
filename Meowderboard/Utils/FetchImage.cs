using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using HMUI;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Random = System.Random;

namespace Meowderboard.Utils
{
    internal static class Cats
    {
        private static readonly HttpClient HttpClient = new HttpClient
        {
            BaseAddress = new Uri("https://public.api.bsky.app")
        };
        private static readonly HttpClient ImageHttpClient = new HttpClient
        {
            BaseAddress = new Uri("https://cdn.bsky.app")
        };

        private static readonly string[] Handles =
        {
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
            "cheesecakethecat.bsky.social"
        };
        
        internal static bool IsFetching;
        private static readonly Dictionary<string, float> CachedResponseTime = new Dictionary<string, float>();
        private static readonly Dictionary<string, JObject> CachedResponse = new Dictionary<string, JObject>();
        internal static string SourceAccount;
        internal static string SourceLink;
        internal static string SourceText;
        internal static DateTime SourceTime;

        private static async Task<byte[]> GetCat()
        {
            ASCIIEncoding ascii = new ASCIIEncoding();
            
            start:
                Plugin.Log.Info("got here 1");
                Plugin.Log.Info("Getting a cat...");
                IsFetching = true;
                
                string wantedHandle = Handles[new Random().Next(0, Handles.Length)];
                Plugin.Log.Info("got here 2");
                
                CachedResponse.TryGetValue(wantedHandle, out JObject cachedResponse);
                CachedResponseTime.TryGetValue(wantedHandle, out float cachedResponseTime);
                Plugin.Log.Info("got here 3");

                if (Time.time - cachedResponseTime > 7200f || cachedResponse == null)
                {
                    Plugin.Log.Info("got here 4");
                    HttpResponseMessage response = await HttpClient.GetAsync($"xrpc/app.bsky.feed.getAuthorFeed?actor={wantedHandle}&filter=posts_with_media&limit=100");
                    if (!response.IsSuccessStatusCode)
                    {
                        Plugin.Log.Info("got here 5");
                        Plugin.Log.Warn("Failed to get a cat, Bluesky JSON response was bad :(");
                        IsFetching = false;
                        return null;
                    }
                    
                    CachedResponse[wantedHandle] = JObject.Parse(await response.Content.ReadAsStringAsync());
                    CachedResponseTime[wantedHandle] = Time.time;
                    Plugin.Log.Info("got here 6");
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

                Plugin.Log.Info("got here 7");
            if (post?["record"]?["embed"]?["$type"]?.ToString() != "app.bsky.embed.images")
            {
                Plugin.Log.Info("got here 7.1");
                Plugin.Log.Info("(post doesn't have an embedded image)");
                goto start;
            }
            
            string imageURL = post["embed"]?["images"]?[0]?["thumb"]?.ToString();
            Plugin.Log.Info("got here 8");

            if (imageURL == null)
            {
                Plugin.Log.Info("got here 8.1");
                Plugin.Log.Info("(post doesn't have an embedded thumbnail (wtf))");
                goto start;
            }

            Uri uri = new Uri(imageURL);
            Plugin.Log.Info("got here 9");
            HttpResponseMessage imageResponse = await ImageHttpClient.GetAsync(uri.PathAndQuery.Substring(1));
            Plugin.Log.Info("got here 10");
            if (!imageResponse.IsSuccessStatusCode)
            {
                Plugin.Log.Info("got here 10.1");
                Plugin.Log.Warn("Failed to get a cat, image response was bad :(");
                IsFetching = false;
                return null;
            }

            SourceAccount = wantedHandle;
            SourceLink = $"https://bsky.app/profile/{wantedHandle}/post/{post["uri"]?.ToString().Split('/').Last()}";
            Plugin.Log.Info("got here 11");

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

            Plugin.Log.Info("got here 12");
            SourceTime = DateTime.Parse(post["record"]?["createdAt"]?.ToString());
            

            Plugin.Log.Info($"Got a cat. :D");
            return await imageResponse.Content.ReadAsByteArrayAsync();
        }

        internal static async Task Fetch(ImageView imageView)
        {
            byte[] catBytes = await GetCat();
            if (catBytes != null)
            {
                imageView.sprite = await Utilities.LoadSpriteAsync(catBytes);
            }
            IsFetching = false;
        }
    }
}