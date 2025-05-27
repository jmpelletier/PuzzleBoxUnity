using UnityEngine;
using System;
using System.Collections.Generic;

namespace PuzzleBox
{
    // Inventoryクラスはアイテムのコレクションを管理します。
    // アイテムの追加、削除、確認、カウントを行うことができます。
    public class Inventory : MonoBehaviour
    {
        public Action<string, int> OnItemAdded;

        // アイテムとその数を格納するディクショナリ。
        // キーはアイテムの種類（文字列）、値はアイテムの数（整数）。
        private Dictionary<string, int> items = new Dictionary<string, int>();

        // アイテムをインベントリに追加します。
        // パラメータ:
        // - type: 追加するアイテムの種類。
        // - count: 追加するアイテムの数（デフォルトは1）。
        public void AddItem(string type, int count = 1)
        {
            // アイテムの種類がインベントリに存在するか確認します。
            if (!items.ContainsKey(type))
            {
                // 存在しない場合、指定された数でアイテムの種類を追加します。
                items.Add(type, count);
            }
            else
            {
                // 存在する場合、指定された数だけアイテムの数を増やします。
                items[type] += count;
            }

            // アイテムが追加されたことを通知します。
            OnItemAdded?.Invoke(type, count);
        }

        // アイテムをインベントリから削除します。
        // パラメータ:
        // - type: 削除するアイテムの種類。
        // - count: 削除するアイテムの数（デフォルトは1）。
        public void RemoveItem(string type, int count = 1)
        {
            // アイテムの種類がインベントリに存在するか確認します。
            if (items.ContainsKey(type))
            {
                // 現在の数が削除する数より多い場合、
                // 指定された数だけアイテムの数を減らします。
                if (items[type] > count)
                {
                    items[type] -= count;
                }
                else
                {
                    // 現在の数が削除する数以下の場合、
                    // インベントリからアイテムの種類を削除します。
                    items.Remove(type);
                }
            }
        }

        // インベントリに特定のアイテムの種類が含まれているか確認します。
        // パラメータ:
        // - type: 確認するアイテムの種類。
        // 戻り値: アイテムの種類が存在し、数が0より大きい場合はtrue、それ以外の場合はfalse。
        public bool HasItem(string type)
        {
            return items.ContainsKey(type) && items[type] > 0;
        }

        // インベントリ内の特定のアイテムの種類の数を取得します。
        // パラメータ:
        // - type: 数を取得するアイテムの種類。
        // 戻り値: アイテムの種類の数、またはアイテムの種類が存在しない場合は0。
        public int GetItemCount(string type)
        {
            // 指定されたアイテムの種類がインベントリに存在するか確認します。
            if (items.ContainsKey(type))
            {
                return items[type];
            }
            else
            {
                // 存在しない場合は0を返します。
                return 0;
            }
        }
    }
}
