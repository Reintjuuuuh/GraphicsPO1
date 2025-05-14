using System;
using System.Data.Common;
using System.Numerics;
using Template;

public class Raytracer
{
	Scene1 scene;
	Camera camera;
	Surface surface;

	public Raytracer(Scene1 scene, Camera camera, Surface surface)
	{
		this.scene = scene;
		this.camera = camera;
		this.surface = surface;
	}

	public void Render() {
		for(int row = (int) camera.screenPlane.downLeft.Y; row < (int) camera.screenPlane.upLeft.Y; row++) {
			for(int col = (int) camera.screenPlane.upLeft.X; col < (int) camera.screenPlane.upRight.X; col++) {
                Ray ray = new Ray(new Vector3(col, row, camera.position.Z), camera.lookAtDirection);
				List<Intersection> intersections = new();
				foreach(Primitive primitive in scene.primitives) {
					Intersection intersection = primitive.Intersection(ray);
					if(intersection != null) {
						intersections.Add(intersection);
					}
				}
				if(intersections.Count > 0) {
					Intersection closestIntersection = intersections.Min();
					surface.Plot(col + 200, row + 200, closestIntersection.primitive.color);
				} else {
					surface.Plot(col + 200, row + 200, new Color3(0.5f, 0.5f, 0.5f));
				}
			}
		}
	}
}
