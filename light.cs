using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Data;
using System.Numerics;
using Template;

public class Light {
    public Vector3 location;
    public Color3 intensity;
    
    //light constructor

    public Light(Vector3 location, Color3 intensity) {
        this.location = location;
        this.intensity = intensity;
    }
}

public class SpotLight : Light {
    public float angle;
    public Vector3 direction;
    
    public SpotLight(Vector3 location, Color3 intensity, Vector3 direction, float angle) : base(location, intensity) {
        this.direction = direction;
        this.angle = angle;
    }
}

