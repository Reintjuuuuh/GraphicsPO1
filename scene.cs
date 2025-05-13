using Assimp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Data;
using Template;

public class Scene {
    public List<Primitive> primitives;
    public List<Light> lights;

    //light constructor
    public Scene(List<Primitive> p, List<Light> l) {
        this.primitives = p;
        this.lights = l;
    }

    //TODO: intersect method.
    //Loop over all primitives and return the closest intersection
}
