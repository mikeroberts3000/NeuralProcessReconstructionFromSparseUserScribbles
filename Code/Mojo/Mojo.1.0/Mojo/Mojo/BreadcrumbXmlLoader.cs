using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DrWPF.Windows.Data;
using Microsoft.Xml.Serialization.GeneratedAssembly;
using Mojo.Interop;
using Mojo.Xml;
using SlimDX;

namespace Mojo
{
    internal class BreadcrumbXmlLoader
    {
        private static readonly char[] TRIM_CHARS = new[] { 'c', 'o', 'n', 't', 'o', 'u', 'r', '-', '.', 'x', 'm', 'l' };

        public ObservableDictionary< int,NeuralProcessDescription > LoadDataset( BreadcrumbXmlLoadDescription breadcrumbXmlLoadDescription )
        {
            var breadcrumbTrailCollectionDirectory = breadcrumbXmlLoadDescription.BreadcrumbTrailCollectionDirectory;

            var breadcrumbCollections = from fileInfo in new DirectoryInfo( breadcrumbTrailCollectionDirectory ).GetFiles( "*.*" )
                                        select new
                                               {
                                                   SectionId = int.Parse( fileInfo.Name.Trim( TRIM_CHARS ) ),
                                                   BreadcrumbCollection =
                                                       XmlReader.ReadFromFile< contours, contoursSerializer >(
                                                           Path.Combine( breadcrumbTrailCollectionDirectory, fileInfo.Name ) )
                                               };

            Release.Assert( breadcrumbCollections.Count() > 0 );
            Release.Assert( breadcrumbCollections.All( breadcrumbCollection => breadcrumbCollection.BreadcrumbCollection.contour.All( breadcrumb => breadcrumb.@class.Equals( "PointContour" ) ) ) );

            var distinctNeuralProcessNames = ( from breadcrumbCollection in breadcrumbCollections
                                               from breadcrumb in breadcrumbCollection.BreadcrumbCollection.contour
                                               select breadcrumb.name ).Distinct();

            var distinctNeuralProcessIds = Enumerable.Range( 1, distinctNeuralProcessNames.Count() + 1 );
            var distinctNeuralProcesses = distinctNeuralProcessIds.Zip( distinctNeuralProcessNames, ( id, name ) => new { Id = id, Name = name } );

            var tmpBreadcrumbDescriptions = from breadcrumbCollection in breadcrumbCollections
                                            from breadcrumb in breadcrumbCollection.BreadcrumbCollection.contour
                                            select new
                                            {
                                                Name = breadcrumb.name,
                                                Color = new Vector3(
                                                    int.Parse( breadcrumb.color.Substring( 1, 2 ), NumberStyles.HexNumber ),
                                                    int.Parse( breadcrumb.color.Substring( 3, 2 ), NumberStyles.HexNumber ),
                                                    int.Parse( breadcrumb.color.Substring( 5, 2 ), NumberStyles.HexNumber ) ),
                                                BreadcrumbDescription = new BreadcrumbDescription
                                                {
                                                    Position = new Vector3(
                                                        breadcrumb.value.point.x - breadcrumbXmlLoadDescription.BreadcrumbTrailCoordinateOffset.X,
                                                        breadcrumb.value.point.y - breadcrumbXmlLoadDescription.BreadcrumbTrailCoordinateOffset.Y,
                                                        breadcrumbCollection.SectionId - breadcrumbXmlLoadDescription.BreadcrumbTrailCoordinateOffset.Z )
                                                }
                                            };

            var neuralProcesssToRemove = Constants.REMOVE_BAD_PROCESSES ? Constants.BadProcesses : new List<string>();
            var neuralProcessDescriptions = new ObservableDictionary< int, NeuralProcessDescription >(
                                                    ( from distinctNeuralProcess in distinctNeuralProcesses
                                                      join breadcrumbDescription in tmpBreadcrumbDescriptions
                                                          on distinctNeuralProcess.Name equals breadcrumbDescription.Name
                                                          into breadcrumbDescriptionsWithTheSameName
                                                      where !neuralProcesssToRemove.Contains( distinctNeuralProcess.Name )
                                                      select new NeuralProcessDescription( distinctNeuralProcess.Id )
                                                             {
                                                                 Name = distinctNeuralProcess.Name,
                                                                 Color = breadcrumbDescriptionsWithTheSameName.First().Color,
                                                                 BreadcrumbDescriptions =
                                                                     ( from breadcrumbDescription in breadcrumbDescriptionsWithTheSameName
                                                                       select breadcrumbDescription.BreadcrumbDescription ).ToList()
                                                             } ).ToDictionary(
                                                                 neuralProcessDescription => neuralProcessDescription.Id,
                                                                 neuralProcessDescription => neuralProcessDescription ) );

            return neuralProcessDescriptions;
        }
    }
}