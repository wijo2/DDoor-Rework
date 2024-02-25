using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace rework.Categories.Enemies
{
    internal class GOTDMods
    {
        //phase hps set
        [HarmonyPatch(typeof(Redeemer), "calcPhase")]
        [HarmonyPostfix]
        public static void calcPhase_post(Redeemer __instance, ref AI_Brain.Phase __result, Damageable ___dmg)
        {
            float currentHealthPercent = ___dmg.GetCurrentHealthPercent();
            if (currentHealthPercent > 0.8f)
            {
                __result = AI_Brain.Phase.Two;
            }
            else
            {
                __result = AI_Brain.Phase.Three;
            }
            if (currentHealthPercent == 1)
            {
                ___dmg.maxHealth = 90;
                ___dmg.HealToFull();
            }
        }

        //missile stats set
        [HarmonyPatch(typeof(Redeemer), "shootMissiles")]
        [HarmonyPrefix]
        public static bool shootMissiles(Redeemer __instance, Animator ___anim, ref float ___rocketCountdown, ref float ___canFireMissilesTimer, ref float ___timeBetweenMissiles)
        {
            __instance.maxMissileCount = 900;
            ___anim.SetBool("rocket", true);
            ___timeBetweenMissiles = 0.3f;
            ___rocketCountdown = 1;
            ___canFireMissilesTimer = 2;
            return false;
        }

        //rocket list updating
        [HarmonyPatch(typeof(Redeemer), "Update")]
        [HarmonyPrefix]
        public static bool Update_pre(Redeemer __instance, ref float ___rocketCountdown)
        {
            var list = UnityEngine.Object.FindObjectOfType<RedeemerMissilePool>().GetComponentsInChildren<RedeemerMissile>(true);
            if (list.Length > 0 && list.Length < 50)
            {
                UnityEngine.Object.Instantiate(list[0], UnityEngine.Object.FindObjectOfType<RedeemerMissilePool>().gameObject.transform);
            }
            if (___rocketCountdown <= 0)
            {
                ___rocketCountdown = 0.5f;
            }
            return true;
        }

        //laser stats set
        [HarmonyPatch(typeof(RedeemerLaser), "Update")]
        [HarmonyPrefix]
        public static bool Update_pre(RedeemerLaser __instance)
        {
            __instance.chargeTime = 1.5f;
            __instance.acceleration = 0.035f;
            return true;
        }

        //roll end removed
        [HarmonyPatch(typeof(RedeemerLaser), "trackPlayer")]
        [HarmonyPrefix]
        public static bool trackPlayer_pre(RedeemerLaser __instance, ref Vector3 ___laserPos, ref float ___fireScale, ref Vector3 ___velocity, ref float ___charge, ref RaycastHit[] ___hitInfoOut, ref Vector3 ___laserScaleVector, ref float ___laserThickness, ref float ___laserScale, ref Collider[] ___colliderList, ref float ___targetScale)
        {
            if (___fireScale > 0.5f)
            {
                ___fireScale -= Time.fixedDeltaTime;
            }
            if (___fireScale < 0.5f)
            {
                ___fireScale = 0.5f;
            }
            Vector3 vector = PlayerGlobal.instance.transform.position - ___laserPos;
            ___velocity += vector.normalized * __instance.acceleration;
            ___velocity *= __instance.friction;
            ___laserPos += ___velocity;
            ___laserPos.y = PlayerGlobal.instance.transform.position.y;
            float num = 1f;
            if (___charge < 1f)
            {
                num = 0.01f;
            }
            Vector3 vector2 = __instance.transform.position - ___laserPos;
            int num2 = Physics.RaycastNonAlloc(__instance.transform.position, -vector2.normalized, ___hitInfoOut, 200f, Globals.instance.solidLayers);
            int num3 = 0;
            float num4 = 9999f;
            for (int i = 0; i < num2; i++)
            {
                if (i < ___hitInfoOut.Length && ___hitInfoOut[i].distance < num4)
                {
                    num3 = i;
                    num4 = ___hitInfoOut[i].distance;
                }
            }
            float num5 = UnityEngine.Random.Range(0.7f, 1f);
            num *= num5;
            ___laserScaleVector.x = ___laserThickness * num;
            ___laserScaleVector.y = ___laserThickness * num;
            ___laserScaleVector.z = ___hitInfoOut[num3].distance * ___laserScale;
            __instance.beam.localScale = ___laserScaleVector;
            if (___charge >= 1f)
            {
                if (RumbleShake.instance != null)
                {
                    RumbleShake.instance.RumbleRanged(0.2f, ___laserPos, 0.1f, 40f, RumbleShake.RumbleCurve.Constant);
                }
                int num6 = Physics.OverlapSphereNonAlloc(___laserPos, 0.5f, ___colliderList);
                for (int j = 0; j < num6; j++)
                {
                    if (___colliderList[j].gameObject.CompareTag("Player"))
                    {
                        Damageable component = ___colliderList[j].gameObject.GetComponent<Damageable>();
                        if (component && !PlayerGlobal.instance.gameObject.GetComponent<DashPower>().IsDashing())
                        {
                            component.ReceiveDamage(1f, 5f, ___laserPos, ___laserPos, Damageable.DamageType.Laser, 1f);
                            if (__instance.master)
                            {
                                __instance.master.StopLaser();
                            }
                            else if (__instance.altMaster)
                            {
                                __instance.altMaster.SendMessage("StopLaser");
                            }
                            SoundEngine.Event("RedeemerLaserStop", __instance.gameObject);
                        }
                    }
                }
            }
            if (__instance.laserTarget)
            {
                if (___targetScale < 1f)
                {
                    ___targetScale += Time.fixedDeltaTime;
                }
                if (___targetScale > 1f)
                {
                    ___targetScale = 1f;
                }
                __instance.laserTarget.position = ___laserPos + Vector3.up * 0.2f;
                __instance.laserTarget.rotation = Quaternion.Euler(0f, __instance.laserTarget.rotation.eulerAngles.y + Time.fixedDeltaTime * 90f, 0f);
                __instance.laserTarget.localScale = Vector3.one * ___targetScale * 0.4f;
            }
            return false;
        }

        //new missiles added to pool
        [HarmonyPatch(typeof(RedeemerMissilePool), nameof(RedeemerMissilePool.SpawnMissile), typeof(Vector3), typeof(Vector3))]
        [HarmonyPrefix]
        public static bool SpawnMissile_pre(RedeemerMissilePool __instance, Vector3 pos, Vector3 target, ref RedeemerMissile[] ___missileList, ref int ___index)
        {
            if (___missileList.Length < __instance.GetComponentsInChildren<RedeemerMissile>(true).Length - 1)
            {
                ___missileList = __instance.GetComponentsInChildren<RedeemerMissile>(true);
                for (int i = 0; i < ___missileList.Length; i++)
                {
                    ___missileList[i].gameObject.SetActive(false);
                }
            }
            if (___missileList[___index])
            {
                ___missileList[___index].transform.position = pos;
                ___missileList[___index].gameObject.SetActive(true);
                ___missileList[___index].ThrowAt(target);
            }
            ___index++;
            if (___index >= ___missileList.Length)
            {
                ___index = 0;
            }
            return false;
        }
    }
}
