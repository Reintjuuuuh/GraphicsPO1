using OpenTK.Mathematics;
using System.Numerics;
using System.Diagnostics;
using System.Globalization;
using System.Data.Common;

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

            for (int row = 0; row < screen.height; row++)
            {
                for (int column = 0; column < screen.width; column++)
                {
                    // REPLACE THIS WITH THE CORRECT COLOR FOR THIS PIXEL FROM YOUR RAY TRACER
                    //screen.Plot(column, row, new Color3(0.5f, 0.5f, 0.5f));
                }
            }

            //ADDED MANUALY, NOT TEMPLATE.....

            //Console.WriteLine(new Sphere(new System.Numerics.Vector3(0, 0, 0), 10).Intersection(new Ray(new System.Numerics.Vector3(0, 0, -10), new System.Numerics.Vector3(0, 0, 1))).position);
                
            List<Primitive> primitives = new List<Primitive>() {
                //new Sphere(new System.Numerics.Vector3(0, 0, 20), 200),
                new Sphere(new System.Numerics.Vector3(0, 0, 100), 40)
            };
            List<Light> lights = new List<Light>();
            System.Numerics.Vector3 origin = new System.Numerics.Vector3(0, 0, 0);
            int x = screen.width / 2;
            int y = screen.height;
            int z = 100;
            ScreenPlane screenPlane = new(origin + new System.Numerics.Vector3(-x/2, y/2, z), origin + new System.Numerics.Vector3(x/2, y/2, z), origin + new System.Numerics.Vector3(x/2, -y/2, z), origin + new System.Numerics.Vector3(-x/2, -y/2, z));
            Camera camera = new Camera(screenPlane);
            Scene1 scene = new Scene1(primitives, lights);
            Raytracer raytracer = new Raytracer(scene, camera, screen);

            //Debugger
            //raytracer.RenderDebug();
            //Normal screen
            raytracer.Render();

            //................................


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