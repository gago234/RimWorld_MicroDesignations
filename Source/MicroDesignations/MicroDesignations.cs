using Harmony;
using System.Reflection;
using Verse;
namespace MicroDesignations
{
    [StaticConstructorOnStartup]
    public class MicroDesignations : Mod
    {
        public MicroDesignations(ModContentPack content) : base(content)
        {
            var harmony = HarmonyInstance.Create("net.avilmask.rimworld.mod.MicroDesignations");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
