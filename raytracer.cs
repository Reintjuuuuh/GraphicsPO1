
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
            Vector3 intersectionPoint = ray.orgin + ray.directionVector * 1000;
            return new Intersection(intersectionPoint, Vector3.Distance(intersectionPoint, ray.orgin), null, null);
        }
    }
}