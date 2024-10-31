using System;
using UnityEngine;
using Modding;
using Modding.Blocks;
using BlockVersionChanger;

namespace BlockVersionChanger
{
	public class Mod : ModEntryPoint
	{
		public override void OnLoad()
		{
			// events
			Events.OnBlockInit += OnBlockInit; //�u���b�N�ݒu���ɃC�x���g����
		}

        /// <summary>
        /// �u���b�N�ݒu���ɌĂ΂��֐�
        /// </summary>
        /// <param name="block">�ݒu�����u���b�N</param>
		private void OnBlockInit(Block block) {

            BlockBehaviour targetComponent = null;
            BlockType type = block.InternalObject.Prefab.Type;

            bool hasVersion = true;
            bool hasAltCollider = false;

            //version�����u���b�N�Ƀo�[�W�����ύX�p�N���X��ǉ�����
			switch(block.InternalObject.Prefab.Type){
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
            if(targetComponent != null)
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
