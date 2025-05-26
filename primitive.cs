using System;
using System.Numerics;
using Template;

public abstract class Primitive
{
	public Color3 color;
	public Vector3 position;
    public bool interpolateNormals = false;
	public bool isMirror = false;
	public Primitive()
	{
		color = new Color3(0, 0, 1);
	}

	public Primitive(Color3 color, bool isMirror) {
		this.color = color;
        this.isMirror = isMirror;
    }

    public abstract Intersection? Intersection(Ray ray);
	public abstract float Distance(Vector3 point); 
}

public class Sphere : Primitive {
	
	public float radius;
	public float distance;

	public Sphere(Vector3 position, int radius) {
        this.position = position;
        this.radius = radius;
    }

	public Sphere(Vector3 position, int radius, Color3 color, bool isMirror) : base(color, isMirror) {
        this.position = position;
        this.radius = radius;
    }

    public override float Distance(Vector3 point) {
		return Vector3.Distance(position, point) - radius;
    }

    public override Intersection? Intersection(Ray ray) {
		//in de vorm ax^2+bx+c zoals conventie
		//neem aan dat de directie normalized is, dan is a 1. Sneller
		//float a = MathF.Pow(ray.directionVector.X, 2) + MathF.Pow(ray.directionVector.Y, 2) + MathF.Pow(ray.directionVector.Z, 2);
		
		Vector3 origin = ray.orgin - position;

		float b = 2f * Vector3.Dot(ray.directionVector, origin);
		float c = Vector3.Dot(origin, origin) - radius * radius;
		float d = b * b - 4f * c;

        //float b = (ray.directionVector.X * (ray.orgin.X - position.X) + ray.directionVector.Y * (ray.orgin.Y - position.Y) + ray.directionVector.Z * (ray.orgin.Z - position.Z)) * 2;
        //float c = MathF.Pow(ray.orgin.X - position.X, 2) + MathF.Pow(ray.orgin.Y - position.Y, 2) + MathF.Pow(ray.orgin.Z - position.Z, 2) - MathF.Pow(radius, 2);
        //float d = MathF.Pow(b, 2) - 4 * c; //removed times a because 1

        Intersection closestIntersection;
		
		if(d < 0) {
			return null;
		} else if(d == 0) {
			float I = (-b) / (2); //removed times a because 1
            Vector3 intersectionPoint = ray.orgin + I * ray.directionVector;
			float distance = Vector3.Distance(intersectionPoint, ray.orgin);
			Vector3 normal = intersectionPoint - this.position;  
			closestIntersection = new Intersection(intersectionPoint, distance, this, normal);
		} else {
            float IMin = (-b - MathF.Sqrt(d))  / (2); //removed times a because 1
            float IPlus = (-b + MathF.Sqrt(d)) / (2); //removed times a because 1

            Vector3 intersectionPointMin = ray.orgin + IMin * ray.directionVector;
            Vector3 intersectionPointPlus = ray.orgin + IPlus * ray.directionVector;

			float distanceMin = Vector3.Distance(intersectionPointMin, ray.orgin);
			float distancePlus = Vector3.Distance(intersectionPointPlus, ray.orgin);

            if (distanceMin < distancePlus) {
				Vector3 normal = intersectionPointMin - this.position;
				closestIntersection = new Intersection(intersectionPointMin, distanceMin, this, normal);
			} else {
				Vector3 normal = intersectionPointPlus - this.position;
                closestIntersection = new Intersection(intersectionPointPlus, distancePlus, this, normal);
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
	public Surface? texture; // om texture in op te slaan

	public Plane(Vector3 normal, Vector3 pointOnPlane, Surface? texture = null) {
		this.normal = Vector3.Normalize(normal);
		this.pointOnPlane = pointOnPlane;
		d = -Vector3.Dot(this.normal, pointOnPlane);
		this.texture = texture;
    }

	public Plane(Vector3 normal, Vector3 pointOnPlane, Color3 color, bool isMirror) : base(color, isMirror) {
        this.normal = Vector3.Normalize(normal);
        this.pointOnPlane = pointOnPlane;
        d = -Vector3.Dot(this.normal, pointOnPlane);
    }

    public override float Distance(Vector3 point) {
        return Math.Abs(Vector3.Dot(normal, point) + d);	
    }

    public float distanceToOrigin() {
		return 0;
	}

    public override Intersection? Intersection(Ray ray) {
		//We use the following form: [x, y, z]^t = [p_x, p_y, p_z]^t + I * [d_x, d_y, d_z]^t
		float factorI = Vector3.Dot(normal, ray.directionVector);

		if (factorI == 0) //ray is parallel
		{
			return null;
		}

		float c = Vector3.Dot(normal, ray.orgin) + d;
		float I = -c / factorI;

		if (I < 0)
		{
			return null;
		}
		
		Vector3 intersectionPoint = ray.orgin + ray.directionVector * I;
		float distance = Vector3.Distance(intersectionPoint, ray.orgin);;
		return new Intersection(intersectionPoint, distance, this, normal);
	}
    public (float u, float v) GetUV(Vector3 point)
    {
        // kies twee assen evenredig aan de normaalvector
        Vector3 uAs = Vector3.Normalize(Vector3.Cross(normal, Math.Abs(normal.Y) < 0.99f ? Vector3.UnitY : Vector3.UnitX));
        Vector3 vAs = Vector3.Normalize(Vector3.Cross(normal, uAs));
        Vector3 rel = point - pointOnPlane;
        float u = Vector3.Dot(rel, uAs) * 0.05f; // schalen
        float v = Vector3.Dot(rel, vAs) * 0.05f;
        // herhaal elke 1
        u = u - (float)Math.Floor(u);
        v = v - (float)Math.Floor(v);
        return (u, v);
    }

}


public class Intersection : IComparable<Intersection>{
	public Vector3 position;
	public float distance;
	public Primitive primitive;
	public Vector3 normal;

	public Intersection(Vector3 position, float distance, Primitive primitive, Vector3 normal) {
		this.position = position;
		this.distance = distance;
		this.primitive = primitive;
		this.normal = Vector3.Normalize(normal);
	}

	public int CompareTo(Intersection? other)
	{
		return distance.CompareTo(other.distance);
	}
}

public class Triangle : Primitive
{
    public Vector3 pA, pB, pC;
    public Vector3 nA, nB, nC;

    public Triangle(Vector3 pA, Vector3 pB, Vector3 pC, Vector3 nA, Vector3 nB, Vector3 nC)
    {
        this.pA = pA;
        this.pB = pB;
        this.pC = pC;
        this.nA = nA;
        this.nB = nB;
        this.nC = nC;
    }

    public override Intersection? Intersection(Ray ray)
    {
        Vector3 edge1 = pB - pA;
        Vector3 edge2 = pC - pA;
        Vector3 normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

        float deler = Vector3.Dot(normal, ray.directionVector);
        if (deler > -0.00001f && deler < 0.00001f)
            return null;

        float t = Vector3.Dot(pA - ray.orgin, normal) / deler;
        if (t < 0)
            return null;

        Vector3 P = ray.orgin + t * ray.directionVector;

        // edge orientation
        Vector3 c0 = Vector3.Cross(pB - pA, P - pA);
        Vector3 c1 = Vector3.Cross(pC - pB, P - pB);
        Vector3 c2 = Vector3.Cross(pA - pC, P - pC);

        if (Vector3.Dot(c0, normal) < 0) return null;
        if (Vector3.Dot(c1, normal) < 0) return null;
        if (Vector3.Dot(c2, normal) < 0) return null;

        // barycentric coords (zie slides)
        Vector3 v0 = pB - pA;
        Vector3 v1 = pC - pA;
        Vector3 v2 = P - pA;
        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);
        float delerBary = d00 * d11 - d01 * d01;
        float v = (d11 * d20 - d01 * d21) / delerBary;
        float w = (d00 * d21 - d01 * d20) / delerBary;
        float u = 1.0f - v - w;

        Vector3 finalNormal = normal;
        if (interpolateNormals)
        {
            finalNormal = Vector3.Normalize(u * nA + v * nB + w * nC);
        }
        return new Intersection(position: P,distance: t,primitive: this,normal: finalNormal);
    }

    public override float Distance(Vector3 point)
    {
        Vector3 edge1 = pB - pA;
        Vector3 edge2 = pC - pA;
        Vector3 normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

        // projecteer punt op plane
        float afstandTotPlane = Vector3.Dot(point - pA, normal);
        Vector3 projected = point - afstandTotPlane * normal;

        // edge orentation checks
        Vector3 c0 = Vector3.Cross(pB - pA, projected - pA);
        Vector3 c1 = Vector3.Cross(pC - pB, projected - pB);
        Vector3 c2 = Vector3.Cross(pA - pC, projected - pC);

        if (Vector3.Dot(c0, normal) >= 0 && Vector3.Dot(c1, normal) >= 0 && Vector3.Dot(c2, normal) >= 0)
        {
            // binnen triangle
            return Math.Abs(afstandTotPlane);
        }
        else
        {
            // buiten triangle: hoek AB
            Vector3 ab = pB - pA;
            float tAB = Vector3.Dot(point - pA, ab) / Vector3.Dot(ab, ab);
            tAB = Math.Clamp(tAB, 0, 1);
            Vector3 closestAB = pA + tAB * ab;
            float dAB = Vector3.Distance(point, closestAB);

            // hoek BC
            Vector3 bc = pC - pB;
            float tBC = Vector3.Dot(point - pB, bc) / Vector3.Dot(bc, bc);
            tBC = Math.Clamp(tBC, 0, 1);
            Vector3 closestBC = pB + tBC * bc;
            float dBC = Vector3.Distance(point, closestBC);

            // hoek CA
            Vector3 ca = pA - pC;
            float tCA = Vector3.Dot(point - pC, ca) / Vector3.Dot(ca, ca);
            tCA = Math.Clamp(tCA, 0, 1);
            Vector3 closestCA = pC + tCA * ca;
            float dCA = Vector3.Distance(point, closestCA);

            return Math.Min(dAB, Math.Min(dBC, dCA));
        }
    }
}

