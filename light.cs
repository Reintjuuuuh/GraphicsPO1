using Assimp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Data;
using Template;

public class Light {
    public Vector3D location;
    public Color3 intensity;
    
    //light constructor
    public Light(Vector3D l, Color3 i) {
        this.location = l;
        this.intensity = i;
    }
}
