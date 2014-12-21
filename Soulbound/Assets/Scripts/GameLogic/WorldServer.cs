using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldServer : MonoBehaviour 
{
	public GameObject playerRep;

	private const string gameType = "Crystalworld - Soulbound";
	
	//public string gameName = "MyGame";
	public string gamePassword;
	public long seed = 0;
	public int radiusInCells = 10;

	private TerrainData data;
	private List<Player> players;

	// Use this for initialization
	void Start ()
	{
		players = new List<Player>();

		data = new TerrainData();
		data.Generator = new TestGenerator();
		data.Generator.SetParameter("seed", seed);
		data.Generator.SetParameter("radius_cells", radiusInCells);
		data.Generator.Initialize();

		//data.saveDataFolder = Application.persistentDataPath + "/" + name + "/";

		if (!string.IsNullOrEmpty(gamePassword))
			Network.incomingPassword = gamePassword;
		Network.InitializeServer(32, 25002, !Network.HavePublicAddress());

		MasterServer.dedicatedServer = true;
		MasterServer.RegisterHost(gameType, name, "no comment");
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnPlayerConnected (NetworkPlayer player)
	{
		Debug.Log("Player connected");
	}

	void OnPlayerDisconnected (NetworkPlayer player)
	{
		Debug.Log("Player disconnected");
	}

	void OnServerInitialized ()
	{
		Debug.Log("Initialized");
	}

	[RPC]
	public void LoginRequest (string name, NetworkMessageInfo info, NetworkViewID netID)
	{
		Player newPlayer = new Player(name);
		newPlayer.netPlayer = info.sender;
		players.Add(newPlayer);

		GameObject playerGo = (GameObject)GameObject.Instantiate (playerRep);
		playerGo.name = "Player " + name;
//		NetworkViewID netID = Network.AllocateViewID();
		NetworkView netView = playerGo.AddComponent<NetworkView>();
		netView.viewID = netID;

		networkView.RPC ("LoginResponse", info.sender, true);

		Debug.Log("Client logged in");
	}

	[RPC]
	public void LoginResponse (bool success)
	{
	}
}
