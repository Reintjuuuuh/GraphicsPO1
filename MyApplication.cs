using OpenTK.Mathematics;
using System.Numerics;
using System.Diagnostics;
using System.Globalization;
using System.Data.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Windows.Input;
using OpenTK.Windowing.Common;
using System.ComponentModel;
using OpenTK.Compute.OpenCL;
using Assimp;
using SixLabors.ImageSharp.PixelFormats;
using OpenTK.Graphics.ES11;
using System.Net;

namespace Template
{
    class MyApplication
    {
        // member variables
        public Surface screen;
        Camera camera;
        private readonly Stopwatch timer = new();
        // constructor
        public MyApplication(Surface screen)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            this.screen = screen;
        }
        // initialize
        public void Init()
        {
            // (optional) example of how you can load a triangle mesh in any file format supported by Assimp
            object? mesh = Util.ImportMesh("../../../assets/cube.obj");
        }
        // tick: renders one frame
        private TimeSpan deltaTime = new();
        private uint frames = 0;
        private string timeString = "---- ms/frame";
        public void Tick()
        {
            timer.Restart();

            screen.Clear(0);

            

            //ADDED MANUALY, NOT TEMPLATE.....

            int screenX = screen.width / 2;
            int screenY = screen.height / 2;

            System.Numerics.Vector3 normal = new(0, 1, 0);
            System.Numerics.Vector3 point = new(0, 0, 0);
            Plane p = new Plane(normal, point);

            List<Primitive> primitives = new List<Primitive>() { //TODO: we maken nu elke tick een nieuwe camera, primitves, light, etc.
                new Sphere(new System.Numerics.Vector3(0, 20, 500), 200), //TODO: bug als je y coordinaat verhoogt in debug
                new Sphere(new System.Numerics.Vector3(90, 0, 200), 10) //TODO: bug als je y coordinaat verhoogt in debug
            };
            
            List<Light> lights = new List<Light>();

            System.Numerics.Vector3 origin = new System.Numerics.Vector3(0, 0, 0);

            System.Numerics.Vector3 camPosition = new System.Numerics.Vector3(0, 0, 0);
            System.Numerics.Vector3 forwardDir = new System.Numerics.Vector3(0, 0, 1);
            System.Numerics.Vector3 upDir = new System.Numerics.Vector3(0, 1, 0);
            System.Numerics.Vector3 rightDir = System.Numerics.Vector3.Normalize(System.Numerics.Vector3.Cross(upDir, forwardDir));
            
            ScreenPlane screenplane = calculateScreenplane(camPosition, forwardDir, upDir, rightDir);

            camera = new Camera(camPosition, forwardDir, upDir, rightDir, screenplane);

            Scene1 scene = new Scene1(primitives, lights);

            Raytracer raytracer = new Raytracer(scene, camera, screen);
            //................................

            bool debug = true;

            if (!debug)
            {
                for (int row = -screenY; row < screenY; row++)
                {
                    for (int col = -screenX; col < screenX; col++)
                    {
                        //normalize to 0-1
                        float u = (col + screenX) / (float)screen.width;
                        float v = (row + screenY) / (float)screen.height;

                        //get screenplane points
                        System.Numerics.Vector3 upLeft = camera.screenPlane.upLeft;
                        System.Numerics.Vector3 upRight = camera.screenPlane.upRight;
                        System.Numerics.Vector3 downRight = camera.screenPlane.downRight;
                        System.Numerics.Vector3 downLeft = camera.screenPlane.downLeft;

                        //calculate the worlpoint coordinates of the pixel
                        System.Numerics.Vector3 top = System.Numerics.Vector3.Lerp(upLeft, upRight, u); 
                        System.Numerics.Vector3 bottom = System.Numerics.Vector3.Lerp(downLeft, downRight, u); 
                        System.Numerics.Vector3 worldPoint = System.Numerics.Vector3.Lerp(top, bottom, v);

                        //trace ray
                        System.Numerics.Vector3 direction = System.Numerics.Vector3.Normalize(worldPoint - camera.position);

                        Color3 pixelcol = raytracer.TraceRay(camera.position, direction);

                        screen.Plot(col + screenX, row + screenY, pixelcol);
                    }
                }
            }
            else
            {
                //get all worldccoordinates as vector 3. Convert to screenpixels and draw.
                float scale = 0.9f; // projection scale (zoom out)
                System.Numerics.Vector2 middleOfScreen = new System.Numerics.Vector2(screenX, screenY);
                
                //Project camera
                System.Numerics.Vector2 camProjection = ProjectToPixel(camera.position);
                System.Numerics.Vector2 camPoint2d = camProjection * scale + middleOfScreen;

                int camSize = (int)(5 * scale);
                screen.Bar((int)camPoint2d.X - camSize, (int)camPoint2d.Y - camSize, (int)camPoint2d.X + camSize, (int)camPoint2d.Y + camSize, new Color3(1f, 0.0f, 0.5f));

                //Project screenplane
                System.Numerics.Vector2 screenPlaneLeftProjection = ProjectToPixel(camera.screenPlane.upLeft);
                System.Numerics.Vector2 screenPlaneRightProjection = ProjectToPixel(camera.screenPlane.upRight);
                System.Numerics.Vector2 screenPlaneLeftPoint2d = screenPlaneLeftProjection * scale + middleOfScreen;
                System.Numerics.Vector2 screenPlaneRightPoint2d = screenPlaneRightProjection * scale + middleOfScreen;

                screen.Line((int)screenPlaneLeftPoint2d.X, (int)screenPlaneLeftPoint2d.Y, (int)screenPlaneRightPoint2d.X, (int)screenPlaneRightPoint2d.Y, new Color3(0f, 1f, 1f));

                //Project primitives
                foreach (Primitive primitive in scene.primitives)
                {
                    System.Numerics.Vector2 primitiveProjection = ProjectToPixel(primitive.position);
                    System.Numerics.Vector2 primitivePoint2d = primitiveProjection * scale + middleOfScreen;

                    //screen.Plot((int)primitivePoint2d.X, (int)primitivePoint2d.Y, primitive.color); //middle of primitive
                    
                    if (primitive is Sphere)
                    {
                        //plot radius as pixels. Keep in mind height etc. can change radius.
                        System.Numerics.Vector3 delta = primitive.position - camera.position;
                        float height = System.Numerics.Vector3.Dot(delta, camera.upDirection);

                        float radius = (int)(((Sphere)primitive).radius);
                        float crossSectionRadius = 0f;
                        if (MathF.Abs(height) < radius)
                        {
                            crossSectionRadius = MathF.Sqrt(radius*radius - height*height);
                        }
                        
                        int pixelRadius = (int)(crossSectionRadius * scale + 0.5f); // always force at least one pixel.
;
                        int cx = (int)primitivePoint2d.X;
                        int cy = (int)primitivePoint2d.Y;
                        int x = pixelRadius;
                        int y = 0;
                        int err = 0;

                        while (x >= y)
                        {
                            screen.Plot(cx + x, cy + y, primitive.color);
                            screen.Plot(cx + y, cy + x, primitive.color);
                            screen.Plot(cx - y, cy + x, primitive.color);
                            screen.Plot(cx - x, cy + y, primitive.color);
                            screen.Plot(cx - x, cy - y, primitive.color);
                            screen.Plot(cx - y, cy - x, primitive.color);
                            screen.Plot(cx + y, cy - x, primitive.color);
                            screen.Plot(cx + x, cy - y, primitive.color);

                            y++;
                            err += 1 + 2 * y;
                            if (2 * (err - x) + 1 > 0)
                            {
                                x--;
                                err += 1 - 2 * x;
                            }
                        }
                    }
                }

                //draw rays
                //vector3 of camposition and vector 3 of screenplane point.
                //project those to pixels
                for (int col = -screenX; col < screenX; col += 10)
                {
                    //normalize to 0-1
                    float u = (col + screenX) / (float)screen.width;
                    float v = (0 + screenY) / (float)screen.height;

                    //get screenplane points
                    System.Numerics.Vector3 upLeft = camera.screenPlane.upLeft;
                    System.Numerics.Vector3 upRight = camera.screenPlane.upRight;
                    System.Numerics.Vector3 downRight = camera.screenPlane.downRight;
                    System.Numerics.Vector3 downLeft = camera.screenPlane.downLeft;

                    //calculate the worlpoint coordinates of the pixel
                    System.Numerics.Vector3 top = System.Numerics.Vector3.Lerp(upLeft, upRight, u);
                    System.Numerics.Vector3 bottom = System.Numerics.Vector3.Lerp(downLeft, downRight, u);
                    System.Numerics.Vector3 worldPoint = System.Numerics.Vector3.Lerp(top, bottom, v);

                    //trace ray
                    System.Numerics.Vector3 direction = System.Numerics.Vector3.Normalize(worldPoint - camera.position);

                    Intersection intersect = raytracer.DebugRay(camera.position, direction);

                    if (intersect != null) {
                        System.Numerics.Vector2 intersectionProjection = ProjectToPixel(intersect.position);
                        System.Numerics.Vector2 intersectionPoint2d = intersectionProjection * scale + middleOfScreen;

                        screen.Line((int)camPoint2d.X, (int)camPoint2d.Y, (int)intersectionPoint2d.X, (int)intersectionPoint2d.Y, new Color3(1f, 0f, 0f));
                    }
                }

            }

            deltaTime += timer.Elapsed;
            frames++;
            if (deltaTime.TotalSeconds > 1)
            {
                timeString = (deltaTime.TotalMilliseconds / frames).ToString("F1") + " ms/frame";
                frames = 0;
                deltaTime = TimeSpan.Zero;
            }


            screen.PrintOutlined(timeString, 2, 2, Color4.White);
        }

        public ScreenPlane calculateScreenplane(System.Numerics.Vector3 camPosition, System.Numerics.Vector3 forwardDir, System.Numerics.Vector3 upDir, System.Numerics.Vector3 rightDir)
        {
            float fov = MathF.PI / 3f;

            float focalDistance = 100f;

            float halfHeight = MathF.Tan(fov*0.5f) * focalDistance;
            float aspectRatio = (float)screen.width / (float)screen.height;
            float halfWidth = halfHeight * aspectRatio;

            System.Numerics.Vector3 center = camPosition + forwardDir * focalDistance;
            System.Numerics.Vector3 up = upDir * halfHeight;
            System.Numerics.Vector3 right = rightDir * halfWidth;

            System.Numerics.Vector3 upLeft = center + up - right;
            System.Numerics.Vector3 upRight = center + up + right;
            System.Numerics.Vector3 downRight = center - up + right;
            System.Numerics.Vector3 downLeft = center - up - right;

            //Console.WriteLine($"UL: {upLeft}, UR: {upRight}, DR: {downRight}, DL: {downLeft}");

            return new ScreenPlane(upLeft, upRight, downRight, downLeft);
        }
        private System.Numerics.Vector2 ProjectToPixel(System.Numerics.Vector3 WorldPoint)
        {
            //calc vector3 differnce with camera position
            System.Numerics.Vector3 camPosition = camera.position;
            System.Numerics.Vector3 forward = camera.lookAtDirection;
            System.Numerics.Vector3 up = camera.upDirection;
            System.Numerics.Vector3 right = camera.rightDirection;

            System.Numerics.Vector3 delta = WorldPoint - camPosition;

            float x = System.Numerics.Vector3.Dot(delta, right);
            float y = System.Numerics.Vector3.Dot(delta, forward);

            return new System.Numerics.Vector2(x, -(y-screen.height/2));
        }
    }
}