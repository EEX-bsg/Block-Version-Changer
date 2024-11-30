using System;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using Modding.Blocks;
using Besiege.UI;
using Besiege.UI.Serialization;
using UnityEngine.UI;

namespace BlockVersionChanger
{
    public class ModController : MonoBehaviour
    {
        private bool isEnglish = true;

        // ローカライズテキスト
        private readonly Dictionary<string, string> texts = new Dictionary<string, string>
        {
            {"title_en", "Attempting to downgrade block version!"},
            {"title_jp", "ブロックバージョンを下げようとしています！"},
            {"contents_en", "Older versions may contain bugs and \nmay become unavailable with future Besiege updates. \n \nThe following blocks contain bugs and have no major behavioral changes, \nso version changes are not recommended: \nStarting Block, Bomb, Grenade, Sail \n \nPlease change versions at your own risk. \nNeither this Mod creator (EEX-slime) nor Spiderling Studio \nis responsible for any machine malfunctions or user disadvantages caused by this."},
            {"contents_jp", "古いバージョンはバグを含む可能性があり、 \nまた将来のBesiegeアップデートで使用できなくなる場合があります。 \n \n以下のブロックはバグを含み、大きな挙動変更はないため、 \nバージョンの変更が推奨されていません。 \nスタートブロック, ボム, リモートグレネード, 帆  \n \nユーザーの自己責任でバージョンを変更してください。  \nそれによって生じたマシンの不具合や、ユーザーの不利益に対して  \n本Mod制作者(EEX-slime) 及びSpiderling Studioは一切の責任を負いません。"},
            {"hide-warning-toggle_en", "I understand and don't show this warning again"},
            {"hide-warning-toggle_jp", "内容を理解し、今後警告を出さない"},
            {"up-ver-button_en", "Update version(V1+)"},
            {"up-ver-button_jp", "バージョンを最新にする(v1+)"},
            {"down-ver-button_en", "Downgrade version(v0)"},
            {"down-ver-button_jp", "バージョンを戻す(v0)"},
        };


        void Awake()
        {
            if(!Mod.NotNeedUIFactory)
            {
                //UIFactory Setup
                Make.RegisterSerialisationProvider(Mod.ModName, new SerializationProvider
                {
                    CreateText = c => Modding.ModIO.CreateText(c),
                    GetFiles = c => Modding.ModIO.GetFiles(c),
                    ReadAllText = c => Modding.ModIO.ReadAllText(c),
                    AllResourcesLoaded = () => ModResource.AllResourcesLoaded,
                    OnAllResourcesLoaded = e => ModResource.OnAllResourcesLoaded += e
                });

                isEnglish = Mod.isEnglish;
            }

            // events
            Events.OnBlockInit += OnBlockInit; //ブロック設置時にイベント発火
        }

        /// <summary>
        /// ブロック設置時に呼ばれる関数
        /// </summary>
        /// <param name="block">設置したブロック</param>
        private void OnBlockInit(Block block)
        {

            BlockBehaviour targetComponent = null;
            BlockType type = block.InternalObject.Prefab.Type;

            bool hasVersion = true;
            bool hasAltCollider = false;

            //UIFactoryのプロジェクトロード
            //別にOnBlockInitでやらなくても良いけど、案外どこでも大丈夫そう
            //OnSceneChangedが安牌ではある(公式推奨)
            if (!Mod.NotNeedUIFactory && Mod.UIPrefab_WarningVersionDown == null)
            {
                //Make.OnReadyを使うと、UIFactoryがロード完了してる時に実行してくれるようになります。神
                //ロードが間に合ってない場合は、ロード終わってから実行してくれる...はず
                Make.OnReady(Mod.ModName, () =>
                {
                    GameObject projectObj = Make.LoadProject(Mod.ModName, "WarningVersionDown").gameObject;
                    Mod.UIPrefab_WarningVersionDown = projectObj;
                    Mod.UIPrefab_WarningVersionDown.SetActive(false);//雛形なので非表示で保持

                    //ローカライズ
                    projectObj.GetComponent<Project>()["Header_Text"].gameObject.GetComponent<Text>().text = texts[isEnglish? "title_en" : "title_jp"];
                    projectObj.GetComponent<Project>()["Contents_Text"].gameObject.GetComponent<Text>().text = texts[isEnglish? "contents_en" : "contents_jp"];
                    projectObj.GetComponent<Project>()["HideWarningToggle_Text"].gameObject.GetComponent<Text>().text = texts[isEnglish? "hide-warning-toggle_en" : "hide-warning-toggle_jp"];
                    projectObj.GetComponent<Project>()["UpVerButton_Text"].gameObject.GetComponent<Text>().text = texts[isEnglish? "up-ver-button_en" : "up-ver-button_jp"];
                    projectObj.GetComponent<Project>()["DownVerButton_Text"].gameObject.GetComponent<Text>().text = texts[isEnglish? "down-ver-button_en" : "down-ver-button_jp"];
                });
            }

            //versionを持つブロックにバージョン変更用クラスを追加する
            switch (block.InternalObject.Prefab.Type)
            {
                case BlockType.Bomb:
                    targetComponent = block.GameObject.GetComponent<ExplodeOnCollideBlock>();
                    break;
                case BlockType.Grenade:
                    targetComponent = block.GameObject.GetComponent<ControllableBomb>();
                    break;
                case BlockType.Wheel:
                case BlockType.LargeWheel:
                    targetComponent = block.GameObject.GetComponent<CogMotorControllerHinge>();
                    hasAltCollider = true;
                    break;
                case BlockType.CogMediumPowered:
                    targetComponent = block.GameObject.GetComponent<CogMotorControllerHinge>();
                    break;
                case BlockType.WheelUnpowered:
                case BlockType.LargeWheelUnpowered:
                    targetComponent = block.GameObject.GetComponent<FreeWheel>();
                    hasVersion = false;
                    hasAltCollider = true;
                    break;
                case BlockType.BuildSurface:
                    targetComponent = block.GameObject.GetComponent<BuildSurface>();
                    break;
                case BlockType.Sail:
                    targetComponent = block.GameObject.GetComponent<SailBlock>();
                    break;
                case BlockType.StartingBlock:
                    targetComponent = block.GameObject.GetComponent<SourceBlock>();
                    break;
                case BlockType.DoubleWoodenBlock:
                case BlockType.WoodenPole:
                case BlockType.Log:
                    targetComponent = block.GameObject.GetComponent<ShorteningBlock>();
                    break;
                case BlockType.WoodenPanel:
                    targetComponent = block.GameObject.GetComponent<ArmorBlock>();
                    break;
            }
            if (targetComponent != null)
            {
                //バージョンタグを持っているブロック
                if (hasVersion)
                {
                    VersionChanger versionChanger = block.GameObject.AddComponent<VersionChanger>();
                    versionChanger.InitializeComponent(targetComponent, type);
                }
                //代替コライダーを持つブロック(ホイール系)
                //無動力(大)ホイールはversionは無いけどこっちはある。逆にサフェはversionで管理
                if (hasAltCollider)
                {
                    AltColliderChanger altColliderChanger = block.GameObject.AddComponent<AltColliderChanger>();
                    altColliderChanger.InitializeComponent(targetComponent);
                }
            }
        }
    }
}