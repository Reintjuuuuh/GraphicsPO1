
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

	public Color3 TraceRay(Vector3 cameraPos, Vector3 direction)
    {
        Ray ray = new Ray(cameraPos, direction);

        List<Intersection> intersections = new();

        Intersection closestIntersection = null;
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
            closestIntersection = intersections.Min();
            float grayScale = 1f;
            if (closestIntersection.primitive is Sphere)
            {
                Sphere sphere = closestIntersection.primitive as Sphere;
                float a = Vector3.Distance(closestIntersection.position, camera.position);
                float b = closestIntersection.primitive.Distance(camera.position);
                float c = (1 / sphere.radius);

                grayScale = Math.Max(0, Math.Min(1, 1 - ((a - b) * c)));
            }
            return new Color3(grayScale, grayScale, grayScale);
        }
        else
        {
            return new Color3(0.1f, 0.1f, 0.1f);
        }
	}

    public Intersection? DebugRay(Vector3 cameraPos, Vector3 direction)
    {
        Ray ray = new Ray(cameraPos, direction);

        List<Intersection> intersections = new();

        Intersection closestIntersection = null;
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
            return new Intersection(intersectionPoint, Vector3.Distance(intersectionPoint, ray.orgin), null, null);
        }
    }
}