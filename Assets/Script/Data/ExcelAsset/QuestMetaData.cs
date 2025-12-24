using System;

[Serializable]
public class QuestMetaData
{
    public int id;
    public QuestType questType;
    public QuestGame questGame;
    public QuestDetailType detailType;
    public QuestGrade grade;
    public int subId;
    public bool hidden;
    public string desc;
    public int count;
    public int coin;
    public int exp;
}