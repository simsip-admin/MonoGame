using System;
using System.Runtime.InteropServices;


#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif ANGLE // Review for iOS and Android, and change to GLES
using OpenTK.Graphics.ES30;
#endif
#endif

#if DIRECTX
#if SIMSIP_DESKTOP
using SharpDX.Direct3D11;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class OcclusionQuery : GraphicsResource
	{
#if OPENGL
		private int glQueryId;
#elif DIRECTX
#if SIMSIP_DESKTOP
        private Query _d3dQuery;
#endif
#endif

		public OcclusionQuery (GraphicsDevice graphicsDevice)
		{
			this.GraphicsDevice = graphicsDevice;
#if OPENGL
			GL.GenQueries (1, out glQueryId);
            GraphicsExtensions.CheckGLError();
#elif DIRECTX
#if SIMSIP_DESKTOP
            _d3dQuery = new Query(graphicsDevice._d3dDevice, new QueryDescription() { Type = QueryType.Occlusion, Flags = QueryFlags.None });
#endif

#endif
		}

		public void Begin ()
		{
#if OPENGL
#if GLES 
			GL.BeginQuery (QueryTarget.AnySamplesPassed, glQueryId);
#else
			GL.BeginQuery (QueryTarget.SamplesPassed, glQueryId);
#endif
            GraphicsExtensions.CheckGLError();
#elif DIRECTX
#if SIMSIP_DESKTOP
            _d3dQuery.Device.ImmediateContext.End(_d3dQuery);
#endif
#endif

		}

		public void End ()
		{
#if OPENGL
#if GLES 
			GL.EndQuery (QueryTarget.AnySamplesPassed);
#else
			GL.EndQuery (QueryTarget.SamplesPassed);
#endif
            GraphicsExtensions.CheckGLError();
#elif DIRECTX
#if SIMSIP_DESKTOP
            _d3dQuery.Device.ImmediateContext.End(_d3dQuery);
#endif
#endif

		}

		protected override void Dispose(bool disposing)
		{
            if (!IsDisposed)
            {
#if OPENGL
                Threading.BlockOnUIThread(() =>
                {
                    GL.DeleteQueries(1, ref glQueryId);
                    GraphicsExtensions.CheckGLError();
                });
#elif DIRECTX
#if SIMSIP_DESKTOP
                _d3dQuery.Dispose();
#endif
#endif
            }
            base.Dispose(disposing);
		}

#if SIMSIP_DESKTOP
        private ulong _result;
#endif
		public bool IsComplete {
			get {
				int resultReady = 0;
#if MONOMAC               
				GetQueryObjectiv(glQueryId,
				                 (int)GetQueryObjectParam.QueryResultAvailable,
				                 out resultReady);
#elif OPENGL
                GL.GetQueryObject(glQueryId, GetQueryObjectParam.QueryResultAvailable, out resultReady);
                GraphicsExtensions.CheckGLError();
#elif DIRECTX
#if SIMSIP_DESKTOP
                return GraphicsDevice._d3dContext.GetData(_d3dQuery, out _result);
#endif
#endif
				return resultReady != 0;
			}
		}
		public int PixelCount {
			get {
				int result = 0;
#if MONOMAC
				GetQueryObjectiv(glQueryId,
				                 (int)GetQueryObjectParam.QueryResult,
				                 out result);
#elif OPENGL
                GL.GetQueryObject(glQueryId, GetQueryObjectParam.QueryResultAvailable, out result);
                GraphicsExtensions.CheckGLError();
#elif DIRECTX     
#if SIMSIP_DESKTOP
                return (int)_result;
#endif
#endif
                return result;
			}
        }

#if MONOMAC
		//MonoMac doesn't export this. Grr.
		const string OpenGLLibrary = "/System/Library/Frameworks/OpenGL.framework/OpenGL";

		[System.Security.SuppressUnmanagedCodeSecurity()]
		[DllImport(OpenGLLibrary, EntryPoint = "glGetQueryObjectiv", ExactSpelling = true)]
		extern static unsafe void GetQueryObjectiv(int id, int pname, out int @params);
#endif
    }
}

