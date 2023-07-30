using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace rework.GUI
{
    public static class MainMenuGUI
    {
        //it's the gui class but I also put the effect here cause it's convenient and I'm lazy

        public static bool hasInit = false;

        public static GUIBox.GUIBox gui;

        public static GUIBox.OptionCategory text = new GUIBox.OptionCategory(title:"Swap to recent weapon bind: ", options:new GUIBox.BaseOption[0], titleSizeMultiplier:1); //yes it's annoying to have to use the title as text but whatever I'm not messing with GUIBox rn xD
        public static GUIBox.ButtonOption bindButton = new GUIBox.ButtonOption("unbound", emptySpaceMultiplier:2);

        public static KeyCode bindKey = KeyCode.None;
        public static bool currentlyBinding = false;

        public static void Init()
        {
            var bcat = new GUIBox.OptionCategory(options:new GUIBox.BaseOption[] { bindButton });

            var cat = new GUIBox.HorizontalOptionCategory(subCategories: new GUIBox.OptionCategory[] { text, bcat });

            gui = new GUIBox.GUIBox(new UnityEngine.Vector2(0.025f, 0.05f), cat);
        }

        public static void OnGui() //unity won't call since it's static but using exact name for indication of where to call
        {
            if (SceneManager.GetActiveScene().name != "TitleScreen") { return; }
            if (!hasInit) { Init(); }

            gui.OnGUI();

            if (bindButton.IsPressed() && !currentlyBinding)
            {
                currentlyBinding = true;
                bindButton.text = "waiting...";
                bindKey = KeyCode.None;
            }
        }

        public static void Update()
        {
            if (PlayerGlobal.instance != null)
            {
                var wc = PlayerGlobal.instance.gameObject.GetComponent<WeaponControl>();
                if (Rework.lastWeapon && !currentlyBinding && Input.GetKeyDown(bindKey) && wc != null && !wc.IsAnyAttackActive())
                {
                    wc.SetWeapon(PlayerEquipment.PlayerEquipSlot.RightHand, Rework.lastWeapon);
                }
            }
            
            if (currentlyBinding)
            {
                foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
                {
                    if (k != KeyCode.None && Input.GetKeyDown(k))
                    {
                        bindKey = k;
                        bindButton.text = k.ToString();
                        currentlyBinding = false;
                        return;
                    }
                }
            }
        }
    }
}
