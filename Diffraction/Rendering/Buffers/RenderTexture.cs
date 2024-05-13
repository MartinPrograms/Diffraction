using Silk.NET.OpenGL;

namespace Diffraction.Rendering.Buffers;

public class RenderTexture
{
    private uint _fbo;
    private uint _texture;
    private uint _rbo;
    private int _width;
    private int _height;
    private GL _gl;
    
    public unsafe RenderTexture(GL gl, int width, int height)
    {
        _gl = gl;
        _width = width;
        _height = height;
        
        _fbo = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        
        // No AA
        _texture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _texture);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba,
            PixelType.UnsignedByte, null);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _texture, 0);
        
        _rbo = _gl.GenRenderbuffer();
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
        _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, (uint)width, (uint)height);
        _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _rbo);
        
        if (_gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            throw new Exception("Framebuffer is not complete!");
        }
        
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public uint Texture => _texture;

    public void Bind()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        _gl.Viewport(0, 0, (uint)_width, (uint)_height);
        
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gl.ClearColor(0.2f, 0.2f, 0.4f, 1.0f);
        _gl.Enable(EnableCap.DepthTest);
        _gl.DepthFunc(DepthFunction.Lequal);
    }
    
    public void Unbind()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
    
    public unsafe void Resize(int width, int height)
    {
        _width = width;
        _height = height;
        
        _gl.BindTexture(TextureTarget.Texture2D, _texture);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba,
            PixelType.UnsignedByte, null);
        
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
        _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, (uint)width, (uint)height);
    }
    
    public void Dispose()
    {
        _gl.DeleteFramebuffer(_fbo);
        _gl.DeleteTexture(_texture);
        _gl.DeleteRenderbuffer(_rbo);
    }

    public void SetSize(int sizeX, int sizeY)
    {
        if (_width != sizeX || _height != sizeY)
        {
            Resize(sizeX, sizeY);
        }
    }

    public void ImBind()
    {
        _gl.BindTexture(TextureTarget.Texture2D, _texture);
    }

    public void ImUnbind()
    {
        _gl.BindTexture(TextureTarget.Texture2D, 0);
    }
}