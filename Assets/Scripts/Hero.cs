using UnityEngine;
using System.Collections;

public class Hero : MonoBehaviour {
	public static Hero S;
	public float gameRestartDelay = 2f;

	[SerializeField]
	private float _shieldLevel = 1;

	public Weapon[] weapons;

	public float speed = 30;
	public float rollMult = -45;
	public float pitchMult = 30;

	public Bounds bounds;

	public delegate void WeaponFireDelegate();
	public WeaponFireDelegate fireDelegate;

	void Awake(){
		//instantiate singleton
		S = this; 

		bounds = Utils.CombineBoundsOfChildren(this.gameObject);
	}

	void Start(){
		ClearWeapons ();
		weapons [0].SetType (WeaponType.blaster);
	}

	void Update () {
		Vector3 pos = transform.position;

		float xAxis = Input.GetAxis("Horizontal");
		float yAxis = Input.GetAxis ("Vertical");

		pos.x += xAxis * speed * Time.deltaTime;
		pos.y += yAxis * speed * Time.deltaTime;

		//Control movement with mouse
		//float xAxis = Input.GetAxis("Mouse X"<0);
		//float yAxis = Input.GetAxis ("Mouse X">0);

		//pos.x += Input.GetAxis ("Mouse X") * speed * Time.deltaTime;
		//pos.y += Input.GetAxis ("Mouse Y") * speed * Time.deltaTime;

		transform.position = pos;

		bounds.center = transform.position;
		Vector3 off = Utils.ScreenBoundsCheck (bounds, BoundsTest.center);
		if (off != Vector3.zero) {
			pos -= off;
			transform.position = pos;
		}

		//rotate ship on y and x axis
		//transform.rotation = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);
		//rotate ship on x axis only
		//transform.rotation = Quaternion.Euler(0, xAxis * rollMult, 0);

		if (Input.GetAxis ("Jump") == 1 && fireDelegate != null) {
			fireDelegate ();
		}
	}

	public GameObject lastTriggerGo = null;

	void OnTriggerEnter(Collider other){
		GameObject go = Utils.FindTaggedParent (other.gameObject);

		if (go != null) {
			if (go == lastTriggerGo) {
				return;
			}
			lastTriggerGo = go;

			if (go.tag == "Enemy") {
				shieldLevel--;
				Destroy (go);
			}else if (go.tag == "PowerUp") {
				AbsorbPowerUp (go);
			}else {
				print ("Triggered: " + go.name);
			}
		}else{
			print ("Triggered: " + other.gameObject.name);
		}
	}

	public void AbsorbPowerUp( GameObject go ) {
		PowerUp pu = go.GetComponent<PowerUp>();
		switch (pu.type) {
		case WeaponType.shield: // If it's the shield
			shieldLevel++;
			break;
		default: // If it's any Weapon PowerUp
			// Check the current weapon type
			if (pu.type == weapons[0].type) {
				// then increase the number of weapons of this type
				/*Weapon w = GetEmptyWeaponSlot(); // Find an available weapon
				if (w != null) {
					// Set it to pu.type
					w.SetType(pu.type);
				}*/
			} else {
				// If this is a different weapon
				//ClearWeapons();
				//weapons[0].SetType(pu.type);
			}
			break;
		}
		pu.AbsorbedBy( this.gameObject );
	}

	Weapon GetEmptyWeaponSlot() {
		for (int i=0; i<weapons.Length; i++) {
			if ( weapons[i].type == WeaponType.none ) {
				return( weapons[i] );
			}
		}
		return( null );
	}

	void ClearWeapons() {
		foreach (Weapon w in weapons) {
			w.SetType(WeaponType.none);
		}
	}

	public float shieldLevel{
		get{ 
			return(_shieldLevel);
		}
		set{ 
			_shieldLevel = Mathf.Min (value, 4); //max value set to 4

			if (value < 0) {
				Destroy (this.gameObject);
				Main.S.DelayedRestart (gameRestartDelay);
			}
		}
	}
}
