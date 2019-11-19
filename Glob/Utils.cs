using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	public static class Utils
	{
		/// <summary>
		/// Sets a shader uniform that is an array of vec4
		/// </summary>
		/// <param name="shader">The shader</param>
		/// <param name="name">The uniform's name in the shader's source code</param>
		/// <param name="value">The value of the uniform</param>
		public static void SetUniform(Shader shader, string name, Vector4[] value)
		{
			float[] array = new float[value.Length * 4];

			for(int i = 0; i < value.Length; i++)
			{
				for(int j = 0; j < 4; j++)
				{
					array[i * 4 + j] = value[i][j];
				}
			}

			int loc = shader.GetUniformLocation(name);
			GL.ProgramUniform4(shader.Handle, loc, value.Length, array);
		}

		/// <summary>
		/// Sets common texture parameters.
		/// Note: changes current texture binding.
		/// </summary>
		/// <param name="device">Glob device</param>
		/// <param name="tex">Texture the parameters of which will be set</param>
		/// <param name="wrapMode">Texture wrap mode, will be used for all three axes</param>
		/// <param name="magFilter">Texture magnification filter</param>
		/// <param name="minFilter">Texture minification filter</param>
		public static void SetTextureParameters(Device device, Texture tex, TextureWrapMode wrapMode, TextureMagFilter magFilter,
			TextureMinFilter minFilter)
		{
			device.BindTexture(tex);
			GL.TexParameter(tex.Target, TextureParameterName.TextureWrapR, (int)wrapMode);
			GL.TexParameter(tex.Target, TextureParameterName.TextureWrapS, (int)wrapMode);
			GL.TexParameter(tex.Target, TextureParameterName.TextureWrapT, (int)wrapMode);
			GL.TexParameter(tex.Target, TextureParameterName.TextureMagFilter, (int)magFilter);
			GL.TexParameter(tex.Target, TextureParameterName.TextureMinFilter, (int)minFilter);
			device.BindTexture(tex.Target, 0);
		}

		/// <summary>
		/// Creates an OpenGL buffer object with immutable storage using GL.BufferStorage
		/// Note: changes buffer binding of the "target" BufferTarget parameter.
		/// </summary>
		/// <param name="target">BufferTarget for the new buffer</param>
		/// <param name="length">Size of the new buffer in bytes</param>
		/// <param name="contents">Pointer to the data that will be copied to the new buffer. Use IntPtr.Zero to avoid data copy</param>
		/// <param name="flags">BufferStorageFlags for the new buffer</param>
		/// <param name="name">Debug label of the new buffer</param>
		/// <returns>Handle of the new buffer</returns>
		public static int CreateBuffer(BufferTarget target, IntPtr length, IntPtr contents, BufferStorageFlags flags,
			string name)
		{
			int buffer = GL.GenBuffer();
			GL.BindBuffer(target, buffer);
			GL.BufferStorage(target, length, contents, flags);
			SetObjectLabel(ObjectLabelIdentifier.Buffer, buffer, name);
			GL.BindBuffer(target, 0);
			return buffer;
		}

		/// <summary>
		/// Creates an OpenGL buffer object with immutable storage using GL.BufferStorage
		/// Note: changes buffer binding of the "target" BufferTarget parameter.
		/// </summary>
		/// <param name="target">BufferTarget for the new buffer</param>
		/// <param name="length">Size of the new buffer in bytes</param>
		/// <param name="contents">Data that will be copied to the new buffer</param>
		/// <param name="flags">BufferStorageFlags for the new buffer</param>
		/// <param name="name">Debug label of the new buffer</param>
		/// <returns>Handle of the new buffer</returns>
		public static int CreateBuffer<T>(BufferTarget target, IntPtr length, T[] contents, BufferStorageFlags flags,
			string name)
			where T : struct
		{
			int buffer = GL.GenBuffer();
			GL.BindBuffer(target, buffer);
			GL.BufferStorage(target, length, contents, flags);
			SetObjectLabel(ObjectLabelIdentifier.Buffer, buffer, name);
			GL.BindBuffer(target, 0);
			return buffer;
		}

		/// <summary>
		/// Copy bytes from one OpenGL buffer to another.
		/// Note: binds buffers to the CopyReadBuffer and CopyWriteBuffer binding points!
		/// </summary>
		/// <param name="bufferSource">Handle of the source buffer</param>
		/// <param name="bufferTarget">Handle of the target buffer</param>
		/// <param name="offsetSource">Offset in the source buffer in bytes</param>
		/// <param name="offsetTarget">Offset in the target buffer in bytes</param>
		/// <param name="size">Size of the region to copy in bytes</param>
		public static void CopyBuffer(int bufferSource, int bufferTarget, int offsetSource, int offsetTarget, int size)
		{
			GL.BindBuffer(BufferTarget.CopyReadBuffer, bufferSource);
			GL.BindBuffer(BufferTarget.CopyWriteBuffer, bufferTarget);
			GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, BufferTarget.CopyWriteBuffer, new IntPtr(offsetSource), new IntPtr(offsetTarget), size);
		}

		/// <summary>
		/// Assigns a debug label to an OpenGL object. The object's handle is automatically added to the label.
		/// </summary>
		/// <param name="handleNamespace">The namespace of the object handle</param>
		/// <param name="handle">Object's handle</param>
		/// <param name="name">Label of the object</param>
		public static void SetObjectLabel(ObjectLabelIdentifier handleNamespace, int handle, string name)
		{
			string label = handleNamespace.ToString() + ": " + name + " (handle: " + handle + ")";
			GL.ObjectLabel(handleNamespace, handle, label.Length, label);
		}

		public static Matrix4 GetCameraMatrix(Vector3 position, Quaternion rotation)
		{
			Matrix4 cam = Matrix4.Identity;
			cam.Row3.Xyz = -position;
			cam *= Matrix4.CreateFromQuaternion(rotation);
			return cam;
		}

		/// <summary>
		/// Creates an orthogonal camera and projection matrix.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		/// <param name="scale"></param>
		/// <returns></returns>
		public static Matrix4 GetCameraMatrixOrtho(Vector3 position, Vector3 direction, Vector3 scale)
		{
			Matrix4 cam = Matrix4.Identity;

			Vector3 right = Vector3.Cross(direction, Vector3.UnitY);

			if(right.LengthSquared > 0)
				right.Normalize();
			else
				right = Vector3.UnitX;

			cam.Column0 = new Vector4(right * scale.X, 0);
			cam.Column1 = new Vector4(Vector3.Cross(right, direction).Normalized() * scale.Y, 0);
			cam.Column2 = new Vector4(direction.Normalized() * scale.Z, 0);

			Matrix4 translate = Matrix4.Identity;
			translate.Row3.Xyz = -position;

			return translate * cam;
		}

		/// <summary>
		/// Creates a perspective projection matrix
		/// </summary>
		/// <param name="x">Screen width in pixels. Used to get the aspect ratio</param>
		/// <param name="y">Screen height in pixels. Used to get the aspect ratio</param>
		/// <param name="hfov">Desired horizontal field of view in degrees</param>
		/// <param name="near">Near plane distance</param>
		/// <param name="far">Far plane distance</param>
		/// <returns></returns>
		public static Matrix4 GetProjectionPerspective(int x, int y, float hfov, float near, float far)
		{
			float ar = x / (float)y;
			float fovn = hfov * (float)Math.PI / 180; // transform fov from degrees to radians
			fovn = (float)(2.0f * Math.Atan(Math.Tan(fovn * 0.5f) / ar)); // transform from horizontal fov to vertical fov
			float tan = (float)Math.Tan(fovn / 2.0f); // tangent of half vertical fov

			Matrix4 proj = Matrix4.Identity;
			proj.Row0 = new Vector4(1f / (ar * tan), 0, 0, 0);
			proj.Row1 = new Vector4(0, 1f / tan, 0, 0);
			proj.Row2 = new Vector4(0, 0, (near + far) / (near - far), -1f);
			proj.Row3 = new Vector4(0, 0, (2f * near * far) / (near - far), 0);

			return proj;
		}

		/// <summary>
		/// Creates a perspective projection matrix for use with the reverse-z trick to achieve infinite far plane distance.
		/// </summary>
		/// <param name="x">Screen width in pixels. Used to get the aspect ratio</param>
		/// <param name="y">Screen height in pixels. Used to get the aspect ratio</param>
		/// <param name="hfov">Desired horizontal field of view in degrees</param>
		/// <param name="near">Near plane distance</param>
		/// <returns></returns>
		public static Matrix4 GetProjectionPerspectiveReverseZ(int x, int y, float hfov, float near)
		{
			Matrix4 proj = GetProjectionPerspective(x, y, hfov, 1, 2);
			proj.Row2 = new Vector4(0, 0, 0, -1f);
			proj.Row3 = new Vector4(0, 0, near, 0);

			return proj;
		}

		/// <summary>
		/// Creates a transform matrix for an object in the game world.
		/// </summary>
		/// <param name="position">The object's position</param>
		/// <param name="rotation">The object's rotation</param>
		/// <param name="scale">The object's scale</param>
		/// <returns></returns>
		public static Matrix4 GetWorldMatrix(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			Matrix4 wm = Matrix4.Identity;
			wm.Row0 = new Vector4(scale.X, 0, 0, 0);
			wm.Row1 = new Vector4(0, scale.Y, 0, 0);
			wm.Row2 = new Vector4(0, 0, scale.Z, 0);

			wm *= Matrix4.CreateFromQuaternion(rotation);
			wm.Row3 = new Vector4(position, 1);

			return wm;
		}

		/// <summary>
		/// Returns vectors that "point" from the camera (center of projection) to the four corners of the screen, their lenght set so that these four vectors form a plane parallel to the screen one unit away from the camera.
		/// If screenspace coordinates are available in shader, one can just lerp these vectors to get a vector pointing in current pixel's direction, and multiply the result by the pixel's depth to get the pixel's position.
		/// </summary>
		/// <param name="projectionMatrix">Projection matrix. Must be a classical projection matrix without the reverse-z trick</param>
		/// <param name="cameraMatrix">Camera matrix that includes translation and rotation</param>
		/// <param name="ray00">Ray to the top left corner of the screen</param>
		/// <param name="ray10">Ray to the top right corner of the screen</param>
		/// <param name="ray11">Ray to the bottom left corner of the screen</param>
		/// <param name="ray01">Ray to the bottom right corner of the screen</param>
		public static void GetCameraCornerRays(Matrix4 projectionMatrix, Matrix4 cameraMatrix, out Vector4 ray00, out Vector4 ray10, out Vector4 ray11, out Vector4 ray01)
		{
			const float scale = 1f;
			Vector3 dir = -cameraMatrix.Column2.Xyz.Normalized();
			Vector3 translation = cameraMatrix.ExtractTranslation();
			Vector4 pos = new Vector4(-translation, 0f);

			Vector4 plane = new Vector4(dir, -Vector3.Dot(pos.Xyz, dir));

			cameraMatrix.Row3 -= new Vector4(translation, 0);
			Matrix4 invViewProjMat = Matrix4.Identity;

			try
			{
				invViewProjMat = (cameraMatrix * projectionMatrix).Inverted();
			}
			catch(InvalidOperationException)
			{

			}

			float w = 1;

			ray00 = Vector4.Transform(new Vector4(-scale, +scale, 0, w), invViewProjMat);
			ray10 = Vector4.Transform(new Vector4(+scale, +scale, 0, w), invViewProjMat);
			ray11 = Vector4.Transform(new Vector4(+scale, -scale, 0, w), invViewProjMat);
			ray01 = Vector4.Transform(new Vector4(-scale, -scale, 0, w), invViewProjMat);

			ray00 /= ray00.W;
			ray10 /= ray10.W;
			ray11 /= ray11.W;
			ray01 /= ray01.W;

			ray00.Xyz = ray00.Xyz.Normalized();
			ray01.Xyz = ray01.Xyz.Normalized();
			ray11.Xyz = ray11.Xyz.Normalized();
			ray10.Xyz = ray10.Xyz.Normalized();

			
			ray00.Xyz /= Vector4.Dot(ray00 + pos, plane);
			ray10.Xyz /= Vector4.Dot(ray10 + pos, plane);
			ray11.Xyz /= Vector4.Dot(ray11 + pos, plane);
			ray01.Xyz /= Vector4.Dot(ray01 + pos, plane);
		}

		/// <summary>
		/// Checks status of currently bound framebuffer, prints a descriptive error message if the FBO is not complete
		/// </summary>
		/// <param name="textOutput"></param>
		public static void CheckFramebufferErrors(Device device)
		{
			var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

			var textOutput = device.TextOutput;

			switch(status)
			{
				case FramebufferErrorCode.FramebufferCompleteExt:
					{
						//KvGlobContext.Current.Printer.Print("fbo: The framebuffer is complete and valid for rendering.");
						break;
					}
				case FramebufferErrorCode.FramebufferIncompleteAttachmentExt:
					{
						textOutput.Print(OutputTypeGlob.PerformanceWarning, "fbo: One or more attachment points are not framebuffer attachment complete. This could mean there’s no texture attached or the format isn’t renderable. For color textures this means the base format must be RGB or RGBA and for depth textures it must be a DEPTH_COMPONENT format. Other causes of this error are that the width or height is zero or the z-offset is out of range in case of render to volume.");
						break;
					}
				case FramebufferErrorCode.FramebufferIncompleteMissingAttachmentExt:
					{
						textOutput.Print(OutputTypeGlob.PerformanceWarning, "fbo: There are no attachments.");
						break;
					}
				/*case  FramebufferErrorCode.GL_FRAMEBUFFER_INCOMPLETE_DUPLICATE_ATTACHMENT_EXT: 
					 {
						 KvGlobContext.Current.Printer.Print("fbo: An object has been attached to more than one attachment point.");
						 break;
					 }*/
				case FramebufferErrorCode.FramebufferIncompleteDimensionsExt:
					{
						textOutput.Print(OutputTypeGlob.PerformanceWarning, "fbo: Attachments are of different size. All attachments must have the same width and height.");
						break;
					}
				case FramebufferErrorCode.FramebufferIncompleteFormatsExt:
					{
						textOutput.Print(OutputTypeGlob.PerformanceWarning, "fbo: The color attachments have different format. All color attachments must have the same format.");
						break;
					}
				case FramebufferErrorCode.FramebufferIncompleteDrawBufferExt:
					{
						textOutput.Print(OutputTypeGlob.PerformanceWarning, "fbo: An attachment point referenced by GL.DrawBuffers() doesn’t have an attachment.");
						break;
					}
				case FramebufferErrorCode.FramebufferIncompleteReadBufferExt:
					{
						textOutput.Print(OutputTypeGlob.PerformanceWarning, "fbo: The attachment point referenced by GL.ReadBuffers() doesn’t have an attachment.");
						break;
					}
				case FramebufferErrorCode.FramebufferUnsupportedExt:
					{
						textOutput.Print(OutputTypeGlob.PerformanceWarning, "fbo: This particular fbo configuration is not supported by the implementation.");
						break;
					}
				default:
					{
						textOutput.Print(OutputTypeGlob.PerformanceWarning, "fbo: Status unknown. (yes, this is really bad.) Error code: " + status.ToString());
						break;
					}
			}
		}
	}
}
