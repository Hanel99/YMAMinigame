using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FinalQuizGameResultPanel : MonoBehaviour
{



    public async UniTask FailUIAnimation()
    {
        await UniTask.Delay(2000);

        // 실패 연출

    }

    public async UniTask CompleteUIAnimation()
    {
        await UniTask.Delay(2000);

        // 성공 연출

    }



}
