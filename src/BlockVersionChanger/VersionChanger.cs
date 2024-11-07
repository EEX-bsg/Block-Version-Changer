using System;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using Besiege.UI.Extensions;
using Besiege.UI.Serialization;
using UnityEngine.UI;


namespace BlockVersionChanger
{
    class VersionChanger : MonoBehaviour
    {
        //UI表示用のテキストリスト
        public static readonly List<String> VersionList = new List<string>() { "BlockVer 0", "BlockVer 1" };
        //バージョン変更用のUI
        public MMenu VersionMenu;

        private GameObject WarningUI;//バージョン変更警告UI
        private GameObject BlockMapperObj;//このブロックの設定UI (設定閉じたの検知してUI消す用)
        private BlockBehaviour targetComponent = null; //versionの入っているコンポーネント
        private BlockType blockType; //ブロックの名前
        private Project project;

        //flag
        private bool isValueChangedBlocked = false; //一時的にversion値が変わってもその先の処理をスキップさせる


        void Start()
        {
            //設定UI追加
            if (targetComponent != null)
            {
                VersionMenu = targetComponent.AddMenu("version", GetVersion(), VersionList, false);
                VersionMenu.ValueChanged += VersionMenu_ValueChanged;
            }
        }

        void Update()
        {
            //シミュ開始した時、もしくはブロック設定UIが消えたときに、警告UIを消す
            if (targetComponent.isSimulating || (WarningUI && !BlockMapperObj))
            {
                DestroyUI();
            }
        }


        /// <summary>
        /// 初期化関数
        /// </summary>
        /// <param name="component">ターゲットになるコンポーネント</param>
        /// <param name="type">ターゲットのブロックタイプ</param>
        public void InitializeComponent(BlockBehaviour component, BlockType type)
        {
            targetComponent = component;
            blockType = type;
        }


        /// <summary>
        /// ブロックバージョンを取得する関数
        /// </summary>
        /// <returns>ブロックバージョン (0 or 1)</returns>
        public int GetVersion()
        {
            // BlockBehaviourとして各コンポーネントタイプごとにキャストして取得
            switch (blockType)
            {
                case BlockType.Bomb:
                    return ((ExplodeOnCollideBlock)targetComponent).version;
                case BlockType.Grenade:
                    return ((ControllableBomb)targetComponent).version;
                case BlockType.Wheel:
                case BlockType.LargeWheel:
                case BlockType.CogMediumPowered:
                    return ((CogMotorControllerHinge)targetComponent).version;
                case BlockType.BuildSurface:
                    return ((BuildSurface)targetComponent).version;
                case BlockType.Sail:
                    return ((SailBlock)targetComponent).version;
                case BlockType.WoodenPanel:
                    return ((ArmorBlock)targetComponent).version;

                //こいつらは真に不本意ながら、versionがprivate変数で宣言されている為、
                //XMLマシンデータから直で呼び出します。
                case BlockType.StartingBlock:
                case BlockType.DoubleWoodenBlock:
                case BlockType.WoodenPole:
                case BlockType.Log:
                    // 蜘蛛のコード見る限りは、シミュ中駄目らしい
                    // でも呼ばれるタイミング的にシミュ中であることはなさそう。
                    if (!targetComponent.isSimulating)
                    {
                        XDataHolder data = targetComponent.LastState;
                        if (data.HasKey("bmt-version"))
                        {
                            return data.ReadInt("bmt-version");
                        }
                        else
                        {
                            //Debug.LogError("[BlockVersionChanger] バージョン読み込めない！");
                        }
                    }
                    return 0;

                default:
                    Debug.LogError("[BlockVersionChanger] 未対応のブロックタイプです");
                    return 0;
            }
        }


        /// <summary>
        /// ブロックバージョンを設定する関数
        /// </summary>
        /// <param name="newVersion">設定するバージョンの値 (0 or 1)</param>
        public void SetVersion(int newVersion)
        {
            switch (blockType)
            {
                case BlockType.Bomb:
                    ((ExplodeOnCollideBlock)targetComponent).version = newVersion;
                    break;
                case BlockType.Grenade:
                    ((ControllableBomb)targetComponent).version = newVersion;
                    break;
                case BlockType.Wheel:
                case BlockType.LargeWheel:
                case BlockType.CogMediumPowered:
                    ((CogMotorControllerHinge)targetComponent).version = newVersion;
                    break;
                case BlockType.BuildSurface:
                    ((BuildSurface)targetComponent).version = newVersion;
                    break;
                case BlockType.Sail:
                    ((SailBlock)targetComponent).version = newVersion;
                    break;
                case BlockType.WoodenPanel:
                    ((ArmorBlock)targetComponent).version = newVersion;
                    break;
                case BlockType.StartingBlock:
                    Debug.Log("[BlockVersionChanger] StartingBlockはバージョン変更不可能です。");
                    break;
                case BlockType.DoubleWoodenBlock:
                case BlockType.WoodenPole:
                case BlockType.Log:
                    SetVersionFroced(newVersion);
                    break;
                default:
                    Debug.LogError("[BlockVersionChanger] 未対応のブロックタイプです");
                    break;
            }
        }

        /// <summary>
        /// 強制バージョン変更
        /// バージョンデータを上書きしたデータでブロックを再設置して強制上書きしています
        /// </summary>
        /// <param name="newVersion">上書きするバージョン</param>
        private void SetVersionFroced(int newVersion)
        {
            if (targetComponent == null) return;
            if (targetComponent.isSimulating) return;
            if (newVersion == GetVersion()) return;

            Machine machine = targetComponent.ParentMachine; //親マシン

            BlockInfo blockInfo = BlockInfo.FromBlockBehaviour(targetComponent);
            blockInfo.BlockData.Write("bmt-version", newVersion);//バージョン書き込み
            blockInfo.BlockData.WasLoadedFromFile = true; //ファイルから読み込んだ体にしないとversionが強制的に1になります

            machine.isLoadingInfo = true; //マシン情報更新中
            machine.RemoveBlock(targetComponent); //元のブロックを削除

            if (!machine.AddBlock(blockInfo, out _)) //バージョンを書き換えたブロックを設置
            {
                Debug.LogError("[BlockVersionChanger] ブロックコピーに失敗した！");
            }
            machine.isLoadingInfo = false;//マシン情報更新中
        }

        /// <summary>
        /// 警告UIの生成
        /// </summary>
        private void SetupWarningUI()
        {
            //ブロック設定UIを見つける、見つからなければ警告UIを生成しない。
            if (BlockMapperObj = GameObject.Find("BlockMapper - " + blockType.ToString()))
            {
                //警告UI生成
                WarningUI = Instantiate(Mod.UIPrefab_WarningVersionDown);
                WarningUI.name = "WarningVersionDown - " + blockType.ToString();
                WarningUI.transform.SetParent(HierarchyUtils.FindObject("Canvas"));//ここかUIFactory以外だと動かない(canvasが無い)
                WarningUI.transform.localPosition = new Vector3(0f, 0f, 0f);
                project = WarningUI.GetComponent<Project>();
                WarningUI.SetActive(true);
                project.RebuildTransformList();//お ま じ な い

                //イベント紐づけ
                project["UpVerButton"].gameObject.GetComponent<Button>().onClick.AddListener(UpVerButton_OnClicked);
                project["DownVerButton"].gameObject.GetComponent<Button>().onClick.AddListener(DownVerButton_OnClicked);

                //UI表示時に音を鳴らす
                ModAudioClip audioClip = ModResource.GetAudioClip("Warning");
                AudioSource audio = WarningUI.AddComponent<AudioSource>();
                audio.clip = audioClip;
                audio.Play();
            }
        }

        /// <summary>
        /// バージョンを1にする
        /// </summary>
        private void UpVerButton_OnClicked()
        {
            DestroyUI();//UI削除
            isValueChangedBlocked = true;//これやらないとまた警告UIでちゃう
            VersionMenu.Value = 1;//スライダー(XML)書き込み
            SetVersion(1);//ブロック書き込み
            UpdateDoNotShowWarning();
        }

        /// <summary>
        /// バージョンを0にする
        /// </summary>
        private void DownVerButton_OnClicked()
        {
            DestroyUI();//UI削除
            isValueChangedBlocked = true;//これやらないとまた警告UIでちゃう
            VersionMenu.Value = 0;//スライダー(XML)書き込み
            SetVersion(0);//ブロック書き込み
            UpdateDoNotShowWarning();
        }

        /// <summary>
        /// 警告UI非表示トグルの値を見てコンフィグ更新
        /// </summary>
        private void UpdateDoNotShowWarning()
        {
            Mod.DoNotShowWarning = project["HideWarningToggleButton"].GetComponent<Toggle>().isOn;
        }

        /// <summary>
        /// バージョン変更メニューのハンドラー
        /// </summary>
        /// <param name="value">入力値</param>
        private void VersionMenu_ValueChanged(int value)
        {
            //一時的に処理スキップ(UI呼び出しとかをしない為)
            if (isValueChangedBlocked)
            {
                isValueChangedBlocked = false;
                return;
            }
            //UIが表示されている場合はメニュー固定化
            if (WarningUI)
            {
                isValueChangedBlocked = true;
                VersionMenu.Value = GetVersion();
            }
            else
            {
                //コアブロの時もメニュー固定化(技術的にバージョン変更不可)
                if (blockType == BlockType.StartingBlock)
                {
                    isValueChangedBlocked = true;
                    VersionMenu.Value = GetVersion();
                    Debug.Log("[BlockVersionChanger] StartingBlockはバージョン変更不可能です。");
                    return;
                }
                //バージョンが1以上か、警告非表示の時はバージョン変更
                if (value >= 1 || Mod.DoNotShowWarning)
                {
                    SetVersion(value);
                    return;
                }
                //それ以外はUI表示 -> ボタンクリックでバージョン変更
                //一旦メニューの変更無効化
                isValueChangedBlocked = true;
                VersionMenu.Value = GetVersion();
                SetupWarningUI();
            }
        }

        private void DestroyUI()
        {
            Destroy(WarningUI);
        }

        //ブロック消える時にUIも消す
        void OnDestroy()
        {
            DestroyUI();
        }
    }
}
