using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace rework.Categories.Enemies
{
    internal class PlagueKnightMods
    {
        //shoot 3
        [HarmonyPatch(typeof(AI_Plague), "Anim_ShootBomb")]
        [HarmonyPrefix]
        public static bool Anim_ShootBomb_pre(AI_Plague __instance, Vector3 ___bombSpawnPos, Quaternion ___bombSpawnRotation, ref float ___timer)
        {
            var angle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
            var distance = 8;

            //1
            HurlPot component = UnityEngine.Object.Instantiate<GameObject>(__instance.plagueBomb, ___bombSpawnPos, Quaternion.identity).GetComponent<HurlPot>();
            ___timer = __instance.shootTime;
            component.maxHeight = 1f / (PlayerGlobal.instance.transform.position - __instance.transform.position).magnitude * 100f;
            if (component.maxHeight > 20f)
            {
                component.maxHeight = 20f;
            }
            component.ThrowAt(PlayerGlobal.instance.transform.position + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * distance);

            angle += Mathf.PI * 2 / 3;

            //2
            component = UnityEngine.Object.Instantiate<GameObject>(__instance.plagueBomb, ___bombSpawnPos, Quaternion.identity).GetComponent<HurlPot>();
            component.maxHeight = 1f / (PlayerGlobal.instance.transform.position - __instance.transform.position).magnitude * 100f;
            if (component.maxHeight > 20f)
            {
                component.maxHeight = 20f;
            }
            component.ThrowAt(PlayerGlobal.instance.transform.position + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * distance);

            angle += Mathf.PI * 2 / 3;

            //3
            component = UnityEngine.Object.Instantiate<GameObject>(__instance.plagueBomb, ___bombSpawnPos, Quaternion.identity).GetComponent<HurlPot>();
            component.maxHeight = 1f / (PlayerGlobal.instance.transform.position - __instance.transform.position).magnitude * 100f;
            if (component.maxHeight > 20f)
            {
                component.maxHeight = 20f;
            }
            component.ThrowAt(PlayerGlobal.instance.transform.position + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * distance);

            //rest
            ParticleSystem componentInChildren = __instance.bombBone.GetComponentInChildren<ParticleSystem>();
            if (componentInChildren)
            {
                componentInChildren.transform.position = ___bombSpawnPos;
                componentInChildren.transform.rotation = ___bombSpawnRotation;
                componentInChildren.Play();
            }
            SoundEngine.Event("PlagueBoyFireGun", __instance.gameObject);
            ScreenShake.ShakeXY(6f, 20f, 6, 0f, __instance.gameObject);
            return false;
        }

        //explosion doesn't hurt enemies
        [HarmonyPatch(typeof(Explosion), "checkDamage")]
        [HarmonyPrefix]
        public static bool checkDamage_pre(Explosion __instance, bool ___canDamagePlayer)
        {
            if (__instance.gameObject.GetComponent<PlagueExplosion>() == null)
            {
                return true;
            }

            if (__instance.damageRadius > 0f)
            {
                Collider[] array = Physics.OverlapSphere(__instance.transform.position, __instance.damageRadius / 2, __instance.layerMask);
                for (int i = 0; i < array.Length; i++)
                {
                    if (___canDamagePlayer && array[i].gameObject.CompareTag("Player"))
                    {
                        Damageable component = array[i].GetComponent<Damageable>();
                        if (component)
                        {
                            Vector3 vector = (__instance.transform.position + array[i].transform.position) * 0.5f;
                            if (__instance.playerBomb)
                            {
                                component.SetPlayerBombPrep();
                            }
                            component.ReceiveDamage(__instance.damage, 99f, __instance.transform.position, vector, __instance.dmgType, 2f);
                            if (__instance.playerBomb)
                            {
                                component.ClearPlayerBombPrep();
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
