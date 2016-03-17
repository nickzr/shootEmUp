using UnityEngine;
using System.Collections;

public class Hero : MonoBehaviour {
	public static Hero S;
	public float gameRestartDelay = 2f;

	[SerializeField]
	private float _shieldLevel = 1;

	//public Weapon[] weapons;
	public Weapon weapon;

	public float speed = 30;
	public float rollMult = -45;
	public float pitchMult = 30;

	public Bounds bounds;

	public delegate void WeaponFireDelegate();
	public WeaponFireDelegate fireDelegate;

	public GameObject explosion;

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
		switch (pu.returnType()) {
		case WeaponType.shield: // If it's the shield
			shieldLevel++;
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
				Destroy (this.gameObject);
				Instantiate (explosion, transform.position, transform.rotation);
				Main.S.DelayedRestart (gameRestartDelay);
			}
		}
	}
}
