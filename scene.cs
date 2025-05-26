using Assimp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Data;
using Template;

public class Scene1 {
    public List<Primitive> primitives;
    public List<Light> lights;

    //light constructor
    public Scene1(List<Primitive> p, List<Light> l) {
        this.primitives = p;
        this.lights = l;
    }

    public List<Intersection> FindIntersections() {
        return null;
    }

}
