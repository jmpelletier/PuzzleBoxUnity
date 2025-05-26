using UnityEngine;

namespace PuzzleBox
{
    // Item�N���X�̓A�C�e���̓�����Ǘ����܂��B
    // �A�C�e�����v���C���[�ƏՓ˂����Ƃ��̏������s���܂��B
    public class Item : MonoBehaviour
    {
        public string type;
        public int count = 1;
        public GameObject uiPrefab;

        // �A�j���[�^�[�R���|�[�l���g���i�[����ϐ��B
        private Animator animator;

        // �Q�[���I�u�W�F�N�g���폜���܂��B
        public void DestroyGameObject()
        {
            Destroy(gameObject);
        }

        // Start���\�b�h�̓Q�[���J�n���ɌĂяo����܂��B
        // �A�j���[�^�[�R���|�[�l���g���擾���܂��B
        void Start()
        {
            // �K�v�ȃR���|�[�l���g�̎Q�Ƃ��擾���܂��B
            animator = GetComponent<Animator>();
        }

        // OnTriggerEnter2D���\�b�h�́A2D�g���K�[�ɏՓ˂����Ƃ��ɌĂяo����܂��B
        // �Փ˂����I�u�W�F�N�g��Inventory�R���|�[�l���g�����邩�m�F���A
        // ����΃A�j���[�V�������Đ����܂��B
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // �Փ˂����I�u�W�F�N�g�̎q�I�u�W�F�N�g����Inventory�R���|�[�l���g���擾���܂��B
            Inventory inventory = collision.GetComponentInChildren<Inventory>();

            // Inventory�R���|�[�l���g��������Ȃ��ꍇ�A�������I�����܂��B
            if (inventory == null)
            {
                return;
            }

            // �A�C�e�����C���x���g���ɒǉ����܂��B
            inventory.AddItem(type, count);

            // �A�j���[�^�[�R���|�[�l���g�����݂���ꍇ�A
            // �uGet�v�g���K�[��ݒ肵�ăA�j���[�V�������Đ����܂��B
            // �A�j���[�^�[�R���g���[���ɁuGet�v�p�����[�^�����݂��Ă��Ȃ��ꍇ�A�G���[����������̂ŁA
            // �K���ݒ肵�Ă����܂��傤�B
            if (animator != null)
            {
                // �A�j���[�^�[������ꍇ�A�uGet�v�g���K�[�ōĐ������A�j���[�V�����N���b�v�̃C�x���g���g���āA
                // �K���uDestroyGameObject�v���\�b�h�����s����悤�ɂ��Ă��������B
                animator.SetTrigger("Get");
            }
            else
            {
                // �A�j���[�^�[�R���|�[�l���g�����݂��Ȃ��ꍇ�A�Q�[���I�u�W�F�N�g���폜���܂��B
                DestroyGameObject();
            }
        }
    }
}
