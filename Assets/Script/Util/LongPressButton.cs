using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum LongPressType
{
    Single,     // 1회성
    Repeat,     // 단순 반복
    Accelerated // 가속/변형 반복
}

public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [EnumToggleButtons, HideLabel]
    public LongPressType pressType = LongPressType.Single;

    [SerializeField, SuffixLabel("sec")]
    private float holdTime = 3f; // 5초 이상 눌렀을 때 발동

    [ShowIf("@this.pressType != LongPressType.Single")]
    [SerializeField, SuffixLabel("sec")]
    private float repeatInterval = 0.3f; // 반복 호출 간격

    [ShowIf("pressType", LongPressType.Accelerated)]
    [BoxGroup("Accelerate Setting")]
    [SerializeField, SuffixLabel("sec")]
    private float accelerateTime = 2.0f; // holdTime 이후 추가로 이 시간만큼 더 누르면 가속 발동

    [ShowIf("pressType", LongPressType.Accelerated)]
    [BoxGroup("Accelerate Setting")]
    [SerializeField, SuffixLabel("sec")]
    private float accelerateInterval = 0.1f; // 가속 시 반복 간격


    private Action<bool> eventAction = null;
    private Action<bool> accelerateEventAction = null; // 2차 가속 시 실행할 별도 액션 (없으면 기본 액션 실행)

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
            switch (pressType)
            {
                case LongPressType.Single:
                    if (hasTriggered == false)
                    {
                        hasTriggered = true;
                        eventAction?.Invoke(false); // 이벤트 발동
                    }
                    break;

                case LongPressType.Repeat:
                    repeatTimer += Time.deltaTime;
                    if (repeatTimer >= repeatInterval)
                    {
                        eventAction?.Invoke(true);
                        repeatTimer = 0f;
                    }
                    break;

                case LongPressType.Accelerated:
                    repeatTimer += Time.deltaTime;

                    // 2차 가속 구간인지 판별
                    bool isAccelerated = holdTimer >= (holdTime + accelerateTime);
                    float currentInterval = isAccelerated ? accelerateInterval : repeatInterval;

                    if (repeatTimer >= currentInterval)
                    {
                        if (isAccelerated && accelerateEventAction != null)
                        {
                            accelerateEventAction.Invoke(true);
                        }
                        else
                        {
                            eventAction?.Invoke(true);
                        }
                        repeatTimer = 0f;
                    }
                    break;
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

    // 호환성을 위해 남겨둠, 필요 시 Deprecated 처리
    public void SetRepeat(bool repeat)
    {
        if (repeat)
        {
            if (pressType == LongPressType.Single) pressType = LongPressType.Repeat;
        }
        else
        {
            pressType = LongPressType.Single;
        }
    }

    public void SetLongPressType(LongPressType type)
    {
        pressType = type;
    }

    public void SetRepeatInterval(float interval)
    {
        repeatInterval = interval;
    }

    public void SetAccelerateInfo(float time, float interval)
    {
        accelerateTime = time;
        accelerateInterval = interval;
    }

    public void SetLongPressAction(Action<bool> action)
    {
        eventAction = action;
    }

    public void SetAccelerateAction(Action<bool> action)
    {
        accelerateEventAction = action;
    }
}
