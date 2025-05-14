using System;
using System.Numerics;


public class Camera
{
	public Vector3 position;
    public Vector3 lookAtDirection;
    public Vector3 upDirection;
    public ScreenPlane screenPlane;

    //Standerd camera has a 200x100 screenplane, with a corner of x=-100 and x=100. The y is up, so the upper corners have an y of 100;
    public Camera() {
        this.position = new Vector3(0, 0, 0);
        this.lookAtDirection = new Vector3(0, 0, 1);
        this.upDirection = new Vector3(0, 1, 0);
        this.screenPlane = new ScreenPlane(new Vector3(-100, 100, 0), new Vector3(100, 100, 0), new Vector3(100, -100, 0), new Vector3(-100, -100, 0));
    }

	public Camera(Vector3 position, Vector3 lookAtDirection, Vector3 upDirection, ScreenPlane screenPlane) {
        this.position = position;
        this.lookAtDirection = lookAtDirection;
        this.upDirection = upDirection;
        this.screenPlane = screenPlane;
    }
}

public class ScreenPlane {
    public Vector3 upLeft;
    public Vector3 upRight;
    public Vector3 downRight;
    public Vector3 downLeft;

    public ScreenPlane(Vector3 upLeft, Vector3 upRight, Vector3 downRight, Vector3 downLeft){
        this.upLeft = upLeft;
        this.upRight = upRight;
        this.downRight = downRight;
        this.downLeft = downLeft;
    }
}