using UnityEngine;
using System.Collections;

public class Hero : MonoBehaviour {
	public static Hero S;
	public float gameRestartDelay = 2f;

	//public Weapon[] weapons;
	public Weapon weapon;

	public delegate void WeaponFireDelegate();
	public WeaponFireDelegate fireDelegate;
	public GameObject explosion;

	public float weaponDuration;
	[HideInInspector]public GameObject lastTriggerGo = null;

	[HideInInspector]public float speed = 30;
	[HideInInspector]public float rollMult = -45;
	[HideInInspector]public float pitchMult = 30;

	[HideInInspector]public Bounds bounds;

	[SerializeField]private float _shieldLevel = 1;

	void Awake(){
		//instantiate singleton
		S = this; 

		bounds = Utils.CombineBoundsOfChildren(this.gameObject);
	}

	void Start(){
		ClearWeapons ();
		//weapons [0].SetType (WeaponType.blaster);
		weapon.SetType (WeaponType.blaster);
	}

	void FixedUpdate(){
		weaponDuration--;

		if (weaponDuration == 0) {
			weapon.SetType (WeaponType.blaster);
		}
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
	void OnTriggerEnter(Collider other){
		GameObject go = Utils.FindTaggedParent (other.gameObject);
		Debug.Log ("Hero: collision registered");

		if (shieldLevel > 0) {
			Main.S.OnHeroHit ();
		}

		if (go != null) {
			if (go == lastTriggerGo) {
				return;
			}
			lastTriggerGo = go;

			if (go.tag == "Enemy") {
				Debug.Log ("Hero: hit by Enemy");
				shieldLevel--;
				if (shieldLevel >= 0) {
					Main.S.SubstractShield (1);
				}
				Destroy (go);
			}
			if (go.tag == "Asteroid") {
				Debug.Log ("Hero: hit by Asteroid");
				shieldLevel--;
				if (shieldLevel >= 0) {
					Main.S.SubstractShield (1);
				}
				Destroy (go);
			}else if (go.tag == "PowerUp") {
				Debug.Log ("Hero: PowerUp absorbed");
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
		switch (pu.returnType()) {
		case WeaponType.shield: // If it's the shield
			shieldLevel++;
			Main.S.AddShield (1);
			break;

		default: 
			//if (pu.returnType() == weapons[0].type) {
				Weapon w = GetEmptyWeaponSlot(); // Find an available weapon
				if (w != null) {
					w.SetType(pu.returnType());
				//}
			} else {
				ClearWeapons();
				weapon.SetType(pu.returnType());
				//weapons[0].SetType(pu.returnType());
			}
			break;
		}
		pu.AbsorbedBy( this.gameObject );

		if (pu.returnType () == WeaponType.spread) {
			SetWeaponDuration ();
		}
	}

	void SetWeaponDuration(){
		weaponDuration = 200f;
	}

	Weapon GetEmptyWeaponSlot() {
		/*for (int i=0; i<weapons.Length; i++) {
			if ( weapons[i].type == WeaponType.none ) {
				return( weapons[i] );
			}
		}
		return( null );*/
		if (weapon.type == WeaponType.none) {
			return(weapon);
		} else {
			return(null);
		}
	}

	void ClearWeapons() {
		/*foreach (Weapon w in weapons) {
			w.SetType(WeaponType.none);
		}*/
		weapon.SetType (WeaponType.none);
	}

	public float shieldLevel{
		get{ 
			return(_shieldLevel);
		}
		set{ 
			_shieldLevel = Mathf.Min (value, 4); //max value set to 4

			if (value < 0) {
				Debug.Log ("Hero: died");
				Main.S.OnHeroDeath ();
				Instantiate (explosion, transform.position, transform.rotation);
				Destroy (this.gameObject);
				Main.S.DelayedRestart (gameRestartDelay);
			}
		}
	}
}
