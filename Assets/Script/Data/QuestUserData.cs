using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class QuestUserData
{
    public List<int> completedQuestIds = new List<int>();


    //TODO 각 항목들 다 변수 파서 만들기
    public int testData;


    public QuestUserData()
    {
        completedQuestIds = new List<int>();
        testData = 0;
    }




}