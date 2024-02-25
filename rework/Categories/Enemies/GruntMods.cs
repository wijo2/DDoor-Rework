using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace rework.Categories.Enemies
{
    internal class GruntMods
    {
        //grunt identifier
        public static GruntType IdentifyGrunt(GameObject grunt)
        {
            if (grunt.name.Contains("Redeemer"))
            {
                return GruntType.GOTD;
            }
            if (grunt.name.Contains("Pot Variant"))
            {
                return GruntType.Pot;
            }
            return GruntType.Forest;
        }

        //c1 = graveyard - grandma and so on
        public enum GruntType
        {
            GOTD,
            Pot,
            Forest,
        }

        //stats set
        [HarmonyPatch(typeof(AI_Grunt), "Start")]
        [HarmonyPostfix]
        public static void FixedUpdate_pre(AI_Grunt __instance, CharacterMovementControl ___movement, ref float ___movementSpeed, ref float ___slowDownMultiplier)
        {
            ___movement.maxSpeed = 30;
            ___movement.slowDownMultiplier = 0.8f;
            ___movementSpeed = 30;
            ___slowDownMultiplier = 0.9f;
            __instance.maxCanAttackTimer = 1.5f;
            __instance.maxSlowTimer = 2f;
            if (IdentifyGrunt(__instance.gameObject) == GruntType.Forest)
            {
                __instance.GetComponent<CharacterMovementControl>().acceleration = 10;
                __instance.jumpAttackChance = 0;
            }
        }

        //forest grunts do running attack after backstep
        [HarmonyPatch(typeof(AI_Grunt), "SetState")]
        [HarmonyPrefix]
        public static void SetState_pre(AI_Grunt __instance, AI_Brain.AIState newState)
        {
            if (IdentifyGrunt(__instance.gameObject) == GruntType.Forest && newState == AI_Brain.AIState.BackStep)
            {
                AccessTools.Field(typeof(AI_Grunt), "doRunningAttackNext").SetValue(__instance, true);
            }
        }

        //running attack mod
        [HarmonyPatch(typeof(AI_Grunt), "aimAttack", typeof(float))]
        [HarmonyPrefix]
        public static bool aimAttack_pre(AI_Grunt __instance, float lerp, CharacterMovementControl ___movement, ref Vector3 ___trackedAimVector, bool ___doRunningAttackNext, float ___attackRotationOffset)
        {
            Vector3 position = PlayerGlobal.instance.transform.position;
            Vector3 vector = __instance.transform.position - position;
            if (___doRunningAttackNext)
            {
                lerp *= 3f;
            }
            ___movement.UpdateRotation(lerp * 3, vector.normalized, ___attackRotationOffset);
            ___trackedAimVector = Vector3.Lerp(___trackedAimVector, vector.normalized, Time.fixedDeltaTime * 2f);
            if (___doRunningAttackNext)
            {
                ___movement.TakeMovementInput((PlayerGlobal.instance.transform.position - __instance.gameObject.transform.position).normalized, 1f);
            }
            return false;
        }

        //stagger removed
        [HarmonyPatch(typeof(AI_Grunt), nameof(AI_Grunt.Stagger))]
        [HarmonyPostfix]
        public static void Stagger_post(AI_Grunt __instance)
        {
            __instance.EndStagger();
        }
    }
}
