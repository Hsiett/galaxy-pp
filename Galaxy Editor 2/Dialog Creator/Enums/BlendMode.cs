using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using XNA_BlendState = Microsoft.Xna.Framework.Graphics.BlendState;
namespace Galaxy_Editor_2.Dialog_Creator.Enums
{
    enum BlendMode
    {
        Normal,
        Add,
        Alpha,
        Darken,
        Lighten,
        Multiply,
        Subtract
    }

    class BlendState
    {
        public static Dictionary<BlendMode, BlendState> BlendStates = new Dictionary<BlendMode, BlendState>();
        static BlendState()
        {
            BlendStates.Add(BlendMode.Alpha, new BlendState(Blend.SourceAlpha, Blend.InverseSourceAlpha, BlendFunction.Add));
            BlendStates.Add(BlendMode.Add, new BlendState(Blend.One, Blend.One, BlendFunction.Add));
            BlendStates.Add(BlendMode.Darken, new BlendState(Blend.One, Blend.One, BlendFunction.Min));
            BlendStates.Add(BlendMode.Normal, BlendStates[BlendMode.Alpha]);
            BlendStates.Add(BlendMode.Lighten, new BlendState(Blend.One, Blend.One, BlendFunction.Max));
            BlendStates.Add(BlendMode.Multiply, new BlendState(Blend.DestinationColor, Blend.Zero, BlendFunction.Add));
            BlendStates.Add(BlendMode.Subtract, new BlendState(Blend.One, Blend.One, BlendFunction.ReverseSubtract));
            /*ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.ReverseSubtract*/
        }

        private Blend sourceBlend, destinationBlend;
        private BlendFunction function;

        private BlendState(Blend sourceBlend, Blend destinationBlend, BlendFunction function)
        {
            this.sourceBlend = sourceBlend;
            this.destinationBlend = destinationBlend;
            this.function = function;
        }

        public void Apply(GraphicsDevice device)
        {
            device.BlendState = XNA_BlendState.AlphaBlend;
          /*  device.RenderState.SourceBlend = sourceBlend;
            device.RenderState.DestinationBlend = destinationBlend;
            device.RenderState.BlendFunction = function;*/
        }
    }
}
