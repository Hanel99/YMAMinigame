using System.Collections.Generic;
using UnityEngine;

public class TabNavigationManager : MonoBehaviour
{
    public static TabNavigationManager Instance { get; private set; }

    private Stack<TabKeyNavigation> popupStack = new Stack<TabKeyNavigation>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Register(TabKeyNavigation popup)
    {
        popupStack.Push(popup);
    }

    public void Unregister(TabKeyNavigation popup)
    {
        if (popupStack.Count > 0 && popupStack.Peek() == popup)
            popupStack.Pop();
    }

    public bool IsTop(TabKeyNavigation popup)
    {
        return popupStack.Count > 0 && popupStack.Peek() == popup;
    }
}
