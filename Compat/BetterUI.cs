using RoR2;
using static BetterUI.ProcItemsCatalog;

namespace ThinkInvisible.ClassicItems {
    public static class Compat_BetterUI {
        public static void AddCatalog(ItemIndex ind, ProcEffect procEffect, float baseValue, float stackValue, Stacking stackingType)
        {
            AddEffect(ind, procEffect, baseValue, stackValue, stackingType);
        }

        private static bool? _enabled;
        public static bool enabled {
            get {
                if(_enabled == null) _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("dev.ontrigger.itemstats");
                return (bool)_enabled;
            }
        }
    }
}
