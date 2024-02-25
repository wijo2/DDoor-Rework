using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace rework.Categories.Enemies
{
    internal class GranMods
    {
        public static List<GameObject> grandmaClones = new List<GameObject>();

        //create clone
        public static GameObject CreateGranClone(float hp, float maxHp)
        {
            var c = UnityEngine.Object.Instantiate(GrandmaBoss.instance.gameObject, GrandmaBoss.instance.gameObject.transform.parent);
            grandmaClones.Add(c);
            var cloneScript = c.AddComponent<GrandmaClone>();
            c.name = "gran clone";
            var d = c.gameObject.GetComponent<DamageableBoss>();
            d.maxHealth = maxHp;
            cloneScript.hp = hp;
            c.transform.position += new Vector3(0, 10, 0);
            return c;
        }

        //grandma is alive
        [HarmonyPatch(typeof(GrandmaBoss), "Awake")]
        [HarmonyPrefix]
        public static bool Awake_pre(GrandmaBoss __instance, ref int ___maxBullets)
        {
            ___maxBullets = 200;
            if (__instance.gameObject.name == "grandma")
            {
                GrandmaBoss.instance = __instance;
                var d = __instance.gameObject.GetComponent<DamageableBoss>();
                d.maxHealth = 125;
                d.HealToFull();
            }
            else
            {
                return false;
            }

            return false;
        }

        //pots thrown not quite at the player
        [HarmonyPatch(typeof(HurlPot), "ThrowAt")]
        [HarmonyPostfix]
        public static void ThrowAt_post(Vector3 pos, HurlPot __instance, ref Vector3 ___endPos)
        {
            if (GrandmaBoss.instance)
            {
                ___endPos = pos + Vector3.up - (pos - __instance.transform.position).normalized * 4;
            }
        }

        //on grandma death rage increases for other grandmas and move instance to another if necessary
        [HarmonyPatch(typeof(DamageableBoss), "Die")]
        [HarmonyPrefix]
        public static bool Die_pre(DamageableBoss __instance)
        {
            if (__instance.gameObject.name == "grandma" || __instance.gameObject.name == "gran clone")
            {
                GrandmaClone.rage++;
                if (GrandmaClone.rage >= 3)
                {
                    var p = __instance.transform;
                    foreach (var thing in __instance.GetComponentsInChildren<Transform>())
                    {
                        if (thing.name == "PotHat_end") { p = thing.parent; break; }
                    }
                    if (p == null) { return true; }
                    UnityEngine.Object.FindObjectOfType<SoulEmerge>().soulOrigin = p;
                    return true;
                }
                SoundEngine.Event("GrandmaBoss_Death", __instance.gameObject);
                if (__instance.GetComponent<GrandmaBoss>() == GrandmaBoss.instance || GrandmaBoss.instance == null)
                {
                    GrandmaBoss.instance = grandmaClones[0].GetComponent<GrandmaBoss>();
                    UnityEngine.Object.Destroy(grandmaClones[0].GetComponent<GrandmaClone>());
                    grandmaClones.RemoveAt(0);
                }
                UnityEngine.Object.Destroy(__instance.gameObject);

                var r = GrandmaClone.rage;
                var v = AccessTools.Field(typeof(GrandmaBoss), "angleBetweenBullets");
                if (r == 1)
                {
                    v.SetValue(GrandmaBoss.instance, 12);
                    if (grandmaClones.Count() == 1)
                    {
                        v.SetValue(grandmaClones[0].GetComponent<GrandmaBoss>(), 12);
                    }
                }
                if (r == 2)
                {
                    v.SetValue(GrandmaBoss.instance, 10);
                }

                return false;
            }
            return true;
        }

        //stop gran fucking up my precious instance
        [HarmonyPatch(typeof(GrandmaBoss), "OnDestroy")]
        [HarmonyPrefix]
        public static bool OnDestroy_pre()
        {
            return GrandmaClone.rage > 2;
        }

        //spawn garndma clones when needed
        [HarmonyPatch(typeof(DamageableBoss), "ReceiveDamage")]
        [HarmonyPostfix]
        public static void recieveDamage_post(DamageableBoss __instance)
        {
            GrandmaClone.CalcHpAndEnable();
        }

        //make gran clones visible when appropriate
        [HarmonyPatch(typeof(GrandmaBoss), "SetState")]
        [HarmonyPostfix]
        public static void SetState_post(GrandmaBoss __instance, AI_Brain.AIState newState)
        {
            if (__instance == GrandmaBoss.instance && newState == AI_Brain.AIState.PrepTeleport)
            {
                foreach (var gran in grandmaClones)
                {
                    var b = gran.GetComponent<GrandmaBoss>();

                    if (!gran.active && gran.GetComponent<GrandmaClone>().shouldBeActive)
                    {
                        gran.SetActive(true);
                        if (b.GetState() != AI_Brain.AIState.PrepTeleport) { b.SetState(AI_Brain.AIState.PrepTeleport); }
                    }

                    //AccessTools.Field(typeof(GrandmaBoss), "timer").SetValue(b, 0.01f);
                    Helper.CopyPrivateValue<GrandmaBoss>(GrandmaBoss.instance, b, "throwCounter");
                }
            }

            if (__instance == GrandmaBoss.instance && newState == AI_Brain.AIState.TeleportIn && grandmaClones.Count() == 0)
            {
                CreateGranClone(90, 150);
                CreateGranClone(50, 150);
            }
        }

        //sync grandmas after spin
        [HarmonyPatch(typeof(GrandmaBoss), "Anim_CannonMode")]
        [HarmonyPostfix]
        public static void SyncOldPerson(GrandmaBoss __instance)
        {
            if (__instance != GrandmaBoss.instance && GrandmaBoss.instance.GetState() == AI_Brain.AIState.Laser) //if a clone is late it's sped up
            {
                foreach (var gran in grandmaClones)
                {
                    if (gran.GetComponent<GrandmaClone>().shouldBeActive)
                    {
                        Helper.CopyPrivateValue<GrandmaBoss>(GrandmaBoss.instance, gran.GetComponent<GrandmaBoss>(), "timer");
                    }
                }
            }
            if (__instance == GrandmaBoss.instance) //if og is late everyone else is slowed down
            {
                foreach (var gran in grandmaClones)
                {
                    if (gran.GetComponent<GrandmaClone>().shouldBeActive && gran.GetComponent<GrandmaBoss>().GetState() == AI_Brain.AIState.Laser)
                    {
                        Helper.CopyPrivateValue<AI_Brain>(GrandmaBoss.instance.GetComponent<AI_Brain>(), gran.GetComponent<AI_Brain>(), "timer");
                    }
                }
            }
        }

        //always phase 3
        [HarmonyPatch(typeof(GrandmaBoss), "getPhase")]
        [HarmonyPostfix]
        public static void getPhase_post(GrandmaBoss __instance, ref AI_Brain.Phase __result)
        {
            __result = AI_Brain.Phase.Three;
        }

        //disable hotpot (sry :/)
        [HarmonyPatch(typeof(GrandmaFireDetector), "FireInTheHole")]
        [HarmonyPrefix]
        public static bool StopArson()
        {
            return false;
        }

        //gran bullet non-reflect fix
        [HarmonyPatch(typeof(GrandmaBullet), "Shoot")]
        [HarmonyPrefix]
        public static void FixbulletCollision(GrandmaBullet __instance)
        {
            if (__instance.gameObject.GetComponent<HitBackProjectile>() == null)
            {
                var c = __instance.gameObject.AddComponent<HitBackProjectile>();
                c.speedMultiplier = 2;
                c.bullet = __instance.GetComponent<Bullet>();
                AccessTools.Field(typeof(HitBackProjectile), "invulTime").SetValue(c, 0);
            }
        }

        //stop error
        [HarmonyPatch(typeof(GrandmaPot), "DestroyAll")]
        [HarmonyPrefix]
        public static void DestroyAll_pre(GrandmaPot __instance)
        {
            if (GrandmaBoss.instance)
            {
                AccessTools.Field(typeof(GrandmaPot), "instance").SetValue(__instance, GrandmaBoss.instance.GetComponent<GrandmaPot>());
            }
        }

        //fix problem of not enough projectiles
        [HarmonyPatch(typeof(GrandmaBoss), "doShot")]
        [HarmonyPrefix]
        public static void FixProjectiles(ref GrandmaBullet[] ___bulletList, int ___bulletIndex)
        {
            if (___bulletList[___bulletIndex].gameObject.activeInHierarchy) { ___bulletList[___bulletIndex].gameObject.SetActive(false); }
        }
    }
}

/* unused code
        public static void CloneGranVisuals(GameObject clone)
        {
            //I shouldn't have to do this tho :c
            var gran = GrandmaBoss.instance;
            if (gran == null) { return; }

            var meshes = new List<SkinnedMeshRenderer>();
            foreach (var kid in gran.GetComponentsInChildren<Transform>())
            {
                if (kid.transform.parent == gran.transform)
                {
                    meshes.Add(Instantiate(kid, clone.transform).GetComponent<SkinnedMeshRenderer>());
                }
            }

            //  -- fix bones -- (fuck unity)

            //find bones
            var bones = new List<string>();
            foreach (var entry in gran.GetComponentInChildren<SkinnedMeshRenderer>().bones)
            {
                bones.Add(entry.name);
            }

            //fill list
            var newBones = new Transform[89];
            Transform root = null;
            foreach (var obj in clone.GetComponentsInChildren<Transform>())
            {
                L("loop");
                if (bones.Contains(obj.gameObject.name))
                {
                    L("bone check " + obj.gameObject.name);
                    newBones[bones.IndexOf(obj.gameObject.name)] = obj;
                    if (obj.gameObject.name == "_Root")
                    {
                        root = obj.transform;
                    }
                }
            }

            //fix them
            foreach (var mesh in meshes)
            {
                if (mesh != null)
                {
                    mesh.bones = newBones;
                    //root bone
                    mesh.rootBone = root;
                }
            }
        }*/