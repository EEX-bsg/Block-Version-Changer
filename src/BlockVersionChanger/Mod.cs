using System;
using UnityEngine;
using Modding;
using BlockVersionChanger.UI;
using Localisation;

namespace BlockVersionChanger
{
    public class Mod : ModEntryPoint
    {
        /** Modで保持するオブジェクトとか */
        public static GameObject ModController; //このModの管理オブジェクト
        public static GameObject UIPrefab_WarningVersionDown; //バージョン変更警告UIの雛形

        /** Modで保持する定数 */
        public static readonly string ModName = "BlockVersionChanger";//Mod名(実質IDだから変更はしないほうが良い)
        //UIFactoryの諸々情報(前提Mod判定と警告UIで使う)
        public static class UIFactory 
        {
            public static readonly string name = "UIFactory";
            public static readonly Guid id = new Guid("61d89dcf-88a2-4a16-8eb2-08aeed441f1d");
            public static readonly string workshopUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=2913469777";
        }

        /** パブリックなフラグとか変数とか */
        public static bool isUIFactory = false; //UIFactoryの導入チェック
        public static bool isEnglish = true; //日本語以外は全部英語

        private static bool initialised = false; //コンフィグ読み込み用

        // バージョンダウンの警告UIを表示するかの管理フラグ
        // コンフィグファイル対応(Besiege終了時に反映される)
        private static bool _doNotShowWarning = false;
        public static bool DoNotShowWarning
        {
            get
            {
                if(!initialised)
                {
                    //コンフィグファイルがあればその値、なければfalse(デフォ値)  --を省略した式
                    _doNotShowWarning = Configuration.GetData().HasKey("HideDowngradeWarning") && Configuration.GetData().ReadBool("HideDowngradeWarning");
                    initialised = true;
                }
                return _doNotShowWarning;
            }
            set
            {
                if(value.Equals(_doNotShowWarning)) return;
                _doNotShowWarning = value;
                Configuration.GetData().Write("HideDowngradeWarning", value);
            }
        }


        public override void OnLoad()
        {
            //Mod管理用オブジェクトの作成、シーンまたいでも消えないように設定
            UnityEngine.Object.DontDestroyOnLoad(ModController = new GameObject(ModName));

            //日本語以外は英語設定
            if(SingleInstance<LocalisationManager>.Instance.currLangName == "日本語"){
                isEnglish = false;
            }

            //UIFactoryがあれば、Mod読み込み用クラスをロード(ModController.cs)
            //なければ前提Modが足りない事を伝える警告UIを表示
            //※これを実現するために、Mod.csで一切UIFactory関連に触れてはいけない(エラー出る)
            isUIFactory = Mods.IsModLoaded(UIFactory.id);
            if(isUIFactory){
                ModController.AddComponent<ModController>();
            }else{
                ModController.AddComponent<ModRequirementNotice>();
            }
        }
    }
}
