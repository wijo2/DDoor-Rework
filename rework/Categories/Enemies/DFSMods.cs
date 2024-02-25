using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace rework.Categories.Enemies
{
    internal class DFSMods
    {
        public static bool canShoot = true;

        //stats set
        [HarmonyPatch(typeof(ForestMother), "Start")]
        [HarmonyPostfix]
        public static void Start_post(ForestMother __instance, Damageable ___dmg)
        {
            if (___dmg.GetCurrentHealthPercent() == 1)
            {
                ___dmg.maxHealth = 60;
                ___dmg.HealToFull();
            }
            __instance.timeBetweenSlams = 1f;
            __instance.maxSlams = 10;
            __instance.timeBetweenShots = 0.5f;
        }

        //spinning speed set
        [HarmonyPatch(typeof(FlowerBase), "LateUpdate")]
        [HarmonyPostfix]
        public static void LateUpdate_post(FlowerBase __instance, bool ___spin)
        {
            var thing = UnityEngine.Object.FindObjectOfType<ForestMother>();
            var state = thing.GetState();
            if (___spin && state != AI_Brain.AIState.Defend && state != AI_Brain.AIState.JumpSlam)
            {
                __instance.StartHyperSpin();
            }
            if (state == AI_Brain.AIState.Defend)
            {
                __instance.StartSlowSpin();
            }
        }

        //shot spread
        [HarmonyPatch(typeof(ForestMother), "ShootSpore")]
        [HarmonyPrefix]
        public static bool ShootSpore_pre(ForestMother __instance, float ___slamTimer, Vector3 ___shootPos)
        {
            if (___slamTimer <= 0f && canShoot)
            {
                SoundEngine.Event("PlantBossBombFire", __instance.gameObject);
                ___slamTimer = 0.1f;
                ScreenShake.ShakeXY(4f, 10f, 8, 0f, null);
                if (RumbleShake.instance != null)
                {
                    RumbleShake.instance.Rumble(0.6f, 0.1f, RumbleShake.RumbleCurve.Linear);
                }
                for (float i = 0; i < 3; i++)
                {
                    HurlPot component = UnityEngine.Object.Instantiate<GameObject>(__instance.sporePrefab, ___shootPos + Vector3.up, Quaternion.identity).GetComponent<HurlPot>();
                    if (component)
                    {
                        Vector3 vector = PlayerGlobal.instance.transform.position - __instance.transform.position;
                        vector.y = 0f;
                        Vector3 target;
                        if (vector.magnitude >= 6f)
                        {
                            target = PlayerGlobal.instance.transform.position;
                        }
                        else
                        {
                            target = __instance.transform.position + vector.normalized * 6f;
                        }
                        var angle = i * 2f / 3f * UnityEngine.Mathf.PI;
                        component.ThrowAt(target + new Vector3(UnityEngine.Mathf.Sin(angle) * 6, 0, UnityEngine.Mathf.Cos(angle) * 6));
                    }
                }
                canShoot = false;
            }
            return false;
        }
    }
}
