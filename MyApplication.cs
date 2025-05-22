//using OpenTK.Mathematics;
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

        public Camera camera;
        public List<Primitive> primitives;
        public List<Light> lights;
        public ScreenPlane screenplane;
        public Scene1 scene;
        public Raytracer raytracer;

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
            primitives = new List<Primitive>() { //TODO: we maken nu elke tick een nieuwe camera, primitves, light, etc.
                new Sphere(new Vector3(0, 0, 500), 200), //TODO: bug als je y coordinaat verhoogt in debug
                new Sphere(new Vector3(90, 0, 200), 10) //TODO: bug als je y coordinaat verhoogt in debug
            };

            lights = new List<Light>();

            Vector3 origin = new Vector3(0, 0, 0);

            Vector3 camPosition = new Vector3(0, 0, 0);
            Vector3 forwardDir = new Vector3(0, 0, 1);
            Vector3 upDir = new Vector3(0, 1, 0);
            Vector3 rightDir = Vector3.Normalize(Vector3.Cross(upDir, forwardDir));

            screenplane = calculateScreenplane(camPosition, forwardDir, upDir, rightDir, screen.width, screen.height);

            camera = new Camera(camPosition, forwardDir, upDir, rightDir, screenplane);

            scene = new Scene1(primitives, lights);

            raytracer = new Raytracer(scene, camera, screen);

            // (optional) example of how you can load a triangle mesh in any file format supported by Assimp
            object? mesh = Util.ImportMesh("../../../assets/cube.obj");
        }
        // tick: renders one frame
        private TimeSpan deltaTime = new();
        private uint frames = 0;
        private string timeString = "---- ms/frame";
        public void Tick(int totalWidth, int totalHeight)
        {
            timer.Restart();

            screen.Clear(0);

            int halfWidth = totalWidth/2;

            Surface surfaceNormal = new Surface(halfWidth, totalHeight);
            Surface surfaceDebug = new Surface(halfWidth, totalHeight);

            camera.screenPlane = calculateScreenplane(camera.position, camera.lookAtDirection, camera.upDirection, camera.rightDirection, halfWidth, totalHeight);

            GL.Viewport(0, 0, halfWidth, totalHeight);
            RenderNormal(surfaceNormal);

            camera.screenPlane = calculateScreenplane(camera.position, camera.lookAtDirection, camera.upDirection, camera.rightDirection, halfWidth, totalHeight);

            GL.Viewport(halfWidth, 0, halfWidth, totalHeight);
            RenderDebug(surfaceDebug);

            //go back to entire screen
            surfaceNormal.CopyTo(screen, 0, 0);
            surfaceDebug.CopyTo(screen, halfWidth, 0);


            GL.Viewport(0, 0, totalWidth, totalHeight);

            deltaTime += timer.Elapsed;
            frames++;
            if (deltaTime.TotalSeconds > 1)
            {
                timeString = (deltaTime.TotalMilliseconds / frames).ToString("F1") + " ms/frame";
                frames = 0;
                deltaTime = TimeSpan.Zero;
            }


            screen.PrintOutlined(timeString, 2, 2, new Color3(1,1,1));
        }

        public ScreenPlane calculateScreenplane(Vector3 camPosition, Vector3 forwardDir, Vector3 upDir, Vector3 rightDir, int screenWidth, int screenHeight)
        {
            float fov = MathF.PI / 3f;

            float focalDistance = 100f;

            float halfHeight = MathF.Tan(fov*0.5f) * focalDistance;
            float aspectRatio = (float)screenWidth / (float)screenHeight;
            float halfWidth = halfHeight * aspectRatio;

            Vector3 center = camPosition + forwardDir * focalDistance;
            Vector3 up = upDir * halfHeight;
            Vector3 right = rightDir * halfWidth;

            Vector3 upLeft = center + up - right;
            Vector3 upRight = center + up + right;
            Vector3 downRight = center - up + right;
            Vector3 downLeft = center - up - right;

            //Console.WriteLine($"UL: {upLeft}, UR: {upRight}, DR: {downRight}, DL: {downLeft}");

            return new ScreenPlane(upLeft, upRight, downRight, downLeft);
        }
        private Vector2 ProjectToPixel(Vector3 WorldPoint)
        {
            //calc vector3 differnce with camera position
            Vector3 camPosition = camera.position;
            Vector3 forward = camera.lookAtDirection;
            Vector3 right = camera.rightDirection;

            Vector3 delta = WorldPoint - camPosition;

            float x = Vector3.Dot(delta, right);
            float y = Vector3.Dot(delta, forward);

            return new Vector2(x, -(y-screen.height/2));
        }

        private void RenderNormal(Surface screen)
        {
            int screenX = screen.width / 2;
            int screenY = screen.height / 2;
            for (int row = -screenY; row < screenY; row++)
            {
                for (int col = -screenX; col < screenX; col++)
                {
                    //normalize to 0-1
                    float u = (col + screenX) / (float)screen.width;
                    float v = (row + screenY) / (float)screen.height;

                    //get screenplane points
                    Vector3 upLeft = camera.screenPlane.upLeft;
                    Vector3 upRight = camera.screenPlane.upRight;
                    Vector3 downRight = camera.screenPlane.downRight;
                    Vector3 downLeft = camera.screenPlane.downLeft;

                    //calculate the worlpoint coordinates of the pixel
                    Vector3 top = Vector3.Lerp(upLeft, upRight, u);
                    Vector3 bottom = Vector3.Lerp(downLeft, downRight, u);
                    Vector3 worldPoint = Vector3.Lerp(top, bottom, v);

                    //trace ray
                    Vector3 direction = Vector3.Normalize(worldPoint - camera.position);

                    Color3 pixelcol = raytracer.TraceRay(camera.position, direction);

                    screen.Plot(col + screenX, row + screenY, pixelcol);
                }
            }
        }
        private void RenderDebug(Surface screen)
        {
            int screenX = screen.width / 2;
            int screenY = screen.height / 2;

            //get all worldccoordinates as vector 3. Convert to screenpixels and draw.
            float scale = 0.9f; // projection scale (zoom out)
            Vector2 middleOfScreen = new Vector2(screenX, screenY);

            //Project camera
            Vector2 camProjection = ProjectToPixel(camera.position);
            Vector2 camPoint2d = camProjection * scale + middleOfScreen;

            int camSize = (int)(5 * scale);
            screen.Bar((int)camPoint2d.X - camSize, (int)camPoint2d.Y - camSize, (int)camPoint2d.X + camSize, (int)camPoint2d.Y + camSize, new Color3(1f, 0.0f, 0.5f));

            //Project screenplane
            Vector2 screenPlaneLeftProjection = ProjectToPixel(camera.screenPlane.upLeft);
            Vector2 screenPlaneRightProjection = ProjectToPixel(camera.screenPlane.upRight);
            Vector2 screenPlaneLeftPoint2d = screenPlaneLeftProjection * scale + middleOfScreen;
            Vector2 screenPlaneRightPoint2d = screenPlaneRightProjection * scale + middleOfScreen;

            screen.Line((int)screenPlaneLeftPoint2d.X, (int)screenPlaneLeftPoint2d.Y, (int)screenPlaneRightPoint2d.X, (int)screenPlaneRightPoint2d.Y, new Color3(0f, 1f, 1f));

            //Project primitives
            foreach (Primitive primitive in scene.primitives)
            {
                Vector2 primitiveProjection = ProjectToPixel(primitive.position);
                Vector2 primitivePoint2d = primitiveProjection * scale + middleOfScreen;

                //screen.Plot((int)primitivePoint2d.X, (int)primitivePoint2d.Y, primitive.color); //middle of primitive

                if (primitive is Sphere)
                {
                    //plot radius as pixels. Keep in mind height etc. can change radius.
                    Vector3 delta = primitive.position - camera.position;
                    float height = Vector3.Dot(delta, camera.upDirection);

                    float radius = (int)(((Sphere)primitive).radius);
                    float crossSectionRadius = 0f;
                    if (MathF.Abs(height) < radius)
                    {
                        crossSectionRadius = MathF.Sqrt(radius * radius - height * height);
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
                Vector3 upLeft = camera.screenPlane.upLeft;
                Vector3 upRight = camera.screenPlane.upRight;
                Vector3 downRight = camera.screenPlane.downRight;
                Vector3 downLeft = camera.screenPlane.downLeft;

                //calculate the worlpoint coordinates of the pixel
                Vector3 top = Vector3.Lerp(upLeft, upRight, u);
                Vector3 bottom = Vector3.Lerp(downLeft, downRight, u);
                Vector3 worldPoint = Vector3.Lerp(top, bottom, v);
                
                //trace ray
                Vector3 direction = Vector3.Normalize(worldPoint - camera.position);

                Intersection intersect = raytracer.DebugRay(camera.position, direction);

                if (intersect != null)
                {
                    Vector2 intersectionProjection = ProjectToPixel(intersect.position);
                    Vector2 intersectionPoint2d = intersectionProjection * scale + middleOfScreen;

                    screen.Line((int)camPoint2d.X, (int)camPoint2d.Y, (int)intersectionPoint2d.X, (int)intersectionPoint2d.Y, new Color3(1f, 0f, 0f));
                }
            }
        }

        public void HandleKeyboardInput(KeyboardState keyboard, float deltaTime)
        {
            float movementSpeed = 50f;
            //Vector3 movementVector = Vector3()
        }
    }
}