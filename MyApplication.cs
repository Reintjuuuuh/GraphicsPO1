using System.Numerics;
using System.Diagnostics;
using System.Globalization;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.ES11;
using OpenTK.Windowing.Common;

namespace Template
{
    class MyApplication
    {
        // member variables
        public Surface screen;

        public Camera camera;
        public List<Primitive> primitives;
        public List<Light> lights;
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
            primitives = new List<Primitive>() {
                new Plane(new Vector3(0, 1, 0), new Vector3(0, -100, 0), new Color3(1, 1, 1), true),
                new Sphere(new Vector3(200, 0, 200), 100),
                new Sphere(new Vector3(-200, 100, 0), 50, new Color3(0, 0, 1),true),
                new Sphere(new Vector3(-200, 100, 120), 50, new Color3(1, 1, 1), true),
                new Sphere(new Vector3(200, 250, 200), 200),
                new Sphere(new Vector3(100, 100, -100), 30),
                new Sphere(new Vector3(-300, 0, -300), 10, new Color3(0.5f, 0.5f, 0.5f), false)
            };

            float lightIntensity = 20000;
            lights = new List<Light>()
            {
                new Light(new Vector3(0, 0, 0), new Color3(1, 1, 1) * lightIntensity),
                new Light(new Vector3(300, 100, 0), new Color3(1, 1, 1) * 2 * lightIntensity),
                new SpotLight(new Vector3(-300, 30, -300), new Color3(0, 1, 0) * lightIntensity, new Vector3(0, -1, 0), (float) Math.PI / 3)
            };

            foreach(Light light in lights) {
                primitives.Add(new Sphere(light.location + new Vector3(0, 15, 0), 5, new Color3(0, 1, 0), false));
            }

            int primitiveCount = 5;
            Random random = new Random();
            for (int i = 0; i < primitiveCount; i++)
            {
                //primitives.Add(new Sphere(new Vector3(-1000 + random.Next(2000), -1000 + random.Next(2000), random.Next(1000)), 1 + random.Next(400), true));
                //lights.Add(new Light(new Vector3(-1000 + random.Next(2000), -1000 + random.Next(2000), random.Next(1000)), new Color3(1, 1, 1)));
            }

            // vertices van 3d driehoek
            Vector3 v0 = new Vector3(0, 60, 200);
            Vector3 v1 = new Vector3(-60, -60, 100);
            Vector3 v2 = new Vector3(60, -60, 100);
            Vector3 v3 = new Vector3(0, 0, 300);
            Vector3 n0 = Vector3.Normalize(new Vector3(0, 2, 1));
            Vector3 n1 = Vector3.Normalize(new Vector3(-2, -2, 1));
            Vector3 n2 = Vector3.Normalize(new Vector3(2, -2, 1));
            Vector3 n3 = Vector3.Normalize(new Vector3(0, 0, 3));

            // f1
            Triangle t1 = new Triangle(v0, v1, v2, n0, n1, n2);
            t1.color = new Color3(1, 0, 0); // Red
            t1.interpolateNormals = true;

            // f2
            Triangle t2 = new Triangle(v0, v1, v3, n0, n1, n3);
            t2.color = new Color3(0, 1, 0); // Green
            t2.interpolateNormals = true;

            // f3
            Triangle t3 = new Triangle(v1, v2, v3, n1, n2, n3);
            t3.color = new Color3(0, 0, 1); // Blue
            t3.interpolateNormals = true;

            // f4
            Triangle t4 = new Triangle(v2, v0, v3, n2, n0, n3);
            t4.color = new Color3(1, 1, 0); // Yellow
            t4.interpolateNormals = true;
            primitives.Add(t1);
            primitives.Add(t2);
            primitives.Add(t3);
            primitives.Add(t4);

            Vector3 camPosition = new Vector3(0, 0, 0);
            Vector3 forwardDir = new Vector3(0, 0, 1);
            Vector3 upDir = new Vector3(0, 1, 0);
            Vector3 rightDir = Vector3.Normalize(Vector3.Cross(upDir, forwardDir));

            camera = new Camera(camPosition, forwardDir, upDir, rightDir, new ScreenPlane(Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero));

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
            //scene.lights[0].location = camera.position;

            timer.Restart();
            screen.Clear(0);

            int halfWidth = totalWidth/2;

            Surface surfaceNormal = new Surface(halfWidth, totalHeight);
            Surface surfaceDebug = new Surface(halfWidth, totalHeight);

            camera.screenPlane = calculateScreenplane(halfWidth, totalHeight);

            GL.Viewport(0, 0, halfWidth, totalHeight);
            RenderNormal(surfaceNormal);

            camera.screenPlane = calculateScreenplane(halfWidth, totalHeight);

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

        public int fovDegrees = 90;
        public ScreenPlane calculateScreenplane(int screenWidth, int screenHeight)
        {
            float fov = fovDegrees * (MathF.PI / 180);

            float focalDistance = 100f;

            float halfHeight = MathF.Tan(fov*0.5f) * focalDistance;
            float aspectRatio = (float)screenWidth / (float)screenHeight;
            float halfWidth = halfHeight * aspectRatio;

            Vector3 center = camera.position + camera.forwardDirection * focalDistance;
            Vector3 up = camera.upDirection * halfHeight;
            Vector3 right = camera.rightDirection * halfWidth;

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
            Vector3 forward = camera.forwardDirection;
            Vector3 right = camera.rightDirection;

            Vector3 delta = WorldPoint - camPosition;

            float x = Vector3.Dot(delta, right);
            float y = Vector3.Dot(delta, forward);

            //return new Vector2(x, -(y-screen.height/2));
            return new Vector2(x, -y);
        }

        private void RenderNormal(Surface screen)
        {
            int screenX = screen.width / 2;
            int screenY = screen.height / 2;
            //get screenplane points
            Vector3 upLeft = camera.screenPlane.upLeft;
            Vector3 upRight = camera.screenPlane.upRight;
            Vector3 downRight = camera.screenPlane.downRight;
            Vector3 downLeft = camera.screenPlane.downLeft;

            Parallel.For(-screenY, screenY, row =>
            {
                for (int col = -screenX; col < screenX; col++)
                {
                    //normalize to 0-1
                    float u = (col + screenX) / (float)screen.width;
                    float v = (row + screenY) / (float)screen.height;

                    //calculate the worlpoint coordinates of the pixel
                    Vector3 top = Vector3.Lerp(upLeft, upRight, u);
                    Vector3 bottom = Vector3.Lerp(downLeft, downRight, u);
                    Vector3 worldPoint = Vector3.Lerp(top, bottom, v);

                    //trace ray
                    Vector3 direction = Vector3.Normalize(worldPoint - camera.position);

                    Color3 pixelcol = raytracer.TraceRay(camera.position, direction, 0);

                    screen.Plot(col + screenX, row + screenY, pixelcol);
                }
            }); 
        }
        float scale = 1f; // projection scale (zoom out)
        private void RenderDebug(Surface screen)
        {
           
            int screenX = screen.width / 2;
            int screenY = screen.height / 2;

            //get all worldccoordinates as vector 3. Convert to screenpixels and draw.
            
            Vector2 middleOfScreen = new Vector2(screenX, screenY + (screen.height / 2) * 0.8f); //+150 to center the camera towards the bottom of the screen

            //Project camera
            Vector2 camProjection = ProjectToPixel(camera.position);
            Vector2 camPoint2d = camProjection * scale + middleOfScreen;

            int camSize = (int)(5 * scale);
            screen.Bar((int)camPoint2d.X - camSize, (int)camPoint2d.Y - camSize, (int)camPoint2d.X + camSize, (int)camPoint2d.Y + camSize, new Color3(0f, 1f, 0f));

            //project light sources
            foreach (Light light in lights)
            {
                Vector2 lightProjection = ProjectToPixel(light.location);
                Vector2 lightPoint2d = lightProjection * scale + middleOfScreen;

                screen.Bar((int)lightPoint2d.X - camSize, (int)lightPoint2d.Y - camSize, (int)lightPoint2d.X + camSize, (int)lightPoint2d.Y + camSize, new Color3(1, 1, 0));
            }

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

                if (primitive is Plane)
                {
                    //gets handled in debugraydrawer
                }
            }

            //draw rays
            //vector3 of camposition and vector 3 of screenplane point.
            //project those to pixels

            //get screenplane points
            Vector3 upLeft = camera.screenPlane.upLeft;
            Vector3 upRight = camera.screenPlane.upRight;
            Vector3 downRight = camera.screenPlane.downRight;
            Vector3 downLeft = camera.screenPlane.downLeft;

            for (int col = -screenX; col < screenX; col += 50)
            {
                //normalize to 0-1
                float u = (col + screenX) / (float)screen.width;
                float v = (0 + screenY) / (float)screen.height;

                //calculate the worlpoint coordinates of the pixel
                Vector3 top = Vector3.Lerp(upLeft, upRight, u);
                Vector3 bottom = Vector3.Lerp(downLeft, downRight, u);
                Vector3 worldPoint = Vector3.Lerp(top, bottom, v);
                
                //trace ray
                Vector3 direction = Vector3.Normalize(worldPoint - camera.position);

                Intersection? intersect = raytracer.DebugRay(camera.position, direction);

                if (intersect == null)
                {
                    Vector3 intersectionPoint = camera.position + direction * 10000;
                    Vector2 intersectionProjection = ProjectToPixel(intersectionPoint);
                    Vector2 intersectionPoint2d = intersectionProjection * scale + middleOfScreen;

                    screen.Line((int)camPoint2d.X, (int)camPoint2d.Y, (int)intersectionPoint2d.X, (int)intersectionPoint2d.Y, new Color3(1f, 0f, 0f));
                }

                if (intersect != null)
                {
                    if (intersect.primitive is Plane)
                    {
                        Vector2 primitiveProjection = ProjectToPixel(intersect.position);
                        Vector2 primitivePoint2d = primitiveProjection * scale + middleOfScreen;

                        int planeSize = 3;

                        screen.Bar((int)primitivePoint2d.X - planeSize, (int)primitivePoint2d.Y - planeSize, (int)primitivePoint2d.X + planeSize, (int)primitivePoint2d.Y + planeSize, intersect.primitive.color);
                    }
                    if (intersect.primitive is Triangle)
                    {
                        Vector2 primitiveProjection = ProjectToPixel(intersect.position);
                        Vector2 primitivePoint2d = primitiveProjection * scale + middleOfScreen;

                        int TriangleSize = 20;

                        Vector2 triangleTop = new Vector2(primitivePoint2d.X, primitivePoint2d.Y - TriangleSize/2);
                        Vector2 triangleBottomLeft = new Vector2(primitivePoint2d.X + TriangleSize / 2, primitivePoint2d.Y + TriangleSize/2);
                        Vector2 triangleBottomRight = new Vector2(primitivePoint2d.X - TriangleSize / 2, primitivePoint2d.Y + TriangleSize/2);

                        screen.Line((int)triangleTop.X, (int)triangleTop.Y, (int)triangleBottomLeft.X, (int)triangleBottomLeft.Y, intersect.primitive.color);
                        screen.Line((int)triangleTop.X, (int)triangleTop.Y, (int)triangleBottomRight.X, (int)triangleBottomRight.Y, intersect.primitive.color);
                        screen.Line((int)triangleBottomLeft.X, (int)triangleBottomLeft.Y, (int)triangleBottomRight.X, (int)triangleBottomRight.Y, intersect.primitive.color);
                    }

                    Vector2 intersectionProjection = ProjectToPixel(intersect.position);
                    Vector2 intersectionPoint2d = intersectionProjection * scale + middleOfScreen;

                    screen.Line((int)camPoint2d.X, (int)camPoint2d.Y, (int)intersectionPoint2d.X, (int)intersectionPoint2d.Y, new Color3(1f, 0f, 0f));

                    //check if mirror, then draw reflective ray.
                    int bounces = 0;
                    while (bounces < 8 && intersect != null && intersect.primitive.isMirror)
                    {
                        bounces++;
                        Vector3 normal = Vector3.Normalize(intersect.normal);
                        direction = Vector3.Normalize(direction - 2 * Vector3.Dot(direction, normal) * normal);

                        Vector3 startingPos = intersect.position;
                        Vector3 startingPosEpsilon = intersect.position + normal;

                        Vector2 startingPosProjection = ProjectToPixel(startingPos);
                        Vector2 startingPosPoint2d = startingPosProjection * scale + middleOfScreen;

                        intersect = raytracer.DebugRay(startingPosEpsilon, direction, true);

                        if (intersect == null)
                        {
                            Vector3 fakeIntersectionPoint = startingPos + direction * 10000;
                            Vector2 fakeIntersectionProjection = ProjectToPixel(fakeIntersectionPoint);
                            Vector2 fakeIntersectionPoint2d = fakeIntersectionProjection * scale + middleOfScreen;
                            screen.Line((int)startingPosPoint2d.X, (int)startingPosPoint2d.Y, (int)fakeIntersectionPoint2d.X, (int)fakeIntersectionPoint2d.Y, new Color3(0.1f, 0.1f, 0.1f)); //draw gray line if reflection goes on forever
                            break;
                        }
                        intersectionProjection = ProjectToPixel(intersect.position);
                        intersectionPoint2d = intersectionProjection * scale + middleOfScreen;
                        
                        screen.Line((int)startingPosPoint2d.X, (int)startingPosPoint2d.Y, (int)intersectionPoint2d.X, (int)intersectionPoint2d.Y, new Color3(1f, 1f, 1f)); //draw white line for intersection point
                    }
                    //draw shadowrays //bug: shadowrays get drawn when looking at an unilluminated surface, like the back of a sphere //bug2: with planes, the shadowrays can overextend the light source. Tmax not set correctly? epsilon?
                    if (intersect != null)
                    {
                        float lightOffset = 0;
                        foreach (Light light in lights)
                        {
                            Vector3 shadowDirection = Vector3.Normalize(light.location - intersect.position);
                            Intersection? shadowIntersect = raytracer.DebugShadowRay(intersect.position, shadowDirection, light);

                            if (shadowIntersect != null)
                            {
                                Vector2 shadowProjection = ProjectToPixel(shadowIntersect.position);
                                Vector2 shadowPoint2d = shadowProjection * scale + middleOfScreen;

                                if (shadowIntersect.position == light.location)
                                {
                                    screen.Line((int)intersectionPoint2d.X, (int)intersectionPoint2d.Y, (int)shadowPoint2d.X, (int)shadowPoint2d.Y, new Color3(1f + lightOffset, 0.5f + lightOffset, 0f + lightOffset));
                                }
                                else
                                {
                                    screen.Line((int)intersectionPoint2d.X, (int)intersectionPoint2d.Y, (int)shadowPoint2d.X, (int)shadowPoint2d.Y, new Color3(0.7f + lightOffset, 0.7f + lightOffset, 0f + lightOffset));
                                }
                            }

                            lightOffset += 0.05f;
                        }
                    }
                }
            }
        }


        public void HandleKeyboardInput(KeyboardState keyboard, float deltaTime)
        {
            float movementSpeed = 50f;
            Vector3 movementVector = Vector3.Zero;

            if (keyboard[Keys.W]) movementVector += camera.forwardDirection;
            if (keyboard[Keys.A]) movementVector -= camera.rightDirection;
            if (keyboard[Keys.S]) movementVector -= camera.forwardDirection;
            if (keyboard[Keys.D]) movementVector += camera.rightDirection;
            if (keyboard[Keys.Space]) movementVector += camera.upDirection;
            if (keyboard[Keys.LeftShift]) movementVector -= camera.upDirection;

            camera.position += movementVector * deltaTime * movementSpeed;
        }
        public void HandleMouseInput(float deltaX, float deltaY)
        {
            float sensitivity = 0.005f;

            float horizontal = deltaX * sensitivity;
            float vertical = deltaY * sensitivity;

            Quaternion horizontalRotation = Quaternion.CreateFromAxisAngle(camera.upDirection, horizontal);
            Quaternion verticalRotation = Quaternion.CreateFromAxisAngle(camera.rightDirection, vertical);

            Quaternion orientation = Quaternion.Normalize(horizontalRotation * verticalRotation);

            //recreate three camera vectors
            camera.forwardDirection = Vector3.Transform(camera.forwardDirection, orientation);
            camera.upDirection = Vector3.Transform(camera.upDirection, orientation);
            camera.rightDirection = Vector3.Cross(camera.upDirection, camera.forwardDirection);
        }
        public void HandleMouseScroll(MouseWheelEventArgs e, Vector2 mousePos)
        {
            if (mousePos.X > screen.width/2) //mouse is in debug screen
            {
                scale = Math.Clamp(scale + (int)e.OffsetY * 0.1f, 0.1f, 3f); //clamp scale between 0.1 and 3 to prevent shenenigans
            } 
            else //mouse is in normal render
            {
                fovDegrees = Math.Clamp(fovDegrees - (int)e.OffsetY, 0, 179); //clamp FOV between 1 and 179 to prevent shenenigans
            }
        }
    }
}