#include <iostream>

#include <boost/filesystem.hpp>
#include <boost/lexical_cast.hpp>

#include <opencv2/core/core.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/video/tracking.hpp>
#include <opencv2/highgui/highgui.hpp>

#include "Mojo.Core/Assert.hpp"
#include "Mojo.Core/Printf.hpp"

int main( int argc, char* argv[] )
{
    if ( argc != 7 )
    {
        Mojo::Core::Printf( "Usage: RawToPngCalculator.exe source_file destination_directory width height num_images num_bytes_per_element_in_raw_file\n\nPress any key to exit." );
        getchar();
        return -1;
    }

    try
    {
        boost::filesystem::path inputPath( boost::filesystem::complete( argv[ 1 ] ) );
        boost::filesystem::path outputDirectoryPath( boost::filesystem::complete( argv[ 2 ] ) );

        int width                       = boost::lexical_cast< int >( argv[ 3 ] );
        int height                      = boost::lexical_cast< int >( argv[ 4 ] );
        int numImages                   = boost::lexical_cast< int >( argv[ 5 ] );
        int numBytesPerElementInRawFile = boost::lexical_cast< int >( argv[ 6 ] );

        RELEASE_ASSERT( boost::filesystem::exists( inputPath ) );
        RELEASE_ASSERT( !boost::filesystem::is_directory( inputPath ) );
        RELEASE_ASSERT( !boost::filesystem::exists( outputDirectoryPath ) || boost::filesystem::is_directory( outputDirectoryPath ) );
        RELEASE_ASSERT( width                       > 0 );
        RELEASE_ASSERT( height                      > 0 );
        RELEASE_ASSERT( numImages                   > 0 );
        RELEASE_ASSERT( numBytesPerElementInRawFile > 0 );

        std::ifstream file( inputPath.native_file_string().c_str(), std::ios::binary );

        file.seekg ( 0, std::ios::end );
        std::streamoff fileLength = file.tellg();
        file.seekg ( 0, std::ios::beg );
        RELEASE_ASSERT( fileLength == width * height * numImages * numBytesPerElementInRawFile );


        float* rawBuffer = new float [ width * height * numImages ];
        file.read ( (char*)rawBuffer, fileLength );
        file.close();

        if ( !boost::filesystem::exists( outputDirectoryPath ) )
        {
            boost::filesystem::create_directory( outputDirectoryPath );
        }

        float* currentRawValue = rawBuffer;
        for ( int i = 0; i < numImages; i++ )
        {
            cv::Mat image( width, height, CV_8UC3 );

            for( int y = 0; y < height; y++ )
            {
                for( int x = 0; x < width; x++ )
                {
                    image.at< cv::Vec3b >( y, x ) = cv::Vec3b( (unsigned char)floor( *currentRawValue * 255.0f ), (unsigned char)floor( *currentRawValue * 255.0f ), (unsigned char)floor( *currentRawValue * 255.0f ) );

                    currentRawValue++;
                }
            }

            if ( i < 10 )
            {
                boost::filesystem::path outputFilePath( outputDirectoryPath / Mojo::Core::ToString( "000", i, ".png" ) );
                cv::imwrite( outputFilePath.native_file_string(), image );
            }
            else
            if ( i < 100 )
            {
                boost::filesystem::path outputFilePath( outputDirectoryPath / Mojo::Core::ToString( "00", i, ".png" ) );
                cv::imwrite( outputFilePath.native_file_string(), image );
            }
            else
            if ( i < 1000 )
            {
                boost::filesystem::path outputFilePath( outputDirectoryPath / Mojo::Core::ToString( "0", i, ".png" ) );
                cv::imwrite( outputFilePath.native_file_string(), image );
            }
            else
            if ( i < 10000 )
            {
                boost::filesystem::path outputFilePath( outputDirectoryPath / Mojo::Core::ToString( i, ".png" ) );
                cv::imwrite( outputFilePath.native_file_string(), image );
            }
            else
            {
                RELEASE_ASSERT( 0 );
            }
        }
    }
    catch( std::exception e )
    {
        Mojo::Core::Printf( "Exception: ", e.what() );
        getchar();
        return -1;
    }

    return 0;
}
