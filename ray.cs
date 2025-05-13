using System;
using System.Numerics;

public class Ray{
	public Vector3 orgin;
	public Vector3 directionVector;
		
	public Ray(Vector3 orgin, Vector3 directionVector){
		this.orgin = orgin;
		this.directionVector = directionVector;
	}
}
