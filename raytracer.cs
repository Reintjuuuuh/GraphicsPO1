
﻿using Assimp;
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
                    return ambientLight;
                } else {
                    //Calculate light color
                    float r = closestIntersection.primitive.Distance(light.location);

                    float dotProduct = Math.Max(Vector3.Dot(Vector3.Normalize(closestIntersection.normal), Vector3.Normalize(shadowRay.directionVector)), 0);

                    float R = (light.intensity.R * (1 / r * r) * dotProduct) * closestIntersection.primitive.color.R + ambientLight.R;
                    float G = (light.intensity.G * (1 / r * r) * dotProduct) * closestIntersection.primitive.color.G + ambientLight.G;
                    float B = (light.intensity.B * (1 / r * r) * dotProduct) * closestIntersection.primitive.color.B + ambientLight.B;

                    Color3 Color = (R, G, B);
                    return Color;

                }
            }
            //In case of no lights
            return new Color3(0, 0, 0);

            /*float grayScale = 1f;
            if (closestIntersection.primitive is Sphere)
            {
                Sphere sphere = closestIntersection.primitive as Sphere;
                float a = Vector3.Distance(closestIntersection.position, camera.position);
                float b = closestIntersection.primitive.Distance(camera.position);
                float c = (1 / sphere.radius);

                grayScale = Math.Max(0, Math.Min(1, 1 - ((a - b) * c)));
            }
            return new Color3(grayScale, grayScale, grayScale);
            */
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
            return new Intersection(intersectionPoint, Vector3.Distance(intersectionPoint, ray.orgin), null, new Vector3(0, 0, 0));
        }
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