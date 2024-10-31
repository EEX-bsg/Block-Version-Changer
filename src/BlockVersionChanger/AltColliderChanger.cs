using System;
using System.Collections.Generic;
using UnityEngine;
using Modding;

namespace BlockVersionChanger
{
    class AltColliderChanger : MonoBehaviour
    {
        public MToggle optimiseColliderToggle;

        private BlockBehaviour targetComponent = null;

        void Start()
        {
            if (targetComponent != null)
            {
                optimiseColliderToggle = targetComponent.AddToggle("Enable Mesh Col", "opt-collider", GetOptColliderValue());
                optimiseColliderToggle.Toggled += optimiseColliderToggle_Toggled;
            }
        }

        /// <summary>
        /// 初期化関数
        /// </summary>
        /// <param name="component">ターゲットになるコンポーネント</param>
        public void InitializeComponent(BlockBehaviour component)
        {
            targetComponent = component;
        }

        /// <summary>
        /// コライダーモードを取得します
        /// XDataHolderから直接もってきます
        /// </summary>
        /// <returns></returns>
        public bool GetOptColliderValue()
        {
            if (!targetComponent.isSimulating)
            {
                XDataHolder data = targetComponent.LastState;
                if (data.HasKey("bmt-opt-collider"))
                {
                    return data.ReadBool("bmt-opt-collider");
                }
            }
            return false;
        }

        /// <summary>
        /// 強制コライダー変更
        /// バージョンデータを上書きしたデータでブロックを再設置して強制上書きしています
        /// </summary>
        /// <param name="isMeshCol">上書きするコライダーモード</param>
        //public void SetOptColliderValue(bool isMeshCol)
        //{
        //    if (targetComponent == null) return;
        //    if (targetComponent.isSimulating) return;
        //    if (isMeshCol == GetOptColliderValue()) return;

        //    Machine machine = targetComponent.ParentMachine; //親マシン
        //    BlockBehaviour blockBehaviour; //コピー後のブロック

        //    BlockInfo blockInfo = BlockInfo.FromBlockBehaviour(targetComponent);
        //    blockInfo.BlockData.Write("bmt-opt-collider", isMeshCol);//バージョン書き込み
        //    blockInfo.BlockData.WasLoadedFromFile = true; //ファイルから読み込んだ体にしないとversionが強制的に1になります

        //    machine.isLoadingInfo = true; //マシン情報更新中

        //    machine.RemoveBlock(targetComponent); //元のブロックを削除

        //    if (!machine.AddBlock(blockInfo, out blockBehaviour)) //バージョンを書き換えたブロックを設置
        //    {
        //        Debug.LogError("[BlockVersionChanger] ブロックコピーに失敗した！");
        //    }
        //    machine.isLoadingInfo = false;//マシン情報更新中
        //}

        /// <summary>
        /// コライダー変更トグルのハンドラー
        /// </summary>
        /// <param name="value">入力値</param>
        private void optimiseColliderToggle_Toggled(bool value)
        {
            optimiseColliderToggle.SetValue(value);

            //どうやら、同名スライダーを追加する事で乗っ取りが出来たようで、
            //ブロック置き換えしなくてもコライダー切り替えが出来てしまった。
            //versionの場合はそもそもスライダーもないので無理です。
            //SetOptColliderValue(value);
        }
    }
}
