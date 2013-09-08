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
    if ( argc != 6 )
    {
        Mojo::Core::Printf( "Usage: DiceScoreCalculator.exe ground_truth_directory segmentation_directory width height num_images\n\nPress any key to exit." );
        getchar();
        return -1;
    }

    try
    {
        boost::filesystem::path gtDir( boost::filesystem::complete( argv[ 1 ] ) );
        boost::filesystem::path sDir ( boost::filesystem::complete( argv[ 2 ] ) );

        int width                       = boost::lexical_cast< int >( argv[ 3 ] );
        int height                      = boost::lexical_cast< int >( argv[ 4 ] );
        int numImages                   = boost::lexical_cast< int >( argv[ 5 ] );

        RELEASE_ASSERT( boost::filesystem::exists( gtDir) );
        RELEASE_ASSERT( boost::filesystem::is_directory( gtDir ) );
        RELEASE_ASSERT( boost::filesystem::exists( sDir) );
        RELEASE_ASSERT( boost::filesystem::is_directory( sDir ) );
        RELEASE_ASSERT( width                       > 0 );
        RELEASE_ASSERT( height                      > 0 );
        RELEASE_ASSERT( numImages                   > 0 );

        boost::filesystem::directory_iterator gtFile;
        boost::filesystem::directory_iterator sFile;

        gtFile = boost::filesystem::directory_iterator( gtDir );
        sFile  = boost::filesystem::directory_iterator( sDir );

        int FP = 0;
        int TP = 0;
        int FN = 0;
        int TN = 0;

        for (; gtFile != boost::filesystem::directory_iterator(); )
        {
            boost::filesystem::path gtFilePath( gtFile->path() );
            boost::filesystem::path sFilePath( sFile->path() );

            if ( boost::filesystem::is_regular_file( gtFilePath ) && boost::filesystem::is_regular_file( sFilePath ) )
            {
                cv::Mat gtImage = cv::imread( gtFilePath.native_file_string() );
                cv::Mat sImage  = cv::imread( sFilePath.native_file_string() );

                std::vector<cv::Mat> gtImageChannels;
                std::vector<cv::Mat> sImageChannels;

                cv::split( gtImage, gtImageChannels );
                cv::split( sImage, sImageChannels );

                cv::Mat gtImageR = gtImageChannels[2];
                cv::Mat gtImageG = gtImageChannels[1];
                cv::Mat gtImageB = gtImageChannels[0];

                cv::Mat sImageR = sImageChannels[0];

                for( int y = 0; y < height; y++ )
                {
                    for( int x = 0; x < width; x++ )
                    {
                        if ( ( gtImageR.at< unsigned char >( y, x ) >  0   ) && ( gtImageG.at< unsigned char >( y, x ) == 0 ) && ( gtImageB.at< unsigned char >( y, x ) == 0 ) && // GROUND TRUTH == POSITIVE
                             ( sImageR.at < unsigned char >( y, x ) >= 1 ) )                                                                                                      // SOURCE       == POSITIVE
                        {
                            TP++;
                        }
                        else
                        if ( ( gtImageR.at< unsigned char >( y, x ) > 0  ) && ( gtImageG.at< unsigned char >( y, x ) == 0 ) && ( gtImageB.at< unsigned char >( y, x ) == 0 ) && // GROUND TRUTH == POSITIVE
                             ( sImageR.at < unsigned char >( y, x ) < 1 ) )                                                                                                     // SOURCE       == NEGATIVE
                        {
                            FN++;
                        }
                        else
                        if ( ( gtImageR.at< unsigned char >( y, x ) == 0   ) && ( gtImageG.at< unsigned char >( y, x ) == 0 ) && ( gtImageB.at< unsigned char >( y, x ) == 0 ) && // GROUND TRUTH == NEGATIVE
                             ( sImageR.at < unsigned char >( y, x ) >= 1 ) )                                                                                                      // SOURCE       == POSITIVE
                        {
                            FP++;
                        }
                        else
                        if ( ( gtImageR.at< unsigned char >( y, x ) == 0   ) && ( gtImageG.at< unsigned char >( y, x ) == 0 ) && ( gtImageB.at< unsigned char >( y, x ) == 0 ) && // GROUND TRUTH == NEGATIVE
                             ( sImageR.at < unsigned char >( y, x ) <  1 ) )                                                                                                      // SOURCE       == NEGATIVE
                        {
                            TN++;
                        }
                        else
                        {
                            int gtr = gtImageR.at< unsigned char >( y, x );
                            int gtg = gtImageB.at< unsigned char >( y, x );
                            int gtb = gtImageG.at< unsigned char >( y, x );
                            int sr  = gtImageR.at< unsigned char >( y, x );

                            Mojo::Core::Printf( "gtr ", gtr, ", gtg ", gtg, ", gtb ", gtb, ", sr ", sr );

                            RELEASE_ASSERT( 0 );
                        }
                    }
                }
            }

            ++gtFile;
            ++sFile;
        }

        RELEASE_ASSERT( TP + FP + TN + FN == width * height * numImages );

        Mojo::Core::Printf( "TP: ", TP );            
        Mojo::Core::Printf( "FP: ", FP );            
        Mojo::Core::Printf( "TN: ", TN );
        Mojo::Core::Printf( "FN: ", FN );
    }
    catch( std::exception e )
    {
        Mojo::Core::Printf( "Exception: ", e.what() );
        return -1;
    }

    return 0;
}
