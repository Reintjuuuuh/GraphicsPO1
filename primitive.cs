using System;
using System.Numerics;
using Template;

public abstract class Primitive
{
	public Color3 color;
	public Primitive()
	{

	}

	public abstract Intersection? Intersection(Ray ray);
}

public class Sphere : Primitive {
	public Vector3 position;
	public float radius;

	public Sphere(Vector3 position, int radius) {
        this.position = position;
        this.radius = radius;
    }

    public override Intersection? Intersection(Ray ray) {
		//in de vorm ax^2+bx+c zoals conventie
		float a = MathF.Pow(ray.directionVector.X, 2) + MathF.Pow(ray.directionVector.Y, 2) + MathF.Pow(ray.directionVector.Z, 2);
		float b = ray.directionVector.X * (ray.orgin.X - position.X) + ray.directionVector.Y * (ray.orgin.Y - position.Y) + ray.directionVector.Z * (ray.orgin.Z - position.Z);
		float c = MathF.Pow(ray.orgin.X - position.X, 2) + MathF.Pow(ray.orgin.Y - position.Y, 2) + MathF.Pow(ray.orgin.Z - position.Z, 2) - MathF.Pow(radius, 2);
		float d = MathF.Sqrt(MathF.Pow(b, 2) - 4 * a * c);

		if(d < 0) {
			return null;
		} else if(d == 0) {
			float I = (-b) / (2 * a);
			Vector3 intersectionPoint = ray.orgin + I * ray.directionVector;
			float distance = Vector3.Distance(intersectionPoint, ray.orgin);
			return new Intersection(intersectionPoint, distance, this, null);
		} else {
            float IMin = (-b - MathF.Sqrt(d))  / (2 * a);
			float IPlus = (-b - MathF.Sqrt(d)) / (2 * a);

            Vector3 intersectionPointMin = ray.orgin + IMin * ray.directionVector;
            Vector3 intersectionPointPlus = ray.orgin + IPlus * ray.directionVector;

			float distanceMin = Vector3.Distance(intersectionPointMin, ray.orgin);
			float distancePlus = Vector3.Distance(intersectionPointPlus, ray.orgin);

            if(distanceMin < distancePlus) {
				return new Intersection(intersectionPointMin, distanceMin, this, null);
			} else {
                return new Intersection(intersectionPointPlus, distancePlus, this, null);
            }
        }
	}
}

public class Plane : Primitive {
	public Vector3 normal;
	public float distance;

	public Plane(Vector3 normal, float distance) {
		this.normal = normal;
		this.distance = distance;
	}

    public override Intersection? Intersection(Ray ray) {
        throw new NotImplementedException();
    }
}


public class Intersection{
	public Vector3 position;
	public float distance;
	public Primitive primitive;
	public Vector3? normal;

	public Intersection(Vector3 position, float distance, Primitive primitive, Vector3? normal) {
		this.position = position;
		this.distance = distance;
		this.primitive = primitive;
		this.normal = normal;
	}
}

