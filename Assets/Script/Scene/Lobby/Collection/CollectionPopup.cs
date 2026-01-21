using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 수집한 카드와 단어를 보여주는 도감 팝업을 관리합니다.
/// 더 모듈화되고, 재사용 가능하며, 안정적으로 리팩토링되었습니다.
/// </summary>
public class CollectionPopup : PopupBase
{
    public static CollectionPopup instance { get; private set; }

    [Header("UI Groups")]
    [SerializeField] private GameObject cardGroup;
    [SerializeField] private GameObject wordGroup;

    [Header("Card Collection")]
    [SerializeField] private Dropdown cardFilterDropdown;
    [SerializeField] private CollectionCard cardPrefab;
    [SerializeField] private Transform cardRoot;

    [Header("Word Collection")]
    [SerializeField] private CollectionWord wordPrefab;
    [SerializeField] private Transform wordRoot;

    // 내부 상태 변수
    private readonly List<CollectionCard> _cardUIItems = new();
    private readonly List<CollectionWord> _wordUIItems = new();
    private readonly List<Func<List<int>>> _cardFilterFuncs = new();

    private CancellationTokenSource _cts;

    protected override void OnAwake()
    {
        instance = this;
        SetupCardFilters();
    }

    public override void ShowPopup(bool enable = true)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        if (enable)
        {
            OnClickCardTab(); // 팝업이 열릴 때 기본으로 카드 탭을 보여줍니다.
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }

    #region Tab Switching

    public void OnClickCardTab()
    {
        cardGroup.SetActive(true);
        wordGroup.SetActive(false);
        OnCardFilterChanged(); // 현재 필터에 맞춰 뷰를 업데이트합니다.
    }

    public void OnClickWordTab()
    {
        cardGroup.SetActive(false);
        wordGroup.SetActive(true);
        UpdateWordView(_cts.Token).Forget();
    }

    #endregion

    #region Card Tab Logic

    /// <summary>
    /// 참고: 이 메서드는 OnDropdownValueChanged에서 이름이 변경되었습니다.
    /// Unity 에디터의 Dropdown 컴포넌트에서 OnValueChanged 이벤트를 이 메서드로 업데이트해주세요.
    /// </summary>
    public void OnCardFilterChanged()
    {
        if (cardFilterDropdown.value < 0 || cardFilterDropdown.value >= _cardFilterFuncs.Count)
        {
            HLLogger.LogWarning($"Invalid card filter index: {cardFilterDropdown.value}");
            return;
        }

        // 선택된 필터 함수를 사용해 카드 ID 리스트를 가져옵니다.
        var cardIds = _cardFilterFuncs[cardFilterDropdown.value]();
        UpdateCardView(cardIds, _cts.Token).Forget();
    }

    /// <summary>
    /// 카드 필터 드롭다운을 채우고 각 옵션을 필터링 함수와 연결합니다.
    /// 이 방식은 드롭다운 인덱스에 의존하는 로직보다 더 안정적입니다.
    /// </summary>
    private void SetupCardFilters()
    {
        _cardFilterFuncs.Clear();
        var options = new List<string>();

        // "ALL" 필터
        options.Add("ALL");
        _cardFilterFuncs.Add(() => GameResourceManager.instance.GetAllCardIds());

        // 등급 필터
        for (var grade = CardGrade.Normal; grade < CardGrade.Count; grade++)
        {
            var currentGrade = grade; // 클로저를 위해 루프 변수를 캡처합니다.
            options.Add($"{LocalizeManager.instance.GetString($"grade.name.{currentGrade}")} 등급");
            _cardFilterFuncs.Add(() => GameResourceManager.instance.GetCardIds(currentGrade));
        }

        // 마스터 필터
        for (var master = CardMaster.Other; master < CardMaster.Count; master++)
        {
            var currentMaster = master; // 클로저를 위해 루프 변수를 캡처합니다.
            options.Add($"{LocalizeManager.instance.GetString($"master.name.{currentMaster}")} 카드");
            _cardFilterFuncs.Add(() => GameResourceManager.instance.GetCardIds(currentMaster));
        }

        cardFilterDropdown.ClearOptions();
        cardFilterDropdown.AddOptions(options);
    }

    // Consts
    private const float SCALE_CARD = 0.2f;
    private const float SCALE_WORD = 1.0f;
    private const int BATCH_SIZE = 20;

    private async UniTask UpdateCardView(List<int> cardIds, CancellationToken token)
    {
        await UpdateCollectionViewAsync(
            dataList: cardIds,
            uiItemList: _cardUIItems,
            itemPrefab: cardPrefab,
            root: cardRoot,
            setDataAction: (item, data) => item.SetImage(data, SaveDataManager.instance.IsOwnCard(data)),
            initializeAction: item => item.transform.localScale = Vector3.one * SCALE_CARD,
            token: token
        );
    }

    #endregion

    #region Word Tab Logic

    private async UniTask UpdateWordView(CancellationToken token)
    {
        // 단어 "ID"는 지역화된 리스트에서의 인덱스입니다.
        int wordCount = LocalizeManager.instance.GetAllAIWordString().Count;
        var wordIndices = new List<int>(wordCount);
        for (int i = 0; i < wordCount; i++)
        {
            wordIndices.Add(i);
        }

        await UpdateCollectionViewAsync(
            dataList: wordIndices,
            uiItemList: _wordUIItems,
            itemPrefab: wordPrefab,
            root: wordRoot,
            setDataAction: (item, data) => item.SetData(data, SaveDataManager.instance.IsOwnWord(data)),
            initializeAction: item => item.transform.localScale = Vector3.one * SCALE_WORD,
            token: token
        );
    }

    #endregion

    #region Generic UI Collection Updater

    /// <summary>
    /// 데이터 목록을 기반으로 UI 아이템 리스트를 업데이트하는 범용적이고 재사용 가능한 메서드입니다.
    /// 필요에 따라 UI 아이템을 생성, 활성화 또는 비활성화합니다.
    /// </summary>
    /// <typeparam name="TData">The type of the data to display.</typeparam>
    /// <typeparam name="TItem">The type of the UI component (e.g., CollectionCard).</typeparam>
    /// <param name="dataList">The list of data to display.</param>
    /// <param name="uiItemList">The list of UI components to manage.</param>
    /// <param name="itemPrefab">The prefab to instantiate for new UI items.</param>
    /// <param name="root">The parent transform for the UI items.</param>
    /// <param name="setDataAction">An action to populate a UI item with data.</param>
    /// <param name="initializeAction">An action to perform on a newly created UI item (e.g., set scale).</param>
    /// <param name="token">A cancellation token for the async operation.</param>
    private async UniTask UpdateCollectionViewAsync<TData, TItem>(
        List<TData> dataList,
        List<TItem> uiItemList,
        TItem itemPrefab,
        Transform root,
        Action<TItem, TData> setDataAction,
        Action<TItem> initializeAction,
        CancellationToken token) where TItem : Component
    {
        if (token.IsCancellationRequested) return;

        // 리스트에서 파괴된 아이템이 있다면 정리합니다.
        uiItemList.RemoveAll(x => x == null);

        // UI 아이템이 충분하지 않다면 더 생성합니다.
        while (uiItemList.Count < dataList.Count)
        {
            if (token.IsCancellationRequested) return;
            var newItem = itemPrefab.Spawn(root);
            initializeAction?.Invoke(newItem);
            uiItemList.Add(newItem);
        }

        // 아이템들을 활성화/비활성화하고 데이터를 채웁니다.
        for (int i = 0; i < uiItemList.Count; i++)
        {
            if (token.IsCancellationRequested) return;

            var uiItem = uiItemList[i];
            if (i < dataList.Count)
            {
                // 이 아이템은 필요하므로 활성화하고 데이터를 설정합니다.
                setDataAction(uiItem, dataList[i]);
                uiItem.gameObject.SetActive(true);
            }
            else
            {
                // 이 아이템은 남는 것이므로 나중에 재사용할 수 있도록 비활성화합니다.
                uiItem.gameObject.SetActive(false);
            }

            // 리스트가 클 경우 UI가 멈추는 것을 방지하기 위해 주기적으로 양보합니다.
            if (i > 0 && i % BATCH_SIZE == 0)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        // 루트 오브젝트를 껐다 켜면 일부 UI 레이아웃 그룹을 강제로 업데이트하는 데 도움이 될 수 있습니다.
        root.gameObject.SetActive(false);
        root.gameObject.SetActive(true);
    }

    #endregion
}