using Assimp;
using OpenTK.Compute.OpenCL;
using System;
using System.Data.Common;
using System.Numerics;
using Template;

public class Raytracer
{
	Scene1 scene;
	Camera camera;
	Surface surface;

	public Raytracer(Scene1 scene, Camera camera, Surface surface){
        this.scene = scene;
		this.camera = camera;
		this.surface = surface;
	}

	public void Render() {
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
				if (intersections.Count > 0) {
					Intersection closestIntersection = intersections.Min();
					surface.Plot(col + 200, row + 200, closestIntersection.primitive.color);

				} else {
					surface.Plot(col + 200, row + 200, new Color3(0.5f, 0.5f, 0.5f));
				}
			}
		}
	}

	public void RenderDebug() {		
		for(int x = (int) (surface.width * -0.5); x < (int)(surface.width * 0.5);  x++) {
            for (int z = (int)(surface.height * -0.5); z < (int)(surface.height * 0.5); z++) {
                surface.Plot(x + (int)(surface.width * 0.5), z + (int)(surface.height * 0.5), new Color3(0.5f, 0.5f, 0.5f));
            }
        }
        
		for (int x = (int)camera.screenPlane.downLeft.X; x <= (int)camera.screenPlane.downRight.X; x++) {
			Ray ray = new Ray(camera.position, new Vector3(x, camera.position.Y, camera.screenPlane.upLeft.Z));
			foreach (Vector3 pixel in ray.getPixels(scene, camera)) {
				surface.Plot(Custom.Round(pixel.X + (int)(surface.width * 0.5)), Custom.Round(pixel.Z + (int)(surface.height * 0.1)), new Color3(1, 0, 0));
			}
        }

		foreach(Primitive primitive in scene.primitives) {
            foreach (Vector3 pixel in primitive.getPixels(scene, camera)) {
                surface.Plot(Custom.Round(pixel.X + (int)(surface.width * 0.5)), Custom.Round(pixel.Z + (int)(surface.height * 0.1)), primitive.color);
            }
        }
 
		surface.Plot((int)camera.position.X + (int)(surface.width * 0.5), (int)camera.position.Z + (int)(surface.height * 0.1), new Color3(1, 0.98f, 0));
        surface.Plot((int)camera.position.X + (int)(surface.width * 0.5), (int)camera.position.Z + (int)(surface.height * 0.1), new Color3(1, 0.98f, 0));
        surface.Plot((int)camera.position.X + (int)(surface.width * 0.5), (int)camera.position.Z + (int)(surface.height * 0.1), new Color3(1, 0.98f, 0));
        surface.Plot((int)camera.position.X + (int)(surface.width * 0.5), (int)camera.position.Z + (int)(surface.height * 0.1), new Color3(1, 0.98f, 0));
    }
}



