using System;
using UnityEngine;
using Modding;
using Modding.Blocks;
using BlockVersionChanger;
using System.Reflection;
using Besiege.UI;
using Besiege.UI.Bridge;
using Besiege.UI.Serialization;

namespace BlockVersionChanger
{
    public class ModController : MonoBehaviour {
        void Awake()
        {
            //UIFactory Setup
            Make.RegisterSerialisationProvider(Mod.ModName, new SerializationProvider {
                CreateText = c => Modding.ModIO.CreateText(c),
                GetFiles = c => Modding.ModIO.GetFiles(c),
                ReadAllText = c => Modding.ModIO.ReadAllText(c),
                AllResourcesLoaded = () => ModResource.AllResourcesLoaded,
                OnAllResourcesLoaded = e => ModResource.OnAllResourcesLoaded += e
            });


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
                case BlockType.CogMediumPowered:
                    targetComponent = block.GameObject.GetComponent<CogMotorControllerHinge>();
                    hasAltCollider = true;
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
                if (hasVersion)
                {
                    VersionChanger versionChanger = block.GameObject.AddComponent<VersionChanger>();
                    versionChanger.InitializeComponent(targetComponent, type);
                }
                if (hasAltCollider)
                {
                    AltColliderChanger altColliderChanger = block.GameObject.AddComponent<AltColliderChanger>();
                    altColliderChanger.InitializeComponent(targetComponent);
                }
            }
        }
    }
}