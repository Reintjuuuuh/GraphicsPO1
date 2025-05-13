using System;
using System.Numerics;


public class Camera
{
	public Vector3 position;
    public Vector3 lookAtDirection;
    public Vector3 upDirection;
    public ScreenPlane screenPlane;

    public Camera() {
        this.position = new Vector3(0, 0, 0);
        this.lookAtDirection = new Vector3(0, 0, 1);
        this.upDirection = new Vector3(0, 1, 0);
        this.screenPlane = new ScreenPlane(new Vector3(10, 0, 0), new Vector3(-10, 0, 0), new Vector3(0, 0, 10), new Vector3(0, 0, -10));
    }

	public Camera(Vector3 position, Vector3 lookAtDirection, Vector3 upDirection, ScreenPlane screenPlane) {
        this.position = position;
        this.lookAtDirection = lookAtDirection;
        this.upDirection = upDirection;
        this.screenPlane = screenPlane;
    }
}

public class ScreenPlane {
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;
    public Vector3 d;

    public ScreenPlane(Vector3 a, Vector3 b, Vector3 c, Vector3 d){
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
    }
}