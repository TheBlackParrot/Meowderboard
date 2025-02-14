using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using Meowderboard.Objects;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace Meowderboard.Configuration
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        [NonNullable]
        [UseConverter(typeof(ListConverter<BlueskyGroup>))]
        public virtual List<BlueskyGroup> Groups { get; set; } = new List<BlueskyGroup>();
    }
}