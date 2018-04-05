using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Bitmap = System.Drawing.Bitmap;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;

namespace OpenGL
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new Game();
            game.Run(30);
        }
    }

    class Game : GameWindow
    {
        public static Game Instance;

        private KeybordInput _keybord;
        
        private Shader _cubeShader;
        private Camera _camera;

        private int VAO;

        private int textureID;

        private int angle;

        private bool _paused;

        private Vector2 lastMousePos;

        public Game()
        {
            Instance = this;

            CursorVisible = false;

            _keybord = new KeybordInput(this);
            
            _cubeShader = new Shader();
            _camera = new Camera();

            var verticies = new float[]
            {
                1, 1, 0,
                1, 0, 0,
                0, 0, 0,
                0, 1, 0,

                1, 1, 1,
                1, 0, 1,
                1, 0, 0,
                1, 1, 0,

                0, 1, 1,
                0, 0, 1,
                1, 0, 1,
                1, 1, 1,

                0, 1, 0,
                0, 0, 0,
                0, 0, 1,
                0, 1, 1,

                0, 1, 0,
                0, 1, 1,
                1, 1, 1,
                1, 1, 0,

                0, 0, 1,
                0, 0, 0,
                1, 0, 0,
                1, 0, 1
            };
            var UVs = new float[]
            {
                0, 0,
                0, 1,
                1, 1,
                1, 0,

                0, 0,
                0, 1,
                1, 1,
                1, 0,

                0, 0,
                0, 1,
                1, 1,
                1, 0,

                0, 0,
                0, 1,
                1, 1,
                1, 0,

                0, 0,
                0, 1,
                1, 1,
                1, 0,

                0, 0,
                0, 1,
                1, 1,
                1, 0
            };

            textureID = LoadTexture("textures/dirt.png");

            VAO = GL.GenVertexArray(); //create a vertex array ID (ID of the model)
            GL.BindVertexArray(VAO); //bind it by the the ID

            storeDataInAttribArray(0, verticies, 3);
            storeDataInAttribArray(1, UVs, 2);

            GL.BindVertexArray(0);

            var t = new Thread(() =>
            {
                int counter = 0;

                while (true)
                {
                    if (Visible)
                    {
                        if (counter >= 5)
                        {
                            counter = 0;
                            Tick();
                        }

                        counter++;
                    }
                    Thread.Sleep(2);
                }
            });

            t.IsBackground = true;
            t.Start();
            
        }

        private void Tick()
        {
            angle = (angle + 1) % 360;
        }

        private void storeDataInAttribArray(int index, float[] data, int coordSize)
        {
            int VBO = GL.GenBuffer(); //create a VBO

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO); //bind the VBO
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.DynamicDraw); //load data into the buffer and specify it's usage
            GL.VertexAttribPointer(index, coordSize, VertexAttribPointerType.Float, false, 0, 0); //specify the coord size (vec3) and what vertex attrib ID to bind this data to
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // cleanup - unbind the VBO
        }

	private void handleMouse()
	{
            if (!_paused)
            {
                var mouse = Mouse.GetState();
		Console.WriteLine(mouse);
                var current = new Vector2(mouse.X, mouse.Y);
                var delta = current - lastMousePos;

                _camera.pitch += delta.Y / 1000f;
                _camera._yaw += delta.X / 1000f;
                lastMousePos = current;
            }

	}

        protected override void OnRenderFrame(FrameEventArgs e)
        { 
	    handleMouse();
            
	    if (_keybord.IsKeyDown(Key.D))
                _camera.pos.Xz += -_camera.left * (float) e.Time; 
            
            if (_keybord.IsKeyDown(Key.A))
                _camera.pos.Xz += _camera.left * (float) e.Time;
            
            if (_keybord.IsKeyDown(Key.W))
                _camera.pos.Xz += _camera.forward * (float) e.Time;
            
            if (_keybord.IsKeyDown(Key.S))
                _camera.pos.Xz += -_camera.forward * (float) e.Time;
            
            _camera.UpdateViewMatrix();
            
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.DepthClamp);
            
            //RENDER
            _cubeShader.Bind();
            _cubeShader.LoadProjectionMat(_camera.Projection);
            _cubeShader.LoadViewMat(_camera.View);

            _cubeShader.LoadTransformMat(Matrix4.Translation(Vector3.One * -0.5f));

            GL.BindVertexArray(VAO); //bind the model by it's VAO ID
            GL.EnableVertexAttribArray(0); //enable 
            GL.EnableVertexAttribArray(1);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.DrawArrays(PrimitiveType.Quads, 0, 24); // draw the bound VAO, 24 vertexes

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.BindVertexArray(0);
            _cubeShader.Unbind();

            //SWAP BUFFERS
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, ClientRectangle.Width, ClientRectangle.Height, 0, Camera.NearPlane, Camera.FarPlane);

            _camera.UpdateProjectionMatrix();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Alt && e.Key == Key.F4)
                Close();

            if (e.Key == Key.Escape)
                _paused = CursorVisible = !CursorVisible;
        }

        private int LoadTexture(string file)
        {
            var img = (Bitmap)Bitmap.FromFile(file);

            using (img)
            {
                var rect = new Rectangle(new Point(), img.Size);
                var data = img.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

                int texID = GL.GenTexture(); //generate a new texture

                GL.BindTexture(TextureTarget.Texture2D, texID); // bind the texture (to start working withthe texture)

                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    img.Size.Width,
                    img.Size.Height,
                    0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0); //specify image format, the data type we're loading in and finally the pointer to the data to load

                img.UnlockBits(data);

                //set filters - this is important
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                return texID;
            }
        }
    }

    class KeybordInput
    {
        private List<Key> _keyDown = new List<Key>();
        
        public KeybordInput(GameWindow window)
        {
            window.KeyDown += (o, e) =>
            {
                if (!_keyDown.Contains(e.Key))
                    _keyDown.Add(e.Key);
            };

            window.KeyUp += (o, e) => { _keyDown.Remove(e.Key); };
            
        }

        public bool IsKeyDown(Key key)
        {
            return _keyDown.Contains(key);
        }
    }

    class Shader
    {
        private int vshID;
        private int fshID;

        private int program;

        private int loc_projection;
        private int loc_view;
        private int loc_transformation;

        public Shader()
        {
            LoadShader("cube");

            //creates and ID for this program
            program = GL.CreateProgram();

            //attaches shaders to this program
            GL.AttachShader(program, vshID);
            GL.AttachShader(program, fshID);

            GL.BindAttribLocation(program, 0, "position");
            GL.BindAttribLocation(program, 1, "uv");

            GL.LinkProgram(program);
            GL.ValidateProgram(program);

            loc_projection = GL.GetUniformLocation(program, "projection");
            loc_view = GL.GetUniformLocation(program, "view");
            loc_transformation = GL.GetUniformLocation(program, "transformation");
        }

        private void LoadShader(string shaderName)
        {
            string vertexShaderCode = File.ReadAllText($"shader/{shaderName}.vsh");
            string fragmentShaderCode = File.ReadAllText($"shader/{shaderName}.fsh");

            vshID = GL.CreateShader(ShaderType.VertexShader);
            fshID = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vshID, vertexShaderCode);
            GL.ShaderSource(fshID, fragmentShaderCode);

            GL.CompileShader(vshID);
            GL.CompileShader(fshID);
        }

        public void LoadProjectionMat(Matrix4 mat)
        {
            GL.UniformMatrix4(loc_projection, false, ref mat);
        }

        public void LoadTransformMat(Matrix4 mat)
        {
            GL.UniformMatrix4(loc_transformation, false, ref mat);
        }

        public void LoadViewMat(Matrix4 mat)
        {
            GL.UniformMatrix4(loc_view, false, ref mat);
        }

        public void Bind()
        {
            GL.UseProgram(program);
        }

        public void Unbind()
        {
            GL.UseProgram(0);
        }
    }
}
