using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour {
	public static Main S;
	public static Dictionary<WeaponType, WeaponDefinition> W_DEFS;

	public GameObject[] prefabEnemies;
	public float enemySpawnPerSecond = 0.5f;
	public float enemySpawnPadding = 1.5f;
	public float enemySpawnRate;

	public AudioSource audioEnemyDeath;
	public AudioSource audioEnemyHit;

	public WeaponDefinition[] weaponDefinitions;
	public GameObject prefabPowerUp;
	public WeaponType[] powerUpFrequency = new WeaponType[]{
		WeaponType.blaster, WeaponType.blaster, 
		WeaponType.spread, WeaponType.shield
	};

	private WeaponType[] activeWeaponTypes;

	void Start(){
		activeWeaponTypes = new WeaponType[weaponDefinitions.Length];
		for (int i = 0; i < weaponDefinitions.Length; i++) {
			activeWeaponTypes [i] = weaponDefinitions [i].type;
		}
	}

	void Awake () {
		S = this;

		Utils.SetCameraBounds (this.GetComponent<Camera>());
		enemySpawnRate = 1f / enemySpawnPerSecond;
		Invoke ("SpawnEnemy", enemySpawnRate);

		W_DEFS = new Dictionary<WeaponType, WeaponDefinition> ();
		foreach (WeaponDefinition def in weaponDefinitions) {
			W_DEFS [def.type] = def;
		}
	}

	public static WeaponDefinition GetWeaponDefinition(WeaponType wt){
		if (W_DEFS.ContainsKey (wt)) {
			return(W_DEFS [wt]);
		}
		return(new WeaponDefinition ());
	}

	public void SpawnEnemy () {
		int randEnemyPrefab = Random.Range (0, prefabEnemies.Length);
		GameObject go = Instantiate (prefabEnemies [randEnemyPrefab]) as GameObject;

		Vector3 pos = Vector3.zero;

		float xMin = Utils.camBounds.min.x+enemySpawnPadding;
		float xMax = Utils.camBounds.max.x-enemySpawnPadding;

		pos.x = Random.Range( xMin, xMax );
		pos.y = Utils.camBounds.max.y + enemySpawnPadding;

		go.transform.position = pos;
		Invoke( "SpawnEnemy", enemySpawnRate );
	}

	public void DelayedRestart(float delay){
		Invoke ("Restart", delay);
	}

	public void Restart(){
		SceneManager.LoadScene ("Scene_0");
	}

	public void OnEnemyHit(){
		audioEnemyHit.PlayOneShot (audioEnemyHit.clip);
	}

	public void OnEnemyDeath(Enemy e){
		audioEnemyDeath.PlayOneShot(audioEnemyDeath.clip);

		if (Random.value < e.powerUpDropChance) {
			int randomPU = Random.Range (0, powerUpFrequency.Length);
			WeaponType puType = powerUpFrequency[randomPU];

			GameObject go = Instantiate (prefabPowerUp) as GameObject;
			PowerUp pu = go.GetComponent<PowerUp> ();
			pu.SetType (puType);
			pu.transform.position = e.transform.position;
		}
	}
}
