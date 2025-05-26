using UnityEngine;

namespace PuzzleBox
{
    // Itemクラスはアイテムの動作を管理します。
    // アイテムがプレイヤーと衝突したときの処理を行います。
    public class Item : MonoBehaviour
    {
        public string type;
        public int count = 1;
        public GameObject uiPrefab;

        // アニメーターコンポーネントを格納する変数。
        private Animator animator;

        // ゲームオブジェクトを削除します。
        public void DestroyGameObject()
        {
            Destroy(gameObject);
        }

        // Startメソッドはゲーム開始時に呼び出されます。
        // アニメーターコンポーネントを取得します。
        void Start()
        {
            // 必要なコンポーネントの参照を取得します。
            animator = GetComponent<Animator>();
        }

        // OnTriggerEnter2Dメソッドは、2Dトリガーに衝突したときに呼び出されます。
        // 衝突したオブジェクトにInventoryコンポーネントがあるか確認し、
        // あればアニメーションを再生します。
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 衝突したオブジェクトの子オブジェクトからInventoryコンポーネントを取得します。
            Inventory inventory = collision.GetComponentInChildren<Inventory>();

            // Inventoryコンポーネントが見つからない場合、処理を終了します。
            if (inventory == null)
            {
                return;
            }

            // アイテムをインベントリに追加します。
            inventory.AddItem(type, count);

            // アニメーターコンポーネントが存在する場合、
            // 「Get」トリガーを設定してアニメーションを再生します。
            // アニメーターコントローラに「Get」パラメータが存在していない場合、エラーが発生するので、
            // 必ず設定しておきましょう。
            if (animator != null)
            {
                // アニメーターがある場合、「Get」トリガーで再生されるアニメーションクリップのイベントを使って、
                // 必ず「DestroyGameObject」メソッドを実行するようにしてください。
                animator.SetTrigger("Get");
            }
            else
            {
                // アニメーターコンポーネントが存在しない場合、ゲームオブジェクトを削除します。
                DestroyGameObject();
            }
        }
    }
}
