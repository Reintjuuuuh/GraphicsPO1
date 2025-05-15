
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

	public void Render() {
        float debuggerScale = 0.5f;


        int debuggerOffsetX = ((3 * screen.width) / 4);
		int debuggerOffsetY = ((7 * screen.height) / 8);


		for (int row = (int)camera.screenPlane.downLeft.Y; row < (int)camera.screenPlane.upLeft.Y; row++) {
			for (int col = (int)camera.screenPlane.upLeft.X; col < (int)camera.screenPlane.upRight.X; col++) {
				Ray ray = new Ray(new Vector3(col, row, camera.screenPlane.upLeft.Z), new Vector3(col, row, camera.screenPlane.upLeft.Z) - camera.position);
				List<Intersection> intersections = new();

				foreach (Primitive primitive in scene.primitives) {
					Intersection intersection = primitive.Intersection(ray);
					if (intersection != null) {
						intersections.Add(intersection);
					}	
				}
				Intersection closestIntersection = null;
                if (intersections.Count > 0) {
                    closestIntersection = intersections.Min();
                    screen.Plot(col + screen.width / 4, row + screen.height / 2, closestIntersection.primitive.color);

                } else {
                    screen.Plot(col + screen.width / 4, row + screen.height / 2, new Color3(0.5f, 0.5f, 0.5f));
                }
				
				//Debugger
				if (row == (camera.screenPlane.downLeft.Y + camera.screenPlane.upLeft.Y) / 2) {
					screen.Plot((int)(camera.position.X * debuggerScale) + debuggerOffsetX + 1, (int)(-camera.position.Z * debuggerScale) + debuggerOffsetY + 1, new Color3(0.0f, 0.5f, 0.5f));
					screen.Plot((int)(camera.position.X * debuggerScale) + debuggerOffsetX, (int)(-camera.position.Z * debuggerScale) + debuggerOffsetY + 1, new Color3(0.0f, 0.5f, 0.5f));
					screen.Plot((int)(camera.position.X * debuggerScale) + debuggerOffsetX + 1, (int)(-camera.position.Z * debuggerScale) + debuggerOffsetY, new Color3(0.0f, 0.5f, 0.5f));
					screen.Plot((int)(camera.position.X * debuggerScale) + debuggerOffsetX, (int)(-camera.position.Z * debuggerScale) + debuggerOffsetY, new Color3(0.0f, 0.5f, 0.5f));

					screen.Line((int)(camera.screenPlane.upLeft.X * debuggerScale) + debuggerOffsetX, (int)(-camera.screenPlane.upLeft.Z * debuggerScale) + debuggerOffsetY, (int)(camera.screenPlane.upRight.X * debuggerScale) + debuggerOffsetX, (int)(-camera.screenPlane.upRight.Z * debuggerScale) + debuggerOffsetY, new Color3(0f, 1f, 1f));

                    if (col % 10 == 0) {
						if (closestIntersection != null) {
							screen.Line((int)(camera.position.X * debuggerScale + debuggerOffsetX), (int)(-camera.position.Z * debuggerScale + debuggerOffsetY), (int)(closestIntersection.position.X * debuggerScale + debuggerOffsetX), (int)(-closestIntersection.position.Z * debuggerScale + debuggerOffsetY), new Color3(1f, 0f, 0f));
						} else {
							screen.Line((int)(camera.position.X * debuggerScale + debuggerOffsetX), (int)(-camera.position.Z * debuggerScale + debuggerOffsetY), (int)(ray.directionVector.X * debuggerScale * 100 + debuggerOffsetX), (int)(-ray.directionVector.Z * debuggerScale * 100 + debuggerOffsetY), new Color3(1f, 0f, 0f));
						}
					}

                    foreach (Primitive primitive in scene.primitives) {
                        foreach (Vector3 pixel in primitive.getPixels(scene, camera)) {
                            screen.Plot((int)(pixel.X * debuggerScale + debuggerOffsetX), (int)(-pixel.Z * debuggerScale + debuggerOffsetY), primitive.color);
                        }
                    }
                }
			}
		}
    }
}