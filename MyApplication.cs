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

namespace Template
{
    class MyApplication
    {
        // member variables
        public Surface screen;
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
                new Sphere(new System.Numerics.Vector3(0, 0, 600), 450) //TODO: bug als je y coordinaat verhoogt in debug
            };
            
            List<Light> lights = new List<Light>();
            
            System.Numerics.Vector3 origin = new System.Numerics.Vector3(0, 0, 0);

            int z = 100;
            //ScreenPlane screenPlane = new(origin + new System.Numerics.Vector3(-x / 2, y / 2, z), origin + new System.Numerics.Vector3(x / 2, y / 2, z), origin + new System.Numerics.Vector3(x / 2, -y / 2, z), origin + new System.Numerics.Vector3(-x / 2, -y / 2, z));
            System.Numerics.Vector3 upLeft = new System.Numerics.Vector3(-screenX, screenY, z) + origin;
            System.Numerics.Vector3 upRight = new System.Numerics.Vector3(screenX, screenY, z) + origin;
            System.Numerics.Vector3 downRight = new System.Numerics.Vector3(screenX, -screenY, z) + origin;
            System.Numerics.Vector3 downLeft = new System.Numerics.Vector3(-screenX, -screenY, z) + origin;

            ScreenPlane screenPlane = new(upLeft, upRight, downRight, downLeft);

            Camera camera = new Camera(screenPlane);

            Scene1 scene = new Scene1(primitives, lights);

            Raytracer raytracer = new Raytracer(scene, camera, screen);
            //................................

            bool debug = false;

            if (!debug)
            {
                for (int row = -screenY; row < screenY; row++)
                {
                    for (int column = -screenX; column < screenX; column++)
                    {
                        // REPLACE THIS WITH THE CORRECT COLOR FOR THIS PIXEL FROM YOUR RAY TRACER
                        //GET color function for row and column.
                        Color3 pixelcol = raytracer.GetPixelColor(row, column);

                        screen.Plot(column + screenX, row + screenY, pixelcol);
                    }
                }
            }
            else
            {
                //draw debug
                int camAnchorX = (int)(camera.position.X) + screenX;
                int camAnchorY = (int)(-camera.position.Z) + (int)(screenY) + screen.height/3;

                screen.Bar(camAnchorX - 5, camAnchorY - 5, camAnchorX + 5, camAnchorY + 5, new Color3(1f, 0.0f, 0.5f));

                screen.Line((int)(camera.screenPlane.upLeft.X) + screenX, camAnchorY + (int)(-camera.screenPlane.upLeft.Z - camera.position.Z), (int)(camera.screenPlane.upRight.X) + screenX, camAnchorY + (int)(-camera.screenPlane.upLeft.Z - camera.position.Z), new Color3(0f, 1f, 1f));

                foreach (Primitive primitive in scene.primitives)
                {
                    foreach (System.Numerics.Vector3 pixel in primitive.getPixels(scene, camera))
                    {
                        screen.Plot((int)(pixel.X + camAnchorX), (int)(-pixel.Z + camAnchorY), primitive.color);
                    }
                }

                //draw camera and primitives, recursively 
                for (int column = -screenX; column < screenX; column++)
                {
                    if (column % 10 == 0)
                    {
                        Intersection closestIntersection = raytracer.DebugRay(0, column);
                        if (closestIntersection != null)
                        {
                            screen.Line(camAnchorX, camAnchorY, camAnchorX + (int)(closestIntersection.position.X), camAnchorY + (int)(-closestIntersection.position.Z), new Color3(1f, 0f, 0f));
                        } else
                        {
                            System.Numerics.Vector3 worldPixel = new System.Numerics.Vector3(column, 0, camera.screenPlane.upLeft.Z);
                            System.Numerics.Vector3 dir = System.Numerics.Vector3.Normalize(worldPixel - camera.position);

                            int dx = (int)(dir.X * 1000);
                            int dy = (int)(dir.Z * 1000);

                            screen.Line(camAnchorX, camAnchorY, camAnchorX + dx, camAnchorY - dy, new Color3(1f, 0f, 0f));
                        }
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
    }
}