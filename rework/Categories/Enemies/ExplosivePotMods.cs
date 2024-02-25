using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace rework.Categories.Enemies
{
    internal class ExplosivePotMods
    {
        //always chase
        [HarmonyPatch(typeof(PotHermit), "Start")]
        [HarmonyPostfix]
        public static void Start_post(PotHermit __instance)
        {
            __instance.alwaysKnowWherePlayerIs = true;
        }
    }
}
