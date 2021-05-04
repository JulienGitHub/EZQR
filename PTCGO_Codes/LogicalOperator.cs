using System;
using System.Drawing;
using System.Drawing.Imaging;


namespace PTCGO_Codes
{
	/// <summary>
	/// Summary description for LogicalOperator.
	/// </summary>
	public class LogicalOperator
	{
		private Bitmap bmpimg;
		public LogicalOperator()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public void setImage( Bitmap bmp )
		{
			bmpimg = ( Bitmap )bmp.Clone( );
		}

		public Bitmap getImage( )
		{
			return ( Bitmap )bmpimg.Clone( );
		}

		public void binary_Dilation( byte[ , ] sele)
		{
			Bitmap tempbmp = ( Bitmap ) this.bmpimg.Clone( );
			BitmapData data2 = tempbmp.LockBits( new Rectangle( 0 , 0 , tempbmp.Width , tempbmp.Height ), ImageLockMode.ReadWrite , PixelFormat.Format24bppRgb );
			BitmapData data = bmpimg.LockBits( new Rectangle( 0 , 0 , bmpimg.Width , bmpimg.Height ), ImageLockMode.ReadWrite , PixelFormat.Format24bppRgb );
			byte [ , ] sElement = sele;

			unsafe
			{
				byte* ptr = ( byte* )data.Scan0;
				byte* tptr = ( byte* )data2.Scan0;

				ptr += data.Stride + 3;
				tptr += data.Stride + 3;

				int remain = data.Stride - data.Width * 3;

				for( int i = 1 ; i < data.Height - 1 ; i ++ )
				{
					for( int j = 1 ; j < data.Width - 1 ; j ++ )
					{
                            
						if( ptr[ 0 ] != 255 )
						{	

							byte* temp = ptr - data.Stride - 3;
							bool condition = false;

							for( int k = 0 ; k < 3 ; k ++ )
							{
								for( int l = 0 ; l < 3 ; l ++ )
								{

									if( sElement[ k , l ] == 1 && temp[ 0 ] == 255 )
									{
										condition = true;
									}

									temp += 3;

								}

								temp += data.Stride - 9;

							}
							if( condition )
							{
								tptr[ 0 ] = tptr[ 1 ] = tptr[ 2 ] = 255;
							}
						}

						ptr += 3;
						tptr += 3;

					}

					ptr += remain + 6;
					tptr += remain + 6;

				}
			}
			
			bmpimg.UnlockBits( data );  
			tempbmp.UnlockBits( data2 );

			bmpimg = ( Bitmap )tempbmp.Clone( );
		}


		public void gray_Dilation( byte[ , ] sele )
		{
			Bitmap tempbmp = ( Bitmap ) this.bmpimg.Clone( );

			BitmapData data2 = tempbmp.LockBits( new Rectangle( 0 , 0 , tempbmp.Width , tempbmp.Height ), ImageLockMode.ReadWrite , PixelFormat.Format24bppRgb );
			BitmapData data = bmpimg.LockBits( new Rectangle( 0 , 0 , bmpimg.Width , bmpimg.Height ), ImageLockMode.ReadWrite , PixelFormat.Format24bppRgb );
			
			byte [ , ] sElement = sele;


			unsafe
			{
				byte* ptr = ( byte* )data.Scan0;
				byte* tptr = ( byte* )data2.Scan0;

				ptr += data.Stride + 3;
				tptr += data.Stride + 3;

				int remain = data.Stride - data.Width * 3;

				for( int i = 1 ; i < data.Height - 1 ; i ++ )
				{
					for( int j = 1 ; j < data.Width - 1 ; j ++ )
					{
                                 
						
						byte* temp = ptr - data.Stride - 3;
						byte min = 255;

						for( int k = 0 ; k < 3 ; k ++ )
						{
							for( int l = 0 ; l < 3 ; l ++ )
							{
								if( min > temp[ data.Stride * k + l * 3 ] )
								{
									if( sElement[ k , l ] != 0 )
									    min = temp[ data.Stride * k + l * 3 ];
								}
							}								
						}
						   
						tptr[ 0 ] = tptr[ 1 ] = tptr[ 2 ] = min;    
						

						ptr += 3;
						tptr += 3;
					}
					ptr += remain + 6;
					tptr += remain + 6;
				}
			}
			
			bmpimg.UnlockBits( data );  
			tempbmp.UnlockBits( data2 );

			bmpimg = ( Bitmap )tempbmp.Clone( );
		}

		public void thresh( )
		{
			BitmapData data = bmpimg.LockBits( new Rectangle( 0 , 0 , bmpimg.Width , bmpimg.Height ), ImageLockMode.ReadWrite , PixelFormat.Format24bppRgb );
			unsafe
			{
				byte* ptr = ( byte* )data.Scan0;
				int remain = data.Stride - data.Width * 3;

				for( int i = 0 ; i < data.Height ; i ++ )
				{
					for( int j = 0 ; j < data.Width ; j ++ )
					{
						ptr[ 0 ] = ptr[ 1 ] = ptr[ 2 ] = ( byte )( ( ptr[ 0 ] + ptr[ 1 ] + ptr[ 2 ] ) / 3 );

						ptr += 3;
					}
					ptr += remain;
				}
			}
			bmpimg.UnlockBits( data );
		}


	}

}
