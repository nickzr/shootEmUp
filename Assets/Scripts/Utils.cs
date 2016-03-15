using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BoundsTest{
	center,
	onScreen,
	offScreen
}

public class Utils : MonoBehaviour {

	// Creates bounds that encapsulate the two Bounds passed in.
	public static Bounds BoundsUnion( Bounds b0, Bounds b1 ) {
		// Ensure b0 and b1 isn't = 0
		if ( b0.size == Vector3.zero && b1.size != Vector3.zero ) { 
			return( b1 );
		} else if ( b0.size != Vector3.zero && b1.size == Vector3.zero ) {
			return( b0 );
		} else if ( b0.size == Vector3.zero && b1.size == Vector3.zero ) {
			return( b0 );
		}

		//expand b0 to include vector3 min and max of b1
		b0.Encapsulate(b1.min); 
		b0.Encapsulate(b1.max);
		return( b0 );
	}

	public static Bounds CombineBoundsOfChildren(GameObject go) {
		Bounds b = new Bounds(Vector3.zero, Vector3.zero);

		if (go.GetComponent<Renderer>() != null) {
			b = BoundsUnion(b, go.GetComponent<Renderer>().bounds);
		}

		if (go.GetComponent<Collider>() != null) {
			b = BoundsUnion(b, go.GetComponent<Collider>().bounds);
		}
			
		foreach( Transform t in go.transform ) { 
			b = BoundsUnion( b, CombineBoundsOfChildren( t.gameObject ) ); 
		}
		return( b );
	}
		
	public static Bounds camBounds { 
		get {
			if (_camBounds.size == Vector3.zero) {
				SetCameraBounds();
			}
			return( _camBounds );
		}
	}

	private static Bounds _camBounds; 

	public static void SetCameraBounds(Camera cam=null) { 
		if (cam == null) cam = Camera.main;

		Vector3 topLeft = new Vector3( 0, 0, 0 );
		Vector3 bottomRight = new Vector3( Screen.width, Screen.height, 0 );

		Vector3 boundTLnear = cam.ScreenToWorldPoint( topLeft );
		Vector3 boundBRfar = cam.ScreenToWorldPoint( bottomRight );

		boundTLnear.z += cam.nearClipPlane;
		boundBRfar.z += cam.farClipPlane;

		Vector3 center = (boundTLnear + boundBRfar)/2f;
		_camBounds = new Bounds( center, Vector3.zero );

		_camBounds.Encapsulate( boundTLnear );
		_camBounds.Encapsulate( boundBRfar );
	}

	// Checks to see whether the Bounds bnd are within the camBounds
	public static Vector3 ScreenBoundsCheck(Bounds bnd, BoundsTest
		test = BoundsTest.center) {
		return( BoundsInBoundsCheck( camBounds, bnd, test ) );
	}

	// Checks to see whether Bounds lilB are within Bounds bigB
	public static Vector3 BoundsInBoundsCheck( Bounds bigB, Bounds lilB, BoundsTest test = BoundsTest.onScreen ) {
		// The behavior of this function is different based on the BoundsTest
		// that has been selected.
		// Get the center of lilB
		Vector3 pos = lilB.center;
		// Initialize the offset at [0,0,0]
		Vector3 off = Vector3.zero;

		switch (test) {
		// The center test determines what off (offset) would have to be applied
		// to lilB to move its center back inside bigB
			case BoundsTest.center:
				if ( bigB.Contains( pos ) ) {
					return( Vector3.zero );
				}
				if (pos.x > bigB.max.x) {
					off.x = pos.x - bigB.max.x;
				} else if (pos.x < bigB.min.x) {
					off.x = pos.x - bigB.min.x;
				}
				if (pos.y > bigB.max.y) {
					off.y = pos.y - bigB.max.y;
				} else if (pos.y < bigB.min.y) {
					off.y = pos.y - bigB.min.y;
				}
				if (pos.z > bigB.max.z) {
					off.z = pos.z - bigB.max.z;
				} else if (pos.z < bigB.min.z) {
					off.z = pos.z - bigB.min.z;
				}
				return( off );
				// The onScreen test determines what off would have to be applied to
				// keep all of lilB inside bigB
			case BoundsTest.onScreen:
				if ( bigB.Contains( lilB.min ) && bigB.Contains( lilB.max ) ) {
					return( Vector3.zero );
				}
				if (lilB.max.x > bigB.max.x) {
					off.x = lilB.max.x - bigB.max.x;
				} else if (lilB.min.x < bigB.min.x) {
					off.x = lilB.min.x - bigB.min.x;
				}
				if (lilB.max.y > bigB.max.y) {
					off.y = lilB.max.y - bigB.max.y;
				} else if (lilB.min.y < bigB.min.y) {
					off.y = lilB.min.y - bigB.min.y;
				}
				if (lilB.max.z > bigB.max.z) {
					off.z = lilB.max.z - bigB.max.z;
				} else if (lilB.min.z < bigB.min.z) {
					off.z = lilB.min.z - bigB.min.z;
				}
				return( off );
			// The offScreen test determines what off would need to be applied to
			// move any tiny part of lilB inside of bigB
			case BoundsTest.offScreen:
				bool cMin = bigB.Contains( lilB.min );
				bool cMax = bigB.Contains( lilB.max );
				if ( cMin || cMax ) {
					return( Vector3.zero );
				}
				if (lilB.min.x > bigB.max.x) {
					off.x = lilB.min.x - bigB.max.x;
				} else if (lilB.max.x < bigB.min.x) {
					off.x = lilB.max.x - bigB.min.x;
				}
				if (lilB.min.y > bigB.max.y) {
					off.y = lilB.min.y - bigB.max.y;
				} else if (lilB.max.y < bigB.min.y) {
					off.y = lilB.max.y - bigB.min.y;
				}
				if (lilB.min.z > bigB.max.z) {
					off.z = lilB.min.z - bigB.max.z;
				} else if (lilB.max.z < bigB.min.z) {
					off.z = lilB.max.z - bigB.min.z;
				}
				return( off );
			}
		return( Vector3.zero );
	}
		
	public static GameObject FindTaggedParent(GameObject go) { 
		//if gameobject is tagged, return it
		if (go.tag != "Untagged") {
			return(go);
		}
		//if the parent of the gameobject transform is null, then nothing has a tag,so return null
		if (go.transform.parent == null) {
			return( null );
		}
		//if we find parent, climb up the hierarchy tree
		return( FindTaggedParent( go.transform.parent.gameObject ) ); 
	}
	// If  a Transform is passed in, use its gameobject
	public static GameObject FindTaggedParent(Transform t) { // 5
		return( FindTaggedParent( t.gameObject ) );
	}

	public static Material[] GetAllMaterials( GameObject go ) {
		List<Material> mats = new List<Material> ();
		if (go.GetComponent<Renderer>() != null) {
			mats.Add (go.GetComponent<Renderer>().material);
		}
		foreach (Transform t in go.transform) {
			mats.AddRange (GetAllMaterials (t.gameObject));
		}
		return(mats.ToArray ());
	}
}
