using System;
using System.Numerics;
using Template;

public abstract class Primitive : Visualizable
{
	public Color3 color;
	public Primitive()
	{
		color = new Color3(0, 0, 1);
	}


    public abstract Intersection? Intersection(Ray ray);
	public abstract List<Vector3> getPixels(Scene1 scene, Camera camera);
	public abstract float Distance(Vector3 point); 
}

public class Sphere : Primitive {
	public Vector3 position;
	public float radius;
	public float distance;

	public Sphere(Vector3 position, int radius) {
        this.position = position;
        this.radius = radius;
    }

    public override float Distance(Vector3 point) {
		return Vector3.Distance(position, point) - radius;
    }

    public override List<Vector3> getPixels(Scene1 scene, Camera camera) {
		List<Vector3> pixels = new List<Vector3>();
		for (int i = (int)(-radius - 1); i <= (int)(radius + 1); i++) {
			for (int j = (int)(-radius - 1); j <= (int)(radius + 1); j++){
				int l2 = i * i + j * j;
				float r2 = radius * radius;
				if (l2 < r2 + 40 && l2 > r2 - 40) {
					pixels.Add(new Vector3(i + position.X, 0, j + position.Z));
				}
			}
		}
		return pixels;
	}

    public override Intersection? Intersection(Ray ray) {
		//in de vorm ax^2+bx+c zoals conventie
		float a = MathF.Pow(ray.directionVector.X, 2) + MathF.Pow(ray.directionVector.Y, 2) + MathF.Pow(ray.directionVector.Z, 2);
		float b = (ray.directionVector.X * (ray.orgin.X - position.X) + ray.directionVector.Y * (ray.orgin.Y - position.Y) + ray.directionVector.Z * (ray.orgin.Z - position.Z)) * 2;
		float c = MathF.Pow(ray.orgin.X - position.X, 2) + MathF.Pow(ray.orgin.Y - position.Y, 2) + MathF.Pow(ray.orgin.Z - position.Z, 2) - MathF.Pow(radius, 2);
		float d = MathF.Pow(b, 2) - 4 * a * c;

		Intersection closestIntersection;
		
		if(d < 0) {
			return null;
		} else if(d == 0) {
			float I = (-b) / (2 * a);
			Vector3 intersectionPoint = ray.orgin + I * ray.directionVector;
			float distance = Vector3.Distance(intersectionPoint, ray.orgin);
			closestIntersection = new Intersection(intersectionPoint, distance, this, null);
		} else {
            float IMin = (-b - MathF.Sqrt(d))  / (2 * a);
			float IPlus = (-b + MathF.Sqrt(d)) / (2 * a);

            Vector3 intersectionPointMin = ray.orgin + IMin * ray.directionVector;
            Vector3 intersectionPointPlus = ray.orgin + IPlus * ray.directionVector;

			float distanceMin = Vector3.Distance(intersectionPointMin, ray.orgin);
			float distancePlus = Vector3.Distance(intersectionPointPlus, ray.orgin);

            if(distanceMin < distancePlus) {
				closestIntersection = new Intersection(intersectionPointMin, distanceMin, this, null);
			} else {
                closestIntersection = new Intersection(intersectionPointPlus, distancePlus, this, null);
            }
        }

		if(Vector3.Dot(Vector3.Normalize((closestIntersection.position - ray.orgin)), Vector3.Normalize(ray.directionVector)) >= 0) {
			return closestIntersection;
		} else {
			return null;
		}
	}
}

public class Plane : Primitive {
	public Vector3 normal;
	public Vector3 pointOnPlane;
	public float distance;
	public float d;

	public Plane(Vector3 normal, float distance) {
		this.normal = normal;
		this.distance = distance;
	}

	public Plane(Vector3 normal, Vector3 pointOnPlane) {
		this.normal = normal;
		this.pointOnPlane = pointOnPlane;
		d = normal.X * -pointOnPlane.X + normal.Y * -pointOnPlane.Y + normal.Z * -pointOnPlane.Z;
    }

    public override float Distance(Vector3 point) {
        throw new NotImplementedException();
    }

    public float distanceToOrigin() {
		return 0;
	}

    public override List<Vector3> getPixels(Scene1 scene, Camera camera) {
        throw new NotImplementedException();
    }

    public override Intersection? Intersection(Ray ray) {
		//We use the following form: [x, y, z]^t = [p_x, p_y, p_z]^t + I * [d_x, d_y, d_z]^t
		float factorI = normal.X * ray.directionVector.X + normal.Y * ray.directionVector.Y + normal.Z * ray.directionVector.Z;
		float c = normal.X * ray.orgin.Y + normal.Y * ray.orgin.Y + normal.Z * ray.orgin.Z + d;
		float I = -c / factorI;

		 if(factorI == 0 && c != 0) {
			return null;
		} else {
			Vector3 intersectionPoint = ray.orgin + ray.directionVector * I;
			float distance = Vector3.Distance(intersectionPoint, ray.orgin);
			return new Intersection(intersectionPoint, distance, this, null);
		}	
	}
}


public class Intersection : IComparable<Intersection>{
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

    public int CompareTo(Intersection? other) {
		return distance.CompareTo(other.distance);
    }
}

