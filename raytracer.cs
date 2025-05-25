
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

            Color3 pixelCol = new Color3(0, 0, 0);
        Color3 baseColor = closestIntersection.primitive.color;

        if (closestIntersection.primitive is Plane plane && plane.texture != null)
        {
            var (u, v) = plane.GetUV(closestIntersection.position);
            int texX = (int)(u * plane.texture.width) % plane.texture.width;
            int texY = (int)(v * plane.texture.height) % plane.texture.height;
            int colorInt = plane.texture.pixels[texX + texY * plane.texture.width];
            baseColor = new Color3(
                ((colorInt >> 16) & 0xFF) / 255f,
                ((colorInt >> 8) & 0xFF) / 255f,
                (colorInt & 0xFF) / 255f
            );
        }

        foreach (Light light in scene.lights)
        {
            Vector3 schaduwRichting = Vector3.Normalize(light.location - closestIntersection.position);
            Vector3 schaduwOorsprong = closestIntersection.position;
            Ray shadowRay = new Ray(schaduwOorsprong, schaduwRichting);

            //controleer voor intersections van light ray
            List<Intersection> shadowRayIntersections = GetIntersections(shadowRay);

            //als er een intersectie is check tussen licht en primitive
            float epsilon = 1f;
            bool primitiveTussenLicht = false;
            float tmax = Vector3.Distance(schaduwOorsprong, light.location);
            foreach (Intersection intersection in shadowRayIntersections)
            {
                float t = Vector3.Distance(schaduwOorsprong, intersection.position);
                if ((t > epsilon && t < tmax - epsilon))
                {
                    primitiveTussenLicht = true;
                    break;
                }
            }

            if (primitiveTussenLicht)
            {
                continue;
            }
            else
            {
                //bereken licht kleur
                float R = closestIntersection.primitive.Distance(light.location);

                float dotpro = Math.Max(Vector3.Dot(Vector3.Normalize(closestIntersection.normal), Vector3.Normalize(shadowRay.directionVector)), 0);
                float r = (light.intensity.R * (1 / R * R) * dotpro) * baseColor.R;
                float g = (light.intensity.G * (1 / R * R) * dotpro) * baseColor.G;
                float b = (light.intensity.B * (1 / R * R) * dotpro) * baseColor.B;

                Color3 Color = new Color3(r, g, b);
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