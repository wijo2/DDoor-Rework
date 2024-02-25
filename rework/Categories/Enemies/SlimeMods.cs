using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace rework.Categories.Enemies
{
    internal class SlimeMods
    {
        //blood scale = timer for growing up
        //blood offset x = 0.1 means don't spawn more slimes
        //blood offset y = projectile slime's timer for when to start moving 

        //small grow up
        [HarmonyPatch(typeof(AI_Slime), "FixedUpdate")]
        [HarmonyPostfix]
        public static void FixedUpdate_post(AI_Slime __instance)
        {
            if (__instance.size == AI_Slime.SlimeSize.Small)
            {
                if (__instance.gameObject.GetComponent<DamageableCharacter>().bloodVolumeScaleModifier > 1.1)
                {
                    if (Rework.LoadThings("mediumSlime")[0])
                    {
                        __instance.gameObject.SetActive(false);
                        var newSlime = UnityEngine.Object.Instantiate(Rework.mediumSlime, __instance.gameObject.transform.position, Quaternion.identity, __instance.transform.parent);
                        newSlime.SetActive(true);
                        newSlime.GetComponent<DamageableCharacter>().bloodVolumeSpawnOffset.x = 0.1f;
                        UnityEngine.Object.Destroy(__instance.gameObject);
                    }
                }
                else
                {
                    __instance.gameObject.GetComponent<DamageableCharacter>().bloodVolumeScaleModifier += Time.fixedDeltaTime / 100;
                }
            }
        }

        //medium spawn small
        [HarmonyPatch(typeof(AI_Slime), "hopSpikeCheck")]
        [HarmonyPostfix]
        public static void hopSpikeCheck_post(bool __result, AI_Slime __instance, ref int ___hopSpikeCounter, ref float ___angle)
        {
            if (!Rework.LoadThings("smallSlime")[0] || __instance.size != AI_Slime.SlimeSize.Medium || !__result || __instance.gameObject.GetComponent<DamageableCharacter>().bloodVolumeSpawnOffset.x == 0.1f)
            {
                return;
            }
            var projectileSlime = UnityEngine.Object.Instantiate(Rework.smallSlime, __instance.gameObject.transform.position + Vector3.up * 5, Quaternion.identity);
            projectileSlime.transform.parent = __instance.gameObject.transform.parent;
            projectileSlime.GetComponent<Rigidbody>().velocity = Vector3.up * 30f;
            projectileSlime.SetActive(true);
            var dmg = projectileSlime.gameObject.GetComponent<DamageableCharacter>();
            dmg.bloodVolumeSpawnOffset.x = 0.1f;
            dmg.bloodVolumeSpawnOffset.y = 0.1f;
            ___hopSpikeCounter += 3;
        }

        //small slime stays still when launching
        [HarmonyPatch(typeof(AI_Slime), "FixedUpdate")]
        [HarmonyPrefix]
        public static bool FixedUpdate_pre(AI_Slime __instance)
        {
            if (__instance.size != AI_Slime.SlimeSize.Small || __instance.gameObject.GetComponent<DamageableCharacter>().bloodVolumeSpawnOffset.y < 0f)
            {
                return true;
            }
            var dir = PlayerGlobal.instance.gameObject.transform.position - __instance.gameObject.transform.position;
            dir.y = 0;
            __instance.gameObject.GetComponent<Rigidbody>().velocity = dir * 1.5f + Vector3.up * (__instance.gameObject.GetComponent<Rigidbody>().velocity.y - 4f * Time.fixedDeltaTime);
            __instance.gameObject.GetComponent<DamageableCharacter>().bloodVolumeSpawnOffset.y -= Time.fixedDeltaTime / 10f;
            if (__instance.gameObject.GetComponent<DamageableCharacter>().bloodVolumeSpawnOffset.y < 0f)
            {
                __instance.SetState(AI_Brain.AIState.Attack);
            }
            return false;
        }
    }
}
