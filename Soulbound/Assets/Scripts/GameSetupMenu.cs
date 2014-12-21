using UnityEngine;
using System.Collections;

public class GameSetupMenu : MonoBehaviour 
{
	public GameObject playerRep;
	
	private const string gameType = "Crystalworld - Soulbound";
	
	private string stateText = "Offline - Idle";
	private string myGameName = "My Game";
	private int selectedGame;
	private string[] foundGameNames = new string[0];
	
	private HostData[] foundGameData;
	
	// Use this for initialization
	void Start () {
		RefreshHostList();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI ()
	{
		foundGameData = MasterServer.PollHostList();
		foundGameNames = new string[foundGameData.Length];
		for (int i = 0; i <  foundGameData.Length; i++)
			foundGameNames[i] = foundGameData[i].gameName;
		
		GUI.Label(new Rect(10,10,410,20), stateText);
		
		myGameName = GUI.TextField(new Rect(10, 40, 200, 20), myGameName);
		
		if (GUI.Button(new Rect(220, 40, 200, 20), "Start Server"))
		{
			stateText = "Starting Server: " + myGameName;
			
			GameObject serverGo = new GameObject(myGameName);
			serverGo.AddComponent<NetworkView>();
			WorldServer ws = serverGo.AddComponent<WorldServer>();
			ws.seed = 12345;
			ws.playerRep = playerRep;

			Invoke("RefreshHostList", 1.0f);
		}
		
		selectedGame = GUI.SelectionGrid(new Rect(10, 70, 200, 20*foundGameNames.Length), selectedGame, foundGameNames, 1);
		
		if (GUI.Button(new Rect(220, 70, 200, 20), "Refresh Games"))
		{
			stateText = "Refreshing Games";
			MasterServer.ClearHostList();
			RefreshHostList();
		}
		
		if (GUI.Button(new Rect(220, 100, 200, 20), "Join Game"))
		{
			if(foundGameNames.Length > selectedGame)
			{
				stateText = "Joining Game: " + foundGameNames[selectedGame];

				GameObject clientGo = new GameObject("Client");
				WorldClient wc = clientGo.AddComponent<WorldClient>();
				clientGo.AddComponent<NetworkView>();
				wc.guid = foundGameData[selectedGame].guid;
				wc.ip = "127.0.0.1";
				wc.port = foundGameData[selectedGame].port;
				wc.gamePassword = "";
				wc.playerRep = playerRep;
			}
		}
	}
	
	private void RefreshHostList ()
	{
		MasterServer.RequestHostList(gameType);
	}
}
