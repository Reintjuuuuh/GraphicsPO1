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

    public bool debug = false;

	public Raytracer(Scene1 scene, Camera camera, Surface surface)
	{
		this.scene = scene;
		this.camera = camera;
		this.surface = surface;
	}

	public void Render() {
        for (int row = (int)camera.screenPlane.downLeft.Y; row < (int)camera.screenPlane.upLeft.Y; row++)
        {
            for (int col = (int)camera.screenPlane.upLeft.X; col < (int)camera.screenPlane.upRight.X; col++)
            {
                Ray ray = new Ray(new Vector3(col, row, camera.position.Z), camera.lookAtDirection);
                List<Intersection> intersections = new();
                foreach (Primitive primitive in scene.primitives) //TODO: this method should be in scene class.
                {
                    Intersection intersection = primitive.Intersection(ray);
                    if (intersection != null)
                    {
                        intersections.Add(intersection);
                    }
                }
                if (intersections.Count > 0)
                {
                    Intersection closestIntersection = intersections.Min();
                    surface.Plot(col + 200, row + 200, closestIntersection.primitive.color);
                }
                else
                {
                    surface.Plot(col + 200, row + 200, new Color3(0.5f, 0.5f, 0.5f));
                }
            }
        
            if (row == (camera.screenPlane.upLeft.Y + camera.screenPlane.downLeft.Y)/2) //if middle row show debug output
            {
                for (int col = (int)camera.screenPlane.upLeft.X; col < (int)camera.screenPlane.upRight.X; col++)
                {
                    Ray ray = new Ray(new Vector3(col, row, camera.position.Z), camera.lookAtDirection);
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
                        foreach (Intersection i in intersections)
                        {
                            surface.Plot((int)i.position.X + 400, (int)i.position.Z + 200, new Color3(0f, 1f, 0f));
                        }

                        Intersection closestIntersection = intersections.Min();

                        if (col % 10 == 0) //draw ray
                        {
                            surface.Line((int)ray.orgin.X + 400, (int)ray.orgin.Z + 200, (int)closestIntersection.position.X + 400, (int)closestIntersection.position.Z + 200, new Color3(0f, 0f, 1f));
                        }
                    }
                    else
                    {
                        surface.Plot(col + 400, row + 200, new Color3(0f, 0f, 0f));
                    }
                    
                }
                surface.Plot((int)camera.position.X + 400, (int)camera.position.Z + 200, new Color3(1f, 0f, 0f));
            }
        }
    }
}
