using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace rework
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Rework : BaseUnityPlugin
    {
        internal static new ManualLogSource Log;

        private const string pluginGuid = "ddoor.gameRework.wijo";
        private const string pluginName = "Game Rework";
        private const string pluginVersion = "0.0.1";

        public static GameObject mageBulletPrefab;
        public static GameObject arrowPrefab;
        public static GameObject daggerModel;
        public static GameObject mediumSlime;
        public static GameObject smallSlime;

        public void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll(typeof(Rework));
        }

        //asset gather
        public static List<bool> LoadThings(params string[] things)
        {
            if (things == new string[] { "all" })
            {
                things = new string[] { "mb", "ga", "dm", "mediumSlime" };
            }
            List<bool> returns = new List<bool>();
            List<bool> f = new List<bool>() { false };
            foreach (string thing in things)
            {
                switch (thing)
                {
                    case "mb":
                        if (mageBulletPrefab == null)
                        {
                            var mageAi = Resources.FindObjectsOfTypeAll<AI_MageBrain>();
                            if (mageAi.Length != 0)
                            {
                                mageBulletPrefab = mageAi[0].bulletPrefab;
                            }
                        }
                        returns.Add(mageBulletPrefab != null);
                        break;

                    case "ga":
                        if (arrowPrefab == null)
                        {
                            var comp = FindObjectOfType<AI_Ghoul>();
                            if (comp != null)
                            {
                                arrowPrefab = comp.arrowPrefab;
                            }
                        }
                        returns.Add(arrowPrefab != null);
                        break;

                    case "dm":
                        if (daggerModel == null)
                        {
                            var weapons = Resources.FindObjectsOfTypeAll<Weapon_Sword>();
                            if (weapons.Length == 0)
                            {
                                returns.Add(false);
                                break;
                            }
                            for (int i = 0; i < weapons.Length; i++)
                            {
                                if (weapons[i].type == _Weapon.WeaponType.Dagger)
                                {
                                    daggerModel = Instantiate(weapons[i].transform.Find("Model1").gameObject);
                                    daggerModel.GetComponentInChildren<Renderer>().enabled = false;
                                    break;
                                }
                            }
                        }
                        returns.Add(daggerModel != null);
                        break;

                    case "mediumSlime":
                        if (mediumSlime == null)
                        {
                            var slimes = Resources.FindObjectsOfTypeAll<AI_Slime>();
                            if (slimes.Length == 0)
                            {
                                returns.Add(false);
                                break;
                            }
                            for (int i = 0; i < slimes.Length; i++)
                            {
                                if (slimes[i].size == AI_Slime.SlimeSize.Medium && slimes[i].gameObject.transform.parent != null && slimes[i].transform.localScale == new Vector3(1, 1, 1))
                                {
                                    mediumSlime = Instantiate(slimes[i].transform.gameObject);
                                    break;
                                }
                            }
                        }
                        returns.Add(mediumSlime != null);
                        break;

                    case "smallSlime":
                        if (smallSlime == null)
                        {
                            var slimes = Resources.FindObjectsOfTypeAll<AI_Slime>();
                            if (slimes.Length == 0)
                            {
                                returns.Add(false);
                                break;
                            }
                            for (int i = 0; i < slimes.Length; i++)
                            {
                                if (slimes[i].size == AI_Slime.SlimeSize.Small && slimes[i].gameObject.transform.parent != null && slimes[i].transform.localScale == new Vector3(1, 1, 1))
                                {
                                    smallSlime = Instantiate(slimes[i].transform.gameObject);
                                    break;
                                }
                            }
                        }
                        returns.Add(smallSlime != null);
                        break;

                    default:
                        Log.LogWarning("LoadThings string doesn't exist");
                        break;
                }
            }
            return returns;
        }

        //load things on map load
        [HarmonyPatch(typeof(GameSceneManager), "LoadScene")]
        [HarmonyPostfix]
        public static void LoadScene_post()
        {
            LoadThings("all");
        }

        //weapon mods

        //new names and desctiptions in menu
        [HarmonyPatch(typeof(UICollectableCell), "updateText")]
        [HarmonyPrefix]
        public static bool updateText_pre(UICollectableCell __instance, InventoryItem ___itemData)
        {
            if (!__instance.itemInfoTextArea || !___itemData || !UIMenuPauseController.instance.IsPaused() || !Resources.FindObjectsOfTypeAll<UIMenuWeapons>()[0].HasControl()) { return true; }
            //Log.LogWarning("item: " + ___itemData.GetItemName());
            switch (___itemData.GetItemName()){
                case "sword_name":
                    __instance.itemNameTextArea.text = "The Non-Zoomynator";
                    __instance.itemInfoTextArea.text = "The other traditional weapon of the crowkind, along with kitchenware.\nNormal attacks are somewhat weak, but backstabs do double damage.";
                    break;
                case "umbrella_name":
                    __instance.itemNameTextArea.text = "The (Bullet) Rain Deflector";
                    __instance.itemInfoTextArea.text = "The traditional weapon of the masochist kind.\nAttacks are extremely weak, but it shoots out a projectile that deflects bullets.";
                    break;
                case "daggers_name":
                    __instance.itemNameTextArea.text = "The Ranged Scrub";
                    __instance.itemInfoTextArea.text = "The traditional weapon of the speedrunners, except they're slow this time.\nAttacks by throwing daggers, upgrade dexterity to increase range.";
                    break;
                case "hammer_name":
                    __instance.itemNameTextArea.text = "The Thought Clouder";
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

            float num = 0.6f * __instance.baseDamage;
            if (ReferenceEquals(__instance, ___attackReference.lightAttack))
            {
                switch (__instance.type)
                {
                    case _Weapon.WeaponType.Sword:
                        num = __instance.baseDamage / 3;
                        var pp = new Vector2(__instance.gameObject.transform.position.x, __instance.gameObject.transform.position.z);
                        var ep = new Vector2(dmg.gameObject.transform.position.x, dmg.gameObject.transform.position.z);
                        var ea = new Vector2(dmg.gameObject.transform.forward.x, dmg.gameObject.transform.forward.z);
                        var diff = (ep - pp).normalized;
                        //Log.LogWarning(Vector2.Angle(ea, diff));
                        if (Vector2.Angle(ea, diff) < 60)
                        {
                            num = __instance.baseDamage * 2;
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
                        num = 0.1f;
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
            switch (__instance.type)
            {
                case _Weapon.WeaponType.Dagger:
                    if (!LoadThings("dm")[0]) { return __result; }
                    var model = Instantiate(daggerModel);
                    model.GetComponentInChildren<Renderer>().enabled = true;
                    if (!LoadThings("mb")[0]) { return __result; }

                    var newPrefab = Instantiate(mageBulletPrefab);
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
                    var dir = PlayerGlobal.instance.GetLastInput();
                    dir.y = 0;
                    dir.Normalize();
                    newPrefab.layer = 11;
                    newPrefab.transform.position = __instance.gameObject.transform.position + dir * (1.5f + Mathf.Pow(Inventory.GetMeleeDamageModifier(), 0.5f)) + new Vector3(0, 0.8f, 0);
                    newPrefab.transform.rotation = Quaternion.identity;       
                    model.transform.rotation = Quaternion.identity;
                    model.transform.SetParent(newPrefab.transform);
                    model.transform.localPosition = Vector3.zero;
                    model.transform.localScale = Vector3.one * 1.5f * Mathf.Pow(Inventory.GetMeleeDamageModifier(), 0.5f);
                    model.transform.localEulerAngles = new Vector3(90, Mathf.Rad2Deg * Mathf.Atan2(dir.x, dir.z), 0);
                    //fix the collider
                    Destroy(newPrefab.GetComponent<SphereCollider>());
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

        //bullets don't double
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

        //slash visuals shrunken
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
                if (__instance.slashPrefab && !(ReferenceEquals(__instance, ___attackReference.lightAttack) && __instance.type == _Weapon.WeaponType.Dagger))
                {
                    GameObject gameObject = Instantiate<GameObject>(__instance.slashPrefab, __instance.transform.position, __instance.transform.rotation);
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

        //spell mods

        //arrows take 2 magic
        [HarmonyPatch(typeof(_ArrowPower), "Update")]
        [HarmonyPrefix]
        public static bool Update_pre(_ArrowPower __instance)
        {
            if (__instance.arrowPrefab.name == "ARROW")
            {
                __instance.requiredCharge = 2;
            }
            return true;
        }

        //enemy mods

        //headRoller
        //bullets when starts rolling
        [HarmonyPatch(typeof(AI_HeadRoller), nameof(AI_HeadRoller.SetState), typeof(AI_Brain.AIState))]
        [HarmonyPrefix]
        public static bool SetState_pre(AI_HeadRoller __instance, AI_Brain.AIState newState)
        {
            if (newState != AI_Brain.AIState.Dash) { return true; }

            if (LoadThings("mb")[0])
            {
                var dir = __instance.gameObject.transform.forward;
                dir.y = 0.3f;
                GameObject bullet;
                var amount = UnityEngine.Random.Range(10, 15);
                //var amount = 12;
                for (int a = 0; a <= 360; a += 360 / amount)
                {
                    bullet = Instantiate(mageBulletPrefab, __instance.gameObject.transform.position + dir * 2, Quaternion.identity);
                    bullet.GetComponent<Bullet>().Shoot(Quaternion.Euler(0, a, 0) * __instance.gameObject.transform.forward, 4);
                }
            }
            return true;
        }

        //mage
        //fast bullets
        [HarmonyPatch(typeof(AI_MageBrain), "shootBullet")]
        [HarmonyPrefix]
        public static bool shootBullet_pre(AI_MageBrain __instance)
        {
            Vector3 vector = PlayerGlobal.instance.transform.position - __instance.shootBone.position;
            float num = -__instance.spreadAngle * 0.5f;
            float num2 = __instance.spreadAngle / __instance.spreadMaxBullets * 2f;
            int num3 = 0;
            while ((float)num3 < __instance.spreadMaxBullets)
            {
                GameObject gameObject = Instantiate<GameObject>(__instance.bulletPrefab, __instance.shootBone.position, Quaternion.identity);
                SoundEngine.Event("MageAttack", gameObject);
                Bullet component = gameObject.GetComponent<Bullet>();
                if (component)
                {
                    component.Shoot(Quaternion.Euler(0f, num, 0f) * vector.normalized, 8f);
                }
                num += num2;
                num3++;
            }
            return false;
        }

        //range and tele timer modified
        [HarmonyPatch(typeof(AI_MageBrain), "Start")]
        [HarmonyPrefix]
        public static bool Start_pre(AI_MageBrain __instance)
        {
            __instance.attackRange = 10;
            __instance.timeBetweenTeleports = 0.1f;
            return true;
        }

        //stagger time set
        [HarmonyPatch(typeof(AI_MageBrain), "SetState")]
        [HarmonyPostfix]
        public static void SetState_post(AI_Brain.AIState newState, ref float ___timer)
        {
            if (newState == AI_Brain.AIState.Feared)
            {
                ___timer = 1;
            }
        }

        //bat
        //hp set and invisibility introduced
        [HarmonyPatch(typeof(AI_Bat), "FixedUpdate")]
        [HarmonyPostfix]
        public static void FixedUpdate_post(AI_Bat __instance)
        {
            var dmg = __instance.gameObject.GetComponentInChildren<Damageable>();
            if (dmg != null && dmg.GetCurrentHealthPercent() == 1)
            {
                dmg.maxHealth = 2;
                dmg.HealToFull();
            }
            if (__instance.GetState() == AI_Brain.AIState.Chase)
            {
                __instance.gameObject.GetComponentInChildren<Renderer>().enabled = false;
                return;
            }
            __instance.gameObject.GetComponentInChildren<Renderer>().enabled = true;
        }

        //grunt
        //stats set
        [HarmonyPatch(typeof(AI_Grunt), "FixedUpdate")]
        [HarmonyPrefix]
        public static void FixedUpdate_pre(AI_Grunt __instance, CharacterMovementControl ___movement, ref float ___movementSpeed, ref float ___slowDownMultiplier)
        {
            ___movement.maxSpeed = 30;
            ___movement.slowDownMultiplier = 0.8f;
            ___movementSpeed = 30;
            ___slowDownMultiplier = 0.9f;
            __instance.maxCanAttackTimer = 1.5f;
            __instance.maxSlowTimer = 2f;
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

        //gotd
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
            var list = FindObjectOfType<RedeemerMissilePool>().GetComponentsInChildren<RedeemerMissile>(true);
            if (list.Length > 0 && list.Length < 50)
            {
                Instantiate(list[0], FindObjectOfType<RedeemerMissilePool>().gameObject.transform);
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

        //dfs
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
            __instance.timeBetweenShots = 0.1f;
        }

        //spinning speed set
        [HarmonyPatch(typeof(FlowerBase), "LateUpdate")]
        [HarmonyPostfix]
        public static void LateUpdate_post(FlowerBase __instance, bool ___spin)
        {
            var thing = FindObjectOfType<ForestMother>();
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

        //archers
        //splitting arrows
        [HarmonyPatch(typeof(ArrowBullet), "hitWall", typeof(Collision))]
        [HarmonyPostfix]
        public static void hitWall(ArrowBullet __instance)
        {
            if (LoadThings("ga")[0] && __instance.pulseScale.x == 8.0f && __instance.gameObject.layer == 12)
            {
                Log.LogWarning("layer = " + __instance.gameObject.layer.ToString());
                var dir = (PlayerGlobal.instance.gameObject.transform.position - __instance.gameObject.transform.position);
                dir.y = 0;
                dir.Normalize();
                var eul = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z)).eulerAngles.y;
                GameObject bullet;
                var amount = UnityEngine.Random.Range(3, 5);
                var angle = 40;
                //var amount = 3; //4 but math
                for (int a = angle / -2; a <= angle / 2; a += angle / amount)
                {
                    bullet = Instantiate(arrowPrefab, __instance.gameObject.transform.position + dir / 2, Quaternion.identity);
                    var rigid = bullet.GetComponent<Rigidbody>();
                    rigid.constraints = RigidbodyConstraints.FreezeRotation;
                    rigid.isKinematic = false;

                    var compo = bullet.GetComponent<ArrowBullet>();
                    compo.pulseScale.x = 8.01f;
                    compo.Shoot(Quaternion.Euler(0, a, 0) * dir, 1f);
                }
                __instance.pulseScale.x = 8.01f;
            }
        }

        //remove stagger
        [HarmonyPatch(typeof(AI_Ghoul), nameof(AI_Ghoul.Stagger))]
        [HarmonyPrefix]
        public static bool Stagger_pre()
        {
            return false;
        }

        //spinny pot
        //faster spinning and hopping
        [HarmonyPatch(typeof(PotMimicMelee), "Start")]
        [HarmonyPrefix]
        public static bool Start_pre(PotMimicMelee __instance)
        {
            __instance.spinSpeed = 10;
            __instance.hopSpeed = 15;
            return true;
        }

        //magic pot
        //faster chaser orbs
        [HarmonyPatch(typeof(ChaserBullet), "Start")]
        [HarmonyPostfix]
        public static void Start_post(ChaserBullet __instance)
        {
            __instance.airTracking = 0.5f;
            __instance.trackingAccel = 2;
            __instance.shootSpeed = 16;
        }

        //smough
        //more turning and extra enemies
        [HarmonyPatch(typeof(AI_Knight), "FixedUpdate")]
        [HarmonyPostfix]
        public static void FixedUpdate_post(AI_Knight __instance, CharacterMoveRootAnim ___movement, Vector3 ___pDiff, Vector3 ___startTurnForward, bool ___turning180dir, bool ___turning180)
        {
            if (__instance.GetState() == AI_Brain.AIState.Attack)
            {
                Vector3 vector = PlayerGlobal.instance.transform.position - __instance.transform.position;
                var cancel = false;
                if (___turning180)
                {
                    float num = Vector3.Dot(___startTurnForward, ___pDiff.normalized);
                    Debug.Log("LHS: " + num);
                    if (num < 0f)
                    {
                        if (___turning180dir)
                        {
                            Debug.Log("LHS: CANCEL");
                            cancel = true;
                        }
                    }
                    else if (!___turning180dir)
                    {
                        Debug.Log("LHS: CANCEL");
                        cancel = true;
                    }
                }
                if (!cancel)
                {
                    float num2 = Mathf.Atan2(vector.normalized.x, vector.normalized.z) * 57.29578f;
                    __instance.transform.rotation = Quaternion.Euler(0f, Mathf.LerpAngle(__instance.transform.rotation.eulerAngles.y, num2, Time.fixedDeltaTime * 4), 0f);
                    ___movement.AbsorbCurrentAngle();

                }
            }

            //extra enemies
            if (__instance.gameObject.transform.localScale != Vector3.one * 0.91f && __instance.GetState() != AI_Brain.AIState.Idle)
            {
                Log.LogWarning("spawning grunts");
                __instance.gameObject.transform.localScale = Vector3.one * 0.91f;
                var gruntFab = Resources.FindObjectsOfTypeAll<AI_Grunt>()[0].gameObject;
                if (gruntFab != null)
                {
                    var newGrunt = Instantiate(gruntFab, __instance.gameObject.transform.position + new Vector3(-5, 0, 0), Quaternion.identity);
                    newGrunt.GetComponent<AI_Brain>().alwaysKnowWherePlayerIs = true;
                    newGrunt = Instantiate(gruntFab, __instance.gameObject.transform.position + new Vector3(5, 0, 0), Quaternion.identity);
                    newGrunt.GetComponent<AI_Brain>().alwaysKnowWherePlayerIs = true;
                    newGrunt = Instantiate(gruntFab, __instance.gameObject.transform.position + new Vector3(0, 0, 5), Quaternion.identity);
                    newGrunt.GetComponent<AI_Brain>().alwaysKnowWherePlayerIs = true;
                }
            }
        }

        //faster attacks
        [HarmonyPatch(typeof(AI_Knight), "Anim_StartAttackAim")]
        [HarmonyPrefix]
        public static bool Anim_StartAttackAim_pre(ref float value)
        {
            value *= 2;
            return true;
        }


        //lickitung
        //spawn mage bullets
        [HarmonyPatch(typeof(AI_GrimaceKnight), nameof(AI_GrimaceKnight.SetState), typeof(AI_Brain.AIState))]
        [HarmonyPostfix]
        public static void SetState_post(AI_GrimaceKnight __instance, AI_Brain.AIState newState)
        {
            if (newState == AI_Brain.AIState.SwordSlam)
            {
                if (LoadThings("mb")[0])
                {
                    var dir = __instance.gameObject.transform.forward;
                    dir.y = 0.3f;
                    GameObject bullet;
                    //var amount = UnityEngine.Random.Range(6, 12);
                    var amount = 12;
                    for (int a = 0; a <= 360; a += 360 / amount)
                    {
                        bullet = Instantiate(mageBulletPrefab, __instance.gameObject.transform.position + dir * 2, Quaternion.identity);
                        bullet.GetComponent<Bullet>().Shoot(Quaternion.Euler(0, a, 0) * __instance.gameObject.transform.forward, 4);
                    }
                }
            }
        }

        //explosive pot
        //always chase
        [HarmonyPatch(typeof(PotHermit), "Start")]
        [HarmonyPostfix]
        public static void Start_post(PotHermit __instance)
        {
            __instance.alwaysKnowWherePlayerIs = true;
        }

        //slime
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
                    if (LoadThings("mediumSlime")[0])
                    {
                        __instance.gameObject.SetActive(false);
                        var newSlime = Instantiate(mediumSlime, __instance.gameObject.transform.position, Quaternion.identity, __instance.transform.parent);
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
            if (!LoadThings("smallSlime")[0] || __instance.size != AI_Slime.SlimeSize.Medium || !__result || __instance.gameObject.GetComponent<DamageableCharacter>().bloodVolumeSpawnOffset.x == 0.1f)
            {
                return;
            }
            var projectileSlime = Instantiate(smallSlime, __instance.gameObject.transform.position + Vector3.up * 5, Quaternion.identity);
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


        //plague knight

        //shoot 3
        [HarmonyPatch(typeof(AI_Plague), "Anim_ShootBomb")]
        [HarmonyPrefix]
        public static bool Anim_ShootBomb_pre(AI_Plague __instance, Vector3 ___bombSpawnPos, Quaternion ___bombSpawnRotation, ref float ___timer)
        {
            var angle = UnityEngine.Random.Range(0, 2*Mathf.PI);
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
