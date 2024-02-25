using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace rework.Categories
{
    internal class WeaponMods
    {
        //new names and desctiptions in menu
        [HarmonyPatch(typeof(UICollectableCell), "updateText")]
        [HarmonyPrefix]
        public static bool updateText_pre(UICollectableCell __instance, InventoryItem ___itemData)
        {
            if (!__instance.itemInfoTextArea || !___itemData || !UIMenuPauseController.instance.IsPaused() || !Resources.FindObjectsOfTypeAll<UIMenuWeapons>()[0].HasControl()) { return true; }
            //Log.LogWarning("item: " + ___itemData.GetItemName());
            switch (___itemData.GetItemName())
            {
                case "sword_name":
                    __instance.itemNameTextArea.text = "The Non-Zoomynator";
                    __instance.itemInfoTextArea.text = "The other traditional weapon of the crowkind, along with kitchenware.\nLight attacks only deal quarter of uncharged heavy dmg, but backstabs deal uncharged heavy dmg.";
                    break;
                case "umbrella_name":
                    __instance.itemNameTextArea.text = "The (Bullet) Rain Deflector";
                    __instance.itemInfoTextArea.text = "The traditional weapon of the masochist kind.\nAttacks are extremely weak, but light attack shoots out a projectile that, wile doesn't deal any damage, deflects bullets for 0.5 dmg each. Str increases size and magic increases lifetime.";
                    break;
                case "daggers_name":
                    __instance.itemNameTextArea.text = "The Ranged Scrub";
                    __instance.itemInfoTextArea.text = "The traditional weapon of the speedrunners, except they're slow this time.\nLight attack throws daggers with 0.5 base dmg (about half of uc heavy), upgrade dexterity to increase range.";
                    break;
                case "hammer_name":
                    __instance.itemNameTextArea.text = "The engineer wrench";
                    __instance.itemInfoTextArea.text = "The weapon only hit-and-run light-heavy spam noobs used, well, not this time c:\nNobody knowssssss";
                    break;
                case "sword_heavy_name":
                    __instance.itemNameTextArea.text = "The Slow Ass Repulsor";
                    __instance.itemInfoTextArea.text = "The traditional Gray Crow murderer, well good luck with that this time.\nA very slow but powerful swing with knockback.";
                    break;

                default:
                    __instance.itemInfoTextArea.text = "Sry m8, can't find the text, plz cry on dc so me is fix c:";
                    break;
            }
            return false;
        }

        //Stats in menu
        [HarmonyPatch(typeof(UIMenuWeapons), "SetWeaponStats")]
        [HarmonyPrefix]
        public static bool SetWeaponStats_pre(UIMenuWeapons __instance, InventoryItem item)
        {
            if (!UIMenuPauseController.instance.IsPaused() || !Resources.FindObjectsOfTypeAll<UIMenuWeapons>()[0].HasControl()) { return true; }

            if (!item)
            {
                __instance.statWindow.SetActive(false);
                return false;
            }
            __instance.statWindow.SetActive(true);
            _Weapon component = item.prefab.GetComponent<_ChargeWeapon>();
            if (component)
            {
                __instance.statDamage.text = (Mathf.Round(10 * component.baseDamage * 0.6f) / 10f).ToString();
                __instance.statSwings.text = "It's a heavy wtf";
                __instance.statSpeed.text = component.slashTime.ToString();
                __instance.statRange.text = component.slashRadius.ToString();
            }
            return false;
        }

        //dealing damage changes
        [HarmonyPatch(typeof(_Weapon), "applyDamage", typeof(Damageable), typeof(Vector3))]
        [HarmonyPrefix]
        public static bool applyDamage_pre(_Weapon __instance, ref bool __result, Damageable dmg, Vector3 impactPos, WeaponAttackReferences ___attackReference)
        {
            if (dmg.gameObject.GetComponent<HitBackProjectile>() != null && ReferenceEquals(__instance, ___attackReference.lightAttack))
            {
                return false;
            }

            var type = __instance.type;
            if (type == _Weapon.WeaponType.Sword && __instance.gameObject.name == "WEAPON_Umbrella(Clone)") { type = _Weapon.WeaponType.Umbrella; }

            float num = 0.6f * __instance.baseDamage;
            if (ReferenceEquals(__instance, ___attackReference.lightAttack))
            {
                switch (type)
                {
                    case _Weapon.WeaponType.Sword:
                        num /= 2; // = 1/4 of uc heavy
                        var pp = new Vector2(__instance.gameObject.transform.position.x, __instance.gameObject.transform.position.z);
                        var ep = new Vector2(dmg.gameObject.transform.position.x, dmg.gameObject.transform.position.z);
                        var ea = new Vector2(dmg.gameObject.transform.forward.x, dmg.gameObject.transform.forward.z);
                        var diff = (ep - pp).normalized;
                        //Log.LogWarning(Vector2.Angle(ea, diff));
                        if (Vector2.Angle(ea, diff) < 60)
                        {
                            num *= 4; // = uc heavy
                            SoundEngine.Event("HeavyAttackFullyCharged", __instance.gameObject);
                            PlayerGlobal.instance.gameObject.GetComponent<WeaponControl>().ResetStamina();
                        }
                        break;
                    case _Weapon.WeaponType.Dagger:
                        num = 0;
                        break;
                    case _Weapon.WeaponType.Hammer:
                        num = 0;
                        break;
                    case _Weapon.WeaponType.Greatsword:
                        num = 0.6f;
                        break;
                    case _Weapon.WeaponType.Umbrella:
                        num = 0;
                        break;
                }
            }
            Damageable.DamageType damageType = __instance.damageType;
            if (___attackReference)
            {
                ___attackReference.DidDamageWithWeapon();
            }
            num *= Inventory.GetMeleeDamageModifier();
            __result = dmg.ReceiveDamage(num, __instance.poiseDamage, __instance.transform.position, impactPos, damageType, 1f);
            return false;

        }

        //charge attack dmg set
        [HarmonyPatch(typeof(_ChargeWeapon), "applyDamage", typeof(Damageable), typeof(Vector3))]
        [HarmonyPrefix]
        public static bool applyDamage_pre(_ChargeWeapon __instance, ref bool __result, Damageable dmg, Vector3 impactPos, ref float ___storedCharge)
        {
            float num = __instance.baseDamage * Inventory.GetMeleeDamageModifier() * 0.6f;
            if (___storedCharge >= 1f)
            {
                num *= 1.5f;
            }
            if (EnchantController.instance)
            {
                num = EnchantController.instance.DamageModifier(num);
            }
            ___storedCharge = 0f;
            __result = dmg.ReceiveDamage(num, __instance.poiseDamage, __instance.transform.position, impactPos, Damageable.DamageType.Standard, 1f);
            return false;
        }

        //slash actions
        [HarmonyPatch(typeof(_Weapon), "Attack", typeof(bool), typeof(bool))]
        [HarmonyPostfix]
        public static bool Attack_post(bool __result, bool bypassInputLock, _Weapon __instance, WeaponAttackReferences ___attackReference, InputLock ___inputLock, WeaponControl ___control)
        {
            if (!ReferenceEquals(__instance, ___attackReference.lightAttack) || !__result) { return __result; }
            var type = __instance.type;
            if (type == _Weapon.WeaponType.Sword && __instance.gameObject.name == "WEAPON_Umbrella(Clone)") { type = _Weapon.WeaponType.Umbrella; }

            var dir = PlayerGlobal.instance.GetLastInput();
            dir.y = 0;
            dir.Normalize();

            switch (type)
            {
                case _Weapon.WeaponType.Dagger:
                    if (!Rework.LoadThings("dm")[0]) { return __result; }
                    if (!Rework.LoadThings("mb")[0]) { return __result; }
                    var model = UnityEngine.Object.Instantiate(Rework.daggerModel);
                    model.SetActive(true);
                    model.GetComponentInChildren<Renderer>().enabled = true;

                    var newPrefab = UnityEngine.Object.Instantiate(Rework.mageBulletPrefab);
                    var particles = newPrefab.transform.Find("Particle System").gameObject;
                    if (particles != null)
                    {
                        UnityEngine.Object.Destroy(particles);
                    }
                    var mesh = newPrefab.transform.Find("Mesh").gameObject;
                    if (mesh != null)
                    {
                        UnityEngine.Object.Destroy(mesh);
                    }
                    var light = newPrefab.transform.Find("Point light").gameObject;
                    if (mesh != null)
                    {
                        UnityEngine.Object.Destroy(light);
                    }
                    newPrefab.layer = 11;
                    newPrefab.transform.position = __instance.gameObject.transform.position + dir * (1.5f + Mathf.Pow(Inventory.GetMeleeDamageModifier(), 0.5f)) + new Vector3(0, 0.8f, 0);
                    newPrefab.transform.rotation = Quaternion.identity;
                    model.transform.rotation = Quaternion.identity;
                    model.transform.SetParent(newPrefab.transform);
                    model.transform.localPosition = Vector3.zero;
                    model.transform.localScale = Vector3.one * 1.5f * Mathf.Pow(Inventory.GetMeleeDamageModifier(), 0.5f);
                    model.transform.localEulerAngles = new Vector3(90, Mathf.Rad2Deg * Mathf.Atan2(dir.x, dir.z), 0);

                    //fix the collider
                    UnityEngine.Object.Destroy(newPrefab.GetComponent<SphereCollider>());
                    CapsuleCollider cc;
                    foreach (MeshRenderer rend in newPrefab.GetComponentsInChildren<MeshRenderer>())
                    {
                        rend.gameObject.layer = 11;
                        var rotatorEmpty = new GameObject();
                        rotatorEmpty.transform.parent = rend.transform;
                        rotatorEmpty.transform.localPosition = Vector3.zero;
                        rotatorEmpty.transform.eulerAngles = Quaternion.LookRotation(dir).eulerAngles + new Vector3(90, 0, 0);
                        cc = rotatorEmpty.AddComponent<CapsuleCollider>();
                        cc.height = rend.bounds.extents.magnitude;
                    }
                    var bulletComponent = newPrefab.GetComponent<Bullet>();
                    bulletComponent.damage = 0.5f * Inventory.GetMeleeDamageModifier();
                    bulletComponent.Shoot(dir, 3.5f / Mathf.Pow(Inventory.GetDexterityModifier(), 0.5f));
                    newPrefab.GetComponent<TimedDelete>().timer = 0.2f / Mathf.Pow(Inventory.GetDexterityModifier(), 1.5f);
                    return __result;

                case _Weapon.WeaponType.Umbrella:
                    if (!Rework.LoadThings("um")[0]) { return __result; }
                    var projectile = UnityEngine.Object.Instantiate(Rework.umbrellaModel);
                    projectile.SetActive(true);
                    var size = ((Inventory.GetMeleeDamageModifier() - 1) * 0.5f + 1) * 0.8f;

                    projectile.SetActive(true);
                    projectile.transform.localPosition = PlayerGlobal.instance.transform.position + dir * 3 + Vector3.up * 3 * size;
                    projectile.transform.rotation = Quaternion.identity;
                    projectile.transform.localScale = Vector3.one * size;

                    var bulletComp = projectile.AddComponent<UmbrellaBullet>();
                    bulletComp.Shoot(dir, 5, size);

                    var t = projectile.AddComponent<TimedDelete>();
                    t.timer = 2 * Inventory.GetMagicDamageModifier();
                    return __result;
            }
            return __result;
        }

        //when a bullet dies it kills a dagger if attatched
        [HarmonyPatch(typeof(Bullet), "destroySelf", typeof(Collision))]
        [HarmonyPostfix]
        public static void destroySelf_post(Bullet __instance)
        {
            var dagger = __instance.gameObject.transform.Find("Model1(Clone)(Clone)");
            if (dagger != null)
            {
                UnityEngine.Object.Destroy(dagger.gameObject);
            }
        }

        //bullets don't double dmg
        [HarmonyPatch(typeof(HitBackProjectile), nameof(HitBackProjectile.ReceiveDamage), typeof(float), typeof(float), typeof(Vector3), typeof(Vector3), typeof(Damageable.DamageType), typeof(float))]
        [HarmonyPostfix]
        public static void ReceiveDamage_post(HitBackProjectile __instance, float dmg)
        {
            __instance.bullet.damage = dmg;
        }

        //slash radius for weapons set
        [HarmonyPatch(typeof(_Weapon), "checkCollisions")]
        [HarmonyPrefix]
        public static bool checkCollisions_pre(_Weapon __instance, WeaponAttackReferences ___attackReference)
        {
            if (ReferenceEquals(__instance, ___attackReference.lightAttack))
            {
                switch (__instance.type)
                {
                    case _Weapon.WeaponType.Sword:
                        __instance.slashRadius = 1;
                        break;
                    case _Weapon.WeaponType.Umbrella:
                        __instance.slashRadius = 0;
                        break;
                    case _Weapon.WeaponType.Dagger:
                        __instance.slashRadius = 0;
                        break;
                    case _Weapon.WeaponType.Hammer:
                        __instance.slashRadius = 0;
                        break;
                }
            }
            return true;
        }

        //slash visuals shrunken (mostly copy paste code)
        [HarmonyPatch(typeof(_Weapon), nameof(_Weapon.VisualSlash), typeof(bool))]
        [HarmonyPrefix]
        public static bool VisualSlash_pre(_Weapon __instance, WeaponAttackReferences ___attackReference, ref bool ___attacking, Light[] ___lightSource, ref Vector3 ___slashRingLocalPos, WeaponControl ___control, float ___timeModifier, string ___attackName, bool playSound = true)
        {
            if (ReferenceEquals(__instance, ___attackReference.lightAttack))
            {
                ___attacking = true;
                if (___lightSource != null)
                {
                    for (int i = 0; i < ___lightSource.Length; i++)
                    {
                        if (___lightSource[i] != null)
                        {
                            ___lightSource[i].color = __instance.color;
                        }
                    }
                }
                if (__instance.slashPrefab && !(ReferenceEquals(__instance, ___attackReference.lightAttack) && (__instance.type == _Weapon.WeaponType.Dagger || __instance.gameObject.name == "WEAPON_Umbrella(Clone)")))
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.slashPrefab, __instance.transform.position, __instance.transform.rotation);
                    __instance.swordSlash = gameObject.GetComponentInChildren<SlashRing>();
                    if (__instance.swordSlash)
                    {
                        ___slashRingLocalPos = __instance.swordSlash.transform.localPosition;
                        float meleeRangeModifier = Inventory.GetMeleeRangeModifier();
                        __instance.swordSlash.SetColor(__instance.color);
                        __instance.swordSlash.swingTime *= Inventory.GetDexterityModifier();
                        gameObject.transform.localScale = new Vector3(0.7f, 1f, meleeRangeModifier / 1.5f);
                    }
                }
                if (__instance.swordSlash)
                {
                    __instance.swordSlash.SetDirection(!___control.slashDirection);
                }
                if (___timeModifier < 1f)
                {
                    if (__instance.swordSlash)
                    {
                        __instance.swordSlash.Trigger(__instance.biggerScale, "FinalSwordSwing");
                    }
                    if (playSound)
                    {
                        if (___attackName == "SwordSwing")
                        {
                            SoundEngine.Event("FinalSwordSwing", __instance.gameObject);
                            return false;
                        }
                        SoundEngine.Event(___attackName, __instance.gameObject);
                        return false;
                    }
                }
                else
                {
                    if (__instance.swordSlash)
                    {
                        __instance.swordSlash.Trigger(1f, "SwordSwing");
                    }
                    if (playSound)
                    {
                        SoundEngine.Event(___attackName, __instance.gameObject);
                    }
                }
                return false;
            }
            return true;
        }
    }
}
