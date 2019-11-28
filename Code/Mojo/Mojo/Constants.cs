using System.Collections.Generic;
using Mojo.Interop;
using SlimDX;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantDefaultFieldInitializer
namespace Mojo
{
    public enum RecordingMode
    {
        NotRecording = 0,
        RecordingWithSoftConstraintsVisible = 1,
        RecordingWithSoftConstraintsInvisible = 2
    }

    public enum ToolMode
    {
        Breadcrumber2D,
        Breadcrumber3D,
        Segmenter2D
    }

    public static class Constants
    {
        public static readonly PrimitiveDictionary Parameters =
            new PrimitiveDictionary
            {
                { PrimitiveType.Float4, "DUAL_MAP_INITIAL_VALUE", new Vector4( 0f, 0f, 0f, 0f ) },                 
                                                                                                                   
                { PrimitiveType.Float2, "OPTICAL_FLOW_MAP_INITIAL_VALUE", new Vector2( -1f, -1f ) },               
                                                                                                                   
                { PrimitiveType.UChar4, "COLOR_MAP_INITIAL_VALUE", new Vector4( 0f, 0f, 0f, 255f ) },       

                { PrimitiveType.Float, "MAX_CONVERGENCE_GAP", 999999f },                                           
                { PrimitiveType.Float, "MAX_CONVERGENCE_GAP_DELTA", 999999f },                                     

                { PrimitiveType.Float, "PRIMAL_MAP_INITIAL_VALUE", 0f },                                           
                { PrimitiveType.Float, "PRIMAL_MAP_FOREGROUND", 1f },                                              
                { PrimitiveType.Float, "PRIMAL_MAP_BACKGROUND", 0f },                                              
                { PrimitiveType.Float, "PRIMAL_MAP_THRESHOLD", 0.4f },                                             
                { PrimitiveType.Float, "PRIMAL_MAP_ERODE_NUM_PASSES", 5f },

                { PrimitiveType.Float, "OLD_PRIMAL_MAP_INITIAL_VALUE", 0f },                                       

                { PrimitiveType.Float, "EDGE_MAP_INITIAL_VALUE", 0f },                                             
                { PrimitiveType.Float, "EDGE_POWER_XY", 0.45f },                                                   
                { PrimitiveType.Float, "EDGE_MULTIPLIER", 10f },                                                   
                { PrimitiveType.Float, "EDGE_MAX_BEFORE_SATURATE", 0.4f },
                { PrimitiveType.Float, "EDGE_SPLIT_BOOST", 0.9f },

                { PrimitiveType.Float, "EDGE_STRENGTH_Z", 0.03f },                                                 
                { PrimitiveType.Float, "EDGE_POWER_Z", 1f },                                                       

                { PrimitiveType.Float, "STENCIL_MAP_INITIAL_VALUE", 0f },                                         
                { PrimitiveType.Float, "STENCIL_MAP_BACKGROUND_VALUE", 1f },                                         
                { PrimitiveType.Float, "STENCIL_MAP_STRONGEST_EDGE_VALUE", 2f },                                         
                { PrimitiveType.Float, "STENCIL_MAP_WEAKEST_EDGE_VALUE", 3f },

                { PrimitiveType.Float, "CONSTRAINT_MAP_INITIAL_VALUE", 0f },
                { PrimitiveType.Float, "CONSTRAINT_MAP_SPLIT_BORDER_NEW_VALUE", -1f },
                { PrimitiveType.Float, "CONSTRAINT_MAP_HARD_FOREGROUND_USER", -100000f },                               
                { PrimitiveType.Float, "CONSTRAINT_MAP_HARD_BACKGROUND_USER", 100000f },
                { PrimitiveType.Float, "CONSTRAINT_MAP_HARD_FOREGROUND_AUTO", -99999f },                               
                { PrimitiveType.Float, "CONSTRAINT_MAP_HARD_BACKGROUND_AUTO", 99999f },        
                { PrimitiveType.Float, "CONSTRAINT_MAP_FALLOFF_GAUSSIAN_SIGMA", 0.12f },                           
                { PrimitiveType.Float, "CONSTRAINT_MAP_MIN_UPDATE_THRESHOLD", 1000f },                             
                { PrimitiveType.Float, "CONSTRAINT_MAP_INITIALIZE_FROM_ID_MAP_DELTA_FOREGROUND", -10f }, 
                { PrimitiveType.Float, "CONSTRAINT_MAP_INITIALIZE_FROM_ID_MAP_DELTA_BACKGROUND", 2000f },
                { PrimitiveType.Float, "CONSTRAINT_MAP_INITIALIZE_FROM_COST_MAP_MIN_FOREGROUND", -100f },          
                { PrimitiveType.Float, "CONSTRAINT_MAP_INITIALIZE_FROM_COST_MAP_MAX_BACKGROUND", 100f },           

                { PrimitiveType.Float, "COST_MAP_INITIAL_VALUE", 999999f },                                        
                { PrimitiveType.Float, "COST_MAP_MAX_VALUE", 999999f },                                            
                { PrimitiveType.Float, "COST_MAP_OPTICAL_FLOW_IMPORTANCE_FACTOR", 10f },                           
                { PrimitiveType.Float, "COST_MAP_INITIAL_MAX_FOREGROUND_COST_DELTA", 50f },                        

                { PrimitiveType.Float, "SCRATCHPAD_MAP_INITIAL_VALUE", 0.0f },

                { PrimitiveType.Float4, "SCRATCHPAD_MAP_INITIAL_VALUE", new Vector4( 0f, 0f, 0f, 0f ) },

                { PrimitiveType.Int, "ID_MAP_INITIAL_VALUE", 0 },
                { PrimitiveType.Int, "CONSTRAINT_MAP_SPLIT_BACKGROUND_CONTRACT_NUM_PASSES", 1 },                                                                                 

                { PrimitiveType.Bool, "DIRECT_SCRIBBLE_PROPAGATION", false },
                { PrimitiveType.Bool, "DIRECT_ANISOTROPIC_TV", false },
                { PrimitiveType.Bool, "DUMP_EDGE_XY_MAP", false },
                { PrimitiveType.Bool, "DUMP_EDGE_Z_MAP", false },
                { PrimitiveType.Bool, "DUMP_CONSTRAINT_MAP", false },
                { PrimitiveType.Bool, "DUMP_PRIMAL_MAP", false },
                { PrimitiveType.Bool, "DUMP_COLOR_MAP", false },
                { PrimitiveType.Bool, "DUMP_ID_MAP", false }
            };

        public static readonly List< string > BadProcesses = new List< string >
                                                             {
                                                                 "Process 189",
                                                                 "Process 201",
                                                                 "Process 414"
                                                             };

        public static readonly NeuralProcessDescription DEFAULT_NEURAL_PROCESS =
            new NeuralProcessDescription( 9999 )
            {
                Name = "Default Neural Process",
                Color = new Vector3( 0f, 255f, 255f ),
                BreadcrumbDescriptions = new List<BreadcrumbDescription>()
            };

        public static readonly NeuralProcessDescription NULL_NEURAL_PROCESS =
            new NeuralProcessDescription( 0 )
            {
                Name = "NULL Neural Process",
                Color = new Vector3( 0f, 0f, 0f ),
                BreadcrumbDescriptions = new List<BreadcrumbDescription>()
            };

        public static RecordingMode RECORDING_MODE = RecordingMode.RecordingWithSoftConstraintsInvisible;
        public static ToolMode TOOL_MODE = ToolMode.Segmenter2D;

        public const float CONVERGENCE_GAP_THRESHOLD = 2.0f;
        public const float CONVERGENCE_DELTA_THRESHOLD = 0.0001f;
                                                                                             
        public const int NUM_ITERATIONS_PER_VISUAL_UPDATE_HIGH_LATENCY_2D = 100;             
        public const int NUM_ITERATIONS_PER_VISUAL_UPDATE_LOW_LATENCY_2D = 20;               
                                                                                             
        public const int NUM_ITERATIONS_PER_VISUAL_UPDATE_HIGH_LATENCY_3D = 50;
        public const int NUM_ITERATIONS_PER_VISUAL_UPDATE_LOW_LATENCY_3D = 10;

        public const int MAX_BRUSH_WIDTH = 50;                                               
        public const int MIN_BRUSH_WIDTH = 8;                                  
        public const int NUM_DETENTS_PER_WHEEL_MOVE = 120;                                   
        public const int NUM_CONSTRAINT_MAP_DILATION_PASSES_INITIALIZE_NEW_PROCESS = 0;

        public static bool DEBUG_D3D11_DEVICE = false;
        public static bool LOAD_BREADCRUMBS = false;
        public static bool LOAD_DATASET = false;
        public static bool AUTOMATIC_SEGMENTATION = false;
        public static bool REMOVE_BAD_PROCESSES = true;
        public static bool DEBUG_SEGMENTER = false;
        public static bool DIRECT_SCRIBBLE_PROPAGATION = false;

        //public static readonly string DATASET_NAME = "OldData.Test.OutOfPlane";
        //public static readonly string DATASET_CONTOUR_NAME = "OldData.Contours.Test.OutOfPlane";
        //public static readonly Vector3 DATASET_BREADCRUMB_TRAIL_COORDINATE_OFFSET = new Vector3( 0, 0, 0 );

        public static readonly string DATASET_NAME = "OldData.Test.2D";
        public static readonly string DATASET_CONTOUR_NAME = "OldData.Contours.Test.2D";
        public static readonly Vector3 DATASET_BREADCRUMB_TRAIL_COORDINATE_OFFSET = new Vector3( 256, 256, 0 );

        //public static readonly string DATASET_NAME = "OldData.Test";
        //public static readonly string DATASET_CONTOUR_NAME = "OldData.Contours.Test";
        //public static readonly Vector3 DATASET_BREADCRUMB_TRAIL_COORDINATE_OFFSET = new Vector3( 256, 256, 0 );

        //public static readonly string DATASET_NAME = "OldData.CC.000.049";
        //public static readonly string DATASET_CONTOUR_NAME = "OldData.Contours.000.049";
        //public static readonly Vector3 DATASET_BREADCRUMB_TRAIL_COORDINATE_OFFSET = new Vector3( 256, 256, 0 );

        //public static readonly string DATASET_NAME = "OldData.CC.050.099";
        //public static readonly string DATASET_CONTOUR_NAME = "OldData.Contours.050.099";
        //public static readonly Vector3 DATASET_BREADCRUMB_TRAIL_COORDINATE_OFFSET = new Vector3( 256, 256, 50 );

        //public static readonly string DATASET_NAME = "OldData.CC.100.149";
        //public static readonly string DATASET_CONTOUR_NAME = "OldData.Contours.100.149";
        //public static readonly Vector3 DATASET_BREADCRUMB_TRAIL_COORDINATE_OFFSET = new Vector3( 256, 256, 100 );

        //public static readonly string DATASET_NAME = "OldData.TL.050.099";
        //public static readonly string DATASET_CONTOUR_NAME = "OldData.Contours.050.099";
        //public static readonly Vector3 DATASET_BREADCRUMB_TRAIL_COORDINATE_OFFSET = new Vector3( 0, 0, 50 );

        //public static readonly string DATASET_NAME = "OldData.TL.100.149";
        //public static readonly string DATASET_CONTOUR_NAME = "OldData.Contours.100.149";
        //public static readonly Vector3 DATASET_BREADCRUMB_TRAIL_COORDINATE_OFFSET = new Vector3( 0, 0, 100 );

        //public static readonly string DATASET_NAME = "OldData.TR.000.049";
        //public static readonly string DATASET_CONTOUR_NAME = "OldData.Contours.000.049";
        //public static readonly Vector3 DATASET_BREADCRUMB_TRAIL_COORDINATE_OFFSET = new Vector3( 512, 0, 0 );

        //public static readonly string DATASET_NAME = "OldData.TR.050.099";
        //public static readonly string DATASET_CONTOUR_NAME = "OldData.Contours.050.099";
        //public static readonly Vector3 DATASET_BREADCRUMB_TRAIL_COORDINATE_OFFSET = new Vector3( 512, 0, 50 );
    }
}
// ReSharper restore RedundantDefaultFieldInitializer
// ReSharper restore InconsistentNaming
