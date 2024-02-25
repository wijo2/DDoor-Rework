using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace rework
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Rework : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        private const string pluginGuid = "ddoor.gameRework.wijo";
        private const string pluginName = "Game Rework";
        private const string pluginVersion = "0.1.5";

        public static GameObject mageBulletPrefab;
        public static GameObject arrowPrefab;
        public static GameObject daggerModel;
        public static GameObject umbrellaModel;
        public static GameObject mediumSlime;
        public static GameObject smallSlime;

        public static string lastWeaponId;

		public static Type[] patchTypes = new Type[]
		{
			typeof(Rework),
			typeof(Categories.SpellMods),
			typeof(Categories.WeaponMods),
			typeof(Categories.Enemies.BatMods),
			typeof(Categories.Enemies.DFSMods),
			typeof(Categories.Enemies.GOTDMods),
			typeof(Categories.Enemies.GranMods),
			typeof(Categories.Enemies.MageMods),
			typeof(Categories.Enemies.GruntMods),
			typeof(Categories.Enemies.ArcherMods),
			typeof(Categories.Enemies.ExplosivePotMods),
			typeof(Categories.Enemies.HeadRollerMods),
			typeof(Categories.Enemies.LickitungMods),
			typeof(Categories.Enemies.MagicPotMods),
			typeof(Categories.Enemies.PlagueKnightMods),
			typeof(Categories.Enemies.SlimeMods),
			typeof(Categories.Enemies.SmoughMods),
			typeof(Categories.Enemies.SpinnyPotMods),
		};

        public void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(pluginGuid);
			foreach (var i in patchTypes)
			{
				harmony.PatchAll(i);
			}
        }

        public void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Categories.Enemies.GranMods.grandmaClones.Clear();
            GrandmaClone.Reset();
            LoadThings(new string[] { "all" });
        }

        public static void L(string s) { Log.LogWarning(s); }

        //asset gather
        public static List<bool> LoadThings(params string[] things)
        {
            if (things[0] == "all")
            {
                things = new string[] { "mb", "ga", "dm", "um", "mediumSlime", "smallSlime" };
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
                                    daggerModel.SetActive(false);
                                    break;
                                }
                            }
                        }
                        returns.Add(daggerModel != null);
                        break;

                    case "um":
                        if (umbrellaModel == null)
                        {
                            foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
                            {
                                if (obj.name == "WEAPON_Umbrella")
                                {
                                    umbrellaModel = Instantiate(obj.GetComponentInChildren<Animator>().gameObject, null);
                                    umbrellaModel.SetActive(false);
                                    break;
                                }
                            }
                        }
                        returns.Add(umbrellaModel != null);
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
                        L("LoadThings string \"" + thing + "\" doesn't exist");
                        break;
                }
            }
            return returns;
        }

        //gui and weapon swap

        public void Update()
        {
            Categories.Enemies.DFSMods.canShoot = true;
            GUI.MainMenuGUI.Update();
        }

        public void OnGUI() => GUI.MainMenuGUI.OnGui();

        [HarmonyPatch(typeof(WeaponControl), "SetWeapon")]
        [HarmonyPrefix]
        public static void SetWeapon_pre(WeaponControl __instance, InventoryItem weaponItem)
        {
            if (__instance.weaponRight == null || __instance.weaponRight.id == weaponItem.id) { return; }
            lastWeaponId = __instance.weaponRight.id;
        }
    }
}
