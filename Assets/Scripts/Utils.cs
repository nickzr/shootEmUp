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

		//if GO has a renderer component
		if (go.GetComponent<Renderer>() != null) {
			b = BoundsUnion(b, go.GetComponent<Renderer>().bounds);
		}

		//if GO has a collider component
		if (go.GetComponent<Collider>() != null) {
			b = BoundsUnion(b, go.GetComponent<Collider>().bounds);
		}

		//Iterate through each child of the GO.transform
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

		//Make vector3's at top and bottom screen coords
		Vector3 topLeft = new Vector3( 0, 0, 0 );
		Vector3 bottomRight = new Vector3( Screen.width, Screen.height, 0 );

		//convert to world coordinates
		Vector3 boundTLnear = cam.ScreenToWorldPoint( topLeft );
		Vector3 boundBRfar = cam.ScreenToWorldPoint( bottomRight );

		//adjust z's to near and far clipping planes
		boundTLnear.z += cam.nearClipPlane;
		boundBRfar.z += cam.farClipPlane;

		//find center
		Vector3 center = (boundTLnear + boundBRfar)/2f;
		_camBounds = new Bounds( center, Vector3.zero );

		//expand cam bounds to encap. extents
		_camBounds.Encapsulate( boundTLnear );
		_camBounds.Encapsulate( boundBRfar );
	}

	// Checks to see whether the Bounds bnd are within the camBounds
	public static Vector3 ScreenBoundsCheck(Bounds bnd, BoundsTest
		test = BoundsTest.center) {
		return( BoundsInBoundsCheck( camBounds, bnd, test ) );
	}

	// Checks to see whether Bounds smallB are within Bounds bigB
	public static Vector3 BoundsInBoundsCheck( Bounds bigB, Bounds smallB, BoundsTest test = BoundsTest.onScreen ) {
		// The behavior of this function is different based on the BoundsTest
		// that has been selected.
		// Get the center of smallB
		Vector3 pos = smallB.center;
		// Initialize the offset at [0,0,0]
		Vector3 off = Vector3.zero;

		switch (test) {
		// The center test determines what off (offset) would have to be applied
		// to smallB to move its center back inside bigB
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
				// keep all of smallB inside bigB
			case BoundsTest.onScreen:
				if ( bigB.Contains( smallB.min ) && bigB.Contains( smallB.max ) ) {
					return( Vector3.zero );
				}
				if (smallB.max.x > bigB.max.x) {
					off.x = smallB.max.x - bigB.max.x;
				} else if (smallB.min.x < bigB.min.x) {
					off.x = smallB.min.x - bigB.min.x;
				}
				if (smallB.max.y > bigB.max.y) {
					off.y = smallB.max.y - bigB.max.y;
				} else if (smallB.min.y < bigB.min.y) {
					off.y = smallB.min.y - bigB.min.y;
				}
				if (smallB.max.z > bigB.max.z) {
					off.z = smallB.max.z - bigB.max.z;
				} else if (smallB.min.z < bigB.min.z) {
					off.z = smallB.min.z - bigB.min.z;
				}
				return( off );
			// The offScreen test determines what off would need to be applied to
			// move any tiny part of smallB inside of bigB
			case BoundsTest.offScreen:
				bool cMin = bigB.Contains( smallB.min );
				bool cMax = bigB.Contains( smallB.max );
				if ( cMin || cMax ) {
					return( Vector3.zero );
				}
				if (smallB.min.x > bigB.max.x) {
					off.x = smallB.min.x - bigB.max.x;
				} else if (smallB.max.x < bigB.min.x) {
					off.x = smallB.max.x - bigB.min.x;
				}
				if (smallB.min.y > bigB.max.y) {
					off.y = smallB.min.y - bigB.max.y;
				} else if (smallB.max.y < bigB.min.y) {
					off.y = smallB.max.y - bigB.min.y;
				}
				if (smallB.min.z > bigB.max.z) {
					off.z = smallB.min.z - bigB.max.z;
				} else if (smallB.max.z < bigB.min.z) {
					off.z = smallB.max.z - bigB.min.z;
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
