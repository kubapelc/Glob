using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	public class FrameBuffer : IDisposable
	{
		public static readonly FrameBuffer BackBuffer = new FrameBuffer(0);

		public int Handle { get; private set; }

		Dictionary<FramebufferAttachment, IRenderTarget> Attachments = new Dictionary<FramebufferAttachment, IRenderTarget>();

		/// <summary>
		/// Creates a new framebuffer object. Can be bound using Device.BindFrameBuffer(), attachments can be added using FrameBuffer.Attach()
		/// </summary>
		public FrameBuffer()
		{
			Handle = GL.GenFramebuffer();
		}

		/// <summary>
		/// Generates an empty framebuffer with specified rasterization parameters.
		/// Useful for techniques such as voxelization, where the actual pixels resulting from rasterization are useless and the pixel shader outputs data in other ways.
		/// </summary>
		public static FrameBuffer GenerateEmptyFramebuffer(Device device, int width, int height, int layers = 0, int samples = 0, bool fixedSamplePositions = false)
		{
			FrameBuffer fbo = new FrameBuffer();
			device.BindFrameBuffer(fbo, FramebufferTarget.Framebuffer);
			GL.FramebufferParameter(FramebufferTarget.Framebuffer, FramebufferDefaultParameter.FramebufferDefaultWidth, width);
			GL.FramebufferParameter(FramebufferTarget.Framebuffer, FramebufferDefaultParameter.FramebufferDefaultHeight, height);
			GL.FramebufferParameter(FramebufferTarget.Framebuffer, FramebufferDefaultParameter.FramebufferDefaultLayers, layers);
			GL.FramebufferParameter(FramebufferTarget.Framebuffer, FramebufferDefaultParameter.FramebufferDefaultSamples, samples);
			GL.FramebufferParameter(FramebufferTarget.Framebuffer, FramebufferDefaultParameter.FramebufferDefaultFixedSampleLocations, fixedSamplePositions ? 1 : 0);
			return fbo;
		}

		private FrameBuffer(int handle)
		{
			Handle = handle;
		}

		/// <summary>
		/// Attaches a rendertarget to the currently bound framebuffer
		/// Note: framebuffer must be bound first
		/// </summary>
		/// <param name="attachmentPoint"></param>
		/// <param name="attachment"></param>
		public void Attach(FramebufferAttachment attachmentPoint, IRenderTarget attachment)
		{
			if(Attachments.ContainsKey(attachmentPoint))
			{
				Attachments[attachmentPoint] = attachment;
			}
			else
			{
				Attachments.Add(attachmentPoint, attachment);
			}
			attachment.AttachToFramebuffer(FramebufferTarget.DrawFramebuffer, attachmentPoint);
		}

		internal IDisposable BindPushViewport(Device device, int x0, int y0 = 0, int x1 = -1, int y1 = -1, string message = null)
		{
			device.BindFrameBuffer(this, FramebufferTarget.Framebuffer);

			if (Attachments.Count > 0)
			{
				var mainAttachment = Attachments.First().Value;
				if(mainAttachment == null)
					throw new Exception("Framebuffer attachment is null!");
				if(mainAttachment.Width < 1 || mainAttachment.Height < 1)
					throw new Exception("Framebuffer attachment dimensions must be greater than zero!");

				if(x1 < 0)
				{
					x1 = mainAttachment.Width;
				}
				if(y1 < 0)
				{
					y1 = mainAttachment.Height;
				}
			}

			GL.PushAttrib(AttribMask.ViewportBit);
			GL.Viewport(x0, y0, x1, y1);

			if(!string.IsNullOrEmpty(message))
			{
				var marker = device.DebugMessageManager.PushGroupMarker(message);

				return new DelegatedDisposable(() =>
				{
					GL.PopAttrib();
					marker.Dispose();
				});
			}

			return new DelegatedDisposable(GL.PopAttrib);
		}

		/// <summary>
		/// Deletes the OpenGL framebuffer object
		/// </summary>
		public void Dispose()
		{
			if(Handle > 0)
				GL.DeleteFramebuffer(Handle);
			Handle = -1;
		}
	}
}
