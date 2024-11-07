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
        /// コライダー変更トグルのハンドラー
        /// </summary>
        /// <param name="value">入力値</param>
        private void optimiseColliderToggle_Toggled(bool value)
        {
            //どうやら、同名スライダーを追加する事で乗っ取りが出来たようで、
            //ブロック置き換えしなくてもコライダー切り替えが出来てしまった。
            //versionの場合はそもそもスライダーもないので無理です。
            optimiseColliderToggle.SetValue(value);
        }
    }
}
