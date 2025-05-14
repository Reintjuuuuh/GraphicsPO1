using System;
using System.Numerics;

public interface Visualizable
{
	public List<Vector3> getPixels(Scene1 scene, Camera camera);
}
