using System;

[Serializable]
public class QuestMetaData
{
    public int id;
    public QuestType questType;
    public QuestGame questGame;
    public QuestDetailType detailType;
    public QuestDetailType2 detailType2;
    public QuestGrade grade;
    public int subId;
    public bool hidden;
    public int reachCount;
    public int tryCount;
    public int coin;
    public int exp;
    public string redeem;
}