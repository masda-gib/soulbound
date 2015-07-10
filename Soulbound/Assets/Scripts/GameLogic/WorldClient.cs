using UnityEngine;
using System.Collections;

public class WorldClient : MonoBehaviour 
{
	public GameObject playerRep;
	private NetworkView ownPlayer;
	
	public string guid;
	public string ip;
	public int port;
	public string gamePassword;

	// Use this for initialization
	void Start () 
	{
		NetworkConnectionError err = Network.Connect(ip, port); //, gamePassword);
		Debug.Log (err);
		//NetworkConnectionError err = Network.Connect(guid, gamePassword);
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnConnectedToServer ()
	{
		Debug.Log("Connected");

		GameObject playerGo = (GameObject)GameObject.Instantiate (playerRep);
		playerGo.name = "Me";
		NetworkViewID netID = Network.AllocateViewID();
		NetworkView netView = playerGo.AddComponent<NetworkView>();
		netView.viewID = netID;
		
		//NetworkView netView = playerGo.AddComponent<NetworkView>();
		//netView.viewID = id;
		ownPlayer = netView;

		GetComponent<NetworkView>().RPC("LoginRequest", RPCMode.Server, "Testplayer", netID);
	}

	void OnDisconnectedFromServer (NetworkDisconnection info)
	{
		Debug.Log("Disconnected");
	}

	void OnFailedToConnect (NetworkConnectionError error)
	{
		Debug.Log(" Failed to connect");
	}
	
	[RPC]
	public void LoginRequest (string name, NetworkViewID netID)
	{
	}

	[RPC]
	public void LoginResponse (bool success)
	{
		Debug.Log ("Got Login response: " + success);
	}
}