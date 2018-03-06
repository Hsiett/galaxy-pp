using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Galaxy_Editor_2.Dialog_Creator.Controls;
using Galaxy_Editor_2.Dialog_Creator.Enums;
using Galaxy_Editor_2.Dialog_Creator.Fonts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WinFormsContentLoading;
using WinFormsGraphicsDevice;
using Button = Galaxy_Editor_2.Dialog_Creator.Controls.Button;
using CheckBox = Galaxy_Editor_2.Dialog_Creator.Controls.CheckBox;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

//using Font = Microsoft.Xna.Framework.Graphics.fon

namespace Galaxy_Editor_2.Dialog_Creator
{
    class GraphicsControl : GraphicsDeviceControl
    {
        const int BoxTypeSelection = 0;
        const int BoxTypeCreateNew = 1;
        private enum Status
        {
            Idle,
            Dragging,
            ResizingTL,
            ResizingTR,
            ResizingBL,
            ResizingBR,
            ResizingT,
            ResizingB,
            ResizingL,
            ResizingR,
            CreatingControl
        }

        private static Texture2D defaultTexture;
        private static Effect Effect;
        private static bool StaticInitialized;
        private static SpriteFont GetFont(FontData data)
        {
            return Fonts[data.FontRef][data.Size];
        }
        private static Dictionary<string, string> FontNames = new Dictionary<string, string>();//Ref to name
        private static Dictionary<string, Dictionary<int, SpriteFont>> Fonts = new Dictionary<string, Dictionary<int, SpriteFont>>(); //Ref, size to font
        static void InitStatic(GraphicsControl control)
        {
            if (StaticInitialized)
                return;
            StaticInitialized = true;

            //Load effect
            /*string code = Properties.Resources.horizontalBorderEffect;
            CompiledEffect cEffect = 
                Effect.CompileEffectFromSource(code, null, null,
                                               CompilerOptions.None, TargetPlatform.Windows);
            if (!cEffect.Success)
                cEffect = cEffect;
            
            Effect = new Effect(control.GraphicsDevice, cEffect.GetEffectCode(), CompilerOptions.None, null);
            //Save effect code
            byte[] effectCode = cEffect.GetEffectCode();
            FileStream stream = File.Open("compiledEffect", FileMode.Create);
            stream.Write(effectCode, 0, effectCode.Length);
            stream.Close();
            stream.Dispose();*/
            Effect = new Effect(control.GraphicsDevice, Properties.Resources.compiledEffect, CompilerOptions.None, null);

            


            //Load fonts
            string[] fontRefferences = new[]
                                           {
                                               @"UI\Fonts\Eurostile-Bol.otf",
                                               @"UI\Fonts\Eurostile-Reg.otf",
                                               @"UI\Fonts\EurostileExt-Med.otf",
                                               @"UI\Fonts\EurostileExt-Reg.otf",
                                               @"UI\Fonts\bl.ttf"
                                           };
            string[] fontNames = new[]
                                     {
                                         "FontHeader",
                                         "FontStandard",
                                         "FontHeaderExtended",
                                         "FontStandardExtended",
                                         "FontInternational"
                                     };
            int[] fontSizes = new[] { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 24, 25, 26, 28, 30, 32, 34, 35, 36, 38, 40, 42, 48, 50, 52, 56, 60, 62, 64, 68 };
            
            ContentManager contentManager = new ContentManager(control.Services);
            for (int i = 0; i < fontRefferences.Length; i++)
            {
                FontNames[fontRefferences[i]] = fontNames[i];
                Fonts[fontRefferences[i]] = new Dictionary<int, SpriteFont>();
                foreach (int fontSize in fontSizes)
                {
                    string name = fontNames[i] + fontSize;
                    if (File.Exists("Fonts\\" + name + ".xnb"))
                        Fonts[fontRefferences[i]][fontSize] = contentManager.Load<SpriteFont>("Fonts\\" + name);
                }
            }

            defaultTexture = new Texture2D(control.GraphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            defaultTexture.SetData(new[] { new Color(0.5f, 0.5f, 1, 1) });
        }

        public bool DisableMouseControl { get; set; }
        public new DialogCreatorControl Parent;
        public List<AbstractControl> SelectedItems = new List<AbstractControl>();
        public DialogData Data = new DialogData();
        public AbstractControl MainSelectedItem { get; private set; }
        private RenderTarget2D renderTarget;
        private DepthStencilBuffer depthStencilBuffer;
        private Status status = Status.Idle;
        private SpriteBatch SpriteBatch;
        private Texture2D backgroundImage;
        //private readonly List<AbstractControl> items = new List<AbstractControl>();
        private List<Dialog> items
        {
            get { return Data.Dialogs; }
        }
        public ReadOnlyCollection<Dialog> Items
        {
            get { return items.AsReadOnly(); }
        }
        private float scale = 1f;
        private AbstractControl controlBeingCreated = null;

        public int DrawWidth { get { return (int)(Width / scale); } }
        public int DrawHeight { get { return (int)(Height / scale); } }

        public void SetDialogData(DialogData d)
        {
            Data = d;
            d.CurrentControl = this;
        }

        public void SetTargetHeight(int height)
        {
            scale = height/1200f;
            DeviceReset();//To make render target again
            Invalidate();
        }

        public void SelectItem(AbstractControl control)
        {
            SelectedItems.Clear();
            if (control != null)
                SelectedItems.Add(control);
            MainSelectedItem = control;
            Parent.UpdateSelectedItem();
            Invalidate();
        }

        public Dialog MainDialog
        {
            get { return items.First(item => item.GetType() == typeof(Dialog)); }
        }

        private Race displayRace;
        public Race DisplayRace
        {
            get { return displayRace; }
            set
            {
                displayRace = value;
                Invalidate();
            }
        }

        public bool EditDisplayRaceOnly { get; set; }

        public void AddDialog(Dialog item)
        {
            items.Add(item);
            SelectedItems.Clear();
            SelectedItems.Add(item);
            MainSelectedItem = item;
        }

        /*public void Resort()
        {
            items.Sort(Comparerer);
            items.Sort(Comparerer);
        }*/


        /*public void RemoveItem(AbstractControl item)
        {
            items.Remove(item);
            if (SelectedItems.Contains(item))
            {
                SelectedItems.Remove(item);
                if (MainSelectedItem == item)
                {
                    if (SelectedItems.Count == 0)
                        MainSelectedItem = null;
                    else
                        MainSelectedItem = SelectedItems[0];
                }
            }
        }*/

        public void Create(AbstractControl control)
        {
            controlBeingCreated = control;
            status = Status.CreatingControl;
        }

        public void CancelCreate()
        {
            if (status == Status.CreatingControl)
                status = Status.Idle;
        }

        private static int Comparerer(IRenderableItem item1, IRenderableItem item2)
        {
            return item1.RenderPriority.CompareTo(item2.RenderPriority);
        }

        /*public void SetBackgroundImage(string path)
        {
            if (backgroundImage != null)
            {
                backgroundImage.Dispose();
            }
            backgroundImage = Texture2D.FromFile(GraphicsDevice, path);
        }*/

        public void SetBackgroundImage(Texture2D texture)
        {
            backgroundImage = texture;
        }

        public void SetBackgroundImage(Bitmap bmp)
        {
            Color[] pixels = new Color[bmp.Width * bmp.Height];
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    System.Drawing.Color c = bmp.GetPixel(x, y);
                    pixels[(y * bmp.Width) + x] = new Color(c.R, c.G, c.B, c.A);
                }
            }

            backgroundImage = new Texture2D(
              GraphicsDevice,
              bmp.Width,
              bmp.Height,
              1,
              TextureUsage.None,
              SurfaceFormat.Color);

            backgroundImage.SetData(pixels);
        }

        protected override void Initialize()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            renderTarget = new RenderTarget2D(GraphicsDevice, (int) (GraphicsDevice.DisplayMode.Width/scale), (int) (GraphicsDevice.DisplayMode.Height/scale), 1, SurfaceFormat.Color);
            depthStencilBuffer = CreateDepthStencil(renderTarget);
            if (Effect == null)
                InitStatic(this);
            displayRace = Race.Terran;
            oldSize = new Size(DrawWidth, DrawHeight);

        }

        public static DepthStencilBuffer CreateDepthStencil(RenderTarget2D target)
        {
            return new DepthStencilBuffer(target.GraphicsDevice, target.Width,
                target.Height, target.GraphicsDevice.DepthStencilBuffer.Format,
                target.MultiSampleType, target.MultiSampleQuality);
        }

        protected override void DeviceReset()
        {
            RenderTarget2D oldTarget = renderTarget;
            DepthStencilBuffer oldStencil = depthStencilBuffer;
            try
            {
                renderTarget = new RenderTarget2D(GraphicsDevice, (int)(GraphicsDevice.DisplayMode.Width / scale), (int)(GraphicsDevice.DisplayMode.Height / scale), 1, SurfaceFormat.Color);
                depthStencilBuffer = CreateDepthStencil(renderTarget);
            }
            catch
            {
                renderTarget = oldTarget;
                depthStencilBuffer = oldStencil;
                throw;
            }
            oldTarget.Dispose();
            oldStencil.Dispose();
        }

        protected override void Draw()
        {
            //Render to a texture first (used to scale and set selection border color)
            //If the target screen height is lower than 1200, this texture will be bigger than the 
            GraphicsDevice.SetRenderTarget(0, renderTarget);
            // Cache the current depth buffer
            DepthStencilBuffer old = GraphicsDevice.DepthStencilBuffer;
            // Set our custom depth buffer
            GraphicsDevice.DepthStencilBuffer = depthStencilBuffer;
            GraphicsDevice.Clear(Color.Black);
            SpriteBatch.GraphicsDevice.RenderState.ScissorTestEnable = false;
            //Render background
            if (backgroundImage != null)
            {
                //Not using a shader for this.. So I must calculate a dest rectangle
                //It is used to keep aspect ratio, and display the background image as big as possible and centered.
                Rectangle bgPos = new Rectangle();
                float wScale = ((float) Width/scale)/backgroundImage.Width;
                float hScale = ((float)Height / scale) / backgroundImage.Height;
                if (wScale > hScale)
                {
                    bgPos.Width = (int)(Width / scale);
                    bgPos.X = 0;
                    bgPos.Height = (int) (wScale*backgroundImage.Height);
                    bgPos.Y = (int)((Height / scale - bgPos.Height) / 2);
                }
                else
                {
                    bgPos.Height = (int)(Height / scale);
                    bgPos.Y = 0;
                    bgPos.Width = (int) (hScale*backgroundImage.Width);
                    bgPos.X = (int)((Width / scale - bgPos.Width) / 2);
                }
                SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                SpriteBatch.Draw(backgroundImage, bgPos, Color.White);
                SpriteBatch.End();
            }
            //items is a list of the dialogs added
            foreach (Dialog d in items)
            {
                //A list of what should be rendered, in the order it should be rendered
                List<AbstractControl> renders = new List<AbstractControl>();
                renders.Add(d);
                //Each dialog has a list of controls, sorted after render priority
                renders.AddRange(d.ChildControls);
                while (renders.Count > 0)
                {
                    AbstractControl item = renders[0];
                    renders.RemoveAt(0);
                    //A single control might add more controls to render correctly, but theese controls
                    //wont be in the output script. 
                    //An example is that the checkbox adds an image if it is set to be checked.
                    renders.InsertRange(0, item.ExtraControlsToRender);
                    if (item.DrawTexture)
                    {
                        //Set image type
                        Effect.CurrentTechnique = Effect.Techniques[Enum.GetName(typeof (ImageType), item.ImageType)];
                        //Clip rectangle used to clip controls when they are dragged outside of their parent dialog
                        SpriteBatch.GraphicsDevice.RenderState.ScissorTestEnable = true;
                        SpriteBatch.GraphicsDevice.ScissorRectangle = item.ClipRect;
                        Texture2D texture = item.Texture ?? defaultTexture;
                        //Buttons/check boxes have both pressed/not pressed images in same texture
                        Effect.Parameters["IsHalfTexture"].SetValue(item.IsHalfTexture);
                        Effect.Parameters["IsBottomHalf"].SetValue(item.IsBottomHalf);
                        Effect.Parameters["Tiled"].SetValue(item.IsTiled);
                        Effect.Parameters["TintColor"].SetValue(item.Color.ToVector4());
                        Effect.Parameters["TextureSize"].SetValue(new Vector2(texture.Width, texture.Height));
                        Effect.Parameters["TargetSize"].SetValue(new Vector2(item.DrawRect.Width, item.DrawRect.Height));
                        SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                        //Set blend function
                        item.BlendState.Apply(GraphicsDevice);
                        Effect.Begin();
                        Effect.CurrentTechnique.Passes[0].Begin();
                        SpriteBatch.Draw(texture, item.DrawRect, null, Color.White);
                        SpriteBatch.End();
                        Effect.CurrentTechnique.Passes[0].End();
                        Effect.End();
                        BlendState.BlendStates[BlendMode.Alpha].Apply(GraphicsDevice);//Reset blend state
                    }
                    //Draw dialog title
                    if (item is Dialog)
                    {
                        Dialog dialog = (Dialog) item;
                        if (!string.IsNullOrEmpty(dialog.Title))
                            DrawString(dialog.Title, dialog.TitleFontRect, dialog.TitleFont, Color.White);
                    }
                    //Draw the control's text if needed
                    if (item is DialogControl)
                    {
                        DialogControl control = (DialogControl) item;
                        if (control.DrawText && !string.IsNullOrEmpty(control.Text))
                            DrawString(control.Text, control.DrawRect, control.TextStyle, control.TextColor);
                    }
                }
            }

            SpriteBatch.GraphicsDevice.RenderState.ScissorTestEnable = false;
            GraphicsDevice.SetRenderTarget(0, null);
            GraphicsDevice.DepthStencilBuffer = old;
            Texture2D background = renderTarget.GetTexture();
            //GraphicsDevice.SetRenderTarget(0, renderTarget);
            SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            //SpriteBatch.Draw(background, new Vector2(0, 0), Color.White);
            SpriteBatch.Draw(background, new Rectangle(0, 0, Width, Height), new Rectangle(0, 0, DrawWidth, DrawHeight), Color.White);
            SpriteBatch.End();
            if (!DisableMouseControl)
            {
                foreach (AbstractControl item in SelectedItems)
                {
                    Effect.CurrentTechnique = Effect.Techniques["SelectionBox"];
                    Effect.Parameters["BoxType"].SetValue(BoxTypeSelection);
                    Rectangle rect = item.SelectRect;
                    rect.X = (int) (rect.X*scale);
                    rect.Y = (int) (rect.Y*scale);
                    rect.Width = (int) (rect.Width*scale);
                    rect.Height = (int) (rect.Height*scale);
                    rect.X -= 6;
                    rect.Y -= 6;
                    rect.Width += 12;
                    rect.Height += 12;
                    Effect.Parameters["TargetPos"].SetValue(new Vector2(rect.X, rect.Y));
                    Effect.Parameters["TargetSize"].SetValue(new Vector2(rect.Width, rect.Height));
                    Effect.Parameters["ScreenSize"].SetValue(new Vector2(renderTarget.Width*scale,
                                                                         renderTarget.Height*scale));
                    SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                    Effect.Begin();
                    Effect.CurrentTechnique.Passes[0].Begin();
                    SpriteBatch.Draw(background, rect, null, Color.White);
                    SpriteBatch.End();
                    Effect.CurrentTechnique.Passes[0].End();
                    Effect.End();
                }
                if (status == Status.CreatingControl && MouseButtons == MouseButtons.Left)
                {
                    Effect.CurrentTechnique = Effect.Techniques["SelectionBox"];
                    Effect.Parameters["BoxType"].SetValue(BoxTypeCreateNew);
                    Point min = new Point((int)(Math.Min(mouseDownPosition.X, lastMousePos.X)*scale),
                                          (int)(Math.Min(mouseDownPosition.Y, lastMousePos.Y)*scale));
                    Point max = new Point((int)(Math.Max(mouseDownPosition.X, lastMousePos.X)*scale),
                                          (int)(Math.Max(mouseDownPosition.Y, lastMousePos.Y) * scale));
                    Rectangle rect = new Rectangle(min.X, min.Y, max.X - min.X, max.Y - min.Y);
                    Effect.Parameters["TargetPos"].SetValue(new Vector2(rect.X, rect.Y));
                    Effect.Parameters["TargetSize"].SetValue(new Vector2(rect.Width, rect.Height));
                    Effect.Parameters["ScreenSize"].SetValue(new Vector2(renderTarget.Width*scale,
                                                                         renderTarget.Height*scale));
                    SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                    Effect.Begin();
                    Effect.CurrentTechnique.Passes[0].Begin();
                    SpriteBatch.Draw(background, rect, null, Color.White);
                    SpriteBatch.End();
                    Effect.CurrentTechnique.Passes[0].End();
                    Effect.End();
                }
            }
        }

        private void DrawString(string s, Rectangle rect, FontData fontData, Color tintColor)
        {
            if (fontData == null)
                return;
            Vector4 cl1 = fontData.TextColor.ToVector4();
            Vector4 cl2 = tintColor.ToVector4();
            Color cl = new Color(cl1.X*cl2.X, cl1.Y*cl2.Y, cl1.Z*cl2.Z, cl1.W*cl2.W);


            float textScale = 0.7f;
            Vector2 position = new Vector2();
            Vector2 origin = new Vector2();
            SpriteFont font = GetFont(fontData);
            Vector2 size = font.MeasureString(s);
            size.X *= textScale;
            size.Y *= textScale;
            if (fontData.Anchor == Enums.Anchor.TopLeft || fontData.Anchor == Enums.Anchor.Left || fontData.Anchor == Enums.Anchor.BottomLeft)
                position.X = rect.X;
            else if (fontData.Anchor == Enums.Anchor.Top || fontData.Anchor == Enums.Anchor.Center || fontData.Anchor == Enums.Anchor.Bottom)
                position.X = rect.X + (rect.Width - size.X)/2;
            else if (fontData.Anchor == Enums.Anchor.TopRight || fontData.Anchor == Enums.Anchor.Right || fontData.Anchor == Enums.Anchor.BottomRight)
                position.X = rect.X + rect.Width - size.X;
            if (fontData.Anchor == Enums.Anchor.TopLeft || fontData.Anchor == Enums.Anchor.Top || fontData.Anchor == Enums.Anchor.TopRight)
                position.Y = rect.Y;
            else if (fontData.Anchor == Enums.Anchor.Left || fontData.Anchor == Enums.Anchor.Center || fontData.Anchor == Enums.Anchor.Right)
                position.Y = rect.Y + (rect.Height - size.Y) / 2;
            else if (fontData.Anchor == Enums.Anchor.BottomLeft || fontData.Anchor == Enums.Anchor.Bottom || fontData.Anchor == Enums.Anchor.BottomRight)
                position.Y = rect.Y + rect.Height - size.Y;
            origin.X = (position.X - rect.X) / (rect.Width - size.X);
            origin.X = (position.X - rect.Y) / (rect.Width - size.Y);
            SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            SpriteBatch.DrawString(font, s, position, cl, 0, origin, textScale, SpriteEffects.None, 0);
            SpriteBatch.End();
        }

        private AbstractControl FindControlAt(int x, int y, bool dialogOnly = false)
        {
            for (int i = this.items.Count - 1; i >= 0; i--)
            {
                Dialog dialog = this.items[i];
                List<AbstractControl> items = new List<AbstractControl>();
                items.AddRange(dialog.ChildControls);
                items.Reverse();
                items.Add(dialog);
                foreach (AbstractControl item in items)
                {
                    if (item.SelectRect.Contains(x, y) && (item is Dialog || !dialogOnly))
                        return item;
                }
            }
            return null;
        }

        private AbstractControl GetResizeStatusAt(int x, int y, out Status s)
        {
            foreach (AbstractControl item in SelectedItems)
            {
                Rectangle selectRect = item.SelectRect;
                selectRect.X = (int)(selectRect.X * scale);
                selectRect.Y = (int)(selectRect.Y * scale);
                selectRect.Width = (int)(selectRect.Width * scale);
                selectRect.Height = (int)(selectRect.Height * scale);
                selectRect.X -= 6;
                selectRect.Y -= 6;
                selectRect.Width += 12;
                selectRect.Height += 12;
                if (selectRect.Contains(x, y))
                {
                    Point p = new Point(x - selectRect.X, y - selectRect.Y);
                    if (p.X < 7)
                    {
                        //Left edge
                        if (p.Y < 7)
                        {
                            s = Status.ResizingTL;
                            return item;
                        }
                        if (p.Y >= selectRect.Height - 7)
                        {
                            s = Status.ResizingBL;
                            return item;
                        }
                        int v = selectRect.Height/2 - 3;
                        if (p.Y >= v && p.Y < v + 7)
                        {
                            s = Status.ResizingL;
                            return item;
                        }
                    }
                    else if (p.Y < 7)
                    {
                        //Top edge
                        if (p.X >= selectRect.Width - 7)
                        {
                            //Top right
                            s = Status.ResizingTR;
                            return item;
                        }
                        else
                        {
                            //Middle top
                            int v = selectRect.Width / 2 - 3;
                            if (p.X >= v && p.X < v + 7)
                            {
                                s = Status.ResizingT;
                                return item;
                            }
                        }
                    }
                    else if (p.Y >= selectRect.Height - 7)
                    {
                        //Bottom edge
                        if (p.X >= selectRect.Width - 7)
                        {
                            //Bottom right
                            s = Status.ResizingBR;
                            return item;
                        }
                        else
                        {
                            //Middle bottom
                            int v = selectRect.Width / 2 - 3;
                            if (p.X >= v && p.X < v + 7)
                            {
                                s = Status.ResizingB;
                                return item;
                            }
                        }
                    }
                    else if (p.X >= selectRect.Width - 7)
                    {
                        //Right edge
                        //Middle right
                        int v = selectRect.Height / 2 - 3;
                        if (p.Y >= v && p.Y < v + 7)
                        {
                            s = Status.ResizingR;
                            return item;
                        }
                    }
                }
            }

            s = Status.Idle; 
            return null;
        }

        private AbstractControl mouseDownItem = null;
        private Point lastMousePos = new Point(0, 0);
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (DisableMouseControl)
                return;
            Point position = new Point((int) (e.X / scale), (int) (e.Y / scale));
            if (status == Status.Idle)
            {
                if (mouseDownItem == null)
                {
                    Status resizeStatus;
                    GetResizeStatusAt(e.X, e.Y, out resizeStatus);
                    if (resizeStatus == Status.ResizingTL || resizeStatus == Status.ResizingBR)
                        Cursor = Cursors.SizeNWSE;
                    else if (resizeStatus == Status.ResizingTR || resizeStatus == Status.ResizingBL)
                        Cursor = Cursors.SizeNESW;
                    else if (resizeStatus == Status.ResizingL || resizeStatus == Status.ResizingR)
                        Cursor = Cursors.SizeWE;
                    else if (resizeStatus == Status.ResizingT || resizeStatus == Status.ResizingB)
                        Cursor = Cursors.SizeNS;
                    else
                    {
                        IRenderableItem item = FindControlAt(position.X, position.Y);
                        Cursor = item == null ? Cursors.Default : Cursors.SizeAll;
                    }
                }
                else if (Math.Abs(lastMousePos.X - position.X) > 2 || Math.Abs(lastMousePos.Y - position.Y) > 2)
                {
                    if (mouseDownItem is Dialog || !SelectedItems.Contains(mouseDownItem))
                        SelectedItems.Clear();
                    if (!SelectedItems.Contains(mouseDownItem))
                        SelectedItems.Add(mouseDownItem);
                    if (!SelectedItems.Contains(MainSelectedItem))
                        MainSelectedItem = mouseDownItem;
                    status = Status.Dragging;
                    Parent.UpdateSelectedItem();
                }
            }
            if (status == Status.Dragging)
            {
                mouseDownItem.Move(position.X - lastMousePos.X, position.Y - lastMousePos.Y);
                Invalidate();
                if (Parent != null)
                    Parent.ControlMovedOrResized();
            }
            if (status == Status.ResizingB ||
                status == Status.ResizingBL ||
                status == Status.ResizingBR ||
                status == Status.ResizingL ||
                status == Status.ResizingR ||
                status == Status.ResizingT ||
                status == Status.ResizingTL ||
                status == Status.ResizingTR)
            {
                mouseDownItem.Resize(position.X - lastMousePos.X, position.Y - lastMousePos.Y, Enum.GetName(typeof(Status), status).Substring(8));
                Invalidate();
                if (Parent != null)
                    Parent.ControlMovedOrResized();
            }
            if (status == Status.CreatingControl)
            {
                Cursor = Cursors.Cross;
                Invalidate();
            }
            if (status != Status.Idle)
                lastMousePos = new Point(position.X, position.Y);
        }

        Point mouseDownPosition = new Point(0, 0);
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            if (DisableMouseControl)
                return;
            Point position = new Point((int)(e.X / scale), (int)(e.Y / scale));
            mouseDownPosition = lastMousePos = new Point(position.X, position.Y);
            Status resizeStatus;
            mouseDownItem = GetResizeStatusAt(e.X, e.Y, out resizeStatus);
            if (status == Status.CreatingControl)
            {
                mouseDownItem = FindControlAt(position.X, position.Y, true);
                if (mouseDownItem != null)
                    controlBeingCreated.SetParent((Dialog) mouseDownItem);
            }
            else if (resizeStatus != Status.Idle)
            {
                status = resizeStatus;
            }
            else
            {
                mouseDownItem = FindControlAt(position.X, position.Y);
            }
            
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (DisableMouseControl)
                return;
            //Select/deselect
            if (status == Status.Idle)
            {
                if (mouseDownItem != null)
                {
                    if (SelectedItems.Contains(mouseDownItem))
                    {
                        if (ModifierKeys == Keys.Shift)
                        {
                            SelectedItems.Remove(mouseDownItem);
                            if (MainSelectedItem == mouseDownItem)
                            {
                                if (SelectedItems.Count == 0)
                                    MainSelectedItem = null;
                                else
                                    MainSelectedItem = SelectedItems[0];
                            }
                        }
                        else
                            MainSelectedItem = mouseDownItem;
                    }
                    else
                    {
                        if (ModifierKeys != Keys.Shift)
                            SelectedItems.Clear();
                        SelectedItems.Add(mouseDownItem);
                        MainSelectedItem = mouseDownItem;
                    }
                }
                else
                {
                    SelectedItems.Clear();
                    MainSelectedItem = null;
                }
                Invalidate();
            }
            else if (status == Status.CreatingControl)
            {
                Point min = new Point(Math.Min(mouseDownPosition.X, lastMousePos.X), Math.Min(mouseDownPosition.Y, lastMousePos.Y));
                Point max = new Point(Math.Max(mouseDownPosition.X, lastMousePos.X), Math.Max(mouseDownPosition.Y, lastMousePos.Y));
                Rectangle rect = new Rectangle(min.X, min.Y, max.X - min.X, max.Y - min.Y);
                if (rect.Width <= 5) rect.Width = controlBeingCreated.DefaultSize.Width;
                if (rect.Height <= 5) rect.Height = controlBeingCreated.DefaultSize.Height;
                controlBeingCreated.MoveTo(rect.X, rect.Y, rect.Width, rect.Height);
                controlBeingCreated.ContextChanged(this);
                /*items.Add(controlBeingCreated);
                items.Sort(Comparerer);
                items.Sort(Comparerer);*/
                if (controlBeingCreated is Dialog)
                    items.Add((Dialog) controlBeingCreated);
                else
                    controlBeingCreated.Parent.AddControl((DialogControl) controlBeingCreated);
                Parent.ControlCreated(controlBeingCreated);
                SelectedItems.Clear();
                SelectedItems.Add(controlBeingCreated);
                MainSelectedItem = controlBeingCreated;
                Invalidate();
                Data.UpdateDesigener();
            }
            status = Status.Idle;
            mouseDownItem = null;
            if (Parent != null)
                Parent.UpdateSelectedItem();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            KeyEventArgs eventArg = new KeyEventArgs(keyData);
            OnKeyDown(eventArg);
            return eventArg.Handled;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (SelectedItems.Count > 0)
            {
                switch (e.KeyData)
                {
                    case Keys.Delete:
                        e.Handled = true;
                        while (SelectedItems.Count > 0)
                        {
                            AbstractControl ctrl = SelectedItems[0];
                            SelectedItems.RemoveAt(0);
                            if (ctrl == MainDialog)
                                continue;
                            if (ctrl is Dialog)
                            {
                                items.Remove((Dialog) ctrl);
                                /*foreach (DialogControl control in ((Dialog)ctrl).ChildControls)
                                {
                            
                                }*/
                            }
                            else
                            {
                                ctrl.Parent.ChildControls.Remove((DialogControl) ctrl);
                            }
                            Parent.ControlRemoved(ctrl);
                            /*foreach (AbstractControl control in Items)
                            {
                                if (control.Parent == ctrl && !SelectedItems.Contains(control))
                                    SelectedItems.Add(control);
                            }*/
                        }
                        Invalidate();
                        Data.UpdateDesigener();
                        return;
                    case Keys.Left: // <--- left arrow.
                        e.Handled = true;
                        foreach (AbstractControl item in SelectedItems)
                        {
                            item.Position = new Point(item.Position.X - 1, item.Position.Y);
                        }
                        Invalidate();
                        break;
                    case Keys.Up: // <--- up arrow.
                        e.Handled = true;
                        foreach (AbstractControl item in SelectedItems)
                        {
                            item.Position = new Point(item.Position.X, item.Position.Y - 1);
                        }
                        Invalidate();
                        break;
                    case Keys.Right: // <--- right arrow.
                        e.Handled = true;
                        foreach (AbstractControl item in SelectedItems)
                        {
                            item.Position = new Point(item.Position.X + 1, item.Position.Y);
                        }
                        Invalidate();
                        break;
                    case Keys.Down: // <--- down arrow.
                        e.Handled = true;
                        foreach (AbstractControl item in SelectedItems)
                        {
                            item.Position = new Point(item.Position.X, item.Position.Y + 1);
                        }
                        Invalidate();
                        break;
                }
            }
            base.OnKeyDown(e);
        }

        

        private Size oldSize;
        public event AbstractControl.ParentSizeChangedDelegate ParentSizeChangedEvent;
        protected override void OnSizeChanged(EventArgs e)
        {
            if (ParentSizeChangedEvent != null)
                ParentSizeChangedEvent(oldSize, new Size(DrawWidth, DrawHeight));
            oldSize = new Size(DrawWidth, DrawHeight);
            base.OnSizeChanged(e);
        }

        
    }
}
