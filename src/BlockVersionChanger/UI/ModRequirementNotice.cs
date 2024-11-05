using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Modding;
using BlockVersionChanger;


namespace BlockVersionChanger.UI
{
    public class ModRequirementNotice : MonoBehaviour
    {
        // ウィンドウ設定
        private Rect windowRect;
        private int windowId;
        private bool isEnglish = true;
        private bool isSetupStyle = false;

        // GUIスタイルのキャッシュ
        private GUIStyle headerStyle;
        private GUIStyle contentStyle;
        private GUIStyle buttonStyle;
        private GUIStyle linkButtonStyle;
        private GUIStyle modNameStyle;

        // 背景用テクスチャ
        private Texture2D darkBgTexture;
        private Texture2D buttonNormalTexture;
        private Texture2D buttonHoverTexture;
        private Texture2D linkButtonNormalTexture;
        private Texture2D linkButtonHoverTexture;

        // ローカライズテキスト
        private readonly Dictionary<string, string> texts = new Dictionary<string, string>
        {
            {"header_en", "Required Mods Notice"},
            {"header_jp", "前提 Modを導入して！"},
            {"content_en", "BlockVersionChanger requires the following prerequisite mod(s). \nPlease subscribe to them on Steam Workshop and enable them in the Mod menu."},
            {"content_jp", "BlockVersionChangerは以下の前提Modが必要だよ。\nSteam Workshopでサブスクライブして、 Modメニューから有効化してね。"},
            {"return_en", "Return to Title"},
            {"return_jp", "タイトルに戻る"},
            {"quit_en", "Quit Game"},
            {"quit_jp", "ゲームを終了"},
        };

        /// <summary>
        /// 初期化処理を行います
        /// </summary>
        private void Awake()
        {
            windowId = ModUtility.GetWindowId();
            isEnglish = Mod.isEnglish;

            // テクスチャの作成
            darkBgTexture = CreateColorTexture(new Color(0, 0, 0, 0.85f));
            buttonNormalTexture = CreateColorTexture(new Color(0.1f, 0.1f, 0.1f));
            buttonHoverTexture = CreateColorTexture(new Color(0.25f, 0.25f, 0.25f));
            linkButtonNormalTexture = CreateColorTexture(new Color(0.2f, 0.3f, 0.4f));
            linkButtonHoverTexture = CreateColorTexture(new Color(0.25f, 0.35f, 0.45f));
        }

        /// <summary>
        /// カスタムGUIスタイルを設定します
        /// </summary>
        private void SetupStyles()
        {
            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 15, 25),
                normal = { textColor = new Color(0.9f, 0.9f, 0.9f) }
            };

            contentStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                wordWrap = true,
                margin = new RectOffset(20, 20, 10, 10),
                normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                padding = new RectOffset(20, 20, 10, 10),
                margin = new RectOffset(10, 10, 5, 5),
                normal = {
                    background = buttonNormalTexture,
                    textColor = new Color(0.9f, 0.9f, 0.9f)
                },
                hover = {
                    background = buttonHoverTexture,
                    textColor = Color.white
                }
            };

            linkButtonStyle = new GUIStyle(buttonStyle)
            {
                fontSize = 16,
                padding = new RectOffset(10, 10, 5, 5),
                margin = new RectOffset(10, 10, 5, 5),
                normal = {
                    background = linkButtonNormalTexture,
                    textColor = new Color(0.9f, 0.9f, 0.9f)
                },
                hover = {
                    background = linkButtonHoverTexture,
                    textColor = Color.white
                }
            };

            modNameStyle = new GUIStyle(contentStyle)
            {
                fontSize = 16,
                padding = new RectOffset(0, 0, 3, 3)
            };
        }

        /// <summary>
        /// 単色のテクスチャを作成します
        /// </summary>
        private Texture2D CreateColorTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// GUIを描画します
        /// </summary>
        void OnGUI()
        {
            if (!StatMaster.isMainMenu)
            {
                if(!isSetupStyle)
                {
                    windowRect = new Rect(Screen.width / 2 - 350, Screen.height / 2 - 200, 700, 400);
                    SetupStyles();
                    isSetupStyle = true;
                }

                // 背景をクリックできないようにブロック
                GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "", GUIStyle.none);

                // 半透明の背景を描画
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), darkBgTexture);

                // メインウィンドウ
                windowRect = GUI.Window(windowId, windowRect, DrawWindow, string.Empty);
            }
        }

        /// <summary>
        /// ウィンドウの内容を描画します
        /// </summary>
        private void DrawWindow(int windowId)
        {
            // ヘッダー
            GUILayout.Label(texts[isEnglish ? "header_en" : "header_jp"], headerStyle);

            // メインコンテンツ領域
            GUILayout.BeginVertical(GUI.skin.box);

            // 説明文
            GUILayout.Label(texts[isEnglish ? "content_en" : "content_jp"], contentStyle);
            GUILayout.Space(20);

            // 前提Modリスト
            DrawRequiredMods();

            GUILayout.Space(40);

            GUILayout.EndVertical();

            // フッター部分（下部に固定）
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(texts[isEnglish ? "return_en" : "return_jp"], buttonStyle))
            {
                SceneManager.LoadScene("TITLE SCREEN");
            }

            GUILayout.Space(20);

            if (GUILayout.Button(texts[isEnglish ? "quit_en" : "quit_jp"], buttonStyle))
            {
                Application.Quit();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(15);

            // 言語切り替えボタン
            if (GUI.Button(new Rect(windowRect.width - 50, 10, 40, 25), isEnglish ? "JP" : "EN"))
            {
                isEnglish = !isEnglish;
            }
        }

        /// <summary>
        /// 前提Modの情報を描画します
        /// </summary>
        private void DrawRequiredMods()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(40);
            GUILayout.Label("・"+Mod.UIFactory.name, modNameStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Workshop Page", linkButtonStyle))
            {
                Application.OpenURL(Mod.UIFactory.workshopUrl);
            }
            GUILayout.Space(40);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// リソースの解放を行います
        /// </summary>
        private void OnDestroy()
        {
            if (darkBgTexture != null) Destroy(darkBgTexture);
            if (buttonNormalTexture != null) Destroy(buttonNormalTexture);
            if (buttonHoverTexture != null) Destroy(buttonHoverTexture);
            if (linkButtonNormalTexture != null) Destroy(linkButtonNormalTexture);
            if (linkButtonHoverTexture != null) Destroy(linkButtonHoverTexture);
        }
    }
}