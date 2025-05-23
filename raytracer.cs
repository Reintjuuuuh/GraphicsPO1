
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

	//public void Render() {
 //       float debuggerScale = 0.5f;

 //       int debuggerOffsetX = ((3 * screen.width) / 4);
	//	int debuggerOffsetY = ((7 * screen.height) / 8);

	//	int camAnchorX = (int)(camera.position.X * debuggerScale) + debuggerOffsetX;
	//	int camAnchorY = (int)(-camera.position.Z * debuggerScale) + debuggerOffsetY;

 //       for (int row = (int)camera.screenPlane.downLeft.Y; row < (int)camera.screenPlane.upLeft.Y; row++) {
	//		for (int col = (int)camera.screenPlane.upLeft.X; col < (int)camera.screenPlane.upRight.X; col++) {
	//			Ray ray = new Ray(new Vector3(col, row, camera.screenPlane.upLeft.Z), new Vector3(col, row, camera.screenPlane.upLeft.Z) - camera.position);
	//			List<Intersection> intersections = new();

	//			Intersection closestIntersection = null;
	//			foreach (Primitive primitive in scene.primitives) {
	//				Intersection intersection = primitive.Intersection(ray);
	//				if (intersection != null) {
	//					intersections.Add(intersection);
	//				}	
	//			}
				
	//			if (intersections.Count > 0) {
	//				closestIntersection = intersections.Min();
	//				float grayScale = 1f;
	//				if(closestIntersection.primitive is Sphere) {
	//					Sphere sphere = closestIntersection.primitive as Sphere;
	//					float a = Vector3.Distance(closestIntersection.position, camera.position);
	//					float b = closestIntersection.primitive.Distance(camera.position);
	//					float c = (1 / sphere.radius);

 //                       grayScale = Math.Max(0, Math.Min(1, 1 - ((a - b) * c)));
 //                   }
 //                   screen.Plot(col + screen.width / 4, row + screen.height / 2, new Color3(grayScale, grayScale, grayScale));
 //               } else {
 //                   screen.Plot(col + screen.width / 4, row + screen.height / 2, new Color3(0.1f, 0.1f, 0.1f));
 //               }

	//			//Debugger
	//			if (row == (camera.screenPlane.downLeft.Y + camera.screenPlane.upLeft.Y) / 2) {
					
	//				screen.Bar(camAnchorX - 5, camAnchorY - 5, camAnchorX + 5, camAnchorY + 5, new Color3(1f, 0.0f, 0.5f));

	//				screen.Line((int)(camera.screenPlane.upLeft.X * debuggerScale) + debuggerOffsetX, (int)(-camera.screenPlane.upLeft.Z * debuggerScale) + debuggerOffsetY, (int)(camera.screenPlane.upRight.X * debuggerScale) + debuggerOffsetX, (int)(-camera.screenPlane.upRight.Z * debuggerScale) + debuggerOffsetY, new Color3(0f, 1f, 1f));

 //                   if (col % 10 == 0) {
	//					if (closestIntersection != null) {
	//						screen.Line(camAnchorX, camAnchorY, (int)(closestIntersection.position.X * debuggerScale + debuggerOffsetX), (int)(-closestIntersection.position.Z * debuggerScale + debuggerOffsetY), new Color3(1f, 0f, 0f));
	//					} else {
	//						screen.Line(camAnchorX, camAnchorY, (int)(ray.directionVector.X * debuggerScale * 100 + debuggerOffsetX), (int)(-ray.directionVector.Z * debuggerScale * 100 + debuggerOffsetY), new Color3(1f, 0f, 0f));
	//					}
	//				}

 //                   foreach (Primitive primitive in scene.primitives) {
 //                       foreach (Vector3 pixel in primitive.getPixels(scene, camera)) {
 //                           screen.Plot((int)(pixel.X * debuggerScale + debuggerOffsetX), (int)(-pixel.Z * debuggerScale + debuggerOffsetY), primitive.color);
 //                       }
 //                   }
 //               }
	//		}
	//	}
 //   }

	public Color3 GetPixelColor(int row, int col)
	{
        Ray viewRay = new Ray(new Vector3(col, row, camera.screenPlane.upLeft.Z), new Vector3(col, row, camera.screenPlane.upLeft.Z) - camera.position);

        List<Intersection> viewIntersections = GetIntersections(viewRay);

        if (viewIntersections.Count > 0)
        {
            Intersection closestIntersection = viewIntersections.Min();
            
            //Finding intersection points that the light also hits
            //Shoot for each light a ray from the primitive intersection to the light source
            //For now this only works with one light
            foreach(Light light in scene.lights) {
                Vector3 direction = light.location - closestIntersection.position;
                Vector3 origin = closestIntersection.position;
                Ray lightRay = new Ray(origin, direction);
                
                //Check for intersections of the light ray
                List<Intersection> lightIntersections = GetIntersections(lightRay);

                //If there is one check if it is between the light and the primitive
                float epsilon = 0.0001f;
                bool primitiveBetweenLight = false;
                foreach (Intersection intersection in lightIntersections) {
                    float distancePrimaryToLight = Vector3.Distance(closestIntersection.position, light.location);
                    float distanceIntersectToLight = Vector3.Distance(intersection.position, light.location);
                    float distanceIntersectToPrimary = Vector3.Distance(intersection.position, closestIntersection.position);
                    if ((epsilon > distanceIntersectToPrimary || distanceIntersectToLight < distancePrimaryToLight - epsilon)) {
                        primitiveBetweenLight = true;
                        break;
                    }
                }

                if (primitiveBetweenLight) {
                    return new Color3(1f, 0.15f, 0.15f);
                } else {
                    //Calculate light color
                    Color3 ambientLight = new Color3(0.15f, 0.15f, 0.15f);
                    float r = closestIntersection.primitive.Distance(light.location);

                    float dotProduct = Math.Max(Vector3.Dot(Vector3.Normalize(closestIntersection.normal), Vector3.Normalize(lightRay.directionVector)), 0);

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