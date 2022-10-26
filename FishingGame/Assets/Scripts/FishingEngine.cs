using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// MonoBehaviour - base class, life cycle functions
// IPointerClickHandler - interface to handle click input using OnPointerClick callbacks
// IDragHandler - onDrag callbacks, detects draging for camera in game

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

    private void OnTriggerEnter(Collider other)  // other identifiying variable pointing to the collider objects 
    {
        // Hold the fishing rod.
        if(other.tag == "rod")
        {
            other.gameObject.SetActive(false);           // removes fishing rod from fixed spot 
            holdingRod.gameObject.SetActive(true);      // picks up rod
            isHoldingRod = true;                        // set to true 
        }
        // if player collides with lake holding the fishing rod
        if (other.tag == "lake" && isHoldingRod == true)
        {
            throwButton.SetActive(true);               //activates throw button asset
        }
    }

    // player leaves lake
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "lake" && isHoldingRod == true)
        {            
            throwButton.SetActive(false);             // deactivates throw button
        }
    }

    private void Update()
    {        
        Cursor.lockState = CursorLockMode.None;   // activates mouse control for camera movement 

        if ((System.DateTime.Now - throwedTime).TotalSeconds >= waitingPeriod && isThrown == true)                                                  // fish bite statement 
        {
            isThrown = false;   // remove throw button
            isFoundFish = true; // set ture
            foundFishTime = System.DateTime.Now;           // set foundFishTime & reeledTime to current time
            reeledTime = System.DateTime.Now;
            reelButton.SetActive(true);             // activate reel button - updates time each click
            infoPanel.SetActive(true);              // show message box 
            infoPanel.GetComponentInChildren<Text>().text = "" + fishName[caughtFishIndex] + " bit the bait!";  // DFS on chindren of objects in file to find name of fish and print on infoPanel
            StartCoroutine(DelayShowPanel());    // pause deactivates infoPanel
        }

        if (isFoundFish == true && (System.DateTime.Now - reeledTime).TotalSeconds > 2.5f)                                                         // if reel is not clicked in 2.5 frames/sec
        {
            infoPanel.SetActive(true);            // show message box 
            infoPanel.GetComponentInChildren<Text>().text = "" + fishName[caughtFishIndex] + " ran away!";   //  DFS on chindren of objects in file to find name of fish and  print on infoPanel
            StartCoroutine(DelayShowPanel());   // pause deactivates infoPanel
            isFoundFish = false;             // set false 
            reelButton.SetActive(false);    // deactivate reel button
            throwButton.SetActive(true);    // activate throw 
        }

        if (isFoundFish == true && (System.DateTime.Now - foundFishTime).TotalSeconds > 12f && caughtPossibility >= 0.2f)                           // 12 frames later you can catch a fish with 80% possibility  (0.2f)
        {
            infoPanel.SetActive(true);                  // show message box     
            infoPanel.GetComponentInChildren<Text>().text = "" + fishName[caughtFishIndex] + " was caught!";   // DFS on chindren of objects in file to find name of fish and print on infoPanel
            StartCoroutine(DelayShowPanel());    // pause deactivates infoPanel
            isFoundFish = false;    
            reelButton.SetActive(false);        // deactivate reel button
            fishes[caughtFishIndex].SetActive(true);    // show fish assest in display 
            StartCoroutine(DelayShowFish());        // pause deactivates fish display
        }
        else if (isFoundFish == true && (System.DateTime.Now - foundFishTime).TotalSeconds > 12f && caughtPossibility < 0.2f)                     // fish ran if possibility is less than 20%
        {
            infoPanel.SetActive(true);           // show message box  
            infoPanel.GetComponentInChildren<Text>().text = "" + fishName[caughtFishIndex] + " ran away!";       //  DFS on chindren of objects in file to find name of fish and print on infoPanel
            StartCoroutine(DelayShowPanel());             // pause deactivates infoPanel
            isFoundFish = false;         
            reelButton.SetActive(false);          // deactivate reel button
            throwButton.SetActive(true);            // activae throw button
        }
    }

    public void ThrowClick()                                                                                                                        // when throw button is clicked 
    {
        player.SetActive(false);                       // turns off player
        playerController.GetComponent<ThirdPersonController>().enabled = false;    // turn off character controls 
        camera.SetActive(true);  // switches to other camera
        fishingRodFixed.SetActive(true);     //activates fixed fishing rod from deactivation from characher hold
        isThrown = true;           
        waitingPeriod = UnityEngine.Random.RandomRange(5, 10);  // waiting time 5 - 10 
        caughtFishIndex = UnityEngine.Random.RandomRange(0, 3);  // fish identifier 
        caughtPossibility = UnityEngine.Random.Range(0f, 1f);   // randimization of catch possiblilty 
        throwedTime = System.DateTime.Now;           // record current time for throw 
    }

    public void MoveClick()                                                                                                                           // when move button is clicked 
    {
        camera.SetActive(false);             // turns off camera switching back to main
        fishingRodFixed.SetActive(false);     // deactivates fishing rod from fix posion back inot characters hands 
        throwButton.SetActive(false);        // removes throw buttom from screen 
        moveButton.SetActive(false);         // removes move button from screen once clicked 
        reelButton.SetActive(false);        // removes reel button once clicked 
        isThrown = false;                 
        isFoundFish = false;        
        player.SetActive(true);          // turns on character
        playerController.GetComponent<ThirdPersonController>().enabled = true;  // turns on character controls 
    }
    
    public void ReelClick()                                                                                                                            // when reel button is clicked 
    {
        reeledTime = System.DateTime.Now;   // updates to current time each time reel button is clicked 
    }
    
    IEnumerator DelayShowPanel()        // IEnumerator helps run time - when fish is not caught
    {
        yield return new WaitForSeconds(3f);  // pause in game 3 sec

        infoPanel.SetActive(false);       // removes message box 
    }
    IEnumerator DelayShowFish()                // IEnumerator helps run time - when fish is caught
    {
        yield return new WaitForSeconds(3f);    // pause in game 3 sec
        throwButton.SetActive(true);        // activates throw button again

        for (int i = 0; i < fishes.Length; i++)
        {
            fishes[i].SetActive(false);        // removes display of the fish on screen 
        }
    }

    
    public void OnDrag(PointerEventData eventData)
    {
       if (eventData.pointerCurrentRaycast.gameObject.tag == "reel")      // RaycastResult associated with the current event.
       {            
           reeledTime = System.DateTime.Now;         // update reel time to current time on first click with mouse drag action from player
       }       
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.tag == "reel")   // RaycastResult associated with the current event.
        {           
            reeledTime = System.DateTime.Now;   // update reel time to current time on first click with holding mouse action from player
        }
    } 
}
