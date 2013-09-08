#include <iostream>

#include <boost/filesystem.hpp>

#include <opencv2/core/core.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/video/tracking.hpp>
#include <opencv2/highgui/highgui.hpp>

#include "Mojo.Core/Assert.hpp"
#include "Mojo.Core/Printf.hpp"

static const double PYRAMID_SCALING                       = 0.5;
static const int    NUM_PYRAMID_LEVELS                    = 10; 
static const int    WINDOW_SIZE_FOR_COMPUTING_AVERAGES    = 40;
static const int    NUM_ITERATIONS_PER_PYRAMID_LEVEL      = 50;
static const int    NEIGHBORHOOD_SIZE                     = 7;
static const double GAUSSIAN_STANDARD_DEVIATION           = 1.5;

static const float  OPTICAL_FLOW_VISUALIZATION_MULTIPLIER = 25.0f;

cv::Mat CalculateOpticalFlowMap( cv::Mat prevImage, cv::Mat nextImage );
cv::Mat CalculateOpticalFlowVisualization( cv::Mat opticalFlowColorMap, cv::Mat opticalFlowMap );
cv::Mat CalculateAlignedImage( cv::Mat originalImage, cv::Mat opticalFlowMap );

void SaveOpticalFlowMap( boost::filesystem::path opticalFlowMapPath, cv::Mat opticalFlowMap );
void SaveImage( boost::filesystem::path path, cv::Mat image );

int main( int argc, char* argv[] )
{
    if ( argc != 4 )
    {
        Mojo::Core::Printf( "Usage: OpticalFlowCalculator.exe source_directory forward_destination_directory backward_destination_directory\n\nPress any key to exit." );
        getchar();
        return -1;
    }

    try
    {
        boost::filesystem::path inputPath( boost::filesystem::complete( argv[ 1 ] ) );
        boost::filesystem::path outputForwardPath( boost::filesystem::complete( argv[ 2 ] ) );
        boost::filesystem::path outputBackwardPath( boost::filesystem::complete( argv[ 3 ] ) );
        boost::filesystem::path outputPath;
        boost::filesystem::path opticalFlowColorMapPath;
        boost::filesystem::directory_iterator prevFile;
        boost::filesystem::directory_iterator nextFile;

        RELEASE_ASSERT( boost::filesystem::exists( "OpticalFlowColorMap.png" ) );

        cv::Mat opticalFlowColorMap = cv::imread( "OpticalFlowColorMap.png" );

        RELEASE_ASSERT( boost::filesystem::exists( inputPath ) );
        RELEASE_ASSERT( boost::filesystem::is_directory( inputPath ) );
        RELEASE_ASSERT( !boost::filesystem::exists( outputForwardPath ) || boost::filesystem::is_directory( outputForwardPath ) );
        RELEASE_ASSERT( !boost::filesystem::exists( outputBackwardPath ) || boost::filesystem::is_directory( outputBackwardPath ) );

        Mojo::Core::Printf( "Calculating optical flow for all consecutive image pairs in:\n\n    ", inputPath.native_directory_string(), "\n" );
        Mojo::Core::Printf( "Storing forward optical flow output in:\n\n    ", outputForwardPath.native_directory_string(), "\n" );
        outputPath                  = outputForwardPath;
        opticalFlowColorMapPath     = boost::filesystem::complete( outputPath / "OpticalFlowColorMap.png" );

        if ( !boost::filesystem::exists( outputPath ) )
        {
            boost::filesystem::create_directory( outputPath );
        }

        if ( !boost::filesystem::exists( opticalFlowColorMapPath ) )
        {
            boost::filesystem::copy_file( "OpticalFlowColorMap.png", opticalFlowColorMapPath );
        }

        prevFile = boost::filesystem::directory_iterator( inputPath );
        nextFile = boost::filesystem::directory_iterator( inputPath );

        ++nextFile;

        for (; nextFile != boost::filesystem::directory_iterator(); )
        {
            boost::filesystem::path prevFilePath( prevFile->path() );
            boost::filesystem::path nextFilePath( nextFile->path() );

            if ( boost::filesystem::is_regular_file( prevFilePath ) && boost::filesystem::is_regular_file( nextFilePath ) )
            {
                Mojo::Core::Printf( "Calculating optical flow from ", prevFilePath.filename(), " to ", nextFilePath.filename() );

                boost::filesystem::path opticalFlowMapPath           = outputPath / boost::filesystem::path( prevFilePath.stem() + "-to-"         + nextFilePath.stem() + ".raw" );
                boost::filesystem::path opticalFlowVisualizationPath = outputPath / boost::filesystem::path( prevFilePath.stem() + "-to-"         + nextFilePath.stem() + ".png" );
                boost::filesystem::path alignedImagePath             = outputPath / boost::filesystem::path( prevFilePath.stem() + "-aligned-to-" + nextFilePath.stem() + ".png" );

                cv::Mat prevImage                = cv::imread( prevFilePath.native_file_string() );
                cv::Mat nextImage                = cv::imread( nextFilePath.native_file_string() );
                cv::Mat opticalFlowMap           = CalculateOpticalFlowMap( prevImage, nextImage );
                cv::Mat opticalFlowVisualization = CalculateOpticalFlowVisualization( opticalFlowColorMap, opticalFlowMap );
                cv::Mat alignedImage             = CalculateAlignedImage( prevImage, opticalFlowMap );

                SaveOpticalFlowMap( opticalFlowMapPath, opticalFlowMap );
                SaveImage( opticalFlowVisualizationPath, opticalFlowVisualization );
                SaveImage( alignedImagePath, alignedImage );
            }

            ++prevFile;
            ++nextFile;
        }

        Mojo::Core::Printf( "\nStoring backward optical flow output in:\n\n    ", outputBackwardPath.native_directory_string(), "\n" );
        outputPath = outputBackwardPath;
        opticalFlowColorMapPath = boost::filesystem::complete( outputPath / "OpticalFlowColorMap.png" );


        if ( !boost::filesystem::exists( outputPath ) )
        {
            boost::filesystem::create_directory( outputPath );
        }

        if ( !boost::filesystem::exists( opticalFlowColorMapPath ) )
        {
            boost::filesystem::path opticalFlowColorMapPath( boost::filesystem::complete( outputPath / "OpticalFlowColorMap.png" ) );
            boost::filesystem::copy_file( "OpticalFlowColorMap.png", opticalFlowColorMapPath );
        }

        prevFile = boost::filesystem::directory_iterator( inputPath );
        nextFile = boost::filesystem::directory_iterator( inputPath );

        ++nextFile;

        for (; nextFile != boost::filesystem::directory_iterator(); )
        {
            boost::filesystem::path prevFilePath( prevFile->path() );
            boost::filesystem::path nextFilePath( nextFile->path() );

            if ( boost::filesystem::is_regular_file( prevFilePath ) && boost::filesystem::is_regular_file( nextFilePath ) )
            {
                Mojo::Core::Printf( "Calculating optical flow from ", nextFilePath.filename(), " to ", prevFilePath.filename() );

                boost::filesystem::path opticalFlowMapPath           = outputPath / boost::filesystem::path( nextFilePath.stem() + "-to-"         + prevFilePath.stem() + ".raw" );
                boost::filesystem::path opticalFlowVisualizationPath = outputPath / boost::filesystem::path( nextFilePath.stem() + "-to-"         + prevFilePath.stem() + ".png" );
                boost::filesystem::path alignedImagePath             = outputPath / boost::filesystem::path( nextFilePath.stem() + "-aligned-to-" + prevFilePath.stem() + ".png" );

                cv::Mat prevImage                = cv::imread( prevFilePath.native_file_string() );
                cv::Mat nextImage                = cv::imread( nextFilePath.native_file_string() );
                cv::Mat opticalFlowMap           = CalculateOpticalFlowMap( nextImage, prevImage );
                cv::Mat opticalFlowVisualization = CalculateOpticalFlowVisualization( opticalFlowColorMap, opticalFlowMap );
                cv::Mat alignedImage             = CalculateAlignedImage( nextImage, opticalFlowMap );

                SaveOpticalFlowMap( opticalFlowMapPath, opticalFlowMap );
                SaveImage( opticalFlowVisualizationPath, opticalFlowVisualization );
                SaveImage( alignedImagePath, alignedImage );
            }

            ++prevFile;
            ++nextFile;
        }
    }
    catch( std::exception e )
    {
        Mojo::Core::Printf( "Exception: ", e.what(), "\nPress any key to exit." );
        getchar();
        return -1;
    }

    Mojo::Core::Printf( "\nPress any key to exit." );
    getchar();
    return 0;
}

cv::Mat CalculateOpticalFlowMap( cv::Mat prevImage, cv::Mat nextImage )
{
    RELEASE_ASSERT( !prevImage.empty() );
    RELEASE_ASSERT( !nextImage.empty() );

    std::vector<cv::Mat> imagePrevChannels;
    std::vector<cv::Mat> imageNextChannels;

    cv::split( prevImage, imagePrevChannels );
    cv::split( nextImage, imageNextChannels );

    cv::Mat prevImageR = imagePrevChannels[0];
    cv::Mat nextImageR = imageNextChannels[0];

    RELEASE_ASSERT( !prevImageR.empty() );
    RELEASE_ASSERT( !nextImageR.empty() );
    RELEASE_ASSERT( prevImageR.data != 0 );
    RELEASE_ASSERT( nextImageR.data != 0 );
    RELEASE_ASSERT( prevImageR.size() == nextImageR.size() );
    RELEASE_ASSERT( prevImageR.channels() == 1 );
    RELEASE_ASSERT( nextImageR.channels() == 1 );   

    cv::Mat opticalFlowMap;

    cv::calcOpticalFlowFarneback(
        prevImageR,
        nextImageR,
        opticalFlowMap,
        PYRAMID_SCALING,                    // scaling for image pyramid
        NUM_PYRAMID_LEVELS,                 // number of pyramid levels used
        WINDOW_SIZE_FOR_COMPUTING_AVERAGES, // window size for computing averages. larger = blurrier motion field
        NUM_ITERATIONS_PER_PYRAMID_LEVEL,   // number of iterations at each pyramid level
        NEIGHBORHOOD_SIZE,                  // size of pixel neighborhood
        GAUSSIAN_STANDARD_DEVIATION,        // standard deviation of the Gaussian that is used to smooth derivatives that are used as a basis for the polynomial expansion
        cv::OPTFLOW_FARNEBACK_GAUSSIAN );

    return opticalFlowMap;
}

cv::Mat CalculateOpticalFlowVisualization( cv::Mat opticalFlowColorMap, cv::Mat opticalFlowMap )
{
    std::vector<cv::Mat> opticalFlowMapChannels;
    cv::split( opticalFlowMap, opticalFlowMapChannels );

    cv::Vec2d minValue, maxValue;
    cv::minMaxLoc( opticalFlowMapChannels[ 0 ], &minValue[ 0 ], &maxValue[ 0 ] );
    cv::minMaxLoc( opticalFlowMapChannels[ 1 ], &minValue[ 1 ], &maxValue[ 1 ] );

    cv::Mat opticalFlowVisualizationMap( opticalFlowMap.rows, opticalFlowMap.cols, CV_8UC3 );

    for ( int y = 0; y < opticalFlowMap.rows; y++ )
    {
        for ( int x = 0; x < opticalFlowMap.cols; x++ )
        {
            cv::Vec2f opticalFlow = opticalFlowMap.at< cv::Vec2f >( y, x ) * OPTICAL_FLOW_VISUALIZATION_MULTIPLIER;
            cv::Vec2i opticalFlowColorMapIndex( cv::saturate_cast< signed char >( opticalFlow[ 0 ] ) + 128, cv::saturate_cast< signed char >( opticalFlow[ 1 ] ) + 128 );

            opticalFlowVisualizationMap.at< cv::Vec3b >( y, x ) = opticalFlowColorMap.at< cv::Vec3b >( opticalFlowColorMapIndex[ 1 ], opticalFlowColorMapIndex[ 0 ] );
        }
    }

    return opticalFlowVisualizationMap;
}

cv::Mat CalculateAlignedImage( cv::Mat originalImage, cv::Mat opticalFlowMap )
{
    RELEASE_ASSERT( !originalImage.empty() );
    RELEASE_ASSERT( !opticalFlowMap.empty() );
    RELEASE_ASSERT( opticalFlowMap.data != 0 );
    RELEASE_ASSERT( originalImage.size() == opticalFlowMap.size() );

    std::vector<cv::Mat> originalImageChannels;
    cv::split( originalImage, originalImageChannels );
    cv::Mat originalImageR = originalImageChannels[0];

    RELEASE_ASSERT( !originalImageR.empty() );
    RELEASE_ASSERT( originalImageR.data != 0 );
    RELEASE_ASSERT( originalImageR.channels() == 1 );

    cv::Mat alignedImage( opticalFlowMap.rows, opticalFlowMap.cols, CV_8UC1 );

    for ( int y = 0; y < opticalFlowMap.rows; y++ )
    {
        for ( int x = 0; x < opticalFlowMap.cols; x++ )
        {
            alignedImage.at< unsigned char >( y, x ) = 127;
        }
    }

    for ( int y = 0; y < opticalFlowMap.rows; y++ )
    {
        for ( int x = 0; x < opticalFlowMap.cols; x++ )
        {
            unsigned char originalColor = originalImageR.at< unsigned char >( y, x );
            cv::Vec2f     opticalFlow   = opticalFlowMap.at< cv::Vec2f >( y, x );

            cv::Vec2i newCoords( (int)floor( x + opticalFlow[ 0 ] ), (int)floor( y + opticalFlow[ 1 ] ) );

            if ( newCoords[ 1 ] >= 0 && newCoords[ 1 ] < opticalFlowMap.rows &&
                 newCoords[ 0 ] >= 0 && newCoords[ 0 ] < opticalFlowMap.cols )
            {
                alignedImage.at< unsigned char >( newCoords[ 1 ], newCoords[ 0 ] ) = originalColor;
            }
        }
    }

    return alignedImage;
}

void SaveOpticalFlowMap( boost::filesystem::path opticalFlowMapPath, cv::Mat opticalFlowMap )
{
    std::ofstream file( opticalFlowMapPath.native_file_string().c_str(), std::ios::binary );
    file.write( (char*)opticalFlowMap.data, opticalFlowMap.rows * opticalFlowMap.cols * sizeof( cv::Vec2f ) );
    file.close();
}

void SaveImage( boost::filesystem::path path, cv::Mat image )
{
    cv::imwrite( path.native_file_string(), image );
}
