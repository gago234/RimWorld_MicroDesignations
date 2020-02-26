using System;
using Harmony;
using Verse;
using System.Reflection;

namespace MicroDesignations
{
    [StaticConstructorOnStartup]
    public class MicroDesignations : Mod
    {
        public static FieldInfo LdisallowedFilters = null;
        public MicroDesignations(ModContentPack content) : base(content)
        {
            var harmony = HarmonyInstance.Create("net.avilmask.rimworld.mod.MicroDesignations");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            LdisallowedFilters = AccessTools.Field(typeof(ThingFilter), "disallowedSpecialFilters");
            if (LdisallowedFilters == null)
                throw new Exception("Couldn't get ThingFilter.disallowedSpecialFilters");
        }
    }
}
