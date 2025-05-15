using Assimp;
using OpenTK.Compute.OpenCL;
using System;
using System.Data.Common;
using System.Numerics;
using Template;

public class Raytracer {
	Scene1 scene;
	Camera camera;
	Surface screen;

	public Raytracer(Scene1 scene, Camera camera, Surface surface) {
		this.scene = scene;
		this.camera = camera;
		this.screen = surface;
	}

	public void Render() {
		int debuggerOffsetX = ((3 * screen.width) / 4);
		int debuggerOffsetY = ((7*screen.height) / 8);

		for (int row = (int)camera.screenPlane.downLeft.Y; row < (int)camera.screenPlane.upLeft.Y; row++) {
			for (int col = (int)camera.screenPlane.upLeft.X; col < (int)camera.screenPlane.upRight.X; col++) {
				Ray ray = new Ray(camera.position, new Vector3(col, row, camera.screenPlane.upLeft.Z) - camera.position);
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
					screen.Plot((int)camera.position.X + debuggerOffsetX + 1, (int)-camera.position.Z + debuggerOffsetY + 1, new Color3(0.0f, 0.5f, 0.5f));
					screen.Plot((int)camera.position.X + debuggerOffsetX, (int)-camera.position.Z + debuggerOffsetY + 1, new Color3(0.0f, 0.5f, 0.5f));
					screen.Plot((int)camera.position.X + debuggerOffsetX + 1, (int)-camera.position.Z + debuggerOffsetY, new Color3(0.0f, 0.5f, 0.5f));
					screen.Plot((int)camera.position.X + debuggerOffsetX, (int)-camera.position.Z + debuggerOffsetY, new Color3(0.0f, 0.5f, 0.5f));


					if (col % 10 == 0) {
						if (closestIntersection != null) {
							screen.Line((int)(ray.orgin.X + debuggerOffsetX), (int)(-ray.orgin.Z + debuggerOffsetY), (int)(closestIntersection.position.X + debuggerOffsetX), (int)(-closestIntersection.position.Z + debuggerOffsetY), new Color3(1f, 0f, 0f));
						} else {
							screen.Line((int)(ray.orgin.X + debuggerOffsetX), (int)(-ray.orgin.Z + debuggerOffsetY), (int)(ray.directionVector.X * 100 + debuggerOffsetX), (int)(-ray.directionVector.Z * 100 + debuggerOffsetY), new Color3(1f, 0f, 0f));
						}
					}

					foreach (Intersection intersection in intersections) {
						screen.Plot((int)(intersection.position.X + debuggerOffsetX), (int)(-intersection.position.Z + debuggerOffsetY), new Color3(0, 0, 1));
					}
				}
			}
		}
	}
}



