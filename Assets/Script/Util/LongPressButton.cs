using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float holdTime = 3f; // 5초 이상 눌렀을 때 발동
    [SerializeField] private bool isRepeat = false;
    [SerializeField] private float repeatInterval = 0.3f; // 반복 호출 간격


    private Action<bool> eventAction;
    private bool isHolding = false;
    private float holdTimer = 0f;

    private float repeatTimer = 0f;
    private bool hasTriggered = false;  // 1회성일 때 이미 실행했는지 체크

    void Update()
    {
        if (isHolding == false)
            return;

        holdTimer += Time.deltaTime;
        if (holdTimer >= holdTime)
        {
            if (isRepeat)
            {
                repeatTimer += Time.deltaTime;

                if (repeatTimer >= repeatInterval)
                {
                    eventAction?.Invoke(true);
                    repeatTimer = 0f;
                }
            }
            else
            {
                if (hasTriggered == false)
                {
                    hasTriggered = true;
                    eventAction?.Invoke(false); // 이벤트 발동
                }
            }
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        holdTimer = 0f;
        hasTriggered = false;
        repeatTimer = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        holdTimer = 0f;
        hasTriggered = false;
        repeatTimer = 0f;
    }

    public void SetLongPressTime(float time)
    {
        holdTime = time;
    }
    public void SetRepeat(bool repeat)
    {
        isRepeat = repeat;
    }
    public void SetRepeatInterval(float interval)
    {
        repeatInterval = interval;
    }

    public void SetLongPressAction(Action<bool> action)
    {
        eventAction = action;
    }
}
