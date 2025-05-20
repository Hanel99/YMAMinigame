using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro; // TMPInputField용

public class TabKeyNavigation : MonoBehaviour
{
    private void OnEnable()
    {
        TabNavigationManager.Instance?.Register(this);
    }

    private void OnDisable()
    {
        TabNavigationManager.Instance?.Unregister(this);
    }

    void Update()
    {
        // 최상단 팝업이 아니면 무시
        if (!TabNavigationManager.Instance.IsTop(this))
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current != null &&
                (current.GetComponent<InputField>() != null || current.GetComponent<TMP_InputField>() != null))
            {
                Selectable next = GetNextInput(current);
                if (next != null)
                {
                    EventSystem.current.SetSelectedGameObject(next.gameObject);
                }
            }
        }
    }

    private Selectable GetNextInput(GameObject current)
    {
        Selectable currentSelectable = current.GetComponent<Selectable>();

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            return currentSelectable.FindSelectableOnUp();

        return currentSelectable.FindSelectableOnDown();
    }
}
