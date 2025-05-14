using OpenTK.Mathematics;
using System.Numerics;
using System.Diagnostics;
using System.Globalization;

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
            
            
            
            List<Primitive> primitives = new List<Primitive>() {
                //new Sphere(new System.Numerics.Vector3(0, 0, 20), 200),
                new Sphere(new System.Numerics.Vector3(0, 0, 50), 20)
            };
            List<Light> lights = new List<Light>();
            Camera camera = new Camera();
            Scene1 scene = new Scene1(primitives, lights);
            Raytracer raytracer = new Raytracer(scene, camera, screen);

            //Debugger
            raytracer.RenderDebug();
            //Normal screen
            //raytracer.Render(false);
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