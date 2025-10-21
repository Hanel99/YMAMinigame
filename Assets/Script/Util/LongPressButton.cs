using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private float holdTime = 5f; // 5초 이상 눌렀을 때 발동
    private Action callbackAction;
    private bool isHolding = false;
    private float holdTimer = 0f;

    void Update()
    {
        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdTime)
            {
                isHolding = false; // 중복 호출 방지
                callbackAction?.Invoke(); // 이벤트 발동
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        holdTimer = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        holdTimer = 0f;
    }

    public void SetLongPressTime(float time)
    {
        holdTime = time;
    }

    public void SetLongPressAction(Action action)
    {
        callbackAction = action;
    }
}
