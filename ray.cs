using Assimp;
using System;
using System.Numerics;

public class Ray : Visualizable{
	public Vector3 orgin;
	public Vector3 directionVector;
		
	public Ray(Vector3 orgin, Vector3 directionVector){
		this.orgin = orgin;
		this.directionVector = directionVector;
	}

    public List<Vector3> getPixels(Scene1 scene, Camera camera) {
        List<Vector3> pixels = new List<Vector3>();
        List<Intersection> intersections = new List<Intersection> ();

		foreach(Primitive primitive in scene.primitives) {
			Intersection intersection = primitive.Intersection(this);
			if (intersection != null) {
				intersections.Add(intersection);
			}
		}

		Intersection closestIntersection = null;
		if(intersections.Count > 0) {
			closestIntersection = intersections.Min();
		}

		bool check = false;
		if(closestIntersection != null) {
			check = true;
		}

		for (float i = 0; i < 400; i+=0.1f) {
			Vector3 location = orgin + i * directionVector;
			if (check) {
				if(Vector3.Distance(location, camera.position) > Vector3.Distance(closestIntersection.position, camera.position)) {
					break;
				}
			}
			pixels.Add(location);
		}
		return pixels;
	}
}
