using System;
using System.Collections.Generic;
using UnityEngine;
using Modding;


namespace BlockVersionChanger
{
    class VersionChanger : MonoBehaviour
    {
        //UI表示用のテキストリスト
        public static readonly List<String> VersionList = new List<string>() { "BlockVer 0", "BlockVer 1" };
        //バージョン変更用のUI
        public MMenu VersionMenu; 

        private BlockBehaviour targetComponent = null; //versionの入っているコンポーネント
        private BlockType blockType; //ブロックの名前


        void Start()
        {
            //設定UI追加
            if (targetComponent != null)
            {
                VersionMenu = targetComponent.AddMenu("version", GetVersion(), VersionList, false);
                VersionMenu.ValueChanged += VersionMenu_ValueChanged;
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
            BlockBehaviour blockBehaviour; //コピー後のブロック

            BlockInfo blockInfo = BlockInfo.FromBlockBehaviour(targetComponent);
            blockInfo.BlockData.Write("bmt-version", newVersion);//バージョン書き込み
            blockInfo.BlockData.WasLoadedFromFile = true; //ファイルから読み込んだ体にしないとversionが強制的に1になります

            machine.isLoadingInfo = true; //マシン情報更新中
            machine.RemoveBlock(targetComponent); //元のブロックを削除

            if(!machine.AddBlock(blockInfo, out blockBehaviour)) //バージョンを書き換えたブロックを設置
            {
                Debug.LogError("[BlockVersionChanger] ブロックコピーに失敗した！");
            }
            machine.isLoadingInfo = false;//マシン情報更新中
        }


        /// <summary>
        /// バージョン変更メニューのハンドラー
        /// </summary>
        /// <param name="value">入力値</param>
        private void VersionMenu_ValueChanged(int value)
        {
            if(blockType == BlockType.StartingBlock)
            {
                VersionMenu.SetValue(GetVersion());
                Debug.Log("[BlockVersionChanger] StartingBlockはバージョン変更不可能です。");
                return;
            }

            SetVersion(value);
        }
    }
}
