﻿using System;
using System.Numerics;
using Template;

public abstract class Primitive
{
	public Color3 color;
	public Vector3 position;

	public Primitive()
	{
		color = new Color3(0, 0, 1);
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
				closestIntersection.secondPoint = intersectionPointPlus;
			} else {
				Vector3 normal = intersectionPointPlus - this.position;
                closestIntersection = new Intersection(intersectionPointPlus, distancePlus, this, normal);
                closestIntersection.secondPoint = intersectionPointMin;
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

	public Plane(Vector3 normal, Vector3 pointOnPlane) {
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
}


public class Intersection : IComparable<Intersection>{
	public Vector3 position;
	public Vector3 secondPoint;
	public float distance;
	public Primitive primitive;
	public Vector3 normal;

	public Intersection(Vector3 position, float distance, Primitive primitive, Vector3 normal) {
		this.position = position;
		this.distance = distance;
		this.primitive = primitive;
		this.normal = Vector3.Normalize(normal);
	}

    public int CompareTo(Intersection? other) {
		return distance.CompareTo(other.distance);
    }
}

