using System;
using System.Numerics;
using Template;

public class Primitive
{
	public Color3 color;
	public Primitive()
	{

	}
}

public class Sphere : Primitive {
	public Vector3 position;
	public float radius;

	public Sphere(Vector3 position, int radius) {
        this.position = position;
        this.radius = radius;
    }
}

public class Plane : Primitive {
	public Vector3 normal;
	public float distance;

	public Plane(Vector3 normal, float distance) {
		this.normal = normal;
		this.distance = distance;
	}
}

public class Intersection{
	public float distance;
	public Primitive? primitive;
	public Vector3 normal;
}