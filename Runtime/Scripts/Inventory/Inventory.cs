using UnityEngine;
using System;
using System.Collections.Generic;

namespace PuzzleBox
{
    // Inventory�N���X�̓A�C�e���̃R���N�V�������Ǘ����܂��B
    // �A�C�e���̒ǉ��A�폜�A�m�F�A�J�E���g���s�����Ƃ��ł��܂��B
    public class Inventory : MonoBehaviour
    {
        public Action<string, int> OnItemAdded;

        // �A�C�e���Ƃ��̐����i�[����f�B�N�V���i���B
        // �L�[�̓A�C�e���̎�ށi������j�A�l�̓A�C�e���̐��i�����j�B
        private Dictionary<string, int> items = new Dictionary<string, int>();

        // �A�C�e�����C���x���g���ɒǉ����܂��B
        // �p�����[�^:
        // - type: �ǉ�����A�C�e���̎�ށB
        // - count: �ǉ�����A�C�e���̐��i�f�t�H���g��1�j�B
        public void AddItem(string type, int count = 1)
        {
            // �A�C�e���̎�ނ��C���x���g���ɑ��݂��邩�m�F���܂��B
            if (!items.ContainsKey(type))
            {
                // ���݂��Ȃ��ꍇ�A�w�肳�ꂽ���ŃA�C�e���̎�ނ�ǉ����܂��B
                items.Add(type, count);
            }
            else
            {
                // ���݂���ꍇ�A�w�肳�ꂽ�������A�C�e���̐��𑝂₵�܂��B
                items[type] += count;
            }

            // �A�C�e�����ǉ����ꂽ���Ƃ�ʒm���܂��B
            OnItemAdded?.Invoke(type, count);
        }

        // �A�C�e�����C���x���g������폜���܂��B
        // �p�����[�^:
        // - type: �폜����A�C�e���̎�ށB
        // - count: �폜����A�C�e���̐��i�f�t�H���g��1�j�B
        public void RemoveItem(string type, int count = 1)
        {
            // �A�C�e���̎�ނ��C���x���g���ɑ��݂��邩�m�F���܂��B
            if (items.ContainsKey(type))
            {
                // ���݂̐����폜���鐔��葽���ꍇ�A
                // �w�肳�ꂽ�������A�C�e���̐������炵�܂��B
                if (items[type] > count)
                {
                    items[type] -= count;
                }
                else
                {
                    // ���݂̐����폜���鐔�ȉ��̏ꍇ�A
                    // �C���x���g������A�C�e���̎�ނ��폜���܂��B
                    items.Remove(type);
                }
            }
        }

        // �C���x���g���ɓ���̃A�C�e���̎�ނ��܂܂�Ă��邩�m�F���܂��B
        // �p�����[�^:
        // - type: �m�F����A�C�e���̎�ށB
        // �߂�l: �A�C�e���̎�ނ����݂��A����0���傫���ꍇ��true�A����ȊO�̏ꍇ��false�B
        public bool HasItem(string type)
        {
            return items.ContainsKey(type) && items[type] > 0;
        }

        // �C���x���g�����̓���̃A�C�e���̎�ނ̐����擾���܂��B
        // �p�����[�^:
        // - type: �����擾����A�C�e���̎�ށB
        // �߂�l: �A�C�e���̎�ނ̐��A�܂��̓A�C�e���̎�ނ����݂��Ȃ��ꍇ��0�B
        public int GetItemCount(string type)
        {
            // �w�肳�ꂽ�A�C�e���̎�ނ��C���x���g���ɑ��݂��邩�m�F���܂��B
            if (items.ContainsKey(type))
            {
                return items[type];
            }
            else
            {
                // ���݂��Ȃ��ꍇ��0��Ԃ��܂��B
                return 0;
            }
        }
    }
}
