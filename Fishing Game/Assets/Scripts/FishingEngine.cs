using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FishingEngine : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    public GameObject camera;
    public GameObject fishingRod;
    public GameObject fishingRodFixed;
    public GameObject holdingRod;
    public GameObject throwButton;
    public GameObject player;
    public GameObject playerController;
    public GameObject reelButton;
    public GameObject moveButton;
    public GameObject mainRod;
    public GameObject rodPrefab;
    public GameObject caughtFish;
    public GameObject infoPanel;
    public GameObject[] fishes;
    public MeshCollider lakeColider;
    public bool isHoldingRod;
    public bool isThrown;    
    public bool isFoundFish;
    public int waitingPeriod;
    public int caughtFishIndex;
    public float caughtPossibility;
    public DateTime throwedTime;
    public DateTime foundFishTime;
    public DateTime reeledTime;
    public string[] fishName;

    [System.Serializable]
    public class Result
    {
        public string result;
        public int id;
        public int bassCaught;
        public int muskieCaught;
        public int blueGillCaught;
        public int bassTotal;
        public int muskieTotal;
        public int blueGillTotal;
        public int coin;
    }

    public Text bluegillCaughtText;
    public Text bluegillSellText;
    public Text bluegillTotalText;
    public Text bassCaughtText;
    public Text bassSellText;
    public Text bassTotalText;
    public Text muskieCaughtText;
    public Text muskieSellText;
    public Text muskieTotalText;
    public Text priceText;
    
    string readURL;
    string sellURL;
    string caughtURL;

    private void OnTriggerEnter(Collider other)
    {
        // Hold the fishing rod.
        if(other.tag == "rod")
        {
            other.gameObject.SetActive(false);
            holdingRod.gameObject.SetActive(true);
            isHoldingRod = true;            
        }

        if (other.tag == "lake" && isHoldingRod == true)
        {
            //lakeColider.isTrigger = false;
            throwButton.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "lake" && isHoldingRod == true)
        {            
            throwButton.SetActive(false);
        }
    }

    void Start() 
    {
        readURL = FishingUI.instance.absURL + "api/getUserData";
        sellURL = FishingUI.instance.absURL + "api/sell";
        caughtURL = FishingUI.instance.absURL + "api/catch";
    }

    private void Update()
    {        
        Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;

        if ((System.DateTime.Now - throwedTime).TotalSeconds >= waitingPeriod && isThrown == true)
        {
            isThrown = false;
            isFoundFish = true;
            foundFishTime = System.DateTime.Now;
            reeledTime = System.DateTime.Now;
            reelButton.SetActive(true);
            infoPanel.SetActive(true);
            infoPanel.GetComponentInChildren<Text>().text = "" + fishName[caughtFishIndex] + " bit the bait!";
            StartCoroutine(DelayShowPanel());
        }

        if (isFoundFish == true && (System.DateTime.Now - reeledTime).TotalSeconds > 2.5f)
        {
            infoPanel.SetActive(true);
            infoPanel.GetComponentInChildren<Text>().text = "" + fishName[caughtFishIndex] + " ran away!";
            StartCoroutine(DelayShowPanel());
            isFoundFish = false;
            reelButton.SetActive(false);
            throwButton.SetActive(true);
        }

        if (isFoundFish == true && (System.DateTime.Now - foundFishTime).TotalSeconds > 12f && caughtPossibility >= 0.2f)
        {
            infoPanel.SetActive(true);
            infoPanel.GetComponentInChildren<Text>().text = "" + fishName[caughtFishIndex] + " was caught!";
            StartCoroutine(DelayShowPanel());
            isFoundFish = false;
            reelButton.SetActive(false);            
            fishes[caughtFishIndex].SetActive(true);
            StartCoroutine(RequestCatch(caughtURL, fishName[caughtFishIndex]));
            StartCoroutine(DelayShowFish());
        }
        else if (isFoundFish == true && (System.DateTime.Now - foundFishTime).TotalSeconds > 12f && caughtPossibility < 0.2f)
        {
            infoPanel.SetActive(true);
            infoPanel.GetComponentInChildren<Text>().text = "" + fishName[caughtFishIndex] + " ran away!";
            StartCoroutine(DelayShowPanel());
            isFoundFish = false;
            reelButton.SetActive(false);
            throwButton.SetActive(true);
        }
    }

    public void ThrowClick()
    {
        player.SetActive(false);
        playerController.GetComponent<ThirdPersonController>().enabled = false;
        camera.SetActive(true);
        fishingRodFixed.SetActive(false);
        fishingRodFixed.SetActive(true);
        //mainRod = Instantiate(rodPrefab, new Vector3(0 , 0, 0), Quaternion.identity);
        //mainRod.transform.position = camera.transform.position + camera.transform.forward * 2;
        isThrown = true;
        waitingPeriod = UnityEngine.Random.RandomRange(5, 10);
        caughtFishIndex = UnityEngine.Random.RandomRange(0, 3);
        caughtPossibility = UnityEngine.Random.Range(0f, 1f);
        throwedTime = System.DateTime.Now;
    }

    public void MoveClick()
    {
        camera.SetActive(false);
        fishingRodFixed.SetActive(false);
        throwButton.SetActive(false);
        moveButton.SetActive(false);
        reelButton.SetActive(false);
        //Destroy(mainRod);
        isThrown = false;
        isFoundFish = false;        
        player.SetActive(true);
        playerController.GetComponent<ThirdPersonController>().enabled = true;
    }

    public void ReelClick()
    {
        reeledTime = System.DateTime.Now;
    }

    IEnumerator DelayShowPanel()
    {
        yield return new WaitForSeconds(3f);

        infoPanel.SetActive(false);        
    }
    IEnumerator DelayShowFish()
    {
        yield return new WaitForSeconds(3f);
        throwButton.SetActive(true);

        for (int i = 0; i < fishes.Length; i++)
        {
            fishes[i].SetActive(false);
        }
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.tag == "reel")
        {            
            reeledTime = System.DateTime.Now;
        }       
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        print("!!!!!!!!!");
        if (eventData.pointerCurrentRaycast.gameObject.tag == "reel")
        {           
            reeledTime = System.DateTime.Now;
        }
    }

    public void SellBtnClick()
    {
        StartCoroutine(RequestSell(sellURL));
    }

    public void PlusBlueGillBtnClick()
    {
        if (int.Parse(bluegillCaughtText.text) > int.Parse(bluegillSellText.text))
        {
            bluegillSellText.text = (int.Parse(bluegillSellText.text) + 1).ToString();
        }
    }

    public void MinusBlueGillBtnClick()
    {
        if (int.Parse(bluegillSellText.text) > 0)
        {
            bluegillSellText.text = (int.Parse(bluegillSellText.text) - 1).ToString();
        }
    }

    public void PlusBassBtnClick()
    {
        if (int.Parse(bassCaughtText.text) > int.Parse(bassSellText.text))
        {
            bassSellText.text = (int.Parse(bassSellText.text) + 1).ToString();
        }
    }

    public void MinusBassBtnClick()
    {
        if (int.Parse(bassSellText.text) > 0)
        {
            bassSellText.text = (int.Parse(bassSellText.text) - 1).ToString();
        }
    }

    public void PlusMuskieBtnClick()
    {
        if (int.Parse(muskieCaughtText.text) > int.Parse(muskieSellText.text))
        {
            muskieSellText.text = (int.Parse(muskieSellText.text) + 1).ToString();
        }
    }

    public void MinusMuskieBtnClick()
    {
        if (int.Parse(muskieSellText.text) > 0)
        {
            muskieSellText.text = (int.Parse(muskieSellText.text) - 1).ToString();
        }
    }

    public void InventoryBtnClick()
    {        
        StartCoroutine(RequestRead(readURL));
    }

    IEnumerator RequestRead(string url)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", PlayerPrefs.GetInt("USER_ID"));

        UnityWebRequest uwr = UnityWebRequest.Post(url, form);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);

            Result loadData = JsonUtility.FromJson<Result>(uwr.downloadHandler.text);
            bluegillCaughtText.text = loadData.blueGillCaught.ToString();
            bassCaughtText.text = loadData.bassCaught.ToString();
            muskieCaughtText.text = loadData.muskieCaught.ToString();
            bluegillTotalText.text = loadData.blueGillTotal.ToString();
            bassTotalText.text = loadData.bassTotal.ToString();
            muskieTotalText.text = loadData.muskieTotal.ToString();
            priceText.text = loadData.coin.ToString();
        }
    }

    IEnumerator RequestCatch(string url, string fishName)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", PlayerPrefs.GetInt("USER_ID"));
        form.AddField("fish_name", fishName);

        UnityWebRequest uwr = UnityWebRequest.Post(url, form);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);

            Result loadData = JsonUtility.FromJson<Result>(uwr.downloadHandler.text);
            bassCaughtText.text = loadData.bassCaught.ToString();
            bluegillCaughtText.text = loadData.blueGillCaught.ToString();
            muskieCaughtText.text = loadData.muskieCaught.ToString();
            priceText.text = loadData.coin.ToString();
            bluegillSellText.text = "0";
            bassSellText.text = "0";
            muskieSellText.text = "0";
        }
    }

    IEnumerator RequestSell(string url)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", PlayerPrefs.GetInt("USER_ID"));
        form.AddField("bass", int.Parse(bassSellText.text));
        form.AddField("blueGill", int.Parse(bluegillSellText.text));
        form.AddField("muskie", int.Parse(muskieSellText.text));
        form.AddField("coin", int.Parse(bluegillSellText.text) * 120 + int.Parse(bassSellText.text) * 150
                            + int.Parse(muskieSellText.text) * 100);

        UnityWebRequest uwr = UnityWebRequest.Post(url, form);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);

            Result loadData = JsonUtility.FromJson<Result>(uwr.downloadHandler.text);
            bassCaughtText.text = loadData.bassCaught.ToString();
            bluegillCaughtText.text = loadData.blueGillCaught.ToString();
            muskieCaughtText.text = loadData.muskieCaught.ToString();
            priceText.text = loadData.coin.ToString();
            bluegillSellText.text = "0";
            bassSellText.text = "0";
            muskieSellText.text = "0";
        }
    }
}