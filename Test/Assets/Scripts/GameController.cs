using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class GameController : MonoBehaviour
{

    public Button create, join;
    public Text textGUI;
    public NetworkManager manager;
    public string matchName = "Carlos";
    public uint matchSize = 5;

    NetworkMatch networkMatch;

    //Add network Match component to controller
    void Awake()
    {
        networkMatch = gameObject.AddComponent<NetworkMatch>();
    }

    // Use this for initialization
    void Start()
    { 
        create.onClick.AddListener(CreateMatch);
        join.onClick.AddListener(Join);
    }

    //Action to create a match
    void CreateMatch() 
    {
        manager.StartMatchMaker();//Start matchmaker
        
        if (!NetworkServer.active && !NetworkClient.active)
        {
            if(manager.matchInfo == null) 
            {

                if(manager.matches == null)
                {
                    create.gameObject.SetActive(false);
                    join.gameObject.SetActive(false);
                    manager.SetMatchHost("mm.unet.unity3d.com", 443, true);//Connect to multiplayer service
                    /* PARAMETERS for CreateMatch()
                     * matchName: roomName which will be based off the user who created the room
                     * matchSize: The amount of players that will be in a room after host "cuts off" the roster
                     * matchAdvertise: Should this room be open to other users (Always set to true now until future enhancements)
                     * matchPassword: Password required to join this room (Will never be used...for now)
                     * publicClientAddress: For online activity (should be fine if it's blank)
                     * privateClientAddress: For local players (not necessary...for now)
                     * eloScoreForMatch: Skills, bonueses, etc.
                     * requestDomain: Perfect for future enhancements for different tiers (or difficulty)
                     * callBack: Callback for when the function completes
                     */
                    manager.matchMaker.CreateMatch(matchName, matchSize, true, "", "", "", 0, 0, manager.OnMatchCreate);//Create match room


                }
            }
        }
       
    }

    void OnMatchCreate(bool success, string info, MatchInfo matchInfoData) 
    {
        if(success && matchInfoData != null) 
        {
            NetworkServer.Listen(matchInfoData, 443);
            NetworkManager.singleton.StartHost(matchInfoData);
        }
        else
        {
            UpdateErrorMessage("Create Match Failed: " + success + ", " + info);
        }
    }


    void Join() {
        networkMatch.ListMatches(0, 20, "", false, 0, 0, OnInternetMatchList);
    }



    void OnInternetMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        if (success)
        {
            if (matches.Count != 0)
            {
                StartCoroutine(JoinMatch(matches.Count, matches)); 
            }
            else
            {
                UpdateErrorMessage("No matches at this time");
            }
        }
        else
        {
            UpdateErrorMessage("Error connecting to matchmaker");
        }
    }

    public IEnumerator JoinMatch(int mcount, List<MatchInfoSnapshot> matches) 
    {
        yield return new WaitForSeconds(1f);
        bool reached = false;
        if(mcount > 0) {
            int selectedMatch = Random.Range(0, mcount);
            int count = 0;
            foreach (var match in matches)
            {
                if (count == selectedMatch)
                {
                    create.gameObject.SetActive(false);
                    join.gameObject.SetActive(false);

                    manager.matchName = match.name;
                    manager.matchSize = (uint)match.currentSize;

                    /* PARAMETERS for CreateMatch()
                     * netID: The network identity of the match to be joined
                     * matchPassword: Password required to join this room (Will never be used...for now)
                     * publicClientAddress: For online activity (should be fine if it's blank)
                     * privateClientAddress: For local players (not necessary...for now)
                     * eloScoreForMatch: Skills, bonueses, etc.
                     * requestDomain: Perfect for future enhancements for different tiers (or difficulty)
                     * callBack: Callback for when the function completes
                     */
                    //manager.matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, OnJoinInternetMatch);
                    //NetworkManager.singleton.matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, OnJoinInternetMatch);
                    networkMatch.JoinMatch(match.networkId, "", "", "", 0, 0, OnJoinInternetMatch);
                    reached = true;
                    break;
                }
                count++;
            }
            if (!reached) {
                UpdateErrorMessage("Error Selecting Matches");
            }
        }
        else
        {
            UpdateErrorMessage("No matches at this time");

        }
    }

    void OnJoinInternetMatch(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (success)
        {
            MatchInfo hostInfo = matchInfo;
            NetworkManager.singleton.StartClient(hostInfo);
        }
        else
        {
            UpdateErrorMessage("Error Joining Matches");
        }
    }

    public void UpdateErrorMessage(string t) 
    {
        textGUI.text = t;
        float seconds = 0.0f;
        while (seconds < 6.0f)
        {
            seconds += Time.deltaTime;
            if (seconds > 5.0f)
            {
                textGUI.text = "";
            }
        }
    }

}



