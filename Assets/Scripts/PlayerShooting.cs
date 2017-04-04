using UnityEngine;
using UnityEngine.Networking;

public class PlayerShooting : NetworkBehaviour {

	[SerializeField] float shotCoolDown = 0.3f;
	[SerializeField] int killsToWin = 5;
	[SerializeField] Transform firePosition;
	[SerializeField] ShotEffectManager shotEffects;

	[SyncVar (hook = "OnScoreChanged")] int score;

	Player player;
	float elaspedTime;
	bool canShot;

	void Start () 
	{
		player = GetComponent<Player> ();
		shotEffects.Initialize ();

		if (isLocalPlayer) 
		{
			canShot = true;
		}
	}

	[ServerCallback]
	void OnEnable()
	{
		score = 0;
	}


	void Update () 
	{
		if (!canShot)
			return;

		elaspedTime += Time.deltaTime;

		if (Input.GetButtonDown ("Fire1") && elaspedTime > shotCoolDown) 
		{
			elaspedTime = 0f;
			CmdFireShot (firePosition.position, firePosition.forward);
		}

	}
	[Command]
	void CmdFireShot(Vector3 origin, Vector3 direction)
	{
		RaycastHit hit;

		Ray ray = new Ray (origin, direction);
		Debug.DrawRay (ray.origin,ray.direction *3f,Color.red, 1f);

		bool result = Physics.Raycast (ray, out hit, 50f); 	

		if (result) 
		{
			//health stuff
			PlayerHealth enemy = hit.transform.GetComponent<PlayerHealth> ();

			if (enemy != null) 
			{
				bool wasKillShot = enemy.TakeDamage ();

				if (wasKillShot && ++score >= killsToWin)
					player.Won (); 

			}
		}
		RpcProcessShotEffect (result, hit.point);
	}

	[ClientRpc]
	void RpcProcessShotEffect( bool playImpact, Vector3 point)
	{
		shotEffects.PlayShotEffects ();

		if (playImpact) 
		{
			shotEffects.PlayImpactEffects (point);
		}
	}

	void OnScoreChanged(int value)
	{
		score = value;
		if (isLocalPlayer)
			PlayerCanvas.canvas.SetKills (value);
	}

	/*public void FireAsBot()
	{
		CmdFireShot (firePosition.position, firePosition.forward);
	}*/
}
