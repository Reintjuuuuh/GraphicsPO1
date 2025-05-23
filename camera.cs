using System;
using System.Numerics;


public class Camera
{
	public Vector3 position = new Vector3(0, 0, 0);
    public Vector3 forwardDirection = new Vector3(0, 0, 1);
    public Vector3 upDirection = new Vector3(0, 1, 0);
    public Vector3 rightDirection = new Vector3(1, 0, 0);
    public ScreenPlane screenPlane = new ScreenPlane(new Vector3(-100, 100, 10), new Vector3(100, 100, 10), new Vector3(100, -100, 10), new Vector3(-100, -100, 10));
    //public ScreenPlane screenPlane;

    //Standerd camera has a 200x100 screenplane, with a corner of x=-100 and x=100. The y is up, so the upper corners have an y of 100;
    public Camera() {
        
    }
    
	public Camera(Vector3 position, Vector3 forwardDirection, Vector3 upDirection, Vector3 rightDirection, ScreenPlane screenPlane) {
        this.position = position;
        this.forwardDirection = forwardDirection;
        this.upDirection = upDirection;
        this.rightDirection = rightDirection;
        this.screenPlane = screenPlane;
    }

    public Camera(ScreenPlane screenPlane) {
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