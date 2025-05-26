
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
    public Raytracer(Scene1 scene, Camera camera, Surface surface)
    {
        this.scene = scene;
        this.camera = camera;
        this.screen = surface;
    }

    public Color3 TraceRay(Vector3 cameraPosition, Vector3 viewDirection, int bounces)
    {
        Ray viewRay = new Ray(cameraPosition, viewDirection);
        List<Intersection> viewIntersections = GetIntersections(viewRay);

        if (viewIntersections.Count > 0)
        {
            Intersection closestIntersection = viewIntersections.Min();
            Color3 ambientLight = new Color3(0.15f, 0.15f, 0.15f);

            if (!scene.lights.Any()) return new Color3(0, 0, 0); // In case of no lights

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

            if (closestIntersection.primitive.isMirror && bounces < 8)
            {
                pixelCol += MirrorRay(closestIntersection, viewRay, bounces);
            }

            foreach (Light light in scene.lights)
            {
                Vector3 shadowDirection = Vector3.Normalize(light.location - closestIntersection.position);
                Vector3 shadowOrigin = closestIntersection.position;
                Ray shadowRay = new Ray(shadowOrigin, shadowDirection);

                if (light is SpotLight spotLight)
                {
                    if (Math.Acos(Vector3.Dot(-shadowRay.directionVector, spotLight.direction)) > spotLight.angle)
                    {
                        continue;
                    }
                }

                // Check for intersections of the light ray
                List<Intersection> shadowRayIntersections = GetIntersections(shadowRay);

                // Check if there is a primitive between the intersection and the light
                float epsilon = 1f;
                bool primitiveBetweenLight = false;
                float tmax = Vector3.Distance(shadowOrigin, light.location);
                foreach (Intersection intersection in shadowRayIntersections)
                {
                    float t = Vector3.Distance(shadowOrigin, intersection.position);
                    if ((t > epsilon && t < tmax - epsilon))
                    {
                        primitiveBetweenLight = true;
                        break;
                    }
                }

                if (primitiveBetweenLight)
                {
                    continue;
                }
                else
                {
                    // Calculate light color
                    float R = closestIntersection.primitive.Distance(light.location);

                    float dotpro = Math.Max(Vector3.Dot(Vector3.Normalize(closestIntersection.normal), Vector3.Normalize(shadowRay.directionVector)), 0);
                    float r = (light.intensity.R * (1 / (R * R)) * dotpro) * baseColor.R;
                    float g = (light.intensity.G * (1 / (R * R)) * dotpro) * baseColor.G;
                    float b = (light.intensity.B * (1 / (R * R)) * dotpro) * baseColor.B;

                    Color3 color = new Color3(r, g, b);
                    pixelCol += color;
                }
            }
            return pixelCol + ambientLight;
        }
        else
        {
            return new Color3(0f, 0f, 0f);
        }
    }

    public Color3 MirrorRay(Intersection intersection, Ray viewRay, int bounces)
    {
        Vector3 normal = Vector3.Normalize(intersection.normal);
        Vector3 reflectedVector = Vector3.Normalize(viewRay.directionVector - 2 * Vector3.Dot(viewRay.directionVector, normal) * normal);
        bounces += 1;
        float offset = 1f;
        Color3 color = intersection.primitive.color * TraceRay(intersection.position + offset * reflectedVector, reflectedVector, bounces);
        return color;
    }

    public Color3 PhongShadingModel(Intersection intersection, Ray shadowRay, Ray viewRay, Light light)
    {
        Vector3 normal = Vector3.Normalize(intersection.normal);
        Vector3 reflectedVector = Vector3.Normalize(-shadowRay.directionVector - 2 * Vector3.Dot(-shadowRay.directionVector, normal) * normal);
        Vector3 inverseViewRay = Vector3.Normalize(-viewRay.directionVector);

        // Calculate light color
        float r = intersection.primitive.Distance(light.location);

        float diffuseFactor = Math.Max(Vector3.Dot(Vector3.Normalize(intersection.normal), Vector3.Normalize(shadowRay.directionVector)), 0);

        Color3 Kd = intersection.primitive.color;

        float n = 2f;
        float glossyFactor = (float)Math.Pow(Math.Max(Vector3.Dot(reflectedVector, inverseViewRay), 0), n);
        Color3 Ks = new Color3(0.9f, 0.9f, 0.9f);

        float R = light.intensity.R * (1 / (r * r)) * (diffuseFactor * Kd.R + glossyFactor * Ks.R);
        float G = light.intensity.G * (1 / (r * r)) * (diffuseFactor * Kd.G + glossyFactor * Ks.G);
        float B = light.intensity.B * (1 / (r * r)) * (diffuseFactor * Kd.B + glossyFactor * Ks.B);

        Color3 color = new Color3(R, G, B);

        return color;
    }

    public bool CheckForPrimitiveBetweenLight(Ray shadowRay, Light light)
    {
        List<Intersection> shadowRayIntersections = GetIntersections(shadowRay);
        float epsilon = 0.01f;
        float tmax = Vector3.Distance(shadowRay.orgin, light.location);
        foreach (Intersection intersection in shadowRayIntersections)
        {
            float t = Vector3.Distance(shadowRay.orgin, intersection.position);
            if ((t > epsilon && t < tmax - epsilon))
            {
                return true;
            }
        }
        return false;
    }

    public Intersection? DebugRay(Vector3 cameraPos, Vector3 direction, bool isMirrorRay = false)
    {
        Ray ray = new Ray(cameraPos, direction);
        List<Intersection> intersections = GetIntersections(ray);

        if (intersections.Count > 0)
        {
            return intersections.Min();
        }
        else
        {
            return null;
        }
    }

    public Intersection? DebugShadowRay(Vector3 origin, Vector3 direction, Light light)
    {
        Ray ray = new Ray(origin, Vector3.Normalize(direction));

        List<Intersection> intersections = GetIntersections(ray);

        // If there is one check if it is between the light and the primitive
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

    public List<Intersection> GetIntersections(Ray ray)
    {
        List<Intersection> intersections = new();

        foreach (Primitive primitive in scene.primitives)
        {
            Intersection intersection = primitive.Intersection(ray);
            if (intersection != null)
            {
                intersections.Add(intersection);
            }
        }

        return intersections;
    }
}
