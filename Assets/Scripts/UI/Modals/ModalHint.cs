using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalHint : M8.UIModal.Controller, M8.UIModal.Interface.IPush {
    public const string parmLevelName = "lvl";

    [System.Serializable]
    public struct HintButtonData {
        public GameObject rootGO;
        public GameObject availableGO;
        public GameObject lockedGO;
        public GameObject highlightGO;

        public bool highlight {
            get { return highlightGO.activeSelf; }
            set { highlightGO.SetActive(value); }
        }

        public bool available {
            get { return mAvailable; }
            set {
                mAvailable = value;

                availableGO.SetActive(mAvailable);
                lockedGO.SetActive(!mAvailable);
            }
        }

        private bool mAvailable;        

        public void Show() {
            rootGO.SetActive(true);
        }

        public void Hide() {
            rootGO.SetActive(false);
        }
    }

    public class PageData {
        public GameObject root;
        public GameObject[] pages;

        private int mCurIndex = -1;

        public string name { get { return root.name; } }

        public int index {
            get { return mCurIndex; }
            set {
                if(mCurIndex != -1)
                    pages[mCurIndex].SetActive(false);

                mCurIndex = value;

                if(mCurIndex != -1)
                    pages[mCurIndex].SetActive(true);
            }
        }

        public void Show() {
            root.SetActive(true);
        }

        public void Hide() {
            root.SetActive(false);
        }
        
        public PageData(Transform t) {
            root = t.gameObject;
                        
            //assumes each child has at least one page and that all UI elements are within each page.
            pages = new GameObject[t.childCount];

            for(int i = 0; i < t.childCount; i++) {
                var go = t.GetChild(i).gameObject;

                go.SetActive(false);

                pages[i] = go;
            }

            root.SetActive(false);
        }
    }

    public Transform pagesRoot;
    public GameObject unlockPageConfirmRoot;

    public HintButtonData[] hintButtons;

    private Dictionary<string, PageData> mPages;
    private PageData mCurPage;

    private int mHintCounter;
        
    public static int GetPageCount(string name) {
        int count = 0;

        if(M8.UIModal.Manager.isInstantiated) {
            var modalHint = M8.UIModal.Manager.instance.ModalGetController<ModalHint>(Modals.hint);

            if(modalHint.mPages == null)
                modalHint.InitPages();

            PageData pageDat;
            if(modalHint.mPages.TryGetValue(name, out pageDat)) {
                count = pageDat.pages.Length;
            }
        }

        return count;
    }

    public void OpenPage(int index) {
        if(mCurPage == null)
            return;

        //locked?
        if(index != -1 && !hintButtons[index].available) {
            unlockPageConfirmRoot.SetActive(true);
        }
        else { //switch page
            if(mCurPage.index != -1)
                hintButtons[mCurPage.index].highlight = false;

            mCurPage.index = index;

            if(mCurPage.index != -1)
                hintButtons[mCurPage.index].highlight = true;
        }
    }

    public void ConfirmUnlock() {
        hintButtons[mHintCounter].available = true;
        OpenPage(mHintCounter);

        mHintCounter++;

        GameData.instance.SetHintCounter(mCurPage.name, mHintCounter);
    }

    void Awake() {
        //generate pages
        if(mPages == null)
            InitPages();
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        if(mCurPage != null) {
            mCurPage.Hide();
            mCurPage = null;
        }

        //grab page
        string levelName;

        if(parms.TryGetValue(parmLevelName, out levelName)) {
            if(!mPages.TryGetValue(levelName, out mCurPage)) {
                Debug.LogWarning("Unknown Page: " + levelName);
                return;
            }
        }

        mCurPage.Show();

        int hintButtonCount = mCurPage.pages.Length;

        //determine hint unlock
        mHintCounter = GameData.instance.GetHintCounter(levelName);

        for(int i = 0; i < mHintCounter; i++) {
            hintButtons[i].Show();
            hintButtons[i].available = true;
            hintButtons[i].highlight = false;
        }

        for(int i = mHintCounter; i < hintButtonCount; i++) {
            hintButtons[i].Show();
            hintButtons[i].available = false;
            hintButtons[i].highlight = false;
        }

        for(int i = hintButtonCount; i < hintButtons.Length; i++)
            hintButtons[i].Hide();

        //set default page
        if(mHintCounter > 0 && hintButtons[0].available)
            OpenPage(0);
        else
            OpenPage(-1);
    }

    private void InitPages() {
        //generate pages
        mPages = new Dictionary<string, PageData>();

        for(int i = 0; i < pagesRoot.childCount; i++) {
            var child = pagesRoot.GetChild(i);

            var newPage = new PageData(child);

            mPages.Add(newPage.name, newPage);
        }
    }
}