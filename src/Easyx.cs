using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static SDL2.SDL;
using SDL2;
namespace SDL_Easyx
{
    public static class Easyx
    {
        // SDL_Window*
        private static nint window;
        // SDL_Renderer*
        private static nint renderer;
        // SDL_Surface*
        private static nint textSurface;
        // SDL_Texture*
        private static nint textTexture;
        // TTF_Font*
        private static nint font;
        public static LineType LineStyle
        {
            get;
            set;
        }
        public static SDL_Color BKcolor
        {
            get;
            set;
        }
        public static SDL_Color FillColor
        {
            get;
            set;
        }
        public static SDL_Color LineColor
        {
            get;
            set;
        }
        public static SDL_Color TextColor
        {
            get;
            set;
        }
        private static LogFont logFont;
        public static LogFont TextStyle
        {
            get
            {
                return logFont;
            }
            set
            {
                logFont=value;
                font=SDL_ttf.TTF_OpenFont(logFont.lfFaceName,logFont.lfHeight);
            }
        }
        public const SDL_WindowFlags EX_NOMINIMIZE=SDL_WindowFlags.SDL_WINDOW_SHOWN;
        public static readonly SDL_Color BLACK=BGR(0x0);
        public static readonly SDL_Color BLUE=BGR(0x0000FF);
        public static readonly SDL_Color GREEN=BGR(0x008000);
        public static readonly SDL_Color CYAN=BGR(0x00FFFF);
        public static readonly SDL_Color RED=BGR(0xFF0000);
        public static readonly SDL_Color MAGENTA=BGR(0xFF00FF);
        public static readonly SDL_Color BROWN=BGR(0xA52A2A);
        public static readonly SDL_Color LIGHTGRAY=BGR(0xD3D3D3);
        public static readonly SDL_Color DARKGRAY=BGR(0xA9A9A9);
        public static readonly SDL_Color LIGHTBLUE=BGR(0xADD8E6);
        public static readonly SDL_Color LIGHTGREEN=BGR(0x90EE90);
        public static readonly SDL_Color LIGHTCYAN=BGR(0xE1FFFF);
        public static readonly SDL_Color LIGHTRED=BGR(0xCD5C5C);
        public static readonly SDL_Color LIGHTMAGENTA=BGR(0xFF55FF);
        public static readonly SDL_Color YELLOW=BGR(0xFFFF00);
        public static readonly SDL_Color WHITE=BGR(0xFFFFFF);
        static Easyx()
        {
            if(SDL_Init(SDL_INIT_EVERYTHING)<0)
                throw new Exception($"Init Failed:\n{SDL_GetError()}");
            if(SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_JPG|SDL_image.IMG_InitFlags.IMG_INIT_PNG)<0)
                throw new Exception($"Init Failed:\n{SDL_image.IMG_GetError()}");
            if(SDL_ttf.TTF_Init()==-1)
                throw new Exception($"Init Failed:\n{SDL_GetError()}");
        }
        public struct LineType
        {
            public int style;
            public int thickness;
        }
        public struct LogFont
        {
            public LogFont(int nHeight,int nWidth,FileInfo lpszFace,int nWeight,bool bItalic,bool bUnderline,bool bStrikeOut)
            {
                lfWidth=nWidth;
                lfHeight=nHeight==0?5:nHeight;
                lfFaceName=lpszFace.Name[0..lpszFace.Name.IndexOf(".")];
                lfWeight=nWeight;
                lfItalic=bItalic;
                lfUnderline=bUnderline;
                lfStrikeOut=bStrikeOut;
            }
            public LogFont(int nHeight,int nWidth,string lpszFace,int nWeight,bool bItalic,bool bUnderline,bool bStrikeOut) : this(nHeight,nWidth,new FileInfo(IsPath(lpszFace)?lpszFace:FindFile(lpszFace+".ttf",@"C:\Windows\Fonts\")),nWeight,bItalic,bUnderline,bStrikeOut)
            {
            }
            public LogFont(int nHeight,int nWidth,FileInfo lpszFace) : this(nHeight,nWidth,lpszFace,0,false,false,false)
            {
            }
            public LogFont(int nHeight,int nWidth,string lpszFace) : this(nHeight,nWidth,new FileInfo(IsPath(lpszFace)?lpszFace:FindFile(lpszFace+".ttf",@"C:\Windows\Fonts\")))
            { 
            }
            private static bool IsPath(string path)
            {
                return Regex.IsMatch(path,@"[A-Za-z0-9_\-\\\/]+");
            }
            private static string FindFile(string file,string dir)
            {
                if(!Directory.GetFiles(dir).Contains(file))
                    throw new FileNotFoundException($"Unable to find {file}");
                return dir+file;
            }
            public int lfHeight;
            public int lfWidth;
            public int lfWeight;
            public bool lfItalic;
            public bool lfUnderline;
            public bool lfStrikeOut;
            public string lfFaceName;
        }
        public static void Swap<T>(ref T a,ref T b)
        {
            T t;
            t=a;
            a=b;
            b=t;
        }
        public static SDL_Color ToSDL_Color(Color c)
        {
            return RGB(c.R,c.G,c.B,c.A);
        }
        public class Image : IDisposable
        {
            // SDL_Surface*
            public nint ImageBuffer
            {
                get;
                set;
            }
            private nint texture;
            public int Width
            {
                get;
                private set;
            }
            public int Height
            {
                get;
                private set;
            }
            public Image(){}
            public Image(int width,int height)
            {
                this.Width=width;
                this.Height=height;
            }
            public void Resize(int width,int height)
            {
                nint newI=SDL_CreateRGBSurface(0,width,height,32,0,0,0,0);
                this.ImageBuffer=newI;
                this.Width=width;
                this.Height=height;
            }
            public void Load(string name)
            {
                this.ImageBuffer=name.Contains("bmp")?SDL_LoadBMP(name):SDL_image.IMG_Load(name);
                if(this.ImageBuffer==0)
                    throw new IOException($"Cannot load Image: {(name.Contains("bmp")?SDL_GetError():SDL_image.IMG_GetError())}");
                this.texture=SDL_CreateTextureFromSurface(renderer,ImageBuffer);
            }
            public void Put(int x,int y)
            {
                SDL_Rect rect=new()
                {
                    x=x,
                    y=y,
                    w=Width,
                    h=Height
                };
                _=SDL_RenderCopy(renderer,texture,0,ref rect);
            }
            public void Put(int x,int y,int w,int h,int srcx,int srcy)
            {
                SDL_Rect rect=new()
                {
                    x=x,
                    y=y,
                    w=w,
                    h=h
                };
                SDL_Rect p=new()
                {
                    x=srcx,
                    y=srcy,
                    w=w,
                    h=h
                };
                _=SDL_RenderCopy(renderer,texture,ref p,ref rect);
            }
            public static void Save(string file,Image? ImageBuffer=null)
            {
                nint tp;
                if(ImageBuffer==null)
                    tp=SDL_GetWindowSurface(window);
                else
                    tp=ImageBuffer.ImageBuffer;
                if(file.Contains("bmp"))
                    SDL_SaveBMP(tp,file);
                else if(file.Contains("jpg"))
                    SDL_image.IMG_SaveJPG(tp,file,66);
                else
                    SDL_image.IMG_SavePNG(tp,file);
            }
            public void Dispose()
            {
                SDL_DestroyTexture(texture);
                SDL_FreeSurface(ImageBuffer);
                GC.SuppressFinalize(this);
            }
            ~Image()
            {
                this.Dispose();
            }
        }
        public static void SetWorkingImage(Image? image)
        {
            _=SDL_SetRenderTarget(renderer,image==null?0:image.ImageBuffer);
        }
        public static Image GetWorkingImage()
        {
            SDL_Surface tp=(SDL_Surface?)Marshal.PtrToStructure(SDL_GetRenderTarget(renderer),typeof(SDL_Surface))??throw new NoNullAllowedException("Cannot be null");
            return new Image(tp.w,tp.h)
            {
                ImageBuffer=SDL_GetRenderTarget(renderer)
            };
        }
        private static void SetColor(SDL_Color c)
        {
            _=SDL_SetRenderDrawColor(renderer,c.r,c.g,c.b,c.a);
        }
        public static nint InitGraph(int width,int height,SDL_WindowFlags flag=0)
        {
            string myName=System.Reflection.Assembly.GetExecutingAssembly().GetName().Name??"Easyx20220901";
            window=SDL_CreateWindow(myName,50,50,width,height,flag==0?SDL_WindowFlags.SDL_WINDOW_SHOWN&SDL_WindowFlags.SDL_WINDOW_MINIMIZED:flag);
            if(window==0)
                throw new IOException($"Cannot create window: {SDL_GetError()}");
            renderer=SDL_CreateRenderer(window,-1,0);
            if(renderer==0)
                throw new IOException($"Cannot create renderer: {SDL_GetError()}");
            Graphdefaults();
            ClearDevice();
            return window;
        }
        public static void CloseGraph()
        {
            SDL_DestroyWindow(window);
            SDL_DestroyRenderer(renderer);
            SDL_image.IMG_Quit();
            SDL_ttf.TTF_Quit();
        }
        public static void ClearDevice()
        {
            SetColor(BKcolor);
            FlushBatchDraw();
            _=SDL_RenderClear(renderer);
        }
        public static void Graphdefaults()
        {
            SetColor(RGB(255,255,255));
            BKcolor=BLACK;
            FillColor=YELLOW;
            LineColor=WHITE;
            LineStyle=new()
            {
                thickness=1
            };
            TextColor=WHITE;
            TextStyle=new(5,0,"Consolas");
        }
        public static byte RGBtoGRAY(SDL_Color color)
        {
            return (byte)(color.r*0.3+color.g*0.59+color.b*0.11);
        }
        public static void RGBtoHSL(SDL_Color c,out double h,out double s,out double l)
        {
            double r=c.r/255.0;
            double g=c.g/255.0;
            double b=c.b/255.0;
            double max=Math.Max(r,Math.Max(g,b));
            double min=Math.Min(r,Math.Min(g,b));
            if(max==min)
                h=0;
            else if(max==r)
                h=(60*((g-b)/(max-min))+360)%360;
            else if(max==g)
                h=60*((b-r)/(max-min))+120;
            else
                h=60*((r-g)/(max-min))+240;
            l=(max+min)/2;
            if(max==min)
                s=0;
            else if(l<=0.5)
                s=(max-min)/(2*l);
            else
                s=(max-min)/(2-2*l);
        }
        public static void RGBtoHSV(SDL_Color c,out double h,out double s,out double v)
        {
            double r=c.r/255.0;
            double g=c.g/255.0;
            double b=c.b/255.0;
            double max=Math.Max(r,Math.Max(g,b));
            double min=Math.Min(r,Math.Min(g,b));
            if(max==min)
                h=0;
            else if(max==r)
                h=(60*((g-b)/(max-min))+360)%360;
            else if(max==g)
                h=60*((b-r)/(max-min))+120;
            else
                h=60*((r-g)/(max-min))+240;
            s=(max==0)?0:(max-min)/max;
            v=max;
        }
        public static SDL_Color HSLtoRGB(double h,double s,double l)
        {
            static double GetColorComponent(double temp1, double temp2, double tempColor)
            {
                if(tempColor<0)
                    tempColor+=1;
                else if(tempColor>1)
                    tempColor -= 1;
                double colorComponent;
                if(tempColor<1.0/6.0)
                    colorComponent=temp1+(temp2-temp1)*6*tempColor;
                else if(tempColor<1.0/2.0)
                    colorComponent=temp2;
                else if(tempColor<2.0/3.0)
                    colorComponent=temp1+(temp2-temp1)*(2.0/3.0-tempColor)*6;
                else
                    colorComponent=temp1;
                return colorComponent;
            }
            h/=360.0;
            double temp1,temp2,tempR,tempG,tempB;
            if(s==0)
            {
                int r=(int)Math.Round(l*255);
                return new()
                {
                    r=(byte)r,
                    g=(byte)r,
                    b=(byte)r
                };
            }
            if(l<0.5)
                temp2=l*(1+s);
            else
                temp2=l+s-(s*l);
            temp1=2*l-temp2;
            tempR=h+1.0/3.0;
            tempG=h;
            tempB=h-1.0/3.0;
            var rgb=new double[3];
            rgb[0]=GetColorComponent(temp1,temp2,tempR);
            rgb[1]=GetColorComponent(temp1,temp2,tempG);
            rgb[2]=GetColorComponent(temp1,temp2,tempB);
            int red=(int)Math.Round(rgb[0]*255);
            int green=(int)Math.Round(rgb[1]*255);
            int blue=(int)Math.Round(rgb[2]*255);
            return RGB(red,green,blue);
        }
        public static SDL_Color HSVtoRGB(double h,double s,double v)
        {
            h/=60.0;
            double c=v*s;
            double x=c*(1-Math.Abs(h%2-1));
            double m=v-c;
            double tempR,tempG,tempB;
            if(h>=0&&h<1)
            {
                tempR=c;
                tempG=x;
                tempB=0;
            }
            else if(h>=1&&h<2)
            {
                tempR=x;
                tempG=c;
                tempB=0;
            }
            else if(h>=2&&h<3)
            {
                tempR=0;
                tempG=c;
                tempB=x;
            }
            else if(h>=3&&h<4)
            {
                tempR=0;
                tempG=x;
                tempB=c;
            }
            else if(h>=4&&h<5)
            {
                tempR=x;
                tempG=0;
                tempB=c;
            }
            else
            {
                tempR=c;
                tempG=0;
                tempB=x;
            }
            int red=(int)Math.Round((tempR+m)*255);
            int green=(int)Math.Round((tempG+m)*255);
            int blue=(int)Math.Round((tempB+m)*255);
            return RGB(red,green,blue);
        }
        public static SDL_Color GetPixel(int x,int y)
        {
            _=SDL_RenderGetViewport(renderer,out var rect);
            rect.x=x;
            rect.y=y;
            rect.w=1;
            rect.h=1;
            nint pixels=Marshal.AllocHGlobal(4);
            int pitch=4;
            _=SDL_RenderReadPixels(renderer,ref rect,SDL_PIXELFORMAT_RGBA8888,pixels,pitch);
            byte[] pixelData=new byte[4];
            Marshal.Copy(pixels,pixelData,0,4);
            return new()
            {
                r=pixelData[0],
                g=pixelData[1],
                b=pixelData[2],
                a=pixelData[3]
            };
        }
        public static void PutPixel(int x,int y,SDL_Color c)
        {
            SetColor(c);
            _=SDL_RenderDrawPoint(renderer,x,y);
        }
        public static void Line(int x1,int y1,int x2,int y2)
        {
            if(LineStyle.thickness<1)
                throw new ArgumentException("Thickness should be >= 1");
            _=SDL_gfx.thickLineRGBA(renderer,(short)x1,(short)y1,(short)x2,(short)y2,(byte)LineStyle.thickness,LineColor.r,LineColor.g,LineColor.b,LineColor.a);
        }
        public static void Rectangle(int left,int top,int right,int bottom)
        {
            if(LineStyle.thickness<1)
                throw new ArgumentException("Thickness should be >= 1");
            Line(left,top,right,top);
            Line(left,bottom,right,bottom);
            Line(left,top,left,bottom);
            Line(right,top,right,bottom);
        }
        public static void FillRectangle(int left,int top,int right,int bottom)
        {
            SolidRectangle(left,top,right,bottom);
            Rectangle(left,top,right,bottom);
        }
        public static void SolidRectangle(int left,int top,int right,int bottom)
        {
            _=SDL_gfx.boxRGBA(renderer,(short)left,(short)top,(short)right,(short)bottom,FillColor.r,FillColor.g,FillColor.b,FillColor.a);
        }
        public static void Circle(int x,int y,int radius)
        {
            _=SDL_gfx.circleRGBA(renderer,(short)x,(short)y,(short)radius,LineColor.r,LineColor.g,LineColor.b,LineColor.a);
        }
        public static void SolidCircle(int x,int y,int radius)
        {
            _=SDL_gfx.filledCircleRGBA(renderer,(short)x,(short)y,(short)radius,LineColor.r,LineColor.g,LineColor.b,LineColor.a);
        }
        public static void FillCircle(int x,int y,int radius)
        {
            SolidCircle(x,y,radius);
            Circle(x,y,radius);
        }
        public static void Ellipse(int left,int top,int right,int bottom)
        {
            _=SDL_gfx.ellipseRGBA(renderer,(short)left,(short)top,(short)right,(short)bottom,LineColor.r,LineColor.g,LineColor.b,LineColor.a);
        }
        public static void FillEllipse(int left,int top,int right,int bottom)
        {
            SolidEllipse(left,top,right,bottom);
            Ellipse(left,top,right,bottom);
        }
        public static void SolidEllipse(int left,int top,int right,int bottom)
        {
            _=SDL_gfx.filledEllipseRGBA(renderer,(short)left,(short)top,(short)right,(short)bottom,FillColor.r,FillColor.g,FillColor.b,FillColor.a);
        }
        public static void RoundRect(int left, int top, int right, int bottom, int ellipse)
        {
            _=SDL_gfx.roundedRectangleRGBA(renderer,(short)left,(short)top,(short)right,(short)bottom,(short)ellipse,LineColor.r,LineColor.g,LineColor.b,LineColor.a);
        }
        public static void FillRoundrect(int left, int top, int right, int bottom, int ellipse)
        {
            SolidRoundrect(left,top,right,bottom,ellipse);
            RoundRect(left,top,right,bottom,ellipse);
        }
        public static void SolidRoundrect(int left, int top, int right, int bottom, int ellipse)
        {
            _=SDL_gfx.roundedBoxRGBA(renderer,(short)left,(short)top,(short)right,(short)bottom,(short)ellipse,FillColor.r,FillColor.g,FillColor.b,FillColor.a);
        }
        public static void Arc(int left, int top, int rad, double stangle, double endangle)
        {
            _=SDL_gfx.arcRGBA(renderer,(short)left,(short)top,(short)rad,(short)stangle,(short)endangle,LineColor.r,LineColor.g,LineColor.b,LineColor.a);
        }
        public static void Pie(int left, int top, int rad, double stangle, double endangle)
        {
            _=SDL_gfx.pieRGBA(renderer,(short)left,(short)top,(short)rad,(short)stangle,(short)endangle,LineColor.r,LineColor.g,LineColor.b,LineColor.a);
        }
        public static void FillPie(int left, int top, int rad, double stangle, double endangle)
        {
            SolidPie(left,top,rad,stangle,endangle);
            Pie(left,top,rad,stangle,endangle);
        }
        public static void SolidPie(int left, int top, int rad, double stangle, double endangle)
        {
            _=SDL_gfx.filledPieRGBA(renderer,(short)left,(short)top,(short)rad,(short)stangle,(short)endangle,FillColor.r,FillColor.g,FillColor.b,FillColor.a);
        }
        public static void PolyLine(SDL_Point[] points,int num=-1)
        {
            if(num==-1)
                num=points.Length;
            for(int i=1;i<num;i++)
                Line(points[i-1].x,points[i-1].y,points[i].x,points[i].y);
        }
        public static void Polygon(SDL_Point[] points,int num=-1)
        {
            if(num==-1)
                num=points.Length;
            var tx=new short[num];
            var ty=new short[num];
            for(int i=0;i<num;i++)
            {
                tx[i]=(short)points[i].x;
                ty[i]=(short)points[i].y;
            }
            _=SDL_gfx.polygonRGBA(renderer,tx,ty,num,LineColor.r,LineColor.g,LineColor.b,LineColor.a);
        }
        public static void SolidPolygon(SDL_Point[] points,int num=-1)
        {
            if(num==-1)
                num=points.Length;
            var tx=new short[num];
            var ty=new short[num];
            for(int i=0;i<num;i++)
            {
                tx[i]=(short)points[i].x;
                ty[i]=(short)points[i].y;
            }
            _=SDL_gfx.filledPolygonRGBA(renderer,tx,ty,num,LineColor.r,LineColor.g,LineColor.b,LineColor.a);
        }
        public static void FilledPolygon(SDL_Point[] points,int num=-1)
        {
            SolidPolygon(points,num);
            Polygon(points,num);
        }
        public static void PolyBezier(SDL_Point[] points,int num=-1)
        {
            if(num==-1)
                num=points.Length;
            if((num-1)%3!=0)
                throw new ArgumentException("A cubic Bessel curve requires 4 points.");
            var tx=new short[num];
            var ty=new short[num];
            for(int i=0;i<num;i++)
            {
                tx[i]=(short)points[i].x;
                ty[i]=(short)points[i].y;
            }
            _=SDL_gfx.bezierRGBA(renderer,tx,ty,num,3,LineColor.r,LineColor.g,LineColor.b,LineColor.a);
        }
        public static void OutTextXY(int x,int y,string s)
        {
            _=SDL_ttf.TTF_SizeText(font,s,out var w,out var h);
            SDL_Rect rect=new()
            {
                x=x,
                y=y,
                w=w,
                h=h
            };
            textSurface=SDL_ttf.TTF_RenderText_Blended(font,s,TextColor);
            textTexture=SDL_CreateTextureFromSurface(renderer,textSurface);
            _=SDL_RenderCopy(renderer,textTexture,0,ref rect);
        }
        public static void OutTextXY(int x,int y,char s)
        {
            OutTextXY(x,y,s.ToString());
        }
        public static int TextWidth(string str)
        {
            _=SDL_ttf.TTF_SizeText(font,str,out var w,out _);
            return w;
        }
        public static int TextWidth(char str)
        {
            return TextWidth(str.ToString());
        }
        public static int TextHeight(string str)
        {
            _=SDL_ttf.TTF_SizeText(font,str,out _,out var h);
            return h;
        }
        public static int TextHeight(char str)
        {
            return TextHeight(str.ToString());
        }

        public static void FlushBatchDraw()
        {
            SDL_RenderPresent(renderer);
        }
        public static nint Window
        {
            get
            {
                return window;
            }
        }
        public const string EasyxVer="20220901";
        public static SDL_Color RGB(int r,int g,int b,int a=255)
        {
            return new SDL_Color()
            {
                r=(byte)r,
                g=(byte)g,
                b=(byte)b,
                a=(byte)a
            };
        }
        public static int RGB(SDL_Color rgb)
        {
            string s;
            s=string.Format("{0:X}",rgb.r)+string.Format("{0:X}",rgb.g)+string.Format("{0:X}",rgb.b)+string.Format("{0:X}",rgb.a);
            return int.Parse(s,System.Globalization.NumberStyles.HexNumber);
        }
        public static SDL_Color BGR(int hex)
        {
            string s;
            s=string.Format("{0:X}",hex);
            if(s.Length<6)
            {
                while(s.Length<6)
                    s=s.Insert(0,"0");
            }
            return RGB(int.Parse(s[..2],System.Globalization.NumberStyles.HexNumber),int.Parse(s[2..4],System.Globalization.NumberStyles.HexNumber),int.Parse(s[4..6],System.Globalization.NumberStyles.HexNumber),s.Length==8?int.Parse(s[6..8],System.Globalization.NumberStyles.HexNumber):255);
        }
        public static int GetWidth()
        {
            _=SDL_GetCurrentDisplayMode(0,out var vm);
            return vm.w;
        }
        public static int GetHeight()
        {
            _=SDL_GetCurrentDisplayMode(0,out var vm);
            return vm.h;
        }
        public struct ExMessage
        {
            public SDL_EventType message;
            public bool ctrl=false;		// Indicates whether the CTRL key is pressed
            public bool shift=false;		// Indicates whether the SHIFT key is pressed
            public bool lbutton=false;		// Indicates whether the left mouse button is pressed
            public bool mbutton=false;		// Indicates whether the middle mouse button is pressed
            public bool rbutton=false;		// Indicates whether the right mouse button is pressed
            public short x;				// The x-coordinate of the cursor
            public short y;				// The y-coordinate of the cursor
            public short wheel;
            public SDL_Keycode vkcode;			// The virtual-key code of the key
            public SDL_Scancode scancode;			// The scan code of the key. The value depends on the OEM
            public bool extended=false;		// Indicates whether the key is an extended key, such as a function key or a key on the numeric keypad. The value is false if the key is an extended key; otherwise, it is false.
            public bool prevdown=false;		// Indicates whether the key is previously up or down
            public ExMessage(){}
            public void Get()
            {
                while(SDL_PollEvent(out SDL_Event e)!=0)
                {
                    prevdown=lbutton||mbutton||rbutton;
                    message=e.type;
                    ctrl=e.key.keysym.sym==SDL_Keycode.SDLK_LCTRL||e.key.keysym.sym==SDL_Keycode.SDLK_RCTRL;
                    shift=e.key.keysym.sym==SDL_Keycode.SDLK_LSHIFT||e.key.keysym.sym==SDL_Keycode.SDLK_RSHIFT;
                    lbutton=e.type==SDL_EventType.SDL_MOUSEBUTTONDOWN&&e.button.button==SDL_BUTTON_LEFT;
                    mbutton=e.type==SDL_EventType.SDL_MOUSEBUTTONDOWN&&e.button.button==SDL_BUTTON_MIDDLE;
                    rbutton=e.type==SDL_EventType.SDL_MOUSEBUTTONDOWN&&e.button.button==SDL_BUTTON_RIGHT;
                    x=(short)e.button.x;
                    y=(short)e.button.y;
                    wheel=(short)e.wheel.y;
                    vkcode=e.key.keysym.sym;
                    scancode=e.key.keysym.scancode;
                }
            }
            public bool Peek()
            {
                bool res=SDL_PollEvent(out SDL_Event e)!=0;
                prevdown=lbutton||mbutton||rbutton;
                message=e.type;
                ctrl=e.key.keysym.sym==SDL_Keycode.SDLK_LCTRL||e.key.keysym.sym==SDL_Keycode.SDLK_RCTRL;
                shift=e.key.keysym.sym==SDL_Keycode.SDLK_LSHIFT||e.key.keysym.sym==SDL_Keycode.SDLK_RSHIFT;
                lbutton=e.type==SDL_EventType.SDL_MOUSEBUTTONDOWN&&e.button.button==SDL_BUTTON_LEFT;
                mbutton=e.type==SDL_EventType.SDL_MOUSEBUTTONDOWN&&e.button.button==SDL_BUTTON_MIDDLE;
                rbutton=e.type==SDL_EventType.SDL_MOUSEBUTTONDOWN&&e.button.button==SDL_BUTTON_RIGHT;
                x=(short)e.button.x;
                y=(short)e.button.y;
                wheel=(short)e.wheel.y;
                vkcode=e.key.keysym.sym;
                scancode=e.key.keysym.scancode;
                return res;
            }
        }
        public static void Getch()
        {
            ExMessage message=new();
            do
            {
                message.Get();
            }
            while(message.message==SDL_EventType.SDL_KEYDOWN);
        }
    }
}

