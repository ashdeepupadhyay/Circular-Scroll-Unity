using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TopView : MonoBehaviour {
    const string sampleJson = "sample.json";
    [SerializeField] private GameObject contentSetParent;
    [SerializeField] private Button previousPage;
    [SerializeField] private Button nextPage;
    [SerializeField] private RectTransform detailListItemsParent;
    [Header("carousel view animation properties")]
    [SerializeField] private float smallListCarouselSlideDuration = 0.5f;
    [SerializeField] private float smallListCarouselFadeDuration = 0.5f;
    [SerializeField] private float smallListCarouselDelayDuration = 0.25f;
    [SerializeField] private float smallListCarouselSlideDistance = 750f;
    private readonly CompositeDisposable disposables = new CompositeDisposable();
    private readonly CompositeDisposable signalsDisposables = new CompositeDisposable();

    private ReactiveProperty<int> selectedPage = new ReactiveProperty<int>(0);
    private CarouselButtons currentClickedCarouselButton = CarouselButtons.None;

    private Values placeholderContent = new Values { placeholderTile = true };
    private CanvasGroup parentCanvasGroup;

    public List<Values> cachedFilteredList = new List<Values>();
    private List<SmallTile> displayContentList = new List<SmallTile>();
    private const int bottomPanelTileCount = 4;
    private bool IsDetailListCarouselAnimating = false;
    private int totalNoPages;
    private float contentSetParentPositionx;
    private enum CarouselButtons
    {
        None,
        Previous,
        Next
    }
    private void Awake()
    {
        parentCanvasGroup = detailListItemsParent.GetComponent<CanvasGroup>();
        displayContentList = contentSetParent.GetComponentsInChildren<SmallTile>(true).ToList();
        totalNoPages = 0;
        contentSetParentPositionx = detailListItemsParent.transform.localPosition.x;
    }
    void Start () {
        string fileName = Path.Combine(Application.dataPath, sampleJson);
        LoadJson(fileName);
    }
    public void LoadJson(string fileName)
    {
        using (StreamReader r = new StreamReader(fileName))
        {
            string json = r.ReadToEnd();
            Debug.Log("json" + json);
            ListItem items = JsonUtility.FromJson<ListItem>(json);
            Debug.Log("***" + items.Values.Length);
            for(int i=0;i<items.Values.Length;i++)
            {
                Debug.Log(items.Values[i].Text);
            }
            cachedFilteredList = items.Values.ToList();
            totalNoPages = Mathf.CeilToInt(cachedFilteredList.Count / (float)bottomPanelTileCount);
            Debug.Log("totalNoPages" + totalNoPages);
            if (totalNoPages <= 1)
            {
                previousPage.gameObject.SetActive(false);
                nextPage.gameObject.SetActive(false);
            }
            int setPage = (selectedPage.Value > GetLastPage()) ? GetLastPage() : selectedPage.Value;
            setPage = (setPage < 0) ? 0 : setPage;
            selectedPage.SetValueAndForceNotify(setPage);
        }
    }
    private int GetLastPage()
    {
        Debug.Log("lastPage"+ Mathf.CeilToInt((cachedFilteredList.Count() / (float)bottomPanelTileCount) - 1));
        return Mathf.CeilToInt((cachedFilteredList.Count() / (float)bottomPanelTileCount) - 1);
    }
    private void AnimateCarousel(int pageNo, int positiveDirectionMultiplier, int negativeDirectionMultiplier)
    {
        if (totalNoPages > 1)
        {
            IsDetailListCarouselAnimating = true;
            
            parentCanvasGroup.DOFade(0.0f, smallListCarouselFadeDuration);
            detailListItemsParent.DOLocalMoveX(smallListCarouselSlideDistance * positiveDirectionMultiplier, smallListCarouselSlideDuration, true).OnComplete(() =>
            {
                ChangePageContents(pageNo);
                Sequence sequence = DOTween.Sequence();
                sequence.Append(detailListItemsParent.DOLocalMoveX(smallListCarouselSlideDistance * negativeDirectionMultiplier, 0f, true))
                    .AppendInterval(smallListCarouselDelayDuration)
                    .Append(detailListItemsParent.DOLocalMoveX(contentSetParentPositionx, smallListCarouselSlideDuration, true))
                    .Join(parentCanvasGroup.DOFade(1.0f, smallListCarouselFadeDuration))
                    .OnComplete(() =>
                    {
                        IsDetailListCarouselAnimating = false;
                    });

                sequence.Play();
            });
            
        }
        else
        {
            ChangePageContents(pageNo);
        }
    }

    private void ChangePageContents(int pageNo)
    {
        int listMax = (cachedFilteredList.Count - pageNo * bottomPanelTileCount) < bottomPanelTileCount ? cachedFilteredList.Count - pageNo * bottomPanelTileCount : bottomPanelTileCount;
        Debug.Log(listMax);
        List<Values> tempList = new List<Values>();
        if (listMax > 0)
        {
            Debug.Log(pageNo * bottomPanelTileCount + "****" + listMax);
            tempList = cachedFilteredList.ToList().GetRange(pageNo * bottomPanelTileCount, listMax);
        }

        for (int i = 0; i < displayContentList.Count; i++)
        {
            displayContentList[i].SetActiveSafely(false);
            displayContentList[i].CleanItem();
            if (i < tempList.Count)
            {
                displayContentList[i].ConfigureFor(tempList[i].Text,true);
            }
            else
            {
                displayContentList[i].ConfigureFor();
            }
            displayContentList[i].transform.SetAsLastSibling();
            displayContentList[i].SetActiveSafely(true);
        }
    }

    void OnDisable()
    {
        nextPage.onClick.RemoveAllListeners();
        previousPage.onClick.RemoveAllListeners();
        disposables.Clear();

    }

    void OnEnable()
    {
        disposables.Clear();
        signalsDisposables.Clear();
        currentClickedCarouselButton = CarouselButtons.None;

        selectedPage.Skip(1)
            .TakeUntilDisable(this)
            .Where(page => page >= 0)
            .Subscribe(currentPage =>
            {
                if (currentClickedCarouselButton == CarouselButtons.Next)
                {
                    AnimateCarousel(currentPage, -1, 1);
                }
                else if (currentClickedCarouselButton == CarouselButtons.Previous)
                {
                    AnimateCarousel(currentPage, 1, -1);
                }
                else
                {
                    ChangePageContents(currentPage);
                }
            })
            .AddTo(disposables);


        previousPage.onClick.AsObservable()
            .TakeUntilDisable(this)
            .Where(_ => !IsDetailListCarouselAnimating)
            .Subscribe(_ => {
                currentClickedCarouselButton = CarouselButtons.Previous;

                if (selectedPage.Value == 0)
                {

                    selectedPage.Value = GetLastPage();
                }
                else
                {
                    selectedPage.Value--;
                }
            })
            .AddTo(disposables);

        nextPage.onClick.AsObservable()
            .TakeUntilDisable(this)
            .Where(_ => !IsDetailListCarouselAnimating)
            .Subscribe(_ => {
                currentClickedCarouselButton = CarouselButtons.Next;
                if (selectedPage.Value == GetLastPage())
                {

                    selectedPage.Value = 0;
                }
                else
                {
                    selectedPage.Value++;
                }
            })
            .AddTo(disposables);
    }
}
[Serializable]
public class ListItem { 
    public Values[] Values;
}
[Serializable]
public class Values
{
    public string Text;
    public bool placeholderTile;
}
