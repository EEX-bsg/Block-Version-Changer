using System;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using Modding.Blocks;
using BlockVersionChanger;
using BlockVersionChanger.UI;
using Localisation;

namespace BlockVersionChanger
{
    public class Mod : ModEntryPoint
    {
        //Modで保持するオブジェクトとか
        public static GameObject ModController;
        //Modで保持する定数
        public static readonly string ModName = "BlockVersionChanger";
        public static class UIFactory
        {
            public static readonly string name = "UIFactory";
            public static readonly Guid id = new Guid("61d89dcf-88a2-4a16-8eb2-08aeed441f1d");
            public static readonly string workshopUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=2913469777";
        }

        //パブリックなフラグとか変数とか
        public static bool isUIFactory = false;
        public static bool isEnglish = true;


        public override void OnLoad()
        {
            UnityEngine.Object.DontDestroyOnLoad(ModController = new GameObject(ModName));
            isUIFactory = Mods.IsModLoaded(UIFactory.id);
            if(SingleInstance<LocalisationManager>.Instance.currLangName == "日本語"){
                isEnglish = false;
            }

            if(isUIFactory){
                ModController.AddComponent<ModController>();
            }else{
                ModController.AddComponent<ModRequirementNotice>();
            }
        }
    }
}
