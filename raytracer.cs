
﻿using Assimp;
using Microsoft.VisualBasic;
using OpenTK.Compute.OpenCL;
using System;
using System.Data.Common;
using System.Numerics;
using Template;

public class Raytracer
{
	Scene1 scene;
	Camera camera;
	Surface screen;
	public Raytracer(Scene1 scene, Camera camera, Surface surface){
        this.scene = scene;
		this.camera = camera;
		this.screen = surface;
	}


	public Color3 TraceRay(Vector3 cameraPosition, Vector3 viewDirection)
	{
        Ray viewRay = new Ray(cameraPosition, viewDirection);
        List<Intersection> viewIntersections = GetIntersections(viewRay);

        if (viewIntersections.Count > 0)
        {
            Intersection closestIntersection = viewIntersections.Min();

            //Finding intersection points that the light also hits
            //Shoot for each light a ray from the primitive intersection to the light source
            //For now this only works with one light
            Color3 ambientLight = new Color3(0.15f, 0.15f, 0.15f);

            if (!scene.lights.Any()) return new Color3(0, 0, 0); //In case of no lights

            Color3 pixelCol = new Color3(0,0,0);
            foreach (Light light in scene.lights) {
                Vector3 shadowDirection = Vector3.Normalize(light.location - closestIntersection.position);
                Vector3 shadowOrigin = closestIntersection.position;
                Ray shadowRay = new Ray(shadowOrigin, shadowDirection);
                
                //Check for intersections of the light ray
                List<Intersection> shadowRayIntersections = GetIntersections(shadowRay);

                //If there is one check if it is between the light and the primitive
                float epsilon = 1f;
                bool primitiveBetweenLight = false;
                float tmax = Vector3.Distance(shadowOrigin, light.location);
                foreach (Intersection intersection in shadowRayIntersections) {
                    float t = Vector3.Distance(shadowOrigin, intersection.position);
                    if ((t > epsilon && t < tmax - epsilon)) {
                        primitiveBetweenLight = true;
                        break;
                    }
                }

                if (primitiveBetweenLight) {
                    continue;
                } else {
                    //Calculate light color
                    float r = closestIntersection.primitive.Distance(light.location);

                    float dotProduct = Math.Max(Vector3.Dot(Vector3.Normalize(closestIntersection.normal), Vector3.Normalize(shadowRay.directionVector)), 0);
                    //Console.Write(dotProduct);
                    float R = (light.intensity.R * (1 / r * r) * dotProduct) * closestIntersection.primitive.color.R;
                    float G = (light.intensity.G * (1 / r * r) * dotProduct) * closestIntersection.primitive.color.G;
                    float B = (light.intensity.B * (1 / r * r) * dotProduct) * closestIntersection.primitive.color.B;

                    Color3 Color = (R, G, B);
                    pixelCol += Color;
                }
            }
            return pixelCol + ambientLight;
            
        }
        else
        {
            return new Color3(0f, 0f, 0f);
        }
	}

    public Intersection? DebugRay(Vector3 cameraPos, Vector3 direction)
    {
        Ray ray = new Ray(cameraPos, direction);
        List<Intersection> intersections = new();

        foreach (Primitive primitive in scene.primitives)
        {
            Intersection intersection = primitive.Intersection(ray);
            if (intersection != null)
            {
                intersections.Add(intersection);
            }
        }

        if (intersections.Count > 0)
        {
            return intersections.Min();
        }
        else
        {
            //create fake intersection far away.
            Vector3 intersectionPoint = ray.orgin + ray.directionVector * 10000;
            return new Intersection(intersectionPoint, Vector3.Distance(intersectionPoint, ray.orgin), null, new Vector3(0, 1, 999)); //very cursed, but the normal vector (0, 1, 999) indicates that it is a fake intersection
        }
    }
    public Intersection? DebugShadowRay(Vector3 origin, Vector3 direction, Light light)
    {
        Ray ray = new Ray(origin, Vector3.Normalize(direction));

        List<Intersection> intersections = GetIntersections(ray);

        //If there is one check if it is between the light and the primitive
        float epsilon = 1f;
        float tmax = Vector3.Distance(origin, light.location);

        Intersection lightIntersect = new Intersection(light.location, tmax, null, new Vector3(0, 0, 0));

        Intersection closestIntersection = lightIntersect;
        float smallestDistance = Vector3.Distance(origin, closestIntersection.position);
        foreach (Intersection intersection in intersections)
        {
            float t = Vector3.Distance(origin, intersection.position);
            if (t > epsilon && t < tmax - epsilon && smallestDistance > t)
            {
                closestIntersection = intersection;
                smallestDistance = t;
            }
        }
        return closestIntersection;
    }

    public List<Intersection> GetIntersections(Ray ray) {
        List<Intersection> intersections = new();

        foreach (Primitive primitive in scene.primitives) {
            Intersection intersection = primitive.Intersection(ray);
            if (intersection != null) {
                intersections.Add(intersection);
            }
        }

        return intersections;
    }
}