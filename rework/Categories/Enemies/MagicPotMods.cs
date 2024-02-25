using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace rework.Categories.Enemies
{
    internal class MagicPotMods
    {
        //faster chaser orbs
        [HarmonyPatch(typeof(ChaserBullet), "Start")]
        [HarmonyPostfix]
        public static void Start_post(ChaserBullet __instance)
        {
            if (SceneManager.GetActiveScene().name != "boss_Grandma" || GrandmaClone.rage == 2)
            {
                __instance.airTracking = 0.5f;
                __instance.trackingAccel = 2;
                __instance.shootSpeed = 16;
            }
            else if (GrandmaClone.rage == 1)
            {
                __instance.airTracking = 0.5f;
                __instance.trackingAccel = 0.7f;
                __instance.shootSpeed = 10;
            }
        }
    }
}
